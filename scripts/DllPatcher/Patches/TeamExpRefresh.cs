using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void VerifyTeamExpRefreshFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".team-exp-staged":"")));
        var apply=game.MainModule.GetType("NGame2.NAccount.AccountManager").Methods.Single(m=>m.Name=="ApplyEndCampaignResponse");
        foreach(var ret in apply.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret)) {
            if(ret.Previous?.Operand is not MethodReference {Name:"ApplyFinal",DeclaringType.Name:"SprkTeamExpRefresh"}
                || ret.Previous.Previous.OpCode!=OpCodes.Ldarg_1 || ret.Previous.Previous.Previous.OpCode!=OpCodes.Ldarg_0)
                throw new InvalidOperationException("Final team EXP must be applied after all native battle rewards.");
        }
        foreach(var name in new[]{"get_ExpResult","get_ExpResultsByGetHero","get_StaminaResult","ApplyHeroExp"})
            if(!apply.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Native battle reward application missing: "+name);
        var proxy=game.MainModule.GetType("NGame2.NUI.NManager.NBattle.EndCampaignProxy").Methods.Single(m=>m.Name=="OnResponse" && m.Parameters[1].ParameterType.FullName=="NShared.EndCampaign.Response");
        if(proxy.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.Name=="BattleExp" && m.DeclaringType.Name=="SprkTeamExpRefresh")!=1)
            throw new InvalidOperationException("Final team EXP is missing from battle level-up presentation.");
        VerifyArenaResultStack(apply);VerifyArenaResultStack(proxy);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkTeamExpRefresh.dll"+(staged?".staged":"")));
        var final=helper.MainModule.GetType("SprkTeamExpRefresh").Methods.Single(m=>m.Name=="ApplyFinal");
        if(!final.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="Apply" && m.Parameters[0].ParameterType.FullName=="NShared.ExpResultInfo"))
            throw new InvalidOperationException("Team EXP must use native UserManager.Apply and UI observers.");
        Console.WriteLine("PASS: final team EXP applies after native rewards and feeds battle level-up presentation.");
    }
    static void InstallTeamExpRefresh(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkTeamExpRefresh.dll");
        var trees=new[]{"TeamExpRefresh","TeamExpRefreshProbe"}.Select(name=>{
            using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name+".cs.txt")!;
            using var reader=new StreamReader(resource);return CSharpSyntaxTree.ParseText(reader.ReadToEnd());
        }).ToArray();
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkTeamExpRefresh",trees,refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var module=game.MainModule;
        var type=helper.MainModule.GetType("SprkTeamExpRefresh");
        var apply=module.GetType("NGame2.NAccount.AccountManager").Methods.Single(m=>m.Name=="ApplyEndCampaignResponse");
        if(!apply.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ApplyFinal" && m.DeclaringType.Name==type.Name)) {
            var il=apply.Body.GetILProcessor();
            foreach(var ret in apply.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
                var arg=il.Create(OpCodes.Ldarg_1);il.InsertAfter(ret,arg);
                var call=il.Create(OpCodes.Call,module.ImportReference(type.Methods.Single(m=>m.Name=="ApplyFinal")));il.InsertAfter(arg,call);
                il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        var proxy=module.GetType("NGame2.NUI.NManager.NBattle.EndCampaignProxy").Methods.Single(m=>m.Name=="OnResponse" && m.Parameters[1].ParameterType.FullName=="NShared.EndCampaign.Response");
        if(!proxy.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="BattleExp" && m.DeclaringType.Name==type.Name)) {
            var getter=proxy.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.Name=="get_ExpResult");
            getter.OpCode=OpCodes.Call;getter.Operand=module.ImportReference(type.Methods.Single(m=>m.Name=="BattleExp"));
        }
        game.Write(dll+".team-exp-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyTeamExpRefreshFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Team EXP fix staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-team-exp-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(dll+".team-exp-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyTeamExpRefreshFile(root);
    }
}
