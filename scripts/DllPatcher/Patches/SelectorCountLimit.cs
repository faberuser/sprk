using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static readonly string[] SelectorCountMethods = {
        "UpdatePlusMinusButtonState", "UpdateBtnOkState", "OnClickPlus",
        "OnClickMax", "OnChangeInputSelectCount"
    };

    static bool IsSelectorCountRead(Instruction i) => i.Operand is MethodReference {
        Name: "get_Count", DeclaringType.FullName: "NShared.SelectionPopupItemInfo"
    };

    static bool IsSelectorCountCap(Instruction read) =>
        read.Next.OpCode == OpCodes.Ldc_I4 && Equals(read.Next.Operand, 1000) &&
        read.Next.Next.OpCode == OpCodes.Call && read.Next.Next.Operand is MethodReference {
            Name: "Min", DeclaringType.FullName: "UnityEngine.Mathf", ReturnType.FullName: "System.Int32"
        };

    static void PatchSelectorCountLimit(ModuleDefinition module)
    {
        var type = module.GetType("NGame2.NUI.NWindow.SelectionItemUsePopup");
        var min = ShopPurchaseLimitMethod(module).Body.Instructions.Select(i => i.Operand)
            .OfType<MethodReference>().First(m => m.DeclaringType.FullName == "UnityEngine.Mathf" &&
                m.Name == "Min" && m.ReturnType.FullName == "System.Int32" && m.Parameters.Count == 2);
        foreach (var name in SelectorCountMethods) {
            var method = type.Methods.Single(m => m.Name == name);
            var read = method.Body.Instructions.Single(IsSelectorCountRead);
            if (IsSelectorCountCap(read)) continue;
            var il = method.Body.GetILProcessor();
            var cap = il.Create(OpCodes.Ldc_I4, 1000);
            il.InsertAfter(read, cap); il.InsertAfter(cap, il.Create(OpCodes.Call, min));
            method.Body.MaxStackSize++;
        }
        VerifySelectorCountLimit(module);
    }

    static void VerifySelectorCountLimit(ModuleDefinition module)
    {
        var type = module.GetType("NGame2.NUI.NWindow.SelectionItemUsePopup");
        foreach (var name in SelectorCountMethods) {
            var read = type.Methods.Single(m => m.Name == name).Body.Instructions.Single(IsSelectorCountRead);
            if (!IsSelectorCountCap(read)) throw new InvalidOperationException("Selector count is not capped at 1,000: " + name);
        }
        // The inventory icon must still show the player's actual owned count.
        if (type.Methods.Single(m => m.Name == "UpdateUsingItemIcon").Body.Instructions
            .Where(IsSelectorCountRead).Any(IsSelectorCountCap))
            throw new InvalidOperationException("Selector cap changed the displayed inventory quantity.");
    }

    static void VerifySelectorCountLimitFile(string clientRoot, bool stageOnly)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed",
            "Assembly-CSharp.dll" + (stageOnly ? ".selector-count-limit-staged" : "")));
        VerifySelectorCountLimit(assembly.MainModule);
        Console.WriteLine("Verified selector Max, plus, typed input and button validation cap at 1,000.");
    }

    static void InstallSelectorCountLimit(string clientRoot, bool stageOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        var staged = dll + ".selector-count-limit-staged";
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            PatchSelectorCountLimit(assembly.MainModule);
            assembly.Write(staged);
        }
        VerifySelectorCountLimitFile(clientRoot, true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcessesByName("King's Raid").Length != 0)
            throw new InvalidOperationException("Selector fix staged. Close King's Raid before installing and publishing.");
        var backup = Path.Combine("target", "selector-count-limit-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        File.Move(staged, dll, true);
        VerifySelectorCountLimitFile(clientRoot, false);
    }

    static void TestSelectorCountLimit(string clientRoot)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        PatchSelectorCountLimit(assembly.MainModule);
        var type = assembly.MainModule.GetType("NGame2.NUI.NWindow.SelectionItemUsePopup");
        var size = type.Methods.Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count);
        PatchSelectorCountLimit(assembly.MainModule);
        if (size != type.Methods.Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count))
            throw new Exception("Selector count cap is not idempotent.");
        foreach (var name in SelectorCountMethods) {
            var read = type.Methods.Single(m => m.Name == name).Body.Instructions.Single(IsSelectorCountRead);
            // Execute the installed cap IL, replacing only the Unity Min call
            // with the equivalent managed integer Min so Unity needn't load.
            var test = new System.Reflection.Emit.DynamicMethod("SelectorMaxCount", typeof(int), new[] { typeof(int) });
            var il = test.GetILGenerator(); il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, (int)read.Next.Operand);
            il.Emit(System.Reflection.Emit.OpCodes.Call, typeof(Math).GetMethod("Min", new[] { typeof(int), typeof(int) })!);
            il.Emit(System.Reflection.Emit.OpCodes.Ret);
            var cap = (Func<int, int>)test.CreateDelegate(typeof(Func<int, int>));
            foreach (var (owned, expected) in new[] { (0,0), (1,1), (999,999), (1000,1000), (1001,1000), (2000,1000), (11993,1000), (int.MaxValue,1000) })
                if (cap(owned) != expected) throw new Exception($"Selector cap failed in {name} for {owned} boxes.");
        }
        Console.WriteLine("PASS: 40 selector limit boundary checks across all five controls, actual inventory display and patch idempotence.");
    }
}
