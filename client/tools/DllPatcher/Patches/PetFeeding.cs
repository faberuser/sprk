using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    const string PetFeedType = "NGame2.NUI.NComponent2.NPet.PetFeedComponent";
    const string PetInteractionFeedType = "NGame2.NUI.NWindow.NPet.PetInteractionFeedButton";

    static void PatchPetFeeding(ModuleDefinition module)
    {
        var feed = module.GetType(PetFeedType);
        var pet = feed.Methods.Single(m => m.Name == "get__petInfo");
        var status = module.GetType("NGame2.NUI.NHelper.NPet.PetHelper").Methods
            .Single(m => m.Name == "CanExplorePetStatus" && m.Parameters.Count == 1);
        var guard = feed.Methods.SingleOrDefault(m => m.Name == "SprkCanFeed");
        if (guard == null) {
            guard = new MethodDefinition("SprkCanFeed", MethodAttributes.Private, module.TypeSystem.Boolean);
            feed.Methods.Add(guard);
        }
        guard.Body = new MethodBody(guard);
        var il = guard.Body.GetILProcessor();
        var no = Instruction.Create(OpCodes.Ldc_I4_0);
        void Field(string name) { il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, feed.Fields.Single(f => f.Name == name)); }
        Field("_listener"); il.Emit(OpCodes.Brfalse, no);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, pet); il.Emit(OpCodes.Brfalse, no);
        Field("_isPress"); il.Emit(OpCodes.Brtrue, no);
        Field("_gaugeAni"); il.Emit(OpCodes.Brtrue, no);
        Field("_feedAmount"); il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Ble, no);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, feed.Methods.Single(m => m.Name == "get__remainFullPoint"));
        il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Ble, no);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, pet); il.Emit(OpCodes.Call, status); il.Emit(OpCodes.Ret);
        il.Append(no); il.Emit(OpCodes.Ret);

        foreach (var name in new[] { "OnPressFeedButton", "OnClickFeedMaxButton" }) {
            var method = feed.Methods.Single(m => m.Name == name);
            method.Body = new MethodBody(method);
            il = method.Body.GetILProcessor();
            var end = Instruction.Create(OpCodes.Ret);
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, guard); il.Emit(OpCodes.Brfalse, end);
            il.Emit(OpCodes.Ldarg_0);
            if (name == "OnClickFeedMaxButton") {
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, feed.Methods.Single(m => m.Name == "GetRemainMaxFeedCount"));
            } else il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stfld, feed.Fields.Single(f => f.Name == "_usedCount"));
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Stfld, feed.Fields.Single(f => f.Name == "_isPress"));
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, feed.Methods.Single(m => m.Name ==
                (name == "OnClickFeedMaxButton" ? "OnReleaseFeedButton" : "UpdateComp")));
            il.Append(end);
        }

        var interaction = module.GetType(PetInteractionFeedType);
        var press = interaction.Methods.Single(m => m.Name == "OnPressEat");
        press.Body = new MethodBody(press);
        il = press.Body.GetILProcessor();
        var done = Instruction.Create(OpCodes.Ret);
        var listener = interaction.Fields.Single(f => f.Name == "_iListener");
        var provider = interaction.Fields.Single(f => f.Name == "_iProvider");
        void Read(FieldDefinition f) { il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, f); }
        Read(listener); il.Emit(OpCodes.Brfalse, done);
        Read(provider); il.Emit(OpCodes.Brfalse, done);
        Read(interaction.Fields.Single(f => f.Name == "_eEatState")); il.Emit(OpCodes.Ldc_I4_2); il.Emit(OpCodes.Bne_Un, done);
        Read(listener); il.Emit(OpCodes.Callvirt, interaction.NestedTypes.Single(t => t.Name == "IListener").Methods.Single(m => m.Name == "get_CheckStartPetEat"));
        il.Emit(OpCodes.Brfalse, done);
        Read(provider); il.Emit(OpCodes.Callvirt, module.GetType("NGame2.NUI.NWindow.NPet.IPetInteractionProvider").Methods.Single(m => m.Name == "get_IsFullFullPoint"));
        il.Emit(OpCodes.Brtrue, done);
        Read(interaction.Fields.Single(f => f.Name == "_remainPetFoodCount"));
        il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Conv_I8); il.Emit(OpCodes.Ble, done);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldc_I4_0); il.Emit(OpCodes.Stfld, interaction.Fields.Single(f => f.Name == "_eEatState"));
        il.Append(done);
        VerifyPetFeeding(module);
        Console.WriteLine("Restored Lil' Raider feeding buttons and native feed/release flow.");
    }

    static void VerifyPetFeeding(ModuleDefinition module)
    {
        var feed = module.GetType(PetFeedType);
        void Calls(string type, string method, params string[] required) {
            var body = module.GetType(type).Methods.Single(m => m.Name == method).Body;
            var calls = body.Instructions.Select(i => i.Operand).OfType<MethodReference>().Select(m => m.Name).ToArray();
            if (required.Any(name => !calls.Contains(name)) || body.Instructions.Any(i => Equals(i.Operand, "RAIDER_CONTENTS_DELETE_DESC")))
                throw new InvalidOperationException("Broken Lil' Raider feeding: " + method);
        }
        Calls(PetFeedType, "SprkCanFeed", "get__petInfo", "get__remainFullPoint", "CanExplorePetStatus");
        Calls(PetFeedType, "OnPressFeedButton", "SprkCanFeed", "UpdateComp");
        Calls(PetFeedType, "OnClickFeedMaxButton", "SprkCanFeed", "GetRemainMaxFeedCount", "OnReleaseFeedButton");
        Calls(PetFeedType, "OnReleaseFeedButton", "OnClickPetFeed");
        Calls(PetInteractionFeedType, "OnPressEat", "get_CheckStartPetEat", "get_IsFullFullPoint");
        Calls(PetInteractionFeedType, "EndPetEat", "EndPetEat");
        foreach (var field in new[] { "_listener", "_isPress", "_gaugeAni", "_feedAmount" })
            if (!feed.Methods.Single(m => m.Name == "SprkCanFeed").Body.Instructions.Any(i => i.Operand is FieldReference f && f.Name == field))
                throw new InvalidOperationException("Missing feeding guard: " + field);
    }

    static void VerifyPetFeedingFile(string root)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        VerifyPetFeeding(assembly.MainModule);
        Console.WriteLine("Verified Lil' Raider feed, Feed Max, interaction and native request hooks.");
    }

    static void InstallPetFeeding(string root, bool stageOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        var staged = dll + ".pet-feeding-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true })) {
            PatchPetFeeding(assembly.MainModule);
            assembly.Write(staged);
        }
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcessesByName("King's Raid").Length != 0)
            throw new InvalidOperationException("Feeding fix staged. Close King's Raid before installing and publishing.");
        var backup = Path.Combine("target", "pet-feeding-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        File.Copy(staged, dll, true);
        VerifyPetFeedingFile(root);
    }

    static void TestPetFeeding(string root)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        PatchPetFeeding(assembly.MainModule);
        var guard = assembly.MainModule.GetType(PetFeedType).Methods.Single(m => m.Name == "SprkCanFeed");
        var size = guard.Body.Instructions.Count;
        PatchPetFeeding(assembly.MainModule);
        if (guard.Body.Instructions.Count != size) throw new Exception("Pet feeding patch is not idempotent.");
        // Execute the production guard IL with native fields/getters supplied as
        // arguments so these cases do not require loading Unity's native engine.
        var test = new System.Reflection.Emit.DynamicMethod("PetFeedGuard", typeof(bool),
            new[] { typeof(object), typeof(object), typeof(bool), typeof(int), typeof(int), typeof(int), typeof(bool) });
        var il = test.GetILGenerator();
        var no = il.DefineLabel();
        var status = il.DeclareLocal(typeof(bool));
        foreach (var instruction in guard.Body.Instructions) {
            if (instruction.OpCode == OpCodes.Ldarg_0) continue;
            if (instruction.Operand is FieldReference field) {
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg, (short)(field.Name switch {
                    "_listener" => 0, "_isPress" => 2, "_gaugeAni" => 3, "_feedAmount" => 4,
                    _ => throw new Exception("Unexpected feeding field " + field.Name) }));
            } else if (instruction.Operand is MethodReference method) {
                il.Emit(System.Reflection.Emit.OpCodes.Ldarg, (short)(method.Name switch {
                    "get__petInfo" => 1, "get__remainFullPoint" => 5, "CanExplorePetStatus" => 6,
                    _ => throw new Exception("Unexpected feeding method " + method.Name) }));
                // The native status call consumes the PetInfo getter's result.
                if (method.Name == "CanExplorePetStatus") {
                    il.Emit(System.Reflection.Emit.OpCodes.Stloc, status);
                    il.Emit(System.Reflection.Emit.OpCodes.Pop);
                    il.Emit(System.Reflection.Emit.OpCodes.Ldloc, status);
                }
            } else if (instruction.OpCode == OpCodes.Ldc_I4_0) {
                if (instruction.Previous.OpCode == OpCodes.Ret) il.MarkLabel(no);
                il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4_0);
            } else if (instruction.OpCode == OpCodes.Brfalse) il.Emit(System.Reflection.Emit.OpCodes.Brfalse, no);
            else if (instruction.OpCode == OpCodes.Brtrue) il.Emit(System.Reflection.Emit.OpCodes.Brtrue, no);
            else if (instruction.OpCode == OpCodes.Ble) il.Emit(System.Reflection.Emit.OpCodes.Ble, no);
            else if (instruction.OpCode == OpCodes.Ret) il.Emit(System.Reflection.Emit.OpCodes.Ret);
            else throw new Exception("Unexpected feeding IL " + instruction);
        }
        var canFeed = (Func<object?, object?, bool, int, int, int, bool, bool>)test.CreateDelegate(
            typeof(Func<object?, object?, bool, int, int, int, bool, bool>));
        var valid = new object();
        if (!canFeed(valid, valid, false, 0, 1, 1, true)) throw new Exception("Ready pet cannot feed.");
        foreach (var failure in Enumerable.Range(0, 7)) {
            if (canFeed(failure == 0 ? null : valid, failure == 1 ? null : valid,
                failure == 2, failure == 3 ? 1 : 0, failure == 4 ? 0 : 1,
                failure == 5 ? 0 : 1, failure != 6)) throw new Exception("Feeding guard failed " + failure);
        }
        Console.WriteLine("PASS: 8 production feeding guard cases and patch idempotence.");
    }
}
