using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static MethodDefinition HeroTicketSetup(ModuleDefinition module) => module.GetType("NGame2.NUI.NComponent2.HeroStatPanel").Methods.Single(m=>m.Name=="SetupAwakeMaterial");
    static void PatchHeroMaterialTickets(ModuleDefinition module, ModuleDefinition helper) {
        PatchEssenceCraftTickets(module,helper);
        var hall=module.GetType("NGame2.NUI.NWindow.TrialChallengeHeroSelect");
        foreach(var i in hall.Methods.Single(m=>m.Name=="SetupAwakeChallengeItemDesc").Body.Instructions)
            if(i.Operand is MethodReference m && m.DeclaringType.Name=="AwakeChallengeItemComponent" && m.Name=="Open") {
                i.OpCode=OpCodes.Call;i.Operand=module.ImportReference(helper.GetType("SprkHeroMaterialTickets").Methods.Single(m=>m.Name=="OpenHall"));
            }
        var refresh=hall.Methods.SingleOrDefault(m=>m.Name=="SprkRefreshTicketMaterials");
        if(refresh==null) {
            refresh=new MethodDefinition("SprkRefreshTicketMaterials",MethodAttributes.Private,module.TypeSystem.Void);hall.Methods.Add(refresh);
            var il=refresh.Body.GetILProcessor();var ret=il.Create(OpCodes.Ret);
            var selected=hall.Fields.Single(f=>f.Name=="_SelectedPortrait");
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,selected);il.Emit(OpCodes.Brfalse,ret);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,selected);
            il.Emit(OpCodes.Callvirt,selected.FieldType.Resolve().Methods.Single(m=>m.Name=="get_HeroInfo"));
            il.Emit(OpCodes.Call,hall.Methods.Single(m=>m.Name=="SetupAwakeChallengeItemDesc"));il.Append(ret);
        }
        var observer=hall.Methods.Single(m=>m.Name.Contains("IUserObserver<NShared.ItemInfo>.OnChanged"));
        if(!observer.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==refresh.Name)) {
            foreach(var ret in observer.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;var il=observer.Body.GetILProcessor();var call=il.Create(OpCodes.Call,refresh);
                il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        foreach(var instruction in observer.Body.Instructions.Where(i=>i.OpCode.OperandType==OperandType.ShortInlineBrTarget))
            instruction.OpCode=typeof(OpCodes).GetFields().Select(f=>f.GetValue(null)).OfType<OpCode>().Single(c=>c.Name==instruction.OpCode.Name[..^2]);
        var method=HeroTicketSetup(module);
        foreach(var i in method.Body.Instructions)
            if(i.Operand is MethodReference m && m.DeclaringType.FullName=="NGame2.NUI.NComponent2.HeroAwakeItemComponent" && m.Name=="Open") {
                i.OpCode=OpCodes.Call;
                i.Operand=module.ImportReference(helper.GetType("SprkHeroMaterialTickets").Methods.Single(m=>m.Name=="Open"));
            }
        var popup=module.GetType("NGame2.NUI.NWindow.SelectionItemUsePopup").Methods.Single(m=>m.Name=="OpenInternal");
        var popupType=popup.DeclaringType;
        var prefillMethod=module.ImportReference(helper.GetType("SprkHeroMaterialTickets").Methods.Single(m=>m.Name=="Prefill"));
        // Upgrade the existing quantity hook as well as fresh baseline installs.
        foreach(var call in popup.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkHeroMaterialTickets" && m.Name=="Prefill").ToArray()) {
            if(((MethodReference)call.Operand).Parameters.Count==2) {
                var il=popup.Body.GetILProcessor();il.InsertBefore(call,il.Create(OpCodes.Ldarg_0));
                il.InsertBefore(call,il.Create(OpCodes.Ldfld,popupType.Fields.Single(f=>f.Name=="_selectableItems")));
            }
            call.Operand=prefillMethod;
        }
        if(!popup.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkHeroMaterialTickets" && m.Name=="Prefill")) {
            var il=popup.Body.GetILProcessor();
            foreach(var ret in popup.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;
                var listener=il.Create(OpCodes.Ldarg_3);il.InsertAfter(ret,listener);
                var self=il.Create(OpCodes.Ldarg_0);il.InsertAfter(listener,self);
                var choices=il.Create(OpCodes.Ldfld,popupType.Fields.Single(f=>f.Name=="_selectableItems"));il.InsertAfter(self,choices);
                var call=il.Create(OpCodes.Call,prefillMethod);
                il.InsertAfter(choices,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        VerifyHeroMaterialTickets(module);
    }
    static void VerifyHeroMaterialTickets(ModuleDefinition module) {
        VerifyEssenceCraftTickets(module);
        var method=HeroTicketSetup(module);
        if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkHeroMaterialTickets" && m.Name=="Open")!=4)
            throw new InvalidOperationException("Missing hero material ticket hooks");
        if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="HeroAwakeItemComponent" && m.Name=="Open"))
            throw new InvalidOperationException("Unpatched hero material slot");
        var native=module.GetType("NGame2.NUI.NComponent2.HeroAwakeItemComponent").Methods.Single(m=>m.Name=="Open");
        foreach(var call in method.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Where(m=>m.DeclaringType.Name=="SprkHeroMaterialTickets"))
            if(call.HasThis || call.ReturnType.MetadataType!=MetadataType.Void || call.Parameters.Count!=native.Parameters.Count+1 ||
                call.Parameters[0].ParameterType.FullName!=native.DeclaringType.FullName ||
                !call.Parameters.Skip(1).Select(p=>p.ParameterType.FullName).SequenceEqual(native.Parameters.Select(p=>p.ParameterType.FullName)))
                throw new InvalidOperationException("Hero material hook changed the native stack signature");
        var observer=method.DeclaringType.Methods.Single(m=>m.Name.Contains("IUserObserver<NShared.ItemInfo>.OnChanged"));
        if(!observer.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="SetupAwakeMaterial"))
            throw new InvalidOperationException("Native item observer no longer refreshes hero awakening");
        var popup=module.GetType("NGame2.NUI.NWindow.SelectionItemUsePopup").Methods.Single(m=>m.Name=="OpenInternal");
        var prefill=popup.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkHeroMaterialTickets" && m.Name=="Prefill");
        if(prefill.Previous.Operand is not FieldReference choices || choices.Name!="_selectableItems" ||
            prefill.Previous.Previous.OpCode!=OpCodes.Ldarg_0 || prefill.Previous.Previous.Previous.OpCode!=OpCodes.Ldarg_3 ||
            prefill.Previous.Previous.Previous.Previous.OpCode!=OpCodes.Ldarg_0 || prefill.Next.OpCode!=OpCodes.Ret)
            throw new InvalidOperationException("Ticket quantity must initialize after native popup setup");
        VerifyArenaResultStack(popup);
        var hall=module.GetType("NGame2.NUI.NWindow.TrialChallengeHeroSelect");
        var hallSetup=hall.Methods.Single(m=>m.Name=="SetupAwakeChallengeItemDesc");
        if(hallSetup.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkHeroMaterialTickets" && m.Name=="OpenHall")!=3)
            throw new InvalidOperationException("Hall of Heroes material hooks missing");
        var hallObserver=hall.Methods.Single(m=>m.Name.Contains("IUserObserver<NShared.ItemInfo>.OnChanged"));
        foreach(var ret in hallObserver.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret))
            if(ret.Previous.Operand is not MethodReference call || call.Name!="SprkRefreshTicketMaterials")
                throw new InvalidOperationException("Hall of Heroes material refresh missing");
    }
    static void VerifyHeroMaterialTicketsFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".hero-tickets-staged":"")));
        VerifyHeroMaterialTickets(game.MainModule);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkHeroMaterialTickets.dll"+(staged?".staged":"")));
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"GetItemCode","GetTranscendItemCode","SetListener","ShowCountLabel","get_ItemIndices","OpenError","Apply","set_ItemCount","set_SelectItemIndex","get_SessionKey","InitialCount","OnChangeInputSelectCount"})
            if(!calls.Any(m=>m.Name==name))throw new InvalidOperationException("Missing ticket behavior: "+name);
        if(calls.Count(m=>m.Name=="Apply")!=4)throw new InvalidOperationException("Hero/crafting ticket consumption/reward application missing");
        Console.WriteLine("Verified hero and Hall material hooks, scoped quantity prefill, fragment/essence gates, native popup/request, session checks and inventory refresh.");
    }
    static void InstallHeroMaterialTickets(string root,bool stageOnly) {
        TestHeroMaterialTickets();
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkHeroMaterialTickets.dll");
        using var source=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.HeroMaterialTickets.cs.txt")!;
        using var reader=new StreamReader(source);
        using var craftSource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.EssenceCraftTickets.cs.txt")!;
        using var craftReader=new StreamReader(craftSource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkHeroMaterialTickets",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd()),CSharpSyntaxTree.ParseText(craftReader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        using var helper=ModuleDefinition.ReadModule(new MemoryStream(compiled.ToArray()));
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        PatchHeroMaterialTickets(game.MainModule,helper);
        var before=string.Join("\n",HeroTicketSetup(game.MainModule).Body.Instructions);
        PatchHeroMaterialTickets(game.MainModule,helper);
        if(before!=string.Join("\n",HeroTicketSetup(game.MainModule).Body.Instructions))throw new InvalidOperationException("Hero tickets patch is not idempotent");
        game.Write(dll+".hero-tickets-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyHeroMaterialTicketsFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Hero tickets staged. Close the game and shared-asset workers before installing.");
        var backup=Path.Combine("target","hero-material-tickets-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));Directory.CreateDirectory(backup);
        File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));
        if(File.Exists(helperPath))File.Copy(helperPath,Path.Combine(backup,"SprkHeroMaterialTickets.dll"));
        File.Move(dll+".hero-tickets-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyHeroMaterialTicketsFile(root);
    }
    static void TestHeroMaterialTickets() {
        string Source(string name) {
            using var stream=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name+".cs.txt")!;
            using var reader=new StreamReader(stream);return reader.ReadToEnd();
        }
        // Execute the exact shipping helper against an isolated service/UI probe.
        // This does not consume tickets or connect to the game server.
        var references=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("HeroMaterialTicketsProbe-"+Guid.NewGuid(),
            new[]{CSharpSyntaxTree.ParseText(Source("HeroMaterialTickets")),CSharpSyntaxTree.ParseText(Source("EssenceCraftTickets")),CSharpSyntaxTree.ParseText(Source("HeroMaterialTicketsTests"))},
            references,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        var assembly=System.Reflection.Assembly.Load(output.ToArray());
        assembly.GetType("HeroTicketTests")!.GetMethod("Run")!.Invoke(null,null);
        assembly.GetType("EssenceCraftTests")!.GetMethod("Run")!.Invoke(null,null);
    }
}
