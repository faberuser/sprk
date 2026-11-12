using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    const string DispatchSelectionHelper="SprkDispatchSelection";
    static void PatchDispatchSelection(ModuleDefinition module,ModuleDefinition helper) {
        MethodReference Hook(string name)=>module.ImportReference(helper.GetType(DispatchSelectionHelper).Methods.Single(m=>m.Name==name));
        bool Has(MethodDefinition m,string name)=>m.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name==DispatchSelectionHelper && r.Name==name);
        void Tail(MethodDefinition method,string hook,bool instance) {
            if(Has(method,hook))return;
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=instance?OpCodes.Ldarg_0:OpCodes.Nop;
                var call=il.Create(OpCodes.Call,Hook(hook));il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
            WidenShortBranches(method);
        }
        void Guard(MethodDefinition method,string hook,params Instruction[] args) {
            if(Has(method,hook))return;
            var il=method.Body.GetILProcessor();var first=method.Body.Instructions[0];
            foreach(var argument in args)il.InsertBefore(first,argument);
            il.InsertBefore(first,Instruction.Create(OpCodes.Call,Hook(hook)));
            il.InsertBefore(first,Instruction.Create(OpCodes.Brtrue,first));
            il.InsertBefore(first,Instruction.Create(OpCodes.Ret));
            WidenShortBranches(method);
        }
        var panel=module.GetType("NGame2.NUI.NWindow.UserHeroListPanel");
        Tail(panel.Methods.Single(m=>m.Name=="SetUserHeroList"),"RefreshList",true);
        var set=panel.Methods.Single(m=>m.Name=="SetDispatch");
        if(!Has(set,"FitList")) {
            // Native code enables this only for dispatchable PvE dungeons.
            // SPRK reserves dispatched heroes across all battle modes.
            var il=set.Body.GetILProcessor();var first=set.Body.Instructions[0];
            il.InsertBefore(first,Instruction.Create(OpCodes.Ldc_I4_1));
            il.InsertBefore(first,Instruction.Create(OpCodes.Starg,set.Parameters[0]));
            Tail(set,"FitList",true);
        }
        var portrait=module.GetType("NGame2.NUI.NComponent2.HeroPortraitComponent");
        Guard(portrait.Methods.Single(m=>m.Name=="OnClickPortrait"),"CanClick",Instruction.Create(OpCodes.Ldarg_0));
        foreach(var type in module.Types.Where(t=>t.Methods.Any(m=>m.Name=="RequestStartBattle"))) {
            var current=type;
            while(current!=null && current.FullName!="NGame2.NUI.NWindow.BattlePartySetting")current=current.BaseType?.Resolve();
            if(current==null)continue;
            foreach(var method in type.Methods.Where(m=>m.Name=="RequestStartBattle" && m.HasBody))
                Guard(method,"CanUseParty",Instruction.Create(OpCodes.Ldarg_0));
        }
        var party=module.GetType("NGame2.NUI.NWindow.BattlePartySetting");
        var list=party.Methods.Single(m=>m.Name=="GetHeroInfoList");
        foreach(var call in list.Body.Instructions.Where(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="DispatchHelper" && r.Name=="IsDispatchable").ToArray()) {
            // Keep the native dispatched/forced-Hero checks, including saved decks.
            call.OpCode=OpCodes.Pop;call.Operand=null;var il=list.Body.GetILProcessor();
            var pop=il.Create(OpCodes.Pop);il.InsertAfter(call,pop);il.InsertAfter(pop,il.Create(OpCodes.Ldc_I4_1));
        }
        WidenShortBranches(list);
        var duel=module.GetType("NGame2.NMatch.DuelManager").Methods.Single(m=>m.Name=="CompletePartySetting");
        Guard(duel,"CanUseHeroes",Instruction.Create(OpCodes.Ldarg_2));
        var match=module.GetType("NGame2.NMatch.MatchManager");
        var request=match.Methods.Single(m=>m.Name=="RequestMatchInternal_Pvp");
        var wrapper=match.NestedTypes.Single(t=>t.Name=="RequestWrapper_Pvp").Methods.Single(m=>m.Name=="get_Value");
        Guard(request,"CanRegister",Instruction.Create(OpCodes.Ldarg_1),Instruction.Create(OpCodes.Brfalse,request.Body.Instructions[0]),
            Instruction.Create(OpCodes.Ldarg_1),Instruction.Create(OpCodes.Callvirt,wrapper));
        var eligible=module.GetType("NGame2.NUI.NManager.MatchManagement").Methods.Single(m=>m.Name=="GetBanPickHeroIndices");
        if(!Has(eligible,"AvailableHeroes")) {
            var il=eligible.Body.GetILProcessor();
            foreach(var ret in eligible.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Call;ret.Operand=Hook("AvailableHeroes");il.InsertAfter(ret,il.Create(OpCodes.Ret));
            }
            WidenShortBranches(eligible);
        }
        var manager=module.GetType("UserDispatchBattleInfoManager");
        foreach(var m in manager.Methods.Where(m=>m.Name is "Set" or "OnAddData" or "Remove"))Tail(m,"RefreshOpenLists",false);
        VerifyDispatchSelection(module);
    }

    static void VerifyDispatchSelection(ModuleDefinition module) {
        foreach(var (type,name,hook) in new[] {
            ("NGame2.NUI.NWindow.UserHeroListPanel","SetUserHeroList","RefreshList"),
            ("NGame2.NUI.NWindow.UserHeroListPanel","SetDispatch","FitList"),
            ("NGame2.NUI.NComponent2.HeroPortraitComponent","OnClickPortrait","CanClick"),
            ("NGame2.NMatch.DuelManager","CompletePartySetting","CanUseHeroes"),
            ("NGame2.NMatch.MatchManager","RequestMatchInternal_Pvp","CanRegister"),
            ("NGame2.NUI.NManager.MatchManagement","GetBanPickHeroIndices","AvailableHeroes") }) {
            var m=module.GetType(type).Methods.Single(m=>m.Name==name);
            if(!m.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name==DispatchSelectionHelper && r.Name==hook))
                throw new InvalidOperationException("Missing dispatch selection hook: "+m.FullName);
            VerifyArenaResultStack(m);
        }
        var list=module.GetType("NGame2.NUI.NWindow.BattlePartySetting").Methods.Single(m=>m.Name=="GetHeroInfoList");
        if(list.Body.Instructions.Any(i=>i.Operand is MethodReference {Name:"IsDispatchable"})
            || !list.Body.Instructions.Any(i=>i.Operand is MethodReference {Name:"IsDispatchedHero"})
            || !list.Body.Instructions.Any(i=>i.Operand is MethodReference {Name:"IsForcedHero"}))
            throw new InvalidOperationException("Dispatch saved-deck filtering must retain forced-Hero exceptions.");
        VerifyArenaResultStack(list);
        foreach(var m in module.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody && m.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name==DispatchSelectionHelper)))
            VerifyArenaResultStack(m);
    }

    static void InstallDispatchSelection(string root,bool verifyOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,DispatchSelectionHelper+".dll");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        if(verifyOnly) {VerifyDispatchSelection(game.MainModule);Console.WriteLine("Verified native dispatch labels, click locks, saved decks, start guards, Honor eligibility and live roster refresh.");return;}
        var trees=new[]{"DispatchSelection","DispatchSelectionProbe"}.Select(name=>{
            using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name+".cs.txt")!;
            using var reader=new StreamReader(input);return CSharpSyntaxTree.ParseText(reader.ReadToEnd());
        });
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create(DispatchSelectionHelper,trees,refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        PatchDispatchSelection(game.MainModule,helper.MainModule);PatchDispatchSelection(game.MainModule,helper.MainModule);
        game.Write(dll+".dispatch-selection-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        using(var staged=AssemblyDefinition.ReadAssembly(dll+".dispatch-selection-staged",new ReaderParameters{AssemblyResolver=resolver}))VerifyDispatchSelection(staged.MainModule);
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Close game clients and native workers before installing dispatch selection changes.");
        var backup=Path.Combine("target","dispatch-selection-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));Directory.CreateDirectory(backup);
        File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));if(File.Exists(helperPath))File.Copy(helperPath,Path.Combine(backup,DispatchSelectionHelper+".dll"));
        game.Dispose();resolver.Dispose();File.Move(dll+".dispatch-selection-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        InstallDispatchSelection(root,true);
    }
}
