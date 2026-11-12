using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace DllPatcher;
partial class Program {
    const string TmAwakenHelper = "SprkTechnomagicAwaken";
    const string TmAwakenWindow = "NGame2.NUI.NWindow.ValanceAwakenWindow";
    const string TmAwakenEncoder = "JM_NShared_ValanceAwaken_Request";

    static bool CallsTmAwaken(MethodDefinition method,string name) =>
        method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==TmAwakenHelper && m.Name==name);

    static void VerifyTechnomagicAwakenTargetFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".tm-awaken-target-staged":"")),new ReaderParameters{AssemblyResolver=resolver});
        var window=game.MainModule.GetType(TmAwakenWindow);
        var setup=window.Methods.Single(m=>m.Name=="SetupResultIcon");
        var rets=setup.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray();
        if(rets.Length==0 || rets.Any(r=>r.Previous?.Operand is not MethodReference{Name:"Setup"} m || m.DeclaringType.Name!=TmAwakenHelper))
            throw new InvalidOperationException("TM awaken target must be applied after every native SetupResultIcon exit.");
        foreach(var (method,hook) in new[]{("ClearSeletedResult","Clear"),("OnClickAwaken","Remember")}) {
            var body=window.Methods.Single(m=>m.Name==method).Body.Instructions;
            if(body[1].Operand is not MethodReference call || call.Name!=hook || call.DeclaringType.Name!=TmAwakenHelper ||
               body.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name==TmAwakenHelper)!=1)
                throw new InvalidOperationException("Missing or duplicate TM awaken hook: "+method);
        }
        // The native single-star request path and its listener stay intact.
        var click=window.Methods.Single(m=>m.Name=="OnClickAwaken");
        if(!click.Body.Instructions.Any(i=>i.Operand is MethodReference{Name:"OnClickAwaken"} m && m.DeclaringType.Name=="IListener"))
            throw new InvalidOperationException("Native TM awaken listener call removed.");
        foreach(var name in new[]{"SetupMaterialIcon","SetReqGoldLabel","SetButton","SetItemStat"})
            if(!window.Methods.Any(m=>m.Name==name))throw new InvalidOperationException("Native TM refresh method missing: "+name);
        foreach(var name in new[]{"AfterStar","AfterStat","ValanceAwakenData"})
            if(window.Properties.SingleOrDefault(p=>p.Name==name)?.SetMethod==null)throw new InvalidOperationException("Native TM property setter missing: "+name);
        if(!window.Fields.Any(f=>f.Name=="_resultComp"))throw new InvalidOperationException("Native TM result icon field missing.");
        var encode=game.MainModule.GetType(TmAwakenEncoder).Methods.Single(m=>m.Name=="EncodeJsonObject");
        var encodeRets=encode.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray();
        if(encodeRets.Any(r=>r.Previous?.Operand is not MethodReference{Name:"Encode"} m || m.DeclaringType.Name!=TmAwakenHelper))
            throw new InvalidOperationException("TM awaken request encoder must add TargetStar before every return.");
        if(!encode.Body.Instructions.Any(i=>i.OpCode==OpCodes.Ldstr && (string?)i.Operand=="EquipItemSlotIndex"))
            throw new InvalidOperationException("Native TM awaken request field removed.");
        foreach(var method in new[]{setup,window.Methods.Single(m=>m.Name=="ClearSeletedResult"),click,encode})VerifyArenaResultStack(method);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,TmAwakenHelper+".dll"+(staged?".staged":"")),new ReaderParameters{AssemblyResolver=resolver});
        var type=helper.MainModule.GetType(TmAwakenHelper)??throw new InvalidOperationException("Missing TM awaken helper.");
        var calls=type.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"GetData","CLI_GetMaxStar_for_Awaken","GetEquipItemStatData","get_LoadGo","Assign","SetReqGoldLabel","SetButton","SetItemStat","SetStarBlink"})
            if(!calls.Any(m=>m.Name==name))throw new InvalidOperationException("Incomplete TM awaken helper: "+name);
        Console.WriteLine("PASS: TM awaken target hooks after native result setup, clear/click/encoder hooks, native refresh members and IL stack.");
    }

    static void InstallTechnomagicAwakenTarget(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,TmAwakenHelper+".dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.TechnomagicAwakenTarget.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create(TmAwakenHelper,new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        var helperType=helper.MainModule.GetType(TmAwakenHelper);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var module=game.MainModule;
        MethodReference Hook(string name)=>module.ImportReference(helperType.Methods.Single(m=>m.Name==name));
        var window=module.GetType(TmAwakenWindow);

        // After native SetupResultIcon: expand its one-star price to the target.
        var setup=window.Methods.Single(m=>m.Name=="SetupResultIcon");
        if(!CallsTmAwaken(setup,"Setup")) {
            var il=setup.Body.GetILProcessor();
            foreach(var ret in setup.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                // Reuse the ret as the first hook instruction so branch targets stay valid.
                ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
                var call=il.Create(OpCodes.Call,Hook("Setup"));
                il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        foreach(var (method,hook) in new[]{("ClearSeletedResult","Clear"),("OnClickAwaken","Remember")}) {
            var target=window.Methods.Single(m=>m.Name==method);
            if(CallsTmAwaken(target,hook))continue;
            var il=target.Body.GetILProcessor();var first=target.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,Hook(hook)));
        }
        // Add TargetStar to the existing request JSON; native clients omit it.
        var encode=module.GetType(TmAwakenEncoder).Methods.Single(m=>m.Name=="EncodeJsonObject");
        if(!CallsTmAwaken(encode,"Encode")) {
            var il=encode.Body.GetILProcessor();
            foreach(var ret in encode.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Dup;ret.Operand=null;
                var arg=il.Create(OpCodes.Ldarg_0);var call=il.Create(OpCodes.Call,Hook("Encode"));
                il.InsertAfter(ret,arg);il.InsertAfter(arg,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        foreach(var method in new[]{setup,encode})
            foreach(var i in method.Body.Instructions.Where(i=>i.OpCode.OperandType==OperandType.ShortInlineBrTarget))
                i.OpCode=typeof(OpCodes).GetFields().Select(f=>f.GetValue(null)).OfType<OpCode>().Single(c=>c.Name==i.OpCode.Name[..^2]);

        game.Write(dll+".tm-awaken-target-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyTechnomagicAwakenTargetFile(root,true);
        TestTechnomagicAwakenEncoder(dll+".tm-awaken-target-staged",helperPath+".staged",managed);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-tm-awaken-target-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(dll+".tm-awaken-target-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyTechnomagicAwakenTargetFile(root);
        Console.WriteLine("Installed TM awaken target-star slider. Restart the client.");
    }

    // Executes the patched native request encoder with the compiled helper in
    // an isolated load context and checks the exact wire fields.
    static void TestTechnomagicAwakenEncoder(string gamePath,string helperPath,string managed) {
        var context=new System.Runtime.Loader.AssemblyLoadContext("SprkTmAwakenTest",true);
        context.Resolving+=(ctx,name)=>{
            var path=Path.Combine(managed,name.Name+".dll");
            return File.Exists(path)?ctx.LoadFromStream(new MemoryStream(File.ReadAllBytes(path))):null;
        };
        try {
            var gameAssembly=context.LoadFromStream(new MemoryStream(File.ReadAllBytes(gamePath)));
            var helperAssembly=context.LoadFromStream(new MemoryStream(File.ReadAllBytes(helperPath)));
            var helper=helperAssembly.GetType(TmAwakenHelper)!;
            var flags=BindingFlags.Static|BindingFlags.NonPublic;
            // Static initialization binds the native private setters/members used at runtime.
            foreach(var name in new[]{"SetAfterStar","SetAfterStat","SetData","SetupMaterialIcon","ResultComp"})
                if(helper.GetField(name,flags)!.GetValue(null)==null)throw new InvalidOperationException("TM helper could not bind native member: "+name);
            var slot=helper.GetField("pendingSlot",flags)!;var target=helper.GetField("pendingTarget",flags)!;
            var requestType=gameAssembly.GetType("NShared.ValanceAwaken.Request")!;
            var encode=gameAssembly.GetType(TmAwakenEncoder)!.GetMethod("EncodeJsonObject")!;
            System.Collections.IDictionary Run(int requestSlot,int pending,int star) {
                slot.SetValue(null,pending);target.SetValue(null,star);
                var request=Activator.CreateInstance(requestType)!;
                requestType.GetProperty("EquipItemSlotIndex")!.SetValue(request,requestSlot);
                return (System.Collections.IDictionary)encode.Invoke(null,new[]{request})!;
            }
            var multi=Run(42,42,5);
            if((string?)multi["TargetStar"]!="5" || (string?)multi["EquipItemSlotIndex"]!="42")
                throw new InvalidOperationException("TM multi-star request missing TargetStar.");
            if(Run(43,42,5).Contains("TargetStar") || Run(42,-1,5).Contains("TargetStar"))
                throw new InvalidOperationException("TM TargetStar leaked into another or a native one-star request.");
            if(encode.Invoke(null,new object?[]{null}) is not System.Collections.IDictionary empty || empty.Count!=0)
                throw new InvalidOperationException("Null TM request encoding changed.");
            Console.WriteLine("PASS: native TM awaken request encoder adds TargetStar only for the remembered slot; one-star and null requests unchanged.");
        } finally {context.Unload();}
    }
}
