using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    // Special Shop search tab, per-account bookmarks (Package > Bookmarks) and
    // Bookmarks as the default tab when the shop opens from the lobby (Hot & New hidden).
    enum ShopSearchHook { Start, End, EndArg1, ReturnFilter, DefaultCategory }

    static readonly (string Type,string Method,int Params,string Helper,ShopSearchHook Hook)[] ShopSearchHooks = {
        ("NGame2.NUI.NWindow.NPayShop.PayShop","Open",2,"BeforeOpen",ShopSearchHook.Start),
        ("NGame2.NUI.NWindow.NPayShop.PayShop","Open",2,"AfterOpen",ShopSearchHook.End),
        ("NGame2.NUI.NComponent2.NPayShop.PayShopCategoryComponent","Open",2,"DecorateCategory",ShopSearchHook.EndArg1),
        ("NGame2.NUI.NComponent2.NPayShop.PayShopProductList","GetRealDataList",1,"Products",ShopSearchHook.ReturnFilter),
        ("NGame2.NUI.NComponent2.NPayShop.PayShopProductList","Sort",2,"AfterSort",ShopSearchHook.EndArg1),
        ("NGame2.NUI.NComponent2.NPayShop.PayShopProductComponent","Init",3,"Decorate",ShopSearchHook.EndArg1),
        ("NGame2.NUI.NWindow.NPayShop.PayShopProductInfoPopup","Init",1,"DecoratePopup",ShopSearchHook.EndArg1),
        ("NGame2.NUI.NManager.PayShopManagement","OpenNewPayShop",0,"DefaultCategory",ShopSearchHook.DefaultCategory),
    };

    static MethodDefinition ShopSearchTarget(ModuleDefinition module,(string Type,string Method,int Params,string Helper,ShopSearchHook Hook) hook) =>
        module.GetType(hook.Type).Methods.Single(m=>m.Name==hook.Method && m.Parameters.Count==hook.Params && m.HasBody);

    static int ShopSearchCalls(MethodDefinition method,string helper) =>
        method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkShopSearch" && m.Name==helper);

    static byte[] CompileShopSearch(string managed,string helperPath) {
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ShopSearch.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>!Path.GetFileName(p).Equals(Path.GetFileName(helperPath),StringComparison.OrdinalIgnoreCase)).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkShopSearch",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        return compiled.ToArray();
    }

    static void PatchShopSearch(ModuleDefinition module,ModuleDefinition helper) {
        var api=helper.GetType("SprkShopSearch");
        foreach(var hook in ShopSearchHooks) {
            var method=ShopSearchTarget(module,hook);
            if(ShopSearchCalls(method,hook.Helper)>0)continue;
            var call=module.ImportReference(api.Methods.Single(m=>m.Name==hook.Helper));
            var il=method.Body.GetILProcessor();
            var first=method.Body.Instructions[0];
            switch(hook.Hook) {
                case ShopSearchHook.Start:
                    il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
                    il.InsertBefore(first,il.Create(OpCodes.Call,call));
                    break;
                case ShopSearchHook.DefaultCategory: {
                    var literal=method.Body.Instructions.Single(i=>i.OpCode==OpCodes.Ldstr && (string)i.Operand=="Hero");
                    il.InsertAfter(literal,il.Create(OpCodes.Call,call));
                    break;
                }
                default:
                    foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                        // The original ret becomes the first hook instruction so branches to it run the hook.
                        var tail=new List<Instruction>();
                        if(hook.Hook==ShopSearchHook.ReturnFilter){tail.Add(il.Create(OpCodes.Ldarg_1));}
                        else if(hook.Hook==ShopSearchHook.EndArg1){tail.Add(il.Create(OpCodes.Ldarg_1));}
                        tail.Add(il.Create(OpCodes.Call,call));
                        tail.Add(il.Create(OpCodes.Ret));
                        ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
                        var cursor=ret;
                        foreach(var instruction in tail){il.InsertAfter(cursor,instruction);cursor=instruction;}
                    }
                    break;
            }
        }
    }

    static void VerifyShopSearch(ModuleDefinition module) {
        foreach(var hook in ShopSearchHooks) {
            var method=ShopSearchTarget(module,hook);
            var body=method.Body.Instructions;
            int calls=ShopSearchCalls(method,hook.Helper);
            switch(hook.Hook) {
                case ShopSearchHook.Start:
                    if(calls!=1 || body[0].OpCode!=OpCodes.Ldarg_0 || body[1].Operand is not MethodReference {Name:var start} || start!=hook.Helper)
                        throw new InvalidOperationException("Shop search hook must run before native code: "+hook.Method);
                    break;
                case ShopSearchHook.DefaultCategory:
                    var literal=body.Where(i=>i.OpCode==OpCodes.Ldstr && (string)i.Operand=="Hero").ToArray();
                    if(calls!=1 || literal.Length!=1 || literal[0].Next.Operand is not MethodReference {Name:"DefaultCategory"})
                        throw new InvalidOperationException("Shop default category hook missing.");
                    break;
                default:
                    var rets=body.Where(i=>i.OpCode==OpCodes.Ret).ToArray();
                    if(calls!=rets.Length || rets.Any(r=>r.Previous?.Operand is not MethodReference {Name:var n} || n!=hook.Helper))
                        throw new InvalidOperationException("Shop search hook must run on every return: "+hook.Type+"."+hook.Method);
                    break;
            }
            VerifyArenaResultStack(method);
        }
    }

    static void TestShopSearch(byte[] bytes,string managed) {
        var context=new System.Runtime.Loader.AssemblyLoadContext("ShopSearch-"+Guid.NewGuid(),isCollectible:true);
        context.Resolving+=(load,name)=>{
            var path=Path.Combine(managed,name.Name+".dll");
            return File.Exists(path)?load.LoadFromStream(new MemoryStream(File.ReadAllBytes(path))):null;
        };
        try {
            var type=context.LoadFromStream(new MemoryStream(bytes)).GetType("SprkShopSearch")!;
            string[] Terms(string q)=>(string[])type.GetMethod("Terms")!.Invoke(null,new object[]{q})!;
            string Join(params string[] v)=>(string)type.GetMethod("Join")!.Invoke(null,new object[]{v})!;
            int Rank(string q,string[] tiers)=>(int)type.GetMethod("Rank")!.Invoke(null,new object[]{Terms(q),tiers})!;
            bool Fuzzy(string q,string[] tiers)=>(bool)type.GetMethod("Fuzzy")!.Invoke(null,new object[]{Terms(q),tiers})!;
            if(!Terms("  Unique-Weapon  ").SequenceEqual(new[]{"uniqueweapon"}) || Terms("").Length!=0)
                throw new InvalidOperationException("Shop search term folding regression.");
            var package=new[]{Join("Beginner Artifact Package"),Join("Artifact Selector","Ruby"),Join("Choose one artifact of your choice.")};
            if(Rank("beginner artifact",package)!=0 || Rank("RUBY",package)!=1 || Rank("choice",package)!=2
                || Rank("artifact choice",package)!=2 || Rank("weapon",package)!=-1 || Rank("",package)!=0)
                throw new InvalidOperationException("Shop search tier ranking regression.");
            if(!Fuzzy("artifcat",package) || Fuzzy("choise",package) || Fuzzy("zzz",package))
                throw new InvalidOperationException("Shop search typo fallback regression.");
            if((string)type.GetMethod("Serialize")!.Invoke(null,new object[]{new List<int>{3093996,7,-2}})!!="3093996,7,-2")
                throw new InvalidOperationException("Bookmark serialization regression.");
            if((string)type.GetMethod("DefaultCategory")!.Invoke(null,new object[]{"Hero"})!!="SprkBookmark")
                throw new InvalidOperationException("Shop must open on Bookmarks.");
            Console.WriteLine("PASS: shop search folding, name/contents/description tiers, typo fallback, bookmark storage and Bookmarks default tab.");
        } finally { context.Unload(); }
    }

    static void VerifyShopSearchFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".shop-search-staged":"")),new ReaderParameters{AssemblyResolver=resolver});
        VerifyShopSearch(game.MainModule);
        var helperPath=Path.Combine(managed,"SprkShopSearch.dll"+(staged?".staged":""));
        using var helper=AssemblyDefinition.ReadAssembly(helperPath);
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().Select(m=>m.Name).ToHashSet();
        foreach(var name in new[]{"JsonLoad_Set","JsonLoad_Get","GetRewardElementList","GetStringDirect","CreateList","set_Value","GetValues","FindInParents"})
            if(!calls.Contains(name))throw new InvalidOperationException("Incomplete shop search helper: "+name);
        var strings=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<string>().ToHashSet();
        foreach(var name in new[]{"SprkSearch","SprkBookmark","Package","SprkShopBookmarks","_currentCategory","Sprite_TagFrame","BaseList","BaseDic"})
            if(!strings.Contains(name))throw new InvalidOperationException("Shop search must bind native element: "+name);
        TestShopSearch(File.ReadAllBytes(helperPath),managed);
        Console.WriteLine("PASS: shop search hooks, IL stack and helper bindings.");
    }

    static void InstallShopSearch(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkShopSearch.dll");
        var bytes=CompileShopSearch(managed,helperPath);
        using(var helper=AssemblyDefinition.ReadAssembly(new MemoryStream(bytes))) {
            using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
            using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
            PatchShopSearch(game.MainModule,helper.MainModule);
            var before=ShopSearchHooks.Select(h=>string.Join("\n",ShopSearchTarget(game.MainModule,h).Body.Instructions)).ToArray();
            PatchShopSearch(game.MainModule,helper.MainModule);
            if(!before.SequenceEqual(ShopSearchHooks.Select(h=>string.Join("\n",ShopSearchTarget(game.MainModule,h).Body.Instructions))))
                throw new InvalidOperationException("Shop search patch is not idempotent.");
            VerifyShopSearch(game.MainModule);
            game.Write(dll+".shop-search-staged");
        }
        File.WriteAllBytes(helperPath+".staged",bytes);
        VerifyShopSearchFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Shop search staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-shop-search-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(dll+".shop-search-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyShopSearchFile(root);
    }
}
