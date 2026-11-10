using System.Reflection;
using System.Runtime.CompilerServices;

namespace DllPatcher;

partial class Program
{
    // Run the actual native override lifecycle and attack-speed multiplier on
    // snapshot fixtures. No Unity scene or replacement combat formula is used.
    static void TestValanceSkillSpeed(Assembly assembly, string managed)
    {
        object New(string name) => RuntimeHelpers.GetUninitializedObject(assembly.GetType("NShared." + name, true)!);
        var container = Activator.CreateInstance(assembly.GetType("NShared.SkillDataContainer", true)!)!;
        var table = Path.GetFullPath(Path.Combine(managed, "../Documents/Patch/StandaloneWindows/TableJit/SkillTable.jit"));
        CTCall(CTMember(container, "Packer"), "Deserialize", File.ReadAllBytes(table));
        var operation = CTCall(container, "GetOperationData", 11130, 1);
        if (CTMember(operation, "Type").ToString() != "OverrideStat"
            || !((string[])CTMember(operation, "Values")).SequenceEqual(new[] { "AttackSpeed", "Custom", "1000" }))
            throw new InvalidOperationException("Unexpected Valance S3 native operation; review the speed fix.");
        var statEnum = assembly.GetType("NShared.StatType", true)!;
        var attackSpeed = Enum.Parse(statEnum, "AttackSpeed");
        var speedKey = Convert.ToInt32(attackSpeed);
        int cases = 0;
        foreach (var sample in new[] {
            (11130, 1, "AttackSpeed", 1000L, false),
            (11130, 2, "AttackSpeed", 1000L, true),
            (11110, 1, "AttackSpeed", 1000L, true),
            (11130, 1, "AttackSpeed", 1500L, true),
            (11130, 1, "MaxMp", 1000L, true) })
        {
            var stat = New("StatController");
            foreach (string fieldName in new[] { "_statOverriderListHolder", "_statObserverListHolder",
                "_cachedAddedStatDictionaryHolder", "_cachedMultedStatDictionaryHolder", "_cachedFinalStatDictionaryHolder" })
            {
                var field = CombatField(stat.GetType(), fieldName);
                CombatSnapshot(stat, fieldName, Activator.CreateInstance(field.FieldType.GenericTypeArguments[0])!);
            }
            var creature = New("BattleCreature"); CombatSnapshot(creature, "_statControllerHolder", stat);
            var fired = New("FiredOperationInfo");
            CombatSnapshot(fired, "_operationDataHolder", CTCall(container, "GetOperationData", sample.Item1, sample.Item2));
            var info = New("OperationInfo"); CombatSnapshot(info, "_creatureHolder", creature); CombatSnapshot(info, "_firedInfoHolder", fired);
            var runtime = Activator.CreateInstance(assembly.GetType("NShared.NOperationRuntime.OverrideStat", true)!)!;
            CombatSnapshot(runtime, "_operationHolder", info);
            var targetStat = Enum.Parse(statEnum, sample.Item3);
            CombatSnapshot(runtime, "_statTypeHolder", targetStat); CombatSnapshot(runtime, "_statValueHolder", sample.Item4);
            var cache = CTCall(CombatField(stat.GetType(), "_cachedFinalStatDictionaryHolder").GetValue(stat)!, "Get");
            CTCall(cache, "Add", speedKey, 1500L);
            CTCall(runtime, "OnBegin");
            object?[] query = { targetStat, 0L };
            var found = (bool)stat.GetType().GetMethod("GetOverrideStat", CTFlags)!.Invoke(stat, query)!;
            if (found != sample.Item5 || (found && (long)query[1]! != sample.Item4))
                throw new InvalidOperationException("Incorrect override registration: " + sample);
            if (!found)
            {
                // The ordinary final-stat cache can change during S3. Test
                // baseline, haste and slows rather than snapshotting cast speed.
                foreach (long speed in new long[] { 500, 1000, 1500, 2000, 700 })
                {
                    CTCall(cache, "Remove", speedKey); CTCall(cache, "Add", speedKey, speed);
                    if ((int)CTCall(stat, "MultiplyAttackSpeed", 100, 1000) != speed / 10)
                        throw new InvalidOperationException("Valance S3 did not track current attack speed: " + speed);
                    cases++;
                }
            }
            CTCall(runtime, "OnEnd");
            query[1] = 0L;
            if ((bool)stat.GetType().GetMethod("GetOverrideStat", CTFlags)!.Invoke(stat, query)!)
                throw new InvalidOperationException("Override remained registered after its native end.");
            cases++;
        }
        Console.WriteLine("PASS: " + cases + " native Valance speed, dynamic haste/slow and override lifecycle checks.");
    }
}
