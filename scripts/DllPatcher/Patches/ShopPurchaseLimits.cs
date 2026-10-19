using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static MethodDefinition ShopPurchaseLimitMethod(ModuleDefinition module) =>
        module.GetType("NGame2.NUI.NWindow.MultipleBuyPopup").Methods.Single(m => m.Name == "GetItemBuyMaxCount");

    static Instruction ShopPurchaseInfoBranch(MethodDefinition method)
    {
        var read = method.Body.Instructions.Single(i => i.OpCode == OpCodes.Ldfld &&
            i.Operand is FieldReference { Name: "_itemInfo" } &&
            (i.Next.OpCode == OpCodes.Brfalse || i.Next.OpCode == OpCodes.Brfalse_S));
        return read.Next;
    }

    static void PatchShopPurchaseLimits(ModuleDefinition module)
    {
        var method = ShopPurchaseLimitMethod(module);
        var branch = ShopPurchaseInfoBranch(method);
        if (branch.Next.Next.Operand is FieldReference { Name: "_shopItemData" } &&
            branch.Next.Next.Next.Operand is MethodReference { Name: "get_PurchasableCount" } &&
            branch.Next.Next.Next.Next.Next.OpCode == OpCodes.Ble) {
            VerifyShopPurchaseLimits(module); return;
        }
        var data = method.DeclaringType.Fields.Single(f => f.Name == "_shopItemData");
        var count = method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>()
            .First(m => m.DeclaringType.FullName == "NShared.ShopItemData" && m.Name == "get_PurchasableCount");
        var next = branch.Next;
        var il = method.Body.GetILProcessor();
        // A purchase ledger entry also exists for unlimited items. Only apply
        // its remaining-stock clamp when the table defines a positive limit.
        foreach (var instruction in new[] {
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, data),
            Instruction.Create(OpCodes.Callvirt, count), Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Ble, (Instruction)branch.Operand)
        }) il.InsertBefore(next, instruction);
        VerifyShopPurchaseLimits(module);
    }

    static void VerifyShopPurchaseLimits(ModuleDefinition module)
    {
        var branch = ShopPurchaseInfoBranch(ShopPurchaseLimitMethod(module));
        var guard = new List<Instruction>();
        for (var i = branch.Next; guard.Count < 5; i = i.Next) guard.Add(i);
        if (!guard.Select(i => i.OpCode.Code).SequenceEqual(new[] {
            Code.Ldarg_0, Code.Ldfld, Code.Callvirt, Code.Ldc_I4_0, Code.Ble }) ||
            guard[1].Operand is not FieldReference { Name: "_shopItemData" } ||
            guard[2].Operand is not MethodReference { Name: "get_PurchasableCount" } ||
            !ReferenceEquals(guard[4].Operand, branch.Operand))
            throw new InvalidOperationException("Shop popup does not preserve unlimited stock with purchase history.");
    }

    static void VerifyShopPurchaseLimitsFile(string clientRoot, bool staged)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed",
            "Assembly-CSharp.dll" + (staged ? ".shop-purchase-limits-staged" : "")));
        VerifyShopPurchaseLimits(assembly.MainModule);
        Console.WriteLine("Verified unlimited shop stock guard; native affordability and finite limits preserved.");
    }

    static void InstallShopPurchaseLimits(string clientRoot, bool stageOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        var staged = dll + ".shop-purchase-limits-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            PatchShopPurchaseLimits(assembly.MainModule);
            assembly.Write(staged);
        }
        VerifyShopPurchaseLimitsFile(clientRoot, true);
        if (stageOnly) return;
        // Loaded Unity assemblies must be installed only after the game closes.
        if (System.Diagnostics.Process.GetProcessesByName("King's Raid").Length != 0)
            throw new InvalidOperationException("Fix staged and verified. Close King's Raid before installing and publishing.");
        var backup = Path.Combine("target", "shop-purchase-limits-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        File.Move(staged, dll, true);
        VerifyShopPurchaseLimitsFile(clientRoot, false);
    }

    static void TestShopPurchaseLimits(string clientRoot)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        PatchShopPurchaseLimits(assembly.MainModule);
        var method = ShopPurchaseLimitMethod(assembly.MainModule);
        var size = method.Body.Instructions.Count;
        PatchShopPurchaseLimits(assembly.MainModule);
        if (size != method.Body.Instructions.Count) throw new Exception("Shop patch is not idempotent.");
        // Execute the actual production branch and inserted guard, substituting
        // the owning object's fields with parameters to avoid loading Unity.
        var test = new System.Reflection.Emit.DynamicMethod("ShopLimitGuard", typeof(bool), new[] { typeof(object), typeof(int) });
        var il = test.GetILGenerator();
        var unlimited = il.DefineLabel();
        var branch = ShopPurchaseInfoBranch(method);
        for (var i = branch.Previous.Previous; i != branch.Next.Next.Next.Next.Next.Next; i = i.Next) {
            if (i.OpCode == OpCodes.Ldarg_0) continue;
            if (i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference { Name: "_itemInfo" }) il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            else if (i.OpCode == OpCodes.Ldfld) { /* owner replaced by count argument below */ }
            else if (i.OpCode == OpCodes.Callvirt) il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
            else if (i.OpCode == OpCodes.Ldc_I4_0) il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
            else if (i.OpCode == OpCodes.Brfalse || i.OpCode == OpCodes.Brfalse_S) il.Emit(System.Reflection.Emit.OpCodes.Brfalse, unlimited);
            else if (i.OpCode == OpCodes.Ble) il.Emit(System.Reflection.Emit.OpCodes.Ble, unlimited);
            else throw new Exception("Unexpected shop guard instruction: " + i);
        }
        il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_1); il.Emit(System.Reflection.Emit.OpCodes.Ret);
        il.MarkLabel(unlimited); il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0); il.Emit(System.Reflection.Emit.OpCodes.Ret);
        var clampsHistory = (Func<object?, int, bool>)test.CreateDelegate(typeof(Func<object?, int, bool>));
        foreach (var history in new object?[] { null, new object() })
            foreach (var limit in new[] { -1, 0, 1, 10, 100 })
                if (clampsHistory(history, limit) != (history != null && limit > 0))
                    throw new Exception($"Incorrect shop clamp: history={history != null}, limit={limit}");
        Console.WriteLine("PASS: 10 production IL history/stock-limit cases and patch idempotence.");
    }
}
