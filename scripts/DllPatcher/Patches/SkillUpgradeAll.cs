using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace DllPatcher;
partial class Program {
    const string SkillAllHelper = "SprkSkillUpgradeAll";
    const string SkillListPanel = "NGame2.NUI.NComponent2.HeroSkillListPanel";

    static bool CallsSkillAll(MethodDefinition method,string name) =>
        method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==SkillAllHelper && m.Name==name);

    static MethodDefinition SkillItemObserver(TypeDefinition panel) => panel.Methods.Single(m=>
        m.Name.Contains("IUserObserver<") && m.Name.EndsWith(".OnChanged") && m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="NShared.ItemInfo");

    static MethodDefinition SkillResponseParse(ModuleDefinition module) => module.GetType("JM_NShared_ExtendHeroSkill_Response").Methods.Single(m=>
        m.Name=="Parse" && m.Parameters.Count==2 && m.Parameters[0].ParameterType.FullName=="System.Collections.IDictionary");

    static IEnumerable<(MethodDefinition method,string hook)> SkillAllExitHooks(ModuleDefinition module) {
        var panel=module.GetType(SkillListPanel);
        yield return (panel.Methods.Single(m=>m.Name=="Open"),"Attach");
        yield return (panel.Methods.Single(m=>m.Name=="UpdateSkillExtend"),"Changed");
        yield return (SkillItemObserver(panel),"Changed");
    }

    static void VerifySkillUpgradeAllFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".skill-upgrade-all-staged":"")),new ReaderParameters{AssemblyResolver=resolver});
        var module=game.MainModule;
        foreach(var (method,hook) in SkillAllExitHooks(module)) {
            var rets=method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray();
            if(rets.Length==0 || rets.Any(r=>r.Previous?.Operand is not MethodReference m || m.Name!=hook || m.DeclaringType.Name!=SkillAllHelper))
                throw new InvalidOperationException("Skill upgrade-all hook must run at every exit: "+method.Name);
            VerifyArenaResultStack(method);
        }
        var reset=module.GetType(SkillListPanel).Methods.Single(m=>m.Name=="Reset");
        if(reset.Body.Instructions[1].Operand is not MethodReference{Name:"Release"} release || release.DeclaringType.Name!=SkillAllHelper ||
           reset.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name==SkillAllHelper)!=1)
            throw new InvalidOperationException("Skill book counter restore hook missing or duplicated.");
        VerifyArenaResultStack(reset);
        var encode=module.GetType("JM_NShared_ExtendHeroSkill_Request").Methods.Single(m=>m.Name=="EncodeJsonObject");
        if(encode.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).Any(r=>r.Previous?.Operand is not MethodReference{Name:"Encode"} m || m.DeclaringType.Name!=SkillAllHelper))
            throw new InvalidOperationException("Skill extend request encoder hook missing.");
        var parse=SkillResponseParse(module);
        if(parse.Body.Instructions[2].Operand is not MethodReference{Name:"Capture"} capture || capture.DeclaringType.Name!=SkillAllHelper ||
           parse.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name==SkillAllHelper)!=1)
            throw new InvalidOperationException("Skill extend response capture hook missing or duplicated.");
        foreach(var method in new[]{encode,parse})VerifyArenaResultStack(method);
        var panel=module.GetType(SkillListPanel);
        if(!panel.Fields.Any(f=>f.Name=="_isMine") || !panel.Fields.Any(f=>f.Name=="SkillBookGrid"))
            throw new InvalidOperationException("Native skill panel fields changed.");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,SkillAllHelper+".dll"+(staged?".staged":"")),new ReaderParameters{AssemblyResolver=resolver});
        var type=helper.MainModule.GetType(SkillAllHelper)??throw new InvalidOperationException("Missing skill upgrade-all helper.");
        var calls=type.Methods.Concat(type.NestedTypes.SelectMany(t=>t.Methods)).Where(m=>m.HasBody)
            .SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"GetSkillDetailInfo","GetSkillExtend","SetSkillExtend","Apply","EnableButton","OpenError","Open","Request"})
            if(!calls.Any(m=>m.Name==name))throw new InvalidOperationException("Incomplete skill upgrade-all helper: "+name);
        Console.WriteLine("PASS: skill upgrade-all panel hooks at every exit, request encoder/response capture hooks, native fields and IL stack.");
    }

    static void InstallSkillUpgradeAll(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,SkillAllHelper+".dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.SkillUpgradeAll.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create(SkillAllHelper,new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        var helperType=helper.MainModule.GetType(SkillAllHelper);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var module=game.MainModule;
        MethodReference Hook(string name)=>module.ImportReference(helperType.Methods.Single(m=>m.Name==name));
        void WidenBranches(MethodDefinition method) {
            foreach(var i in method.Body.Instructions.Where(i=>i.OpCode.OperandType==OperandType.ShortInlineBrTarget))
                i.OpCode=typeof(OpCodes).GetFields().Select(f=>f.GetValue(null)).OfType<OpCode>().Single(c=>c.Name==i.OpCode.Name[..^2]);
        }
        // Panel exits: attach/refresh after native setup and book/tier updates.
        foreach(var (method,hook) in SkillAllExitHooks(module)) {
            if(CallsSkillAll(method,hook))continue;
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
                var call=il.Create(OpCodes.Call,Hook(hook));
                il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
            WidenBranches(method);
        }
        // Restore narrowed pooled book counters before native Reset releases them.
        var reset=module.GetType(SkillListPanel).Methods.Single(m=>m.Name=="Reset");
        if(!CallsSkillAll(reset,"Release")) {
            var il=reset.Body.GetILProcessor();var first=reset.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,Hook("Release")));
        }
        var encode=module.GetType("JM_NShared_ExtendHeroSkill_Request").Methods.Single(m=>m.Name=="EncodeJsonObject");
        if(!CallsSkillAll(encode,"Encode")) {
            var il=encode.Body.GetILProcessor();
            foreach(var ret in encode.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Dup;ret.Operand=null;
                var arg=il.Create(OpCodes.Ldarg_0);var call=il.Create(OpCodes.Call,Hook("Encode"));
                il.InsertAfter(ret,arg);il.InsertAfter(arg,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
            WidenBranches(encode);
        }
        var parse=SkillResponseParse(module);
        if(!CallsSkillAll(parse,"Capture")) {
            var il=parse.Body.GetILProcessor();var first=parse.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first,il.Create(OpCodes.Call,Hook("Capture")));
        }
        game.Write(dll+".skill-upgrade-all-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifySkillUpgradeAllFile(root,true);
        TestSkillUpgradeAllWire(dll+".skill-upgrade-all-staged",helperPath+".staged",managed);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-skill-upgrade-all-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(dll+".skill-upgrade-all-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifySkillUpgradeAllFile(root);
        Console.WriteLine("Installed skill Upgrade All button. Restart the client.");
    }

    // Runs the patched native encoder and response parser with the compiled
    // helper in an isolated load context, using a server-shaped JSON reply.
    static void TestSkillUpgradeAllWire(string gamePath,string helperPath,string managed) {
        var context=new System.Runtime.Loader.AssemblyLoadContext("SprkSkillAllTest",true);
        context.Resolving+=(ctx,name)=>{
            var path=Path.Combine(managed,name.Name+".dll");
            return File.Exists(path)?ctx.LoadFromStream(new MemoryStream(File.ReadAllBytes(path))):null;
        };
        try {
            var game=context.LoadFromStream(new MemoryStream(File.ReadAllBytes(gamePath)));
            var helper=context.LoadFromStream(new MemoryStream(File.ReadAllBytes(helperPath))).GetType(SkillAllHelper)!;
            var requestType=game.GetType("NShared.ExtendHeroSkill.Request")!;
            var encode=game.GetType("JM_NShared_ExtendHeroSkill_Request")!.GetMethod("EncodeJsonObject")!;
            System.Collections.IDictionary Encode(int extend) {
                var request=Activator.CreateInstance(requestType)!;
                requestType.GetProperty("HeroIndex")!.SetValue(request,7);requestType.GetProperty("SkillExtend")!.SetValue(request,extend);
                return (System.Collections.IDictionary)encode.Invoke(null,new[]{request})!;
            }
            var all=Encode(-1);
            if((string?)all["ExtendAll"]!="1" || (string?)all["HeroIndex"]!="7")throw new InvalidOperationException("Upgrade-all request missing ExtendAll.");
            if(Encode(2).Contains("ExtendAll"))throw new InvalidOperationException("ExtendAll leaked into a native extend request.");
            // MiniJSON shapes: Dictionary<string,object>, List<object>, long numbers.
            Dictionary<string,object> Item(long id,long add,long count)=>new(){{"ItemIndex",id},{"AddCount",add},{"NewCount",count},{"Locked",0L}};
            var reply=new Dictionary<string,object>{
                {"Result","Success"},{"BaseResult","Success"},{"ItemResult",Item(1001,-200,0)},
                {"ItemResults",new List<object>{Item(1001,-200,0),Item(1002,-20,59)}},
                {"SkillExtends",new List<object>{new Dictionary<string,object>{{"SkillSlotIndex",1L},{"SkillIndex",11L},{"SkillExtend",3L}},
                    new Dictionary<string,object>{{"SkillSlotIndex",2L},{"SkillIndex",12L},{"SkillExtend",1L}}}}};
            var parse=game.GetType("JM_NShared_ExtendHeroSkill_Response")!.GetMethods().Single(m=>m.Name=="Parse" && m.GetParameters().Length==1 &&
                m.GetParameters()[0].ParameterType==typeof(System.Collections.IDictionary));
            var response=parse.Invoke(null,new object[]{reply})!;
            var table=helper.GetField("Captured",BindingFlags.Static|BindingFlags.NonPublic)!.GetValue(null)!;
            var args=new object?[]{response,null};
            if(!(bool)table.GetType().GetMethod("TryGetValue")!.Invoke(table,args)!)throw new InvalidOperationException("Upgrade-all response extras not captured.");
            var extras=args[1]!;
            var items=(Array)extras.GetType().GetField("Items")!.GetValue(extras)!;
            var extends=(System.Collections.IEnumerable)extras.GetType().GetField("Extends")!.GetValue(extras)!;
            var pairs=extends.Cast<object>().Select(p=>((int)p.GetType().GetProperty("Key")!.GetValue(p)!,(int)p.GetType().GetProperty("Value")!.GetValue(p)!)).ToArray();
            var second=items.GetValue(1)!;
            if(items.Length!=2 || (int)second.GetType().GetProperty("NewCount")!.GetValue(second)!!=59 || !pairs.SequenceEqual(new[]{(1,3),(2,1)}))
                throw new InvalidOperationException("Upgrade-all response extras parsed incorrectly.");
            var native=parse.Invoke(null,new object[]{new Dictionary<string,object>{{"Result","Success"},{"ItemResult",Item(1001,-20,0)}}})!;
            if((bool)table.GetType().GetMethod("TryGetValue")!.Invoke(table,new object?[]{native,null})!)
                throw new InvalidOperationException("Native single extend response captured as upgrade-all.");
            Console.WriteLine("PASS: native extend encoder adds ExtendAll only for upgrade-all; response parser captures every book result and tier; native responses unchanged.");
        } finally {context.Unload();}
    }
}
