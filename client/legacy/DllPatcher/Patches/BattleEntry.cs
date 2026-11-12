using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    // Zero means that no request has been sent. The native subtraction treated
    // that sentinel as a request at process startup, swallowing early confirms.
    static void PatchBattleEntry(ModuleDefinition module)
    {
        var type = module.GetType("NGame2.NUI.NManager.NLobby.LobbyCampaignDungeon");
        var method = type.Methods.Single(m => m.Name == "NGame2.NUI.NWindow.BattlePartySetting.Listener.StartBattle");
        var stamp = type.Fields.Single(f => f.Name == "_battleRequestedTime");
        var body = method.Body.Instructions;
        var throttle = body.FirstOrDefault(i => i.OpCode == OpCodes.Ldc_R4 && Equals(i.Operand, 10f));
        if (throttle?.Next?.Operand is not Instruction ready ||
            (throttle.Next.OpCode != OpCodes.Bge_Un && throttle.Next.OpCode != OpCodes.Bge_Un_S) ||
            throttle.Next.Next.OpCode != OpCodes.Ret)
            throw new InvalidOperationException("Unexpected campaign entry throttle; refusing to patch.");
        var firstRead = body.First(i => i.Operand is FieldReference f && f.FullName == stamp.FullName);
        // An existing sentinel branch is the idempotent patch marker.
        if (firstRead.Next.OpCode == OpCodes.Ldc_R4 && Equals(firstRead.Next.Operand, 0f) &&
            firstRead.Next.Next.OpCode == OpCodes.Beq && ReferenceEquals(firstRead.Next.Next.Operand, ready))
            return;
        // Keep the compiler's captured callback arguments initialized before
        // bypassing the throttle. Only skip the clock comparison itself.
        var anchor = body.Single(i => i.Operand is MethodReference r &&
            r.DeclaringType.FullName == "UnityEngine.Time" && r.Name == "get_time");
        foreach (var i in new[] { Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, stamp),
            Instruction.Create(OpCodes.Ldc_R4, 0f), Instruction.Create(OpCodes.Beq, ready) })
            method.Body.GetILProcessor().InsertBefore(anchor, i);
        VerifyBattleEntry(module);
    }

    static void VerifyBattleEntry(ModuleDefinition module)
    {
        var method = module.GetType("NGame2.NUI.NManager.NLobby.LobbyCampaignDungeon").Methods
            .Single(m => m.Name == "NGame2.NUI.NWindow.BattlePartySetting.Listener.StartBattle");
        var body = method.Body.Instructions;
        var read = body.First(i => i.Operand is FieldReference f && f.Name == "_battleRequestedTime");
        var timeout = body.Single(i => i.OpCode == OpCodes.Ldc_R4 && Equals(i.Operand, 10f));
        if (read.Next.OpCode != OpCodes.Ldc_R4 || !Equals(read.Next.Operand, 0f) ||
            read.Next.Next.OpCode != OpCodes.Beq || !ReferenceEquals(read.Next.Next.Operand, timeout.Next.Operand) ||
            !module.GetTypes().Where(t => t.FullName.StartsWith(method.DeclaringType.FullName + "/"))
                .SelectMany(t => t.Methods).Where(m => m.HasBody).SelectMany(m => m.Body.Instructions)
                .Any(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "_battleRequestedTime"))
            throw new InvalidOperationException("Campaign entry sentinel or duplicate-request guard is missing.");
    }

    static void InstallBattleEntry(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".battle-entry-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            PatchBattleEntry(assembly.MainModule);
            VerifyBattleEntry(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var verify = AssemblyDefinition.ReadAssembly(staged)) VerifyBattleEntry(verify.MainModule);
        if (!stageOnly) {
            var backup = Path.Combine("target", "battle-entry-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified first battle entry fix." : "Installed and verified first battle entry fix. Restart the client.");
    }

    static void TestBattleEntry(string clientRoot)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll"),
            new ReaderParameters { AssemblyResolver = resolver });
        PatchBattleEntry(assembly.MainModule);
        var method = assembly.MainModule.GetType("NGame2.NUI.NManager.NLobby.LobbyCampaignDungeon").Methods
            .Single(m => m.Name.EndsWith("Listener.StartBattle"));
        var body = method.Body.Instructions;
        var firstRead = body.First(i => i.Operand is FieldReference f && f.Name == "_battleRequestedTime");
        var timeout = body.Single(i => i.OpCode == OpCodes.Ldc_R4 && Equals(i.Operand, 10f));
        var ready = (Instruction)timeout.Next.Operand;
        // Execute the actual patched IL comparison with controlled clock values.
        // Replace only Unity's clock and the owning object's timestamp field.
        var test = new System.Reflection.Emit.DynamicMethod("CampaignEntryGuard", typeof(bool),
            new[] { typeof(float), typeof(float) });
        var il = test.GetILGenerator();
        var pass = il.DefineLabel();
        for (var i = firstRead.Previous; !ReferenceEquals(i, ready); i = i.Next) {
            if (i.OpCode == OpCodes.Ldarg_0) continue; // owner consumed by ldfld
            if (i.OpCode == OpCodes.Ldfld) il.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
            else if (i.OpCode == OpCodes.Call && i.Operand is MethodReference { Name: "get_time" })
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            else if (i.OpCode == OpCodes.Ldc_R4) il.Emit(System.Reflection.Emit.OpCodes.Ldc_R4, (float)i.Operand);
            else if (i.OpCode == OpCodes.Sub) il.Emit(System.Reflection.Emit.OpCodes.Sub);
            else if (i.OpCode == OpCodes.Beq) il.Emit(System.Reflection.Emit.OpCodes.Beq, pass);
            else if (i.OpCode == OpCodes.Bge_Un || i.OpCode == OpCodes.Bge_Un_S)
                il.Emit(System.Reflection.Emit.OpCodes.Bge_Un, pass);
            else if (i.OpCode == OpCodes.Ret) {
                il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0); il.Emit(System.Reflection.Emit.OpCodes.Ret);
            }
            else throw new InvalidOperationException("Unexpected guard instruction: " + i);
        }
        il.MarkLabel(pass); il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_1); il.Emit(System.Reflection.Emit.OpCodes.Ret);
        var allow = (Func<float, float, bool>)test.CreateDelegate(typeof(Func<float, float, bool>));
        foreach (var (clock, stamp, expected) in new[] {
            (0f, 0f, true), (1f, 0f, true), (9.9f, 0f, true), (100f, 0f, true),
            (2f, 1f, false), (10.99f, 1f, false), (11f, 1f, true), (12f, 1f, true) })
            if (allow(clock, stamp) != expected) throw new InvalidOperationException($"Entry guard failed: clock={clock}, last request={stamp}");
        var instructions = method.Body.Instructions.Count;
        PatchBattleEntry(assembly.MainModule);
        VerifyBattleEntry(assembly.MainModule);
        if (instructions != method.Body.Instructions.Count) throw new InvalidOperationException("Battle entry patch is not idempotent.");
        Console.WriteLine("Passed 8 native IL clock/sentinel/duplicate-request cases and patch idempotence.");
    }
}
