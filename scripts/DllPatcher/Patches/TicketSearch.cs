using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    // Search fields for selector tickets: unique weapons/treasures, artifacts,
    // soul weapons, hero tickets (both layouts) and costume tickets.
    enum TicketSearchHook { Start, End, GuardBool, GuardVoid, HandleMove }

    static readonly (string Type,string Method,int Params,string Helper,TicketSearchHook Hook)[] TicketSearchHooks = {
        ("NGame2.NUI.NWindow.HeroPortraitList","Open",2,"BeforeHeroGrid",TicketSearchHook.Start),
        ("NGame2.NUI.NWindow.HeroPortraitList","Open",2,"AfterHeroGrid",TicketSearchHook.End),
        ("NGame2.NUI.NWindow.HeroPortraitList","MoveGridItem",3,"MoveHeroGrid",TicketSearchHook.HandleMove),
        ("NGame2.NUI.NWindow.SelectFilter","InitFilter",5,"BeforeFilter",TicketSearchHook.Start),
        ("NGame2.NUI.NWindow.SelectFilter","InitFilter",5,"AfterFilter",TicketSearchHook.End),
        ("NGame2.NUI.NWindow.WeaponUniqueSelect","CheckAddEquipItem",1,"MatchEquip",TicketSearchHook.GuardBool),
        ("NGame2.NUI.NWindow.SoulStoneSelect","CheckAddSoulStoneItem",1,"MatchSoulStone",TicketSearchHook.GuardBool),
        ("NGame2.NUI.NWindow.InvenUseHeroSelect","AddHeroByFilter",1,"MatchHero",TicketSearchHook.GuardVoid),
        ("NGame2.NUI.NWindow.InvenUseHeroCostumeSelect","AddHeroByFilter",1,"MatchCostume",TicketSearchHook.GuardVoid),
        ("NGame2.NUI.NWindow.InvenUseAttrTypeHeroSelect","AddComponent",3,"MatchHeroInfo",TicketSearchHook.GuardVoid),
        ("NGame2.NUI.NWindow.InvenUseAttrTypeHeroSelect","Init",3,"BeforeAttr",TicketSearchHook.Start),
        ("NGame2.NUI.NWindow.InvenUseAttrTypeHeroSelect","Init",3,"AfterAttr",TicketSearchHook.End),
    };

    static MethodDefinition TicketSearchTarget(ModuleDefinition module,(string Type,string Method,int Params,string Helper,TicketSearchHook Hook) hook) =>
        module.GetType(hook.Type).Methods.Single(m=>m.Name==hook.Method && m.Parameters.Count==hook.Params && m.HasBody);

    static int TicketSearchCalls(MethodDefinition method,string helper) =>
        method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkTicketSearch" && m.Name==helper);

    static byte[] CompileTicketSearch(string managed,string helperPath) {
        var trees=new[]{"TicketSearch","TicketSearchHeroInfo","TicketSearchProbe"}.Select(name=> {
            using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name+".cs.txt")!;
            using var reader=new StreamReader(resource);return CSharpSyntaxTree.ParseText(reader.ReadToEnd());
        }).ToArray();
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>!Path.GetFileName(p).Equals(Path.GetFileName(helperPath),StringComparison.OrdinalIgnoreCase)).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkTicketSearch",trees,refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        return compiled.ToArray();
    }

    static void PatchTicketSearch(ModuleDefinition module,ModuleDefinition helper) {
        var api=helper.GetType("SprkTicketSearch");
        foreach(var hook in TicketSearchHooks) {
            var method=TicketSearchTarget(module,hook);
            if(TicketSearchCalls(method,hook.Helper)>0)continue;
            var call=module.ImportReference(api.Methods.Single(m=>m.Name==hook.Helper));
            var il=method.Body.GetILProcessor();
            var first=method.Body.Instructions[0];
            switch(hook.Hook) {
                case TicketSearchHook.Start:
                    il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
                    il.InsertBefore(first,il.Create(OpCodes.Call,call));
                    break;
                case TicketSearchHook.End:
                    foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                        // Branches that targeted the return now run the hook first.
                        ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
                        var invoke=il.Create(OpCodes.Call,call);il.InsertAfter(ret,invoke);il.InsertAfter(invoke,il.Create(OpCodes.Ret));
                    }
                    break;
                case TicketSearchHook.GuardBool:
                case TicketSearchHook.GuardVoid:
                case TicketSearchHook.HandleMove:
                    il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
                    il.InsertBefore(first,il.Create(OpCodes.Ldarg_1));
                    if(hook.Hook==TicketSearchHook.HandleMove) {
                        il.InsertBefore(first,il.Create(OpCodes.Ldarg_2));
                        il.InsertBefore(first,il.Create(OpCodes.Ldarg_3));
                    }
                    il.InsertBefore(first,il.Create(OpCodes.Call,call));
                    il.InsertBefore(first,il.Create(OpCodes.Brtrue,first));
                    if(hook.Hook==TicketSearchHook.GuardBool)il.InsertBefore(first,il.Create(OpCodes.Ldc_I4_0));
                    il.InsertBefore(first,il.Create(OpCodes.Ret));
                    break;
            }
        }
    }

    static void VerifyTicketSearch(ModuleDefinition module) {
        foreach(var hook in TicketSearchHooks) {
            var method=TicketSearchTarget(module,hook);
            int calls=TicketSearchCalls(method,hook.Helper);
            var body=method.Body.Instructions;
            switch(hook.Hook) {
                case TicketSearchHook.Start:
                    if(calls!=1 || body[1].Operand is not MethodReference {Name:var start} || start!=hook.Helper)
                        throw new InvalidOperationException("Ticket search must reset before native code: "+hook.Method);
                    break;
                case TicketSearchHook.End:
                    var rets=body.Where(i=>i.OpCode==OpCodes.Ret).ToArray();
                    if(calls!=rets.Length || rets.Any(r=>r.Previous?.Operand is not MethodReference {Name:var n} || n!=hook.Helper))
                        throw new InvalidOperationException("Ticket search must lay out on every return: "+hook.Method);
                    break;
                default:
                    if(hook.Hook==TicketSearchHook.HandleMove) {
                        if(calls!=1 || body[4].Operand is not MethodReference {Name:"MoveHeroGrid"} || body[5].OpCode!=OpCodes.Brtrue || body[6].OpCode!=OpCodes.Ret || body[5].Operand!=body[7])
                            throw new InvalidOperationException("Hero grid must preserve full-list bookmark indices while filtered.");
                        break;
                    }
                    if(calls!=1 || body[2].Operand is not MethodReference {Name:var guard} || guard!=hook.Helper || body[3].OpCode!=OpCodes.Brtrue)
                        throw new InvalidOperationException("Ticket search guard missing: "+hook.Type+"."+hook.Method);
                    var native=(Instruction)body[3].Operand;
                    bool returnsBool=hook.Hook==TicketSearchHook.GuardBool;
                    if(returnsBool && (body[4].OpCode!=OpCodes.Ldc_I4_0 || body[5].OpCode!=OpCodes.Ret || native!=body[6]))
                        throw new InvalidOperationException("Ticket search must only narrow native eligibility: "+hook.Method);
                    if(!returnsBool && (body[4].OpCode!=OpCodes.Ret || native!=body[5]))
                        throw new InvalidOperationException("Ticket search must only skip native entries: "+hook.Method);
                    break;
            }
            VerifyArenaResultStack(method);
        }
    }

    static void VerifyTicketSearchFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".ticket-search-staged":"")),new ReaderParameters{AssemblyResolver=resolver});
        VerifyTicketSearch(game.MainModule);
        // The artifact source filter shares SelectFilter.InitFilter.
        var init=game.MainModule.GetType("NGame2.NUI.NWindow.SelectFilter").Methods.Single(m=>m.Name=="InitFilter");
        if(init.Body.Instructions.Count(i=>i.Operand is MethodReference {Name:"Reset"} m && m.DeclaringType.Name=="SprkArtifactSourceFilter")>1)
            throw new InvalidOperationException("Artifact source reset duplicated.");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkTicketSearch.dll"+(staged?".staged":"")));
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"GetDataByCode","get_CreatureIndex","get_CostumeName","get_CodeName","EnableButton","ResetPosition","set_defaultText"})
            if(!calls.Any(m=>m.Name==name))throw new InvalidOperationException("Incomplete ticket search behavior: "+name);
        var strings=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<string>().ToHashSet();
        foreach(var name in new[]{"Common_Box_2","Common_Close","CreateHeroPortraitList","SetHeroList","SelectHero","_callback","_buyItemData"})
            if(!strings.Contains(name))throw new InvalidOperationException("Ticket search must reuse native element: "+name);
        foreach(var name in new[]{"SprkHeroClass","SprkHeroFilterRow","Search hero","_heroPortraitList"})
            if(!strings.Contains(name))throw new InvalidOperationException("Hero Info filter binding missing: "+name);
        Console.WriteLine("PASS: ticket search hooks, IL stack, native list eligibility and helper bindings.");
    }

    static void TestTicketSearch(string root) {
        // Idempotence on an in-memory copy of the installed assembly.
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"),new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        using var helper=AssemblyDefinition.ReadAssembly(new MemoryStream(File.ReadAllBytes(Path.Combine(managed,"SprkTicketSearch.dll"))));
        var before=TicketSearchHooks.Select(h=>string.Join("\n",TicketSearchTarget(game.MainModule,h).Body.Instructions)).ToArray();
        PatchTicketSearch(game.MainModule,helper.MainModule);
        var after=TicketSearchHooks.Select(h=>string.Join("\n",TicketSearchTarget(game.MainModule,h).Body.Instructions)).ToArray();
        if(!before.SequenceEqual(after))throw new InvalidOperationException("Ticket search patch is not idempotent.");
        Console.WriteLine("PASS: ticket search reapplication leaves every hooked method unchanged.");
    }

    static void InstallTicketSearch(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkTicketSearch.dll");
        var bytes=CompileTicketSearch(managed,helperPath);
        using(var helper=AssemblyDefinition.ReadAssembly(new MemoryStream(bytes))) {
            using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
            using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
            PatchTicketSearch(game.MainModule,helper.MainModule);
            VerifyTicketSearch(game.MainModule);
            game.Write(dll+".ticket-search-staged");
        }
        File.WriteAllBytes(helperPath+".staged",bytes);
        VerifyTicketSearchFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Ticket search staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-ticket-search-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(dll+".ticket-search-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyTicketSearchFile(root);
        TestTicketSearch(root);
    }
}
