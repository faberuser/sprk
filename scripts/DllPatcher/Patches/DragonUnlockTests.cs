using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DllPatcher;

partial class Program
{
    static void TestDragonUnlock(string dllPath)
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Portal.cs.txt")!;
        using var reader = new StreamReader(stream);
        var methods = CSharpSyntaxTree.ParseText(reader.ReadToEnd()).GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().Where(m => new[] { "IsOpenedDragonHard", "DragonHardRequirement",
                "CheckDragonHardEntry", "CheckDragonHardEntryByIndex" }.Contains(m.Identifier.Text));
        // Compile the production method against small table/progress providers.
        // No Unity singleton or live account is touched by this test.
        var source = "using System; using System.Collections.Generic; public static class Subject {" + string.Join("\n",methods) + "}" + """
            public enum RaidType { DragonNormal, DragonHard, DragonHardSingle }
            public class RaidData {
                public RaidType Type; public int Index, SingleRaidIndex, Level;
                public string Name, Step_Name; public int[] OpenCondition;
            }
            public class Container {
                public List<RaidData> Rows = new List<RaidData>();
                public IEnumerable<RaidData> GetAllData() { return Rows; }
                public RaidData GetData(int id, int level) { return Rows.Find(r => r.Index == id && r.Level == level); }
            }
            public class GameRoot {
                public static GameRoot Instance = new GameRoot();
                public Container RaidDataContainer = new Container();
            }
            namespace NGame2.NUI.NWindow {
                public static class RaidHelper {
                    public static Dictionary<int,int> Progress = new Dictionary<int,int>();
                    public static int GetRaidMaxLevel(int id) {
                        return Progress.TryGetValue(id, out var level) ? level : 7;
                    }
                }
            }
            namespace NShared.NLocalization { public enum Common { INFORM } }
            namespace NVespa.NGlobal {
                public static class LocalizationManager {
                    public static string GetStringDirect(string key) { return key; }
                    public static string GetString(NShared.NLocalization.Common key) { return "Notice"; }
                }
            }
            namespace NGame2.NUI.NWindow {
                public static class MessagePopup {
                    public static string Message;
                    public static void Open(string title, string message) { Message = message; }
                }
            }
            public static class Cases {
                public static int Run() {
                    int count = 0;
                    for (int dragon = 1; dragon <= 4; dragon++) {
                        GameRoot.Instance.RaidDataContainer.Rows.Add(new RaidData {
                            Index = dragon, SingleRaidIndex = 100 + dragon, Level = 7,
                            Name = "Dragon " + dragon, Step_Name = "Stage 1"
                        });
                        // Multiple normal levels arrive in arbitrary table order.
                        GameRoot.Instance.RaidDataContainer.Rows.Add(new RaidData {
                            Index = dragon, SingleRaidIndex = 100 + dragon, Level = 9,
                            Name = "Dragon " + dragon, Step_Name = "Stage 3"
                        });
                    }
                    for (int dragon = 1; dragon <= 4; dragon++)
                    foreach (var type in new[]{RaidType.DragonHard, RaidType.DragonHardSingle})
                    foreach (int shared in new[]{0,7,8,9})
                    foreach (int solo in new[]{0,7,8,9}) {
                        var progress = NGame2.NUI.NWindow.RaidHelper.Progress;
                        progress.Clear();
                        if(shared > 0) progress[dragon] = shared;
                        if(solo > 0) progress[100 + dragon] = solo;
                        // An unrelated dragon's progress must not unlock this one.
                        progress[dragon % 4 + 1] = 9;
                        var raid = new RaidData { Type = type, OpenCondition = new[]{dragon,8} };
                        bool expected = shared >= 8 || solo >= 8;
                        if(Subject.IsOpenedDragonHard(raid) != expected)
                            throw new Exception("Dragon unlock failed: " + dragon + "/" + shared + "/" + solo);
                        var message = Subject.DragonHardRequirement(raid);
                        if (expected ? message != null : message != "Clear Dragon " + dragon + " (Normal), Stage 1 once to unlock this Hard raid.")
                            throw new Exception("Wrong prerequisite message: " + message);
                        NGame2.NUI.NWindow.MessagePopup.Message = null;
                        if (Subject.CheckDragonHardEntry(raid) != expected)
                            throw new Exception("Wrong entry gate result");
                        if (NGame2.NUI.NWindow.MessagePopup.Message != message)
                            throw new Exception("Entry gate popup mismatch");
                        raid.Index = 10 + dragon; raid.Level = 1;
                        GameRoot.Instance.RaidDataContainer.Rows.Add(raid);
                        if (Subject.CheckDragonHardEntryByIndex(raid.Index, 1) != expected)
                            throw new Exception("Direct entry route bypassed requirement");
                        GameRoot.Instance.RaidDataContainer.Rows.Remove(raid);
                        count++;
                    }
                    foreach(var raid in new RaidData[]{null, new RaidData(),
                        new RaidData {Type=RaidType.DragonHard, OpenCondition=new[]{1}},
                        new RaidData {Type=RaidType.DragonHard, OpenCondition=new[]{0,8}},
                        new RaidData {Type=RaidType.DragonNormal, OpenCondition=new[]{1,8}}}) {
                        if(Subject.IsOpenedDragonHard(raid)) throw new Exception("Invalid raid unlocked");
                        count++;
                    }
                    foreach (var raid in new RaidData[]{null, new RaidData {Type=RaidType.DragonNormal}}) {
                        NGame2.NUI.NWindow.MessagePopup.Message = null;
                        if (!Subject.CheckDragonHardEntry(raid) || NGame2.NUI.NWindow.MessagePopup.Message != null)
                            throw new Exception("Unaffected route changed");
                        count++;
                    }
                    if (Subject.DragonHardRequirement(new RaidData {Type=RaidType.DragonHard}) == null)
                        throw new Exception("Missing prerequisite should have a readable fallback");
                    count++;
                    return count;
                }
            }
            """;
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("DragonUnlockRegression", new[] { CSharpSyntaxTree.ParseText(source) },
            references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output = new MemoryStream();
        var result = compilation.Emit(output);
        if (!result.Success) throw new Exception(string.Join("\n", result.Diagnostics));
        var assembly = System.Reflection.Assembly.Load(output.ToArray());
        var cases = assembly.GetType("Cases")!.GetMethod("Run")!.Invoke(null, null);
        Console.WriteLine($"PASS: {cases} production dragon unlock cases.");
        using var client = Mono.Cecil.AssemblyDefinition.ReadAssembly(dllPath);
        var hooks = client.MainModule.GetTypes().Where(t => t.Name != "RestoredPortal")
            .SelectMany(t => t.Methods).Where(m => m.HasBody)
            .SelectMany(m => m.Body.Instructions.Where(i => i.Operand is Mono.Cecil.MethodReference r
                && r.DeclaringType.Name == "RestoredPortal" && r.Name.StartsWith("CheckDragonHardEntry"))
                .Select(i => (Method:m, Call:i))).ToArray();
        var expectedHooks = new[] { "EnterRaidInternal", "OpenSingleRaidPartySetting", "OpenSingleRaidPartySetting",
            "OpenRaidPartySetting", "OnClickStartBattleButton", "RequestStartBattle" }.OrderBy(n => n).ToArray();
        if (!hooks.Select(h => h.Method.Name).OrderBy(n => n).SequenceEqual(expectedHooks))
            throw new Exception("Expected six dragon entry/start guards in the client");
        foreach (var hook in hooks) {
            if (hook.Call.Next.OpCode != Mono.Cecil.Cil.OpCodes.Brtrue
                || hook.Call.Next.Next.OpCode != Mono.Cecil.Cil.OpCodes.Ret)
                throw new Exception("Entry guard must return immediately when the prerequisite fails");
        }
        Console.WriteLine("PASS: six installed entry/start guards return before opening a locked raid or starting battle.");
    }
}
