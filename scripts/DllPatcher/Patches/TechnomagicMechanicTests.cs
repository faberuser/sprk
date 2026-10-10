using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DllPatcher;

partial class Program
{
    static void TestTechnomagicMechanics(Assembly assembly)
    {
        object New(string name) => RuntimeHelpers.GetUninitializedObject(assembly.GetType("NShared." + name, true)!);
        int cases = 0;
        var operation = New("OperationInfo");
        foreach (var sample in new[] {
            ("ModDefensePower", 900, 1, 900), // Ascalon's shield phase
            ("ModDefensePower", 200, 1, 200), // normal Zeta's second phase
            ("ModRecoverPower", 700, 1, 700), // Galgoria's healing bonus
            ("ModRecoverPower", -5, 120, -600),
            ("ModRecoverPower", -10, 120, -1200),
            ("ModToughness", 0, 100, 0), // hit counter state has no toughness value
            ("ModShieldPowerR", -600, 1, -600),
            ("ModAttackPower", 700, 1, 700) })
        {
            CombatSnapshot(operation, "_stackCountHolder", sample.Item3);
            var modifier = New("NOperationRuntime." + sample.Item1);
            CombatSnapshot(modifier, "_operationHolder", operation);
            CombatSnapshot(modifier, "_statHolder", sample.Item2);
            int actual = (int)modifier.GetType().GetMethod("GetAddedStat")!.Invoke(modifier, null)!;
            if (actual != sample.Item4) throw new Exception($"Technomagic stacked modifier failed: {sample.Item1}.");
            cases++;
        }

        var stat = New("StatController");
        CombatSnapshot(stat, "_overridenMaxHpHolder", 1_000_000_000L);
        foreach (var sample in new[] { (4, 4_000_000L), (110, 110_000_000L), (1000, 1_000_000_000L) })
        {
            long actual = (long)stat.GetType().GetMethod("GetHpAmountByMaxHp")!.Invoke(stat, new object[] { sample.Item1 })!;
            if (actual != sample.Item2) throw new Exception("Technomagic HP ratio failed.");
            cases++;
        }

        // Exercise the real StateController and StateCount condition. Only the
        // state modifier is a test double; no alternative counter/comparator.
        var creature = New("BattleCreature");
        var controller = New("StateController");
        CombatSnapshot(creature, "_stateControllerHolder", controller);
        var interfaceType = assembly.GetType("NShared.IStateModifier", true)!;
        var proxy = DispatchProxy.Create(interfaceType, typeof(CombatTestStateModifier));
        var proxyData = (CombatTestStateModifier)proxy;
        proxyData.State = New("StateData");
        proxyData.State.GetType().GetProperty("StateType")!.SetValue(proxyData.State, "PredatorState1");
        proxyData.State.GetType().GetProperty("Group")!.SetValue(proxyData.State, "PredatorCounters");
        var listType = assembly.GetType("NShared.SnapshotList`1", true)!.MakeGenericType(interfaceType);
        var snapshotList = RuntimeHelpers.GetUninitializedObject(listType);
        var items = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(interfaceType))!;
        items.Add(proxy);
        CombatField(listType, "_list").SetValue(snapshotList, items);
        CombatSnapshot(controller, "_stateModifierListHolder", snapshotList);
        var condition = New("NOperationCondition.StateCount");
        var meet = condition.GetType().GetMethod("NShared.IOperationCondition.Meet", CombatInstanceFields)!;
        foreach (int remaining in new[] { 120, 100, 80, 60, 1, 0 })
        {
            proxyData.Stack = remaining;
            int actual = (int)controller.GetType().GetMethod("GetStateStackCount")!
                .Invoke(controller, new object?[] { "PredatorState1", null })!;
            if (actual != remaining) throw new Exception("Enchantment hit count failed.");
            foreach (var sample in new[] {
                (new[] { "PredatorState1", "0", "LessOrEqual" }, remaining <= 0),
                (new[] { "PredatorCounters", "80", "MoreOrEqual" }, remaining >= 80) })
            {
                bool result = (bool)meet.Invoke(condition, new object?[] { null, creature, sample.Item1 })!;
                if (result != sample.Item2) throw new Exception("Enchantment state threshold failed.");
                cases++;
            }
        }
        Console.WriteLine($"PASS: {cases} native Technomagic checks: stacked stats, HP ratios, and Enchantment counter thresholds.");
    }
}

public class CombatTestStateModifier : DispatchProxy
{
    public object State = null!;
    public int Stack;
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod?.Name switch
    {
        "GetStateData" => State,
        "GetStackCount" => Stack,
        "GetFiredCreature" => null,
        _ => throw new NotSupportedException(targetMethod?.Name)
    };
}
