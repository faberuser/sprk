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
        PatchSkillBookTickets(module,helper);
        VerifyHeroMaterialTickets(module);
    }
    static MethodDefinition SkillBookExtendSite(ModuleDefinition module) => module.GetType("NGame2.NUI.NWindow.HeroSkillDesc").Methods.Single(m=>m.Name=="LearnExtendSkill");
    static bool IsNativeExtendSkill(Instruction i) => i.Operand is MethodReference m && m.Name=="ExtendSkill" && m.DeclaringType.FullName=="NGame2.NUI.NWindow.HeroSkillDesc/Listener";
    // Hero Info skill tiers: route the listener's ExtendSkill through the book-ticket shortcut.
    // The helper receives the native arguments plus the HeroSkillDesc and calls the listener itself.
    static void PatchSkillBookTickets(ModuleDefinition module, ModuleDefinition helper) {
        var method=SkillBookExtendSite(module);
        var extend=module.ImportReference(helper.GetType("SprkSkillBookTickets").Methods.Single(m=>m.Name=="Extend"));
        var il=method.Body.GetILProcessor();
        foreach(var call in method.Body.Instructions.Where(IsNativeExtendSkill).ToArray()) {
            il.InsertBefore(call,il.Create(OpCodes.Ldarg_0));
            call.OpCode=OpCodes.Call;call.Operand=extend;
        }
        foreach(var instruction in method.Body.Instructions.Where(i=>i.OpCode.OperandType==OperandType.ShortInlineBrTarget))
            instruction.OpCode=typeof(OpCodes).GetFields().Select(f=>f.GetValue(null)).OfType<OpCode>().Single(c=>c.Name==instruction.OpCode.Name[..^2]);
        // Book counters (skill list and skill window) show books plus convertible tickets.
        var counter=SkillBookCounter(module);
        if(!counter.Body.Instructions.Any(IsSkillBookDisplay)) {
            var cil=counter.Body.GetILProcessor();var first=counter.Body.Instructions[0];
            cil.InsertBefore(first,cil.Create(OpCodes.Ldarg_0));
            cil.InsertBefore(first,cil.Create(OpCodes.Ldarg_1));
            cil.InsertBefore(first,cil.Create(OpCodes.Call,module.ImportReference(helper.GetType("SprkSkillBookTickets").Methods.Single(m=>m.Name=="Display"))));
            cil.InsertBefore(first,cil.Create(OpCodes.Starg,counter.Parameters[0]));
        }
        // Tier status (UPGRADE marker, Skills badge, hero-list indicators) counts tickets as books.
        // Only this comparison changes; SkillBookInfo.CurItemCount keeps the real book count.
        var usable=module.ImportReference(helper.GetType("SprkSkillBookTickets").Methods.Single(m=>m.Name=="Usable"));
        foreach(var load in SkillTierStatus(module).Body.Instructions.Where(IsTierBookCount).ToArray()) {
            load.OpCode=OpCodes.Call;load.Operand=usable;
        }
    }
    static MethodDefinition SkillTierStatus(ModuleDefinition module) => module.GetType("NGame2.NAccount.Helper").Methods.Single(m=>m.Name=="GetSkillDetailInfo");
    static bool IsTierBookCount(Instruction i) => i.OpCode==OpCodes.Ldfld && i.Operand is FieldReference f && f.Name=="CurItemCount" &&
        f.DeclaringType.FullName=="NGame2.NAccount.Helper/SkillBookInfo" && i.Previous?.Operand is FieldReference b && b.Name=="BookInfo";
    static MethodDefinition SkillBookCounter(ModuleDefinition module) => module.GetType("NGame2.NUI.NComponent2.UpgradeBookItemIconComponent").Methods.Single(m=>m.Name=="SetItemCount");
    static bool IsSkillBookDisplay(Instruction i) => i.Operand is MethodReference m && m.DeclaringType.Name=="SprkSkillBookTickets" && m.Name=="Display";
    static void VerifySkillBookTickets(ModuleDefinition module) {
        var counter=SkillBookCounter(module);var ci=counter.Body.Instructions;
        if(ci.Count(IsSkillBookDisplay)!=1 || ci[0].OpCode!=OpCodes.Ldarg_0 || ci[1].OpCode!=OpCodes.Ldarg_1 || !IsSkillBookDisplay(ci[2]) ||
            ci[3].OpCode.Code is not (Code.Starg or Code.Starg_S) || ci[3].Operand!=counter.Parameters[0] ||
            ((MethodReference)ci[2].Operand).ReturnType.MetadataType!=MetadataType.Int32 || counter.Parameters[0].ParameterType.MetadataType!=MetadataType.Int32)
            throw new InvalidOperationException("Skill book counters must add convertible tickets before native display");
        var tier=SkillTierStatus(module).Body.Instructions;
        var usable=tier.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkSkillBookTickets" && m.Name=="Usable").ToArray();
        if(tier.Any(IsTierBookCount) || usable.Length!=1 || usable[0].Previous.Operand is not FieldReference bookInfo || bookInfo.Name!="BookInfo" ||
            ((MethodReference)usable[0].Operand).Parameters.Single().ParameterType.FullName!=bookInfo.FieldType.FullName ||
            ((MethodReference)usable[0].Operand).ReturnType.MetadataType!=MetadataType.Int32)
            throw new InvalidOperationException("Skill tier status must count convertible tickets exactly once");
        var method=SkillBookExtendSite(module);
        if(method.Body.Instructions.Any(IsNativeExtendSkill))throw new InvalidOperationException("Unpatched skill tier listener call");
        var hooks=method.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkSkillBookTickets" && m.Name=="Extend").ToArray();
        if(hooks.Length!=2)throw new InvalidOperationException("Missing skill book ticket hooks");
        var native=module.GetType("NGame2.NUI.NWindow.HeroSkillDesc").NestedTypes.Single(t=>t.Name=="Listener").Methods.Single(m=>m.Name=="ExtendSkill");
        foreach(var hook in hooks) {
            var call=(MethodReference)hook.Operand;
            if(hook.OpCode!=OpCodes.Call || call.HasThis || call.ReturnType.MetadataType!=MetadataType.Void || hook.Previous.OpCode!=OpCodes.Ldarg_0 ||
                call.Parameters.Count!=native.Parameters.Count+2 || call.Parameters[0].ParameterType.FullName!=native.DeclaringType.FullName ||
                !call.Parameters.Skip(1).Take(native.Parameters.Count).Select(p=>p.ParameterType.FullName).SequenceEqual(native.Parameters.Select(p=>p.ParameterType.FullName)) ||
                call.Parameters[^1].ParameterType.FullName!=method.DeclaringType.FullName)
                throw new InvalidOperationException("Skill book hook changed the native stack signature");
        }
    }
    static void VerifyHeroMaterialTickets(ModuleDefinition module) {
        VerifyEssenceCraftTickets(module);
        VerifySkillBookTickets(module);
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
        var open=helper.MainModule.GetType("SprkHeroMaterialTickets").Methods.Single(m=>m.Name=="Open");
        var count=open.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.Name=="ShowCountLabel");
        if(count.Previous.Operand is not ParameterDefinition ready || ready.Name!="canStart")
            throw new InvalidOperationException("Material counts must preserve native readiness visibility to avoid overlapping purification status.");
        static TypeDefinition Outer(TypeDefinition t){while(t.DeclaringType!=null)t=t.DeclaringType;return t;}
        MethodReference[] CallsIn(bool books)=>helper.MainModule.GetTypes().Where(t=>(Outer(t).Name=="SprkSkillBookTickets")==books)
            .SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        if(CallsIn(false).Count(m=>m.Name=="Apply")!=4)throw new InvalidOperationException("Hero/crafting ticket consumption/reward application missing");
        var bookCalls=CallsIn(true);
        foreach(var name in new[]{"Apply","ExtendSkill","GetSkillBookInfo","Refresh","Open2","get_SessionKey","Request","Matches","SetItemCount","get_ItemData"})
            if(!bookCalls.Any(m=>m.Name==name))throw new InvalidOperationException("Missing skill book ticket behavior: "+name);
        if(bookCalls.Count(m=>m.Name=="Apply")!=4)throw new InvalidOperationException("Skill book ticket consumption/reward application missing");
        if(!helper.MainModule.GetType("SprkSkillBookTickets").NestedTypes.Single(t=>t.Name=="UseBook").Interfaces.Any(i=>i.InterfaceType.Name=="ITicketPrefill"))
            throw new InvalidOperationException("Skill book popup lost its shortage prefill");
        Console.WriteLine("Verified hero and Hall material hooks, Hero Info skill book tickets, scoped quantity prefill, fragment/essence gates, native popup/request, session checks and inventory refresh.");
    }
    static void InstallHeroMaterialTickets(string root,bool stageOnly) {
        TestHeroMaterialTickets();
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkHeroMaterialTickets.dll");
        using var source=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.HeroMaterialTickets.cs.txt")!;
        using var reader=new StreamReader(source);
        using var craftSource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.EssenceCraftTickets.cs.txt")!;
        using var craftReader=new StreamReader(craftSource);
        using var bookSource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.SkillBookTickets.cs.txt")!;
        using var bookReader=new StreamReader(bookSource);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var hallCountType=game.MainModule.GetType("NGame2.NUI.NComponent2.AwakeChallengeItemComponent")
            .Methods.Single(m=>m.Name=="Open" && m.Parameters.Count==4).Parameters[1].ParameterType.FullName;
        if(hallCountType is not ("System.Int32" or "System.Int64"))throw new InvalidOperationException("Unexpected Hall material count type: "+hallCountType);
        var parseOptions=CSharpParseOptions.Default.WithPreprocessorSymbols(hallCountType=="System.Int32"?new[]{"SPRK_HALL_OPEN_INT"}:Array.Empty<string>());
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkHeroMaterialTickets",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd(),parseOptions),CSharpSyntaxTree.ParseText(craftReader.ReadToEnd()),CSharpSyntaxTree.ParseText(bookReader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        using var helper=ModuleDefinition.ReadModule(new MemoryStream(compiled.ToArray()));
        PatchHeroMaterialTickets(game.MainModule,helper);
        string Snapshot()=>string.Join("\n",HeroTicketSetup(game.MainModule).Body.Instructions)+"\n"+string.Join("\n",SkillBookExtendSite(game.MainModule).Body.Instructions)+"\n"+string.Join("\n",SkillBookCounter(game.MainModule).Body.Instructions)+"\n"+string.Join("\n",SkillTierStatus(game.MainModule).Body.Instructions);
        var before=Snapshot();
        PatchHeroMaterialTickets(game.MainModule,helper);
        if(before!=Snapshot())throw new InvalidOperationException("Hero tickets patch is not idempotent");
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
            new[]{CSharpSyntaxTree.ParseText(Source("HeroMaterialTickets")),CSharpSyntaxTree.ParseText(Source("EssenceCraftTickets")),CSharpSyntaxTree.ParseText(Source("HeroMaterialTicketsTests")),
                CSharpSyntaxTree.ParseText(Source("SkillBookTickets")),CSharpSyntaxTree.ParseText(Source("SkillBookTicketsTests"))},
            references,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release));
        using var output=new MemoryStream();var result=compilation.Emit(output);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        var assembly=System.Reflection.Assembly.Load(output.ToArray());
        assembly.GetType("HeroTicketTests")!.GetMethod("Run")!.Invoke(null,null);
        assembly.GetType("EssenceCraftTests")!.GetMethod("Run")!.Invoke(null,null);
        assembly.GetType("SkillBookTicketTests")!.GetMethod("Run")!.Invoke(null,null);
    }
}
