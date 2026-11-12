using System.Reflection;
using Mono.Cecil;

namespace DllPatcher;
partial class Program
{
    static void TestBatchGrind(AssemblyDefinition game, byte[] helperBytes, string managed)
    {
        ResolveEventHandler resolve = (_, e) => {
            var path = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        AppDomain.CurrentDomain.AssemblyResolve += resolve;
        try
        {
            using var bytes = new MemoryStream(); game.Write(bytes);
            var loaded = Assembly.Load(bytes.ToArray());
            var listenerType = loaded.GetType("NGame2.NUI.NWindow.NInventory.ItemListListener")!;
            var ctor = listenerType.GetConstructors().Single();
            var listener = ctor.Invoke(new[] { Enum.ToObject(ctor.GetParameters()[0].ParameterType, 0), (object)0 });
            var mode = listenerType.GetField("functionType")!;
            var filtering = listenerType.GetProperty("isFiltering")!;
            var guard = listenerType.GetMethod(BatchGrindGuard, BindingFlags.Instance | BindingFlags.NonPublic)!;
            foreach (var name in Enum.GetNames(mode.FieldType))
                foreach (var isFiltering in new[] { false, true })
                {
                    mode.SetValue(listener, Enum.Parse(mode.FieldType, name));
                    filtering.SetValue(listener, isFiltering);
                    if ((bool)guard.Invoke(listener, null)! != (name == "Break" || isFiltering))
                        throw new InvalidOperationException("Incorrect grind selection warning policy: " + name);
                }

            var helper = Assembly.Load(helperBytes).GetType("SprkBatchGrind")!;
            var add = helper.GetMethod("Add")!;
            var format = helper.GetMethod("Format")!;
            var rewards = new SortedDictionary<string, long[]>();
            void Add(string code, int min, int max, int bonus1 = 0, int rate1 = 0, int bonus2 = 0, int rate2 = 0) =>
                add.Invoke(null, new object[] { rewards, code, min, max, bonus1, rate1, bonus2, rate2 });
            // Mixed, awakened uniques aggregate by reward; chance bonuses are
            // ranges, guaranteed bonuses contribute to both bounds.
            Add("Unique Treasure Fragments", 200, 200);
            Add("Unique Treasure Fragments", 600, 600);
            Add("Unique Weapon Fragments", 250, 300, 50, 1000, 100, 500);
            Add("", 999, 999);
            if (rewards.Count != 2 || !rewards["Unique Treasure Fragments"].SequenceEqual(new long[] { 800, 800 })
                || !rewards["Unique Weapon Fragments"].SequenceEqual(new long[] { 300, 450 }))
                throw new InvalidOperationException("Incorrect combined grind reward preview.");
            var message = (string)format.Invoke(null, new object[] { "native question", 4, 1, 2, rewards, false })!;
            foreach (var text in new[] { "Grind 4 selected items?", "Unique Weapons: 1", "Unique Treasures: 2", "Other gear: 1",
                "Unique Treasure Fragments: 800", "Unique Weapon Fragments: 300 - 450", "permanently destroyed", "cannot be undone" })
                if (!message.Contains(text)) throw new InvalidOperationException("Missing batch grind confirmation information: " + text);
            if ((string)format.Invoke(null, new object[] { "native question", 4, 0, 0, rewards, false })! != "native question")
                throw new InvalidOperationException("Ordinary gear confirmation changed.");
            Console.WriteLine("PASS: executed native selection policy across all modes/filter states; mixed unique counts, fragment totals/ranges and irreversible warning verified.");
        }
        finally { AppDomain.CurrentDomain.AssemblyResolve -= resolve; }
    }
}
