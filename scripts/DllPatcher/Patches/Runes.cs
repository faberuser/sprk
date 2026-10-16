using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static MethodDefinition RuneCategoryGenerator(ModuleDefinition module) => module
        .GetType("NGame2.NUI.NComponent2.HeroRuneListPanel").Methods
        .Single(m => m.Name == "GenerateCategoryButtonComponent");

    static bool IsRuneCategoryOpen(Instruction instruction) => instruction.Operand is MethodReference m
        && m.DeclaringType.FullName == "NGame2.NUI.NComponent2.RuneCategoryButtonComponent"
        && m.Name == "Open";

    static int RuneCategoryValue(Instruction instruction) => instruction.OpCode.Code switch {
        Code.Ldc_I4 => (int)instruction.Operand,
        Code.Ldc_I4_S => (sbyte)instruction.Operand,
        Code.Ldc_I4_0 => 0, Code.Ldc_I4_1 => 1, Code.Ldc_I4_2 => 2,
        Code.Ldc_I4_3 => 3, Code.Ldc_I4_4 => 4, Code.Ldc_I4_5 => 5,
        Code.Ldc_I4_6 => 6, Code.Ldc_I4_7 => 7, Code.Ldc_I4_8 => 8,
        _ => throw new InvalidOperationException("Unexpected rune category argument.")
    };

    static bool PatchRuneCategories(ModuleDefinition module)
    {
        var method = RuneCategoryGenerator(module);
        int EnumValue(string name) => (int)module.GetType("NShared.ItemType").Fields.Single(f => f.Name == name).Constant;
        int normal = EnumValue("Rune"), crafted = EnumValue("PunishmentRune");
        var opens = method.Body.Instructions.Where(IsRuneCategoryOpen).ToArray();
        if (opens.Length == 2 && RuneCategoryValue(opens[0].Previous) == normal
            && RuneCategoryValue(opens[1].Previous) == crafted) return false;
        if (opens.Length != 1 || RuneCategoryValue(opens[0].Previous) != normal)
            throw new InvalidOperationException("Unrecognized rune category generator; refusing to patch.");

        // Reuse the native prefab factory, listener and pooled list ownership.
        // Close() releases both buttons; the existing selector already handles
        // PunishmentRune storage, equipped-item exclusion and valid rune slots.
        var instructions = method.Body.Instructions;
        var start = instructions.Single(i => i.OpCode == OpCodes.Ldsfld
            && i.Operand is FieldReference f && f.Name == "PrefabAuto"
            && f.DeclaringType.FullName == "NGame2.NUI.NComponent2.RuneCategoryButtonComponent");
        var end = instructions.Single(i => i.Operand is MethodReference m && m.Name == "Add"
            && m.DeclaringType.FullName.Contains("RuneCategoryButtonComponent"));
        var block = instructions.Skip(instructions.IndexOf(start))
            .Take(instructions.IndexOf(end) - instructions.IndexOf(start) + 1).ToArray();
        if (!block.Contains(opens[0]) || block.Any(i => i.Operand is Instruction or Instruction[]))
            throw new InvalidOperationException("Unexpected control flow in rune category creation.");
        var il = method.Body.GetILProcessor();
        var cursor = end;
        foreach (var original in block)
        {
            var copy = Instruction.Create(OpCodes.Nop);
            copy.OpCode = original.OpCode;
            copy.Operand = original.Operand;
            if (original == opens[0].Previous) { copy.OpCode = OpCodes.Ldc_I4; copy.Operand = crafted; }
            il.InsertAfter(cursor, copy);
            cursor = copy;
        }
        // Cecil does not widen short branches automatically after insertion.
        foreach (var i in instructions)
            if (i.OpCode == OpCodes.Brfalse_S) i.OpCode = OpCodes.Brfalse;
            else if (i.OpCode == OpCodes.Brtrue_S) i.OpCode = OpCodes.Brtrue;
            else if (i.OpCode == OpCodes.Br_S) i.OpCode = OpCodes.Br;
        Console.WriteLine("Restored the native Lucern rune category alongside Normal Rune.");
        return true;
    }

    static string RuneCategoryFile(string clientRoot, bool staged) => Path.GetFullPath(Path.Combine(
        clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll" + (staged ? ".runes-staged" : "")));

    static bool PatchCachedRuneOptions(ModuleDefinition module)
    {
        var cached = module.GetType("NShared.CachedHeroInfo");
        var ctor = cached.Methods.Single(m => m.IsConstructor && m.Parameters.Count == 4
            && m.Parameters[0].ParameterType.FullName == "NShared.HeroInfo");
        var rolls = ctor.Parameters[3];
        if (ctor.Body.Instructions.Any(i => i.OpCode == OpCodes.Starg && i.Operand == rolls)) return false;
        var refresh = cached.Methods.Single(m => m.Name == "RefreshRuneInfo");
        var getRollsCall = refresh.Body.Instructions.Single(i => i.Operand is MethodReference m
            && m.Name == "GetPunishmentRuneOptionInfos");
        var getRolls = (MethodReference)getRollsCall.Operand;
        var toArrayCall = getRollsCall.Next;
        if (toArrayCall.Operand is not MethodReference toArray || toArray.Name != "ToArray")
            throw new InvalidOperationException("Unexpected cached rune option array conversion.");
        var source = new VariableDefinition(cached);
        ctor.Body.Variables.Add(source);
        ctor.Body.InitLocals = true;
        var first = ctor.Body.Instructions[0];
        var il = ctor.Body.GetILProcessor();
        // BattleSimulationInstance and other native copies pass null options.
        // Inherit live rolls when the input is already a CachedHeroInfo, while
        // explicit options supplied by login/server snapshots keep precedence.
        foreach (var i in new[] {
            Instruction.Create(OpCodes.Ldarg, rolls), Instruction.Create(OpCodes.Brtrue, first),
            Instruction.Create(OpCodes.Ldarg_1), Instruction.Create(OpCodes.Isinst, cached),
            Instruction.Create(OpCodes.Stloc, source), Instruction.Create(OpCodes.Ldloc, source),
            Instruction.Create(OpCodes.Brfalse, first), Instruction.Create(OpCodes.Ldloc, source),
            Instruction.Create(OpCodes.Callvirt, getRolls), Instruction.Create(toArrayCall.OpCode, toArray),
            Instruction.Create(OpCodes.Starg, rolls)
        }) il.InsertBefore(first, i);
        ctor.Body.MaxStackSize = Math.Max(ctor.Body.MaxStackSize, 2);
        Console.WriteLine("Preserved Lucern rune rolls when native battles copy cached heroes.");
        return true;
    }

    static void VerifyRuneCategories(string clientRoot, bool staged)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(RuneCategoryFile(clientRoot, staged));
        if (PatchRuneCategories(assembly.MainModule)) throw new Exception("Lucern rune category is missing.");
        if (PatchCachedRuneOptions(assembly.MainModule)) throw new Exception("Cached rune inheritance is missing.");
        Console.WriteLine("PASS: native Normal/Lucern categories and cached rune inheritance are present; patches are idempotent.");
    }

    static void InstallRuneCategories(string clientRoot, bool stageOnly)
    {
        var dll = RuneCategoryFile(clientRoot, false);
        var staged = RuneCategoryFile(clientRoot, true);
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchRuneCategories(assembly.MainModule);
            PatchCachedRuneOptions(assembly.MainModule);
            if (PatchRuneCategories(assembly.MainModule)) throw new Exception("Rune category patch is not idempotent.");
            if (PatchCachedRuneOptions(assembly.MainModule)) throw new Exception("Rune inheritance patch is not idempotent.");
            assembly.Write(staged);
        }
        VerifyRuneCategories(clientRoot, true);
        if (!stageOnly)
        {
            File.Copy(dll, dll + ".before-runes-" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? $"Staged: {staged}" : "Installed rune categories. Restart the game to apply them.");
    }
}
