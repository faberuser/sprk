using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static string[] MotionOwnershipBodies(ModuleDefinition module) => module.Types
        .Where(t=>t.Namespace=="NGame2.NUtil.NCostume")
        .SelectMany(t=>t.Methods).Where(m=>m.HasBody)
        .Select(m=>m.FullName+string.Join("|",m.Body.Instructions.Select(i=>i.ToString()))).ToArray();
    static void PatchMotionCostumes(ModuleDefinition game,ModuleDefinition helper) {
        var type=game.GetType("NGame2.NUI.NWindow.HeroDetailView");
        var setup=type.Methods.Single(m=>m.Name=="SetupCostumeList");
        setup.Body=new MethodBody(setup);
        var il=setup.Body.GetILProcessor();
        foreach(var parameter in setup.Parameters) il.Append(il.Create(OpCodes.Ldarg,parameter));
        il.Append(il.Create(OpCodes.Call,game.ImportReference(helper.GetType("SprkMotionCostumes").Methods.Single(m=>m.Name=="Setup"))));
        il.Append(il.Create(OpCodes.Ret));
        foreach(var method in type.Methods.Where(m=>m.HasBody)) {
            var code=method.Body.Instructions;
            foreach(var call in code.Where(i=>i.Operand is MethodReference r && r.Name=="UpdateCostumeListState").ToArray()) {
                call.OpCode=OpCodes.Call;
                call.Operand=game.ImportReference(helper.GetType("SprkMotionCostumes").Methods.Single(m=>m.Name=="Refresh"));
            }
        }
        PatchDressingRoomMotions(game,helper);
        VerifyMotionCostumes(game);
    }
    // Dressing Room: See Motions (Button_Hide) stays visible for every preview,
    // and opens on a snapshot of the previewed appearance.
    static Instruction DressingRoomMotionFlag(ModuleDefinition game) {
        var method=game.GetType("NGame2.NUI.NWindow.HeroCostumeView").Methods.Single(m=>m.Name=="SetVisibleHideButton");
        var code=method.Body.Instructions;
        var load=code.Single(i=>i.OpCode==OpCodes.Ldfld && i.Operand is FieldReference f && f.Name=="ButtonHide");
        var flag=load.Next;
        if(flag.Next.OpCode!=OpCodes.Ldc_I4_0 || flag.Next.Next.Operand is not MethodReference {Name:"EnableGameObject"})
            throw new InvalidOperationException("Unexpected Dressing Room See Motions visibility shape.");
        return flag;
    }
    static MethodDefinition DressingRoomMotionOpen(ModuleDefinition game) =>
        game.GetType("NGame2.NUI.NManager.CostumeManagement").Methods.Single(m=>m.Name.EndsWith("HeroCostumeView.Listener.OnClickViewMotion"));
    static void PatchDressingRoomMotions(ModuleDefinition game,ModuleDefinition helper) {
        var flag=DressingRoomMotionFlag(game);
        if(flag.OpCode!=OpCodes.Ldc_I4_1){flag.OpCode=OpCodes.Ldc_I4_1;flag.Operand=null;}
        var open=DressingRoomMotionOpen(game);
        if(!open.Body.Instructions.Any(i=>i.Operand is MethodReference {Name:"Snapshot"} r && r.DeclaringType.Name=="SprkMotionCostumes")) {
            var get=open.Body.Instructions.Single(i=>i.Operand is MethodReference {Name:"get_CurrentLocalInfo"});
            open.Body.GetILProcessor().InsertAfter(get,Instruction.Create(OpCodes.Call,game.ImportReference(helper.GetType("SprkMotionCostumes").Methods.Single(m=>m.Name=="Snapshot"))));
        }
    }
    static void VerifyMotionCostumes(ModuleDefinition game) {
        if(DressingRoomMotionFlag(game).OpCode!=OpCodes.Ldc_I4_1)
            throw new InvalidOperationException("Dressing Room See Motions can still be hidden.");
        var open=DressingRoomMotionOpen(game).Body.Instructions;
        var snapshot=open.Where(i=>i.Operand is MethodReference {Name:"Snapshot"} r && r.DeclaringType.Name=="SprkMotionCostumes").ToArray();
        if(snapshot.Length!=1 || snapshot[0].Previous.Operand is not MethodReference {Name:"get_CurrentLocalInfo"} || snapshot[0].Next.OpCode!=OpCodes.Stloc_0)
            throw new InvalidOperationException("Dressing Room See Motions must open on a preview snapshot.");
        VerifyArenaResultStack(DressingRoomMotionOpen(game));
        var type=game.GetType("NGame2.NUI.NWindow.HeroDetailView");
        var setup=type.Methods.Single(m=>m.Name=="SetupCostumeList");
        if(setup.Body.Instructions.Count!=7 || !setup.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkMotionCostumes" && m.Name=="Setup"))
            throw new InvalidOperationException("Motion costume list hook missing.");
        var refreshCalls=type.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkMotionCostumes" && m.Name=="Refresh");
        if(refreshCalls!=3)
            throw new InvalidOperationException("Expected three scoped motion preview state calls, got "+refreshCalls);
        VerifyArenaResultStack(setup);
        var shared=game.GetType("NGame2.NUI.NWindow.HeroCostumeSceneView").Methods.Single(m=>m.Name=="UpdateCostumeListState");
        if(!shared.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="GetCostumeState"))
            throw new InvalidOperationException("Shared costume ownership states were removed.");
    }
    static void VerifyMotionCostumesFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".motion-costumes-staged":"")));
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkMotionCostumes.dll"+(staged?".staged":"")));
        VerifyMotionCostumes(game.MainModule);
        var type=helper.MainModule.GetType("SprkMotionCostumes");
        var calls=type.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"GetAllData","get_HeroIndex","AddGridItem","Open","RepositionGrid","SetState","SetSelected","Clone"})
            if(!calls.Any(m=>m.Name==name))throw new InvalidOperationException("Missing preview operation "+name);
        if(calls.Any(m=>m.Name=="get_IsOpen" || m.Name=="get_IsShow" || m.Name=="get_PreviewableWhenOwned" || m.DeclaringType.Name.Contains("Manager") || m.Name=="GetCostumeState"))
            throw new InvalidOperationException("Preview list still depends on ownership/visibility.");
        Console.WriteLine("Verified all matching-hero costumes, scoped selectable preview states, preserved shared ownership, Dressing Room See Motions snapshot/visibility and idempotent patch.");
    }
    static void InstallMotionCostumes(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkMotionCostumes.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.MotionCostumes.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkMotionCostumes",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var protectedBefore=MotionOwnershipBodies(game.MainModule);
        PatchMotionCostumes(game.MainModule,helper.MainModule);
        if (!protectedBefore.SequenceEqual(MotionOwnershipBodies(game.MainModule)))
            throw new InvalidOperationException("Costume ownership methods changed.");
        string Bodies()=>string.Join("\n",new[]{game.MainModule.GetType("NGame2.NUI.NWindow.HeroDetailView").Methods.Single(m=>m.Name=="SetupCostumeList"),
            game.MainModule.GetType("NGame2.NUI.NWindow.HeroCostumeView").Methods.Single(m=>m.Name=="SetVisibleHideButton"),DressingRoomMotionOpen(game.MainModule)}
            .SelectMany(m=>m.Body.Instructions).Select(i=>i.ToString()));
        var count=Bodies();
        PatchMotionCostumes(game.MainModule,helper.MainModule);
        if(count!=Bodies())
            throw new InvalidOperationException("Motion costume patch is not idempotent.");
        var staged=dll+".motion-costumes-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyMotionCostumesFile(root,true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        var backup=dll+".before-motion-costumes-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        File.Copy(dll,backup);
        if (File.Exists(helperPath)) File.Copy(helperPath,helperPath+".before-motion-costumes-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyMotionCostumesFile(root);
        Console.WriteLine("Installed See Motions costume previews. Restart the client.");
    }
}

