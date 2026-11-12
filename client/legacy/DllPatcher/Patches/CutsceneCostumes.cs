using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace DllPatcher;

partial class Program
{
    const string CutsceneCreatureEnterType = "NGame2.NCutScene.NScript.CreatureEnter";
    const string CutscenePlayerCostumeMethod = "SprkUsePlayerCostume";
    // Weapon prefix used by plain story entries (the hero's default unique
    // weapon); it is not a story-specific outfit.
    const string CutscenePlainWeaponPrefix = "Unique01";

    // Cutscene scripts from chapter 9 onwards set CreatureEnter.CreateFromMyHero
    // to false, so heroes appear in their default model instead of the player's
    // costume/accessories. Use the player's hero whenever the script asks for it
    // or the entry requests no story-specific look (empty Body/Hair prefix and
    // an empty or default unique weapon). Story outfits such as Trans,
    // Substory01 or event costumes keep the scripted model. Native fallbacks
    // remain: unowned heroes, NPCs/monsters and Legend costumes still use the
    // script's own creature code and prefixes.
    static void PatchCutsceneCostumes(ModuleDefinition module)
    {
        var type = CutsceneCreatureEnter(module);
        var predicate = type.Methods.SingleOrDefault(m => m.Name == CutscenePlayerCostumeMethod)
            ?? AddCutscenePlayerCostumePredicate(module, type);
        var getBuilder = CutsceneGetBuilder(module);
        var il = getBuilder.Body.GetILProcessor();
        var changed = 0;
        foreach (var load in getBuilder.Body.Instructions.Where(IsCreateFromMyHeroLoad).ToList())
        {
            il.Replace(load, il.Create(OpCodes.Call, predicate));
            changed++;
        }
        // Upgrade the earlier unconditional form (ldarg.0; pop; ldc.i4.1).
        foreach (var pop in getBuilder.Body.Instructions.Where(IsForcedCutsceneFlag).ToList())
        {
            il.Remove(pop.Next);
            il.Replace(pop, il.Create(OpCodes.Call, predicate));
            changed++;
        }
        VerifyCutsceneCostumes(module);
        Console.WriteLine(changed > 0
            ? "Patched cutscene heroes to use the player's costume unless a story outfit is scripted."
            : "Cutscene costume patch already applied.");
    }

    static MethodDefinition AddCutscenePlayerCostumePredicate(ModuleDefinition module, TypeDefinition type)
    {
        FieldDefinition Field(string name) => type.Fields.Single(f => f.Name == name);
        var empty = new MethodReference("IsNullOrEmpty", module.TypeSystem.Boolean, module.TypeSystem.String) { HasThis = false };
        empty.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        var equal = new MethodReference("op_Equality", module.TypeSystem.Boolean, module.TypeSystem.String) { HasThis = false };
        equal.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        equal.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));

        var method = new MethodDefinition(CutscenePlayerCostumeMethod,
            MethodAttributes.Private | MethodAttributes.HideBySig, module.TypeSystem.Boolean);
        method.Body = new MethodBody(method) { MaxStackSize = 2 };
        var il = method.Body.GetILProcessor();
        var yes = il.Create(OpCodes.Ldc_I4_1);
        var no = il.Create(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, Field("CreateFromMyHero")); il.Emit(OpCodes.Brtrue, yes);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, Field("BodyPrefix")); il.Emit(OpCodes.Call, empty); il.Emit(OpCodes.Brfalse, no);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, Field("HairPrefix")); il.Emit(OpCodes.Call, empty); il.Emit(OpCodes.Brfalse, no);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, Field("WeaponPrefix")); il.Emit(OpCodes.Call, empty); il.Emit(OpCodes.Brtrue, yes);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, Field("WeaponPrefix")); il.Emit(OpCodes.Ldstr, CutscenePlainWeaponPrefix);
        il.Emit(OpCodes.Call, equal); il.Emit(OpCodes.Ret);
        il.Append(yes); il.Emit(OpCodes.Ret);
        il.Append(no); il.Emit(OpCodes.Ret);
        type.Methods.Add(method);
        return method;
    }

    static TypeDefinition CutsceneCreatureEnter(ModuleDefinition module) =>
        module.GetType(CutsceneCreatureEnterType)
            ?? throw new InvalidOperationException("CreatureEnter cutscene script type not found.");

    static MethodDefinition CutsceneGetBuilder(ModuleDefinition module) =>
        CutsceneCreatureEnter(module).Methods.Single(m => m.Name == "GetBuilder" && !m.IsStatic && !m.HasParameters);

    static bool IsCreateFromMyHeroLoad(Instruction i) =>
        i.OpCode == OpCodes.Ldfld && i.Operand is FieldReference f &&
        f.DeclaringType.FullName == CutsceneCreatureEnterType && f.Name == "CreateFromMyHero";

    static bool IsForcedCutsceneFlag(Instruction i) =>
        i.OpCode == OpCodes.Pop && i.Previous?.OpCode == OpCodes.Ldarg_0 && i.Next?.OpCode == OpCodes.Ldc_I4_1;

    static void VerifyCutsceneCostumes(ModuleDefinition module)
    {
        var type = CutsceneCreatureEnter(module);
        var predicate = type.Methods.SingleOrDefault(m => m.Name == CutscenePlayerCostumeMethod && !m.IsStatic &&
                !m.HasParameters && m.ReturnType.FullName == "System.Boolean")
            ?? throw new InvalidOperationException("Cutscene player-costume predicate is missing.");
        var body = predicate.Body.Instructions;
        var fields = body.Where(i => i.OpCode == OpCodes.Ldfld).Select(i => ((FieldReference)i.Operand).Name).ToArray();
        if (!fields.SequenceEqual(new[] { "CreateFromMyHero", "BodyPrefix", "HairPrefix", "WeaponPrefix", "WeaponPrefix" }))
            throw new InvalidOperationException("Cutscene predicate must check the flag, then Body/Hair/Weapon story prefixes.");
        var calls = body.Select(i => i.Operand).OfType<MethodReference>().Select(m => m.Name).ToArray();
        if (!calls.SequenceEqual(new[] { "IsNullOrEmpty", "IsNullOrEmpty", "IsNullOrEmpty", "op_Equality" }) ||
            !body.Any(i => i.OpCode == OpCodes.Ldstr && Equals(i.Operand, CutscenePlainWeaponPrefix)))
            throw new InvalidOperationException("Cutscene predicate must keep scripted story outfits.");

        var instructions = CutsceneGetBuilder(module).Body.Instructions;
        if (instructions.Any(IsCreateFromMyHeroLoad) || instructions.Any(IsForcedCutsceneFlag))
            throw new InvalidOperationException("Cutscene GetBuilder must decide through the player-costume predicate.");
        var use = instructions.Where(i => i.OpCode == OpCodes.Call && i.Operand is MethodReference m &&
            m.Name == CutscenePlayerCostumeMethod).ToList();
        if (use.Count != 1 || use[0].Previous?.OpCode != OpCodes.Ldarg_0)
            throw new InvalidOperationException("Cutscene GetBuilder must call the player-costume predicate once.");
        // Native order and fallbacks must remain: legend-costume check, the
        // player's hero builder, then the script's own prefixed model.
        var names = instructions.Select(i => i.Operand).OfType<MethodReference>().Select(m => m.Name).ToList();
        var legend = names.IndexOf("IsWearLegendCostume");
        var mine = names.IndexOf("GetBuilderFromMyHero");
        if (legend < 0 || mine < 0 || legend > mine || names.IndexOf(CutscenePlayerCostumeMethod) > legend)
            throw new InvalidOperationException("Cutscene GetBuilder lost its native hero/legend-costume path.");
        foreach (var prefix in new[] { "BodyPrefix", "WeaponPrefix", "HairPrefix" })
            if (!instructions.Any(i => i.Operand is MemberReference m && (m.Name == prefix || m.Name == "set_" + prefix) &&
                    (i.OpCode == OpCodes.Stfld || i.OpCode == OpCodes.Callvirt || i.OpCode == OpCodes.Call)))
                throw new InvalidOperationException("Cutscene GetBuilder lost its fallback model prefix: " + prefix);
        Console.WriteLine("Verified cutscene heroes use the player's costume except scripted story outfits, with native fallbacks retained.");
    }

    static void VerifyCutsceneCostumesFile(string clientRoot, bool stageOnly = false)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var path = Path.Combine(managed, "Assembly-CSharp.dll" + (stageOnly ? ".cutscene-costumes-staged" : ""));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver });
        VerifyCutsceneCostumes(assembly.MainModule);
    }

    static void InstallCutsceneCostumes(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var path = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = path + ".cutscene-costumes-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(path,
                   new ReaderParameters { AssemblyResolver = resolver, InMemory = true }))
        {
            PatchCutsceneCostumes(assembly.MainModule);
            var type = CutsceneCreatureEnter(assembly.MainModule);
            var size = (type.Methods.Count, CutsceneGetBuilder(assembly.MainModule).Body.Instructions.Count);
            PatchCutsceneCostumes(assembly.MainModule);
            if (size != (type.Methods.Count, CutsceneGetBuilder(assembly.MainModule).Body.Instructions.Count))
                throw new InvalidOperationException("Cutscene costume patch is not idempotent.");
            assembly.Write(staged);
        }
        VerifyCutsceneCostumesFile(clientRoot, true);
        if (stageOnly)
        {
            Console.WriteLine("Staged cutscene costume patch: " + staged);
            return;
        }
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, clientRoot)))
            throw new InvalidOperationException("Cutscene costume patch staged. Close game clients before installation.");
        var backup = Path.Combine("target", "cutscene-costumes-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);
        File.Copy(path, Path.Combine(backup, "Assembly-CSharp.dll"));
        File.Move(staged, path, true);
        Console.WriteLine("Installed cutscene costume patch. Restart the client.");
    }
}
