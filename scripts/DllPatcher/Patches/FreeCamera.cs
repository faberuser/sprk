using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void VerifyFreeCamera(ModuleDefinition module) {
        var detail=module.GetType("NGame2.NUI.NWindow.HeroDetailView");
        var randomClick=detail.Methods.Single(m=>m.Name=="OnTouchForRandomAnimation");
        if(randomClick.Body.Instructions.Count!=1 || randomClick.Body.Instructions[0].OpCode!=OpCodes.Ret)throw new InvalidOperationException("See Motions preview clicks must not select random animations.");
        var animationButton=detail.Methods.Single(m=>m.Name.EndsWith("Listener.PlayAnimation"));
        if(!animationButton.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="Play" && m.DeclaringType.Name=="MotionSequence"))throw new InvalidOperationException("Explicit animation selection must remain available.");
        var refresh=module.GetType("NGame2.NUI.NWindow.BaseWindow").Methods.Single(m=>m.Name=="UpdateDerived");
        if(refresh.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkFreeCamera" && m.Name=="RefreshLayout")!=1)throw new InvalidOperationException("Missing or duplicated ultrawide layout hook.");
        VerifyArenaResultStack(refresh);
        var window=module.GetType("NGame2.NUI.NWindow.HeroFullSceneViewBase");
        foreach(var pair in new[]{("UpdateHeroCamera","InputCamera"),("LateUpdate","Apply"),("Reset","Reset")}) {
            var method=window.Methods.Single(m=>m.Name==pair.Item1);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkFreeCamera" && m.Name==pair.Item2))throw new InvalidOperationException("Missing free camera hook: "+pair.Item1);
            if(!method.Body.HasExceptionHandlers)VerifyArenaResultStack(method);
        }
        var lobby=module.GetType("NGame2.NUI.NWorldMap.NMapLayout.NMapViewer.WorldMapMode");
        foreach(var pair in new[]{("DragWorldMap","ScaleLobbyDrag"),("ZoomWorldMap","ScaleLobbyZoom")}) {
            var method=lobby.Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkFreeCamera" && m.Name==pair.Item2)!=1)
                throw new InvalidOperationException("Missing or duplicated lobby sensitivity hook: "+pair.Item1);
            VerifyArenaResultStack(method);
        }
        if(!window.Methods.Single(m=>m.Name=="LateUpdate").Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="ChainLateUpdate"))throw new InvalidOperationException("Native preview camera was removed.");
    }
    static void VerifyFreeCameraFile(string root,bool staged=false) {
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".free-camera-staged":"")));
        VerifyFreeCamera(game.MainModule);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","SprkFreeCamera.dll"+(staged?".staged":"")));
        var eligible=helper.MainModule.GetType("SprkFreeCamera").Methods.Single(m=>m.Name=="Eligible");
        if(eligible.Body.Instructions.Any(i=>i.Operand is FieldReference f && f.Name=="detailTab"))throw new InvalidOperationException("Free camera must remain enabled on all hero preview tabs.");
        var scope=helper.MainModule.GetType("SprkFreeCamera").Methods.Single(m=>m.Name=="SupportsWindow");
        foreach(var name in new[]{"HeroDetailView","HeroCostumeView","HeroModelView"})
            if(!scope.Body.Instructions.Any(i=>i.Operand is TypeReference t && t.Name==name))throw new InvalidOperationException("Missing preview window scope: "+name);
        var apply=helper.MainModule.GetType("SprkFreeCamera").Methods.Single(m=>m.Name=="Apply");
        if(!apply.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="StabilizeOrbit"))throw new InvalidOperationException("Preview orbit must not follow animated head motion.");
        if(!apply.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="get_ShapeBounds"))throw new InvalidOperationException("Free camera must use the hero's body bounds as its orbit pivot.");
        if(!apply.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="FitPreviewAspect"))throw new InvalidOperationException("Missing displayed preview aspect correction.");
        var fit=helper.MainModule.GetType("SprkFreeCamera").Methods.Single(m=>m.Name=="FitPreviewAspect");
        foreach(var name in new[]{"get_drawingDimensions","WorldToScreenPoint","get_uvRect","set_aspect"})
            if(!fit.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Incomplete preview aspect correction: "+name);
        var layout=helper.MainModule.GetType("SprkFreeCamera").Methods.Single(m=>m.Name=="RefreshLayout");
        if(!layout.Body.Instructions.Any(i=>i.Operand is TypeReference t && t.Name=="HeroDeck2"))throw new InvalidOperationException("Missing Heroes List layout scope.");
        var layoutRoot=helper.MainModule.GetType("SprkFreeCamera").Methods.Single(m=>m.Name=="RefreshLayoutRoot");
        foreach(var name in new[]{"ResetAnchors","ForceUpdateAnchors"})
            if(!layoutRoot.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Missing native layout refresh: "+name);
        if(!game.MainModule.GetType("NGame2.NUI.NWindow.HeroDetailView").Methods.Any(m=>m.Name=="OpenOnlyViewMotion"))throw new InvalidOperationException("Native See Motions path missing.");
        Console.WriteLine("Verified scoped free camera hooks and preserved native camera update.");
    }
    static void InstallFreeCamera(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkFreeCamera.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.FreeCamera.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkFreeCamera",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var module=game.MainModule;
        var bridge=helper.MainModule.GetType("SprkFreeCamera");
        // The model texture's UIEventTrigger dispatches both desktop click
        // buttons here. Leave pointer press/release and animation buttons alone.
        var randomClick=module.GetType("NGame2.NUI.NWindow.HeroDetailView").Methods.Single(m=>m.Name=="OnTouchForRandomAnimation");
        randomClick.Body=new MethodBody(randomClick);randomClick.Body.GetILProcessor().Emit(OpCodes.Ret);
        var refresh=module.GetType("NGame2.NUI.NWindow.BaseWindow").Methods.Single(m=>m.Name=="UpdateDerived");
        if(!refresh.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkFreeCamera" && m.Name=="RefreshLayout")) {
            var il=refresh.Body.GetILProcessor();var first=refresh.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="RefreshLayout"))));
        }
        var window=module.GetType("NGame2.NUI.NWindow.HeroFullSceneViewBase");
        foreach(var pair in new[]{("LateUpdate","Apply"),("Reset","Reset")}) {
            var method=window.Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkFreeCamera" && m.Name==pair.Item2))continue;
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
                var call=il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name==pair.Item2)));
                il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        var input=window.Methods.Single(m=>m.Name=="UpdateHeroCamera");
        if(!input.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkFreeCamera")) {
            var first=input.Body.Instructions[0];var il=input.Body.GetILProcessor();
            foreach(var instruction in new[]{il.Create(OpCodes.Ldarg_0),il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="InputCamera"))),il.Create(OpCodes.Brfalse,first),il.Create(OpCodes.Ret)})il.InsertBefore(first,instruction);
        }
        // Hook only native camera input, not NGUI/global pointer events or
        // automatic chase/focus camera updates. Scale the consumed argument once.
        var lobby=module.GetType("NGame2.NUI.NWorldMap.NMapLayout.NMapViewer.WorldMapMode");
        foreach(var pair in new[]{("DragWorldMap","ScaleLobbyDrag"),("ZoomWorldMap","ScaleLobbyZoom")}) {
            var method=lobby.Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkFreeCamera" && m.Name==pair.Item2))continue;
            var il=method.Body.GetILProcessor();var first=method.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first,il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name==pair.Item2))));
            il.InsertBefore(first,il.Create(OpCodes.Starg,method.Parameters[0]));
        }
        var awake=module.GetType("UICamera").Methods.Single(m=>m.Name=="Awake");
        if(!awake.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkFreeCamera" && m.Name=="EnsureProbe"))
            awake.Body.GetILProcessor().InsertBefore(awake.Body.Instructions[0],Instruction.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="EnsureProbe"))));
        VerifyFreeCamera(game.MainModule);
        var staged=dll+".free-camera-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyFreeCameraFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>p.ProcessName=="King's Raid" || p.ProcessName.StartsWith("SprkTest-")))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-free-camera-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyFreeCameraFile(root);
    }
}
