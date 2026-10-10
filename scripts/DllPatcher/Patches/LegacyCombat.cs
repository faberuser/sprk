using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    // Verified against the ARM64 executable in the user's 4.86.6 APK:
    // GiveDamage 0x30e8a8c; GiveDamageNoTrigger 0x30eb33c.
    // The retained Season1 methods have the same calculation order. Switching
    // entry points alone is insufficient: newer runtimes moved these modifiers
    // into phases that Season1 never visits, or ahead of defense calculation.
    static readonly string[] LegacyFinalModifiers =
    {
        "CreateShieldR", "CreateShieldByDamageR", "ModAttackByBoss",
        "ModAttackByRace", "ModDamage", "ModDamageByState", "ReflectDamage"
    };

    static int CombatPhase(ModuleDefinition module, string name) => Convert.ToInt32(module.Types
        .Single(t => t.FullName == "NShared.DamagePipelinePosition").Fields.Single(f => f.Name == name).Constant);

    static int[] DamagePhaseCalls(MethodDefinition method)
    {
        var calls = new List<int>();
        foreach (var instruction in method.Body.Instructions.Where(i => i.Operand is MethodReference r && r.Name == "ModifyDamage"))
        {
            // this, attacker, phase, ref amount, ref flag, call
            var constant = instruction.Previous.Previous.Previous;
            calls.Add(ReadCombatConstant(constant));
        }
        return calls.ToArray();
    }

    static int ReadCombatConstant(Instruction instruction) => instruction.OpCode.Code switch
    {
        Code.Ldc_I4 => (int)instruction.Operand,
        Code.Ldc_I4_S => (sbyte)instruction.Operand,
        Code.Ldc_I4_0 => 0, Code.Ldc_I4_1 => 1, Code.Ldc_I4_2 => 2,
        Code.Ldc_I4_3 => 3, Code.Ldc_I4_4 => 4, Code.Ldc_I4_5 => 5,
        Code.Ldc_I4_6 => 6, Code.Ldc_I4_7 => 7, Code.Ldc_I4_8 => 8,
        _ => throw new InvalidOperationException("Unrecognized combat phase argument.")
    };

    static void ValidateLegacyDamage(ModuleDefinition module)
    {
        var stat = module.Types.Single(t => t.FullName == "NShared.StatController");
        var expected = new[] { "Beginning", "AfterDodge", "Final", "AfterFinal", "EndOfFinal" }
            .Select(n => CombatPhase(module, n)).ToArray();
        foreach (string name in new[] { "GiveDamage_Season1", "GiveDamageNoTrigger_Season1" })
        {
            var method = stat.Methods.Single(m => m.Name == name);
            if (!method.HasBody || !DamagePhaseCalls(method).SequenceEqual(expected))
                throw new InvalidOperationException($"Unrecognized legacy damage phases in {name}; refusing to patch.");
        }
        var damage = stat.Methods.Single(m => m.Name == "GiveDamage_Season1");
        int defense = damage.Body.Instructions.ToList().FindIndex(i => i.Operand is MethodReference r &&
            r.Name is "CalculateDamage_Season1" or "CalculateDamage" or "PunishmentDamage");
        int critical = damage.Body.Instructions.ToList().FindIndex(i => i.Operand is MethodReference r && r.Name == "MultStat");
        int final = damage.Body.Instructions.ToList().FindLastIndex(i => i.Operand is MethodReference r && r.Name == "ForEachDamageModifier");
        if (defense < 0 || critical <= defense || final <= critical)
            throw new InvalidOperationException("Unrecognized legacy defense/critical/modifier order; refusing to patch.");
    }

    static bool PatchLegacyCombat(ModuleDefinition module)
    {
        ValidateLegacyDamage(module);
        var stat = module.Types.Single(t => t.FullName == "NShared.StatController");
        int finalPhase = CombatPhase(module, "Final");
        bool changed = false;
        var legacyPhases = new[] { "Beginning", "AfterDodge", "Final", "AfterFinal", "EndOfFinal" }
            .Select(n => CombatPhase(module, n)).ToHashSet();
        foreach (var type in module.GetTypes())
        foreach (var getter in type.Methods.Where(m => m.Name == "NShared.IDamageModifier.GetPipelinePosition"))
        {
            if (LegacyFinalModifiers.Contains(type.Name) || type.FullName == "NShared.NOperationRuntime.PropagateDamage") continue;
            if (!getter.HasBody || getter.Body.Instructions.Count != 2 ||
                getter.Body.Instructions[1].OpCode != OpCodes.Ret ||
                !legacyPhases.Contains(ReadCombatConstant(getter.Body.Instructions[0])))
                throw new InvalidOperationException($"Unreviewed damage phase in {type.FullName}; refusing to patch.");
        }
        foreach (string name in LegacyFinalModifiers)
        {
            var type = module.Types.Single(t => t.FullName == "NShared.NOperationRuntime." + name);
            var method = type.Methods.Single(m => m.Name == "NShared.IDamageModifier.GetPipelinePosition");
            if (method.Body.Instructions.Count != 2 || method.Body.Instructions[1].OpCode != OpCodes.Ret)
                throw new InvalidOperationException($"Unrecognized {name} modifier position; refusing to patch.");
            int original = CombatPhase(module, name.StartsWith("CreateShield") ? "AfterDodge" :
                name.StartsWith("ModAttack") ? "DamageAmplifier" : "BeforeFinal");
            int current = ReadCombatConstant(method.Body.Instructions[0]);
            if (current == finalPhase) continue;
            if (current != original) throw new InvalidOperationException($"Unexpected {name} modifier position; refusing to patch.");
            method.Body.Instructions[0].OpCode = OpCodes.Ldc_I4;
            method.Body.Instructions[0].Operand = finalPhase;
            changed = true;
        }
        foreach (string name in new[] { "GiveDamage", "GiveDamageNoTrigger" })
        {
            var entry = stat.Methods.Single(m => m.Name == name);
            var legacy = stat.Methods.Single(m => m.Name == name + "_Season1");
            if (entry.ReturnType.FullName != legacy.ReturnType.FullName ||
                !entry.Parameters.Select(p => p.ParameterType.FullName).SequenceEqual(legacy.Parameters.Select(p => p.ParameterType.FullName)))
                throw new InvalidOperationException($"Incompatible legacy signature for {name}.");
            if (entry.Body.Instructions.Count == entry.Parameters.Count + 3 &&
                entry.Body.Instructions[^2].Operand == legacy) continue;
            if (!entry.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "ModifyDamage"))
                throw new InvalidOperationException($"Unrecognized modern {name}; refusing to patch.");
            entry.Body = new MethodBody(entry) { MaxStackSize = entry.Parameters.Count + 1 };
            var il = entry.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_0));
            foreach (var parameter in entry.Parameters) il.Append(il.Create(OpCodes.Ldarg, parameter));
            il.Append(il.Create(OpCodes.Call, legacy));
            il.Append(il.Create(OpCodes.Ret));
            changed = true;
        }
        if (changed) Console.WriteLine("Restored pre-doomsday damage order, separate bonuses, and shield/modifier phases.");
        return changed;
    }
}
