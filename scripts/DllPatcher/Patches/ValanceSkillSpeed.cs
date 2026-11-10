using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string ValanceSpeedGuard = "SprkIsValanceS3SpeedClamp";

    static void PatchValanceSkillSpeed(ModuleDefinition module)
    {
        var type = module.GetType("NShared.NOperationRuntime.OverrideStat");
        var guard = type.Methods.SingleOrDefault(m => m.Name == ValanceSpeedGuard);
        if (guard == null)
        {
            guard = new MethodDefinition(ValanceSpeedGuard, Mono.Cecil.MethodAttributes.Private,
                module.TypeSystem.Boolean);
            type.Methods.Add(guard);
        }
        guard.Body = new Mono.Cecil.Cil.MethodBody(guard) { MaxStackSize = 2 };
        var il = guard.Body.GetILProcessor();
        var no = Instruction.Create(OpCodes.Ldc_I4_0);
        var info = module.GetType("NShared.BaseOperationRuntime").Methods.Single(m => m.Name == "get_OperationInfo");
        var data = module.GetType("NShared.OperationInfo").Methods.Single(m => m.Name == "get_OperationData");
        // Match the operation, stat and fixed value. Other heroes, skills, stats
        // and override modes continue through the unchanged native lifecycle.
        foreach (var (name, value) in new[] { ("SkillIndex", 11130), ("OperationNo", 1) })
        {
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, info); il.Emit(OpCodes.Callvirt, data);
            il.Emit(OpCodes.Callvirt, module.GetType("NShared.OperationData").Methods.Single(m => m.Name == "get_" + name));
            il.Emit(OpCodes.Ldc_I4, value); il.Emit(OpCodes.Bne_Un, no);
        }
        foreach (var (fieldName, value) in new[] {
            ("_statTypeHolder", Convert.ToInt32(module.GetType("NShared.StatType").Fields.Single(f => f.Name == "AttackSpeed").Constant)),
            ("_statValueHolder", 1000) })
        {
            var field = type.Fields.Single(f => f.Name == fieldName);
            var get = (MethodReference)type.Methods.Single(m => m.Name.EndsWith(fieldName == "_statTypeHolder"
                ? ".GetStatType" : ".GetOverridenStatValue")).Body.Instructions.Single(i => i.Operand is MethodReference { Name: "Get" }).Operand;
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, field); il.Emit(OpCodes.Callvirt, get);
            il.Emit(OpCodes.Ldc_I4, value);
            if (fieldName == "_statValueHolder") il.Emit(OpCodes.Conv_I8);
            il.Emit(OpCodes.Bne_Un, no);
        }
        il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Ret); il.Append(no); il.Emit(OpCodes.Ret);
        foreach (string name in new[] { "OnBegin", "OnEnd" })
        {
            var method = type.Methods.Single(m => m.Name == name);
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == ValanceSpeedGuard)) continue;
            var body = method.Body.GetILProcessor(); var first = method.Body.Instructions[0];
            body.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            body.InsertBefore(first, Instruction.Create(OpCodes.Call, guard));
            body.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, first));
            body.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        VerifyValanceSkillSpeed(module);
    }

    static void VerifyValanceSkillSpeed(ModuleDefinition module)
    {
        var type = module.GetType("NShared.NOperationRuntime.OverrideStat");
        var guard = type.Methods.Single(m => m.Name == ValanceSpeedGuard);
        VerifyArenaResultStack(guard);
        foreach (var (name, native) in new[] { ("OnBegin", "AddOverrider"), ("OnEnd", "RemoveOverrider") })
        {
            var method = type.Methods.Single(m => m.Name == name);
            var calls = method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().ToArray();
            if (calls.Count(m => m.Name == ValanceSpeedGuard) != 1 || calls.Count(m => m.Name == native) != 1
                || method.Body.Instructions[1].Operand != guard || method.Body.Instructions[2].OpCode != OpCodes.Brfalse
                || method.Body.Instructions[3].OpCode != OpCodes.Ret)
                throw new InvalidOperationException("Invalid Valance S3 speed guard: " + name);
            VerifyArenaResultStack(method);
        }
    }

    static void InstallValanceSkillSpeed(string root, bool verifyOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using (var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true }))
        {
            if (!verifyOnly) { PatchValanceSkillSpeed(game.MainModule); PatchValanceSkillSpeed(game.MainModule); }
            VerifyValanceSkillSpeed(game.MainModule);
            AppDomain.CurrentDomain.AssemblyResolve += (_, e) => {
                var path = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
            // Prove the behavioral test rejects the original 1000-speed clamp,
            // then run it against the exact bytes staged for installation.
            using (var regression = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true }))
            {
                PatchValanceSkillSpeed(regression.MainModule);
                var runtime = regression.MainModule.GetType("NShared.NOperationRuntime.OverrideStat");
                foreach (string name in new[] { "OnBegin", "OnEnd" })
                    for (int i = 0; i < 4; i++) runtime.Methods.Single(m => m.Name == name).Body.Instructions.RemoveAt(0);
                using var original = new MemoryStream(); regression.Write(original);
                bool detected = false;
                try { TestValanceSkillSpeed(Assembly.Load(original.ToArray()), managed); }
                catch (InvalidOperationException e) when (e.Message.StartsWith("Incorrect override registration: (11130, 1, AttackSpeed, 1000, False)")) { detected = true; }
                if (!detected) throw new InvalidOperationException("Valance speed test did not detect the original clamp.");
                Console.WriteLine("PASS: native regression fixture detects the original fixed-1000 behavior.");
            }
            using var bytes = new MemoryStream(); game.Write(bytes);
            TestValanceSkillSpeed(Assembly.Load(bytes.ToArray()), managed);
            if (!verifyOnly)
            {
                if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
                    throw new InvalidOperationException("Close game clients and battle workers before installing Valance's S3 fix.");
                var backup = Path.Combine("target", "valance-s3-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
                Directory.CreateDirectory(backup); File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
                game.Write(dll + ".valance-s3-staged");
            }
        }
        resolver.Dispose();
        if (!verifyOnly) File.Move(dll + ".valance-s3-staged", dll, true);
        Console.WriteLine("Verified Valance S3 native attack-speed scaling and unrelated override lifecycles.");
    }
}
