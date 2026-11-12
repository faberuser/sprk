using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;

namespace DllPatcher;

partial class Program
{
    static readonly BindingFlags CombatInstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    static FieldInfo CombatField(Type type, string name)
    {
        for (Type? current = type; current != null; current = current.BaseType)
            if (current.GetField(name, CombatInstanceFields | BindingFlags.DeclaredOnly) is { } field) return field;
        throw new MissingFieldException(type.FullName, name);
    }

    // Build only the snapshot data used by the actual native methods. No Unity
    // scene, account, GameRoot singleton, or alternative damage implementation.
    static void CombatSnapshot(object owner, string fieldName, object value)
    {
        var field = CombatField(owner.GetType(), fieldName);
        var holder = RuntimeHelpers.GetUninitializedObject(field.FieldType);
        CombatField(field.FieldType, "_data").SetValue(holder, value);
        field.SetValue(owner, holder);
    }

    static void TestLegacyCombat(Assembly assembly, ModuleDefinition module)
    {
        if (PatchLegacyCombat(module)) throw new Exception("Legacy combat was not already patched.");
        object New(string name) => RuntimeHelpers.GetUninitializedObject(assembly.GetType("NShared." + name, true)!);
        var phaseType = assembly.GetType("NShared.DamagePipelinePosition", true)!;
        var flagType = assembly.GetType("NShared.DamageFlag", true)!;
        object Flags(string name) => Enum.Parse(flagType, name);
        int cases = 0;
        foreach (var pair in LegacyFinalModifiers.Select(n => (n, "Final")).Concat(new[] {
            ("IgnoreDamageUnderHpR", "AfterFinal"), ("IgnoreDamageByMaxHpR", "AfterFinal"),
            ("EscapeDeath", "EndOfFinal"), ("UseSkillByDamageStackFixed", "EndOfFinal"),
            ("SetDamage", "Final") }))
        {
            var obj = New("NOperationRuntime." + pair.Item1);
            var method = obj.GetType().GetMethod("NShared.IDamageModifier.GetPipelinePosition", CombatInstanceFields)!;
            if (!Equals(method.Invoke(obj, null), Enum.Parse(phaseType, pair.Item2)))
                throw new Exception($"Wrong phase for {pair.Item1}.");
            cases++;
        }

        var stock = New("BattleCreatureStockData");
        var create = New("BattleCreatureCreateInfo");
        create.GetType().GetProperty("IsBoss")!.SetValue(create, true);
        stock.GetType().GetProperty("CreateInfo")!.SetValue(stock, create);
        var boss = New("BattleCreature");
        CombatField(boss.GetType(), "_stockData").SetValue(boss, stock);
        var operation = New("OperationInfo");
        CombatSnapshot(operation, "_stackCountHolder", 1);
        var modifier = New("NOperationRuntime.ModAttackByBoss");
        CombatSnapshot(modifier, "_operationHolder", operation);
        CombatSnapshot(modifier, "_isBossHolder", true);
        CombatSnapshot(modifier, "_addAttackRatioHolder", 1000);
        CombatSnapshot(modifier, "_addDamageRatioHolder", -200);
        var attack = modifier.GetType().GetMethod("NShared.IAttackModifier.Modify", CombatInstanceFields)!;
        var damage = modifier.GetType().GetMethod("NShared.IDamageModifier.Modify", CombatInstanceFields)!;
        object[] attackArgs = { boss, null!, 1_000_000L, Flags("Physical") };
        attack.Invoke(modifier, attackArgs);
        attack.Invoke(modifier, attackArgs);
        if ((long)attackArgs[2] != 4_000_000L) throw new Exception("Separate boss bonuses were not multiplied.");
        cases++;
        foreach (var sample in new[] {
            ("Physical", 800_000L), ("Physical,DerivedDamage", 1_000_000L),
            ("Physical,IgnoreModAttackByBoss", 1_000_000L) })
        {
            object[] args = { boss, 1_000_000L, Flags(sample.Item1) };
            damage.Invoke(modifier, args);
            if ((long)args[1] != sample.Item2) throw new Exception("Boss reduction/flag behavior changed.");
            cases++;
        }

        var calculate = assembly.GetType("NShared.StatController", true)!.GetMethod("CalculateDamage")!;
        var shield = New("HpShield");
        CombatSnapshot(shield, "_shieldHpHolder", 200_000L);
        CombatSnapshot(shield, "_shieldMaxHpHolder", 200_000L);
        CombatSnapshot(shield, "_shieldCountHolder", 0);
        CombatSnapshot(shield, "_damageFlagHolder", Flags("Physical,Magical"));
        long afterDefense = (long)calculate.Invoke(null, new object[] { 1_000_000L, 100_000L, 100, 100 })!;
        object[] shieldArgs = { null!, afterDefense, Flags("Physical") };
        shield.GetType().GetMethod("Modify")!.Invoke(shield, shieldArgs);
        if (afterDefense != 178_000L || (long)shieldArgs[1] != 0 ||
            (long)shield.GetType().GetProperty("ShieldHp")!.GetValue(shield)! != 22_000L)
            throw new Exception("Post-defense shield absorption failed.");
        cases++;

        var stat = New("StatController");
        CombatSnapshot(stat, "_overridenMaxHpHolder", 1_000_000_000L);
        CombatSnapshot(boss, "_statControllerHolder", stat);
        CombatSnapshot(operation, "_creatureHolder", boss);
        var cap = New("NOperationRuntime.IgnoreDamageUnderHpR");
        CombatSnapshot(cap, "_operationHolder", operation);
        var capMethod = cap.GetType().GetMethod("NShared.IDamageModifier.Modify", CombatInstanceFields)!;
        foreach (var sample in new[] {
            (660, 1_000_000_000L, 400_000_000L, 340_000_000L),
            (660, 660_000_000L, 100_000_000L, 0L),
            (330, 660_000_000L, 400_000_000L, 330_000_000L),
            (330, 330_000_000L, 100_000_000L, 0L),
            // Normal/Hard dragon, Otherworldly Shakmeh, and Apocalypsion gates.
            (499, 1_000_000_000L, 800_000_000L, 501_000_000L),
            (499, 499_000_000L, 100_000_000L, 0L),
            (699, 1_000_000_000L, 800_000_000L, 301_000_000L),
            (699, 699_000_000L, 100_000_000L, 0L),
            (650, 1_000_000_000L, 800_000_000L, 350_000_000L),
            (650, 650_000_000L, 100_000_000L, 0L),
            (320, 650_000_000L, 800_000_000L, 330_000_000L),
            (320, 320_000_000L, 100_000_000L, 0L),
            (5, 1_000_000_000L, 1_000_000_000L, 995_000_000L),
            (5, 5_000_000L, 100_000_000L, 0L) })
        {
            CombatSnapshot(stat, "_hpHolder", sample.Item2);
            CombatSnapshot(cap, "_limitRatioHolder", sample.Item1);
            object[] args = { null!, sample.Item3, Flags("Physical") };
            capMethod.Invoke(cap, args);
            if ((long)args[1] != sample.Item4) throw new Exception($"Boss HP gate {sample.Item1}/1000 failed.");
            cases++;
        }
        // Msama's totem phase sets each incoming hit to 1. Exercise the real
        // operation value reader and stack consumption, without scene events.
        var fixedDamage = New("NOperationRuntime.SetDamage");
        var data = New("OperationData");
        var load = RuntimeHelpers.GetUninitializedObject(assembly.GetType("NShared.OperationData+LoadData", true)!);
        load.GetType().GetProperty("Value")!.SetValue(load, new[] { "9999", "1", "None", "None" });
        CombatField(data.GetType(), "_load").SetValue(data, load);
        var fired = New("FiredOperationInfo");
        CombatSnapshot(fired, "_operationDataHolder", data);
        CombatSnapshot(fired, "_firedCreatureHolder", null!);
        CombatSnapshot(fired, "_skillLevelHolder", 1);
        CombatSnapshot(operation, "_firedInfoHolder", fired);
        CombatSnapshot(operation, "_valuesHolder", null!);
        CombatSnapshot(operation, "_stackCountHolder", 2);
        CombatSnapshot(fixedDamage, "_operationHolder", operation);
        CombatSnapshot(fixedDamage, "_stateDataHolder", null!);
        CombatSnapshot(boss, "_stateControllerHolder", New("StateController"));
        var fixedMethod = fixedDamage.GetType().GetMethod("NShared.IDamageModifier.Modify", CombatInstanceFields)!;
        foreach (var sample in new[] { (1, 1L, true), (0, 1L, true), (0, 1_000_000L, false) })
        {
            object[] args = { null!, 1_000_000L, Flags("Physical") };
            bool changed = (bool)fixedMethod.Invoke(fixedDamage, args)!;
            if ((long)args[1] != sample.Item2 || changed != sample.Item3 ||
                (int)operation.GetType().GetProperty("StackCount")!.GetValue(operation)! != sample.Item1 ||
                !Equals(args[2], Flags(sample.Item3 ? "Physical,Modified" : "Physical")))
                throw new Exception("Trial fixed-damage stack consumption failed.");
            cases++;
        }
        Console.WriteLine($"PASS: {cases} native legacy checks: modifier phases, separate boss bonuses, shields, and boss HP gates.");
    }
}
