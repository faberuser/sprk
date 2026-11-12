using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static string MissionResource(string name) {
        using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name)!;
        using var reader=new StreamReader(input);return reader.ReadToEnd();
    }
    static void TestMissionHistory() {
        var compilation=CSharpCompilation.Create("MissionHistoryTests-"+Guid.NewGuid().ToString("N"),
            new[]{CSharpSyntaxTree.ParseText(MissionResource("MissionHistoryState.cs.txt")),CSharpSyntaxTree.ParseText(MissionResource("MissionHistoryTests.cs.txt"))},
            ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        System.Reflection.Assembly.Load(output.ToArray()).GetType("MissionHistoryTests")!.GetMethod("Run")!.Invoke(null,null);
        Console.WriteLine("PASS: mission history cross-device merge, new categories, failed replies, concurrent marks, account/server/session isolation and legacy import.");
    }
    static void PatchMissionHistory(ModuleDefinition module, ModuleDefinition helper) {
        MethodReference Hook(string name)=>module.ImportReference(helper.GetType("SprkMissionHistory").Methods.Single(m=>m.Name==name));
        var history=module.GetType("NGame2.NUI.NManager.Mission.PlayerPrefsOpenHistoryData");
        foreach(var (methodName,hook) in new[]{("IsOpened_NewMark","Read"),("SetOpen_NewMark","Mark")}) {
            var method=history.Methods.Single(m=>m.Name==methodName);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name=="SprkMissionHistory"))continue;
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                // Read retains the native bool already on the evaluation stack.
                ret.OpCode=OpCodes.Ldarg_1;ret.Operand=null;
                var sub=il.Create(OpCodes.Ldarg_2);il.InsertAfter(ret,sub);
                var call=il.Create(OpCodes.Call,Hook(hook));il.InsertAfter(sub,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        var tick=module.Types.Single(t=>t.Name=="GlobalUpdateBehaviour").Methods.Single(m=>m.Name=="Update");
        if(!tick.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name=="SprkMissionHistory"))
            tick.Body.GetILProcessor().InsertBefore(tick.Body.Instructions[0],Instruction.Create(OpCodes.Call,Hook("Tick")));
    }
    static void VerifyMissionHistory(ModuleDefinition module) {
        foreach(var (type,name,hook) in new[]{
            ("NGame2.NUI.NManager.Mission.PlayerPrefsOpenHistoryData","IsOpened_NewMark","Read"),
            ("NGame2.NUI.NManager.Mission.PlayerPrefsOpenHistoryData","SetOpen_NewMark","Mark"),
            ("GlobalUpdateBehaviour","Update","Tick")}) {
            var method=(type=="GlobalUpdateBehaviour"?module.Types.Single(t=>t.Name==type):module.GetType(type)).Methods.Single(m=>m.Name==name);
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m&&m.DeclaringType.Name=="SprkMissionHistory"&&m.Name==hook)!=1)
                throw new InvalidOperationException("Missing/duplicate mission history hook: "+hook);
        }
        Console.WriteLine("Verified native mission marker read/write hooks and account sync scheduling.");
    }
    static void VerifyMissionHistoryFile(string root,bool staged=false) {
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".mission-history-staged":"")));
        VerifyMissionHistory(game.MainModule);
    }
    static void InstallMissionHistory(string root,bool stageOnly) {
        root=Path.GetFullPath(root);var managed=Path.Combine(root,"King's Raid_Data","Managed");
        var helperPath=Path.Combine(managed,"SprkMissionHistory.dll");var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var compilation=CSharpCompilation.Create("SprkMissionHistory",new[]{"MissionHistory.cs.txt","MissionHistoryState.cs.txt"}.Select(n=>CSharpSyntaxTree.ParseText(MissionResource(n))),
            Directory.GetFiles(managed,"*.dll").Where(p=>Path.GetFileName(p)!="SprkMissionHistory.dll").Select(p=>MetadataReference.CreateFromFile(p)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        output.Position=0;using var helper=AssemblyDefinition.ReadAssembly(output);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver});
        PatchMissionHistory(game.MainModule,helper.MainModule);
        int count=game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
        PatchMissionHistory(game.MainModule,helper.MainModule);
        if(count!=game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count))throw new InvalidOperationException("Mission history patch is not idempotent");
        VerifyMissionHistory(game.MainModule);TestMissionHistory();
        var staged=dll+".mission-history-staged";game.Write(staged);File.WriteAllBytes(helperPath+".staged",output.ToArray());
        VerifyMissionHistoryFile(root,true);
        if(stageOnly)return;
        foreach(var process in System.Diagnostics.Process.GetProcesses())using(process)
            if(UsesClientFiles(process,root))throw new InvalidOperationException("Mission history patch staged. Close clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-mission-history-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        game.Dispose();File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyMissionHistoryFile(root);
    }
}
