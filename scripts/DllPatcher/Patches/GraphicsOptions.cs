using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static Instruction OutlineDefaultArgument(ModuleDefinition module) {
        var initializer=module.GetType("NVespa.SystemOptionData").Methods.Single(m=>m.Name==".cctor");
        var store=initializer.Body.Instructions.Single(i=>i.OpCode==OpCodes.Stsfld && i.Operand is FieldReference f && f.Name=="_render_EnableOutline");
        var constructor=store.Previous;var key=constructor?.Previous;var argument=key?.Previous;
        if(constructor?.OpCode!=OpCodes.Newobj || constructor.Operand is not MethodReference method || method.DeclaringType.FullName!="NVespa.LocalStorageBool" ||
           key?.OpCode!=OpCodes.Ldstr || !Equals(key.Operand,"KingsRaid_GameOption_render_EnableOutline3") ||
           argument==null || (argument.OpCode!=OpCodes.Ldc_I4_0 && argument.OpCode!=OpCodes.Ldc_I4_1))
            throw new InvalidOperationException("Unexpected native Outline preference initializer.");
        return argument;
    }
    static void VerifyGraphicsOptions(ModuleDefinition module) {
        if(Convert.ToInt32(module.GetType("NShared.EResolutionControl").Fields.Single(f=>f.Name=="Custom").Constant)!=5)throw new InvalidOperationException("Missing persisted custom resolution mode.");
        var applyResolution=module.GetType("NVespa.SystemOptionData").Methods.Single(m=>m.Name=="ApplyResoultionSetting");
        if(!applyResolution.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions" && m.Name=="ApplyCustomResolution"))throw new InvalidOperationException("Missing custom window resolution hook.");
        VerifyArenaResultStack(applyResolution);
        foreach(var name in new[]{"SetToggle","SetValueByParent"}) {
            var method=module.GetType("NGame2.NUI.NComponent2.GameOptionToggle4Component").Methods.Single(m=>m.Name==name);
            if(method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).Any(i=>i.Previous.Operand is not MethodReference m || m.Name!="RestoreResolutionChoices" || m.DeclaringType.Name!="SprkGraphicsOptions"))throw new InvalidOperationException("Missing resolution visibility restoration: "+name);
            VerifyArenaResultStack(method);
        }
        if(OutlineDefaultArgument(module).OpCode!=OpCodes.Ldc_I4_0)throw new InvalidOperationException("Outline must default to Off while preserving its native preference key.");
        foreach(var pair in new[]{("NGame2.NUI.NComponent2.GameOptionToggle3Component","SetToggle","CenterTextureChoices"),("UICamera","Awake","Ensure"),("NGame2.NUI.NWindow.GameOptionWindow","SetupOptionGrids","Setup"),("NShader.FilterBase","CreateRenderTarget","CreateNativeTarget"),("NVespa.GlobalCameraManager","HasMainRenderTarget","HasMainRenderTarget"),("NGame2.NUI.NWindow.HeroFullSceneViewBase","UpdateDerived","PollComparisonKeys")}) {
            var method=module.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions" && m.Name==pair.Item3))throw new InvalidOperationException("Missing graphics hook: "+pair.Item3);
            if(!method.Body.HasExceptionHandlers)VerifyArenaResultStack(method);
        }
        foreach(var type in ScaledPreviewTypes) {
            var methods=module.GetType(type).Methods.Where(m=>m.HasBody).ToArray();
            if(!methods.SelectMany(m=>m.Body.Instructions).Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && (r.Name=="CreateTarget" || r.Name=="CreateScreenTarget")))
                throw new InvalidOperationException("Missing scoped preview target hook: "+type);
            if(methods.SelectMany(m=>m.Body.Instructions).Any(i=>i.Operand is MethodReference r && r.DeclaringType.FullName=="NShader.FilterBase" && (r.Name=="CreateRenderTarget" || r.Name=="CreateScreenRenderTarget")))
                throw new InvalidOperationException("Unpatched preview target: "+type);
        }
        foreach(var pair in VisibilityPreviewMethods) {
            var method=module.GetType(pair.Type).Methods.Single(m=>m.Name==pair.Method);
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && r.Name=="ShouldUpdatePreview")!=1)
                throw new InvalidOperationException("Missing or duplicated native preview visibility guard: "+method.FullName);
            VerifyArenaResultStack(method);
        }
        var facade=module.GetType("NGame2.NRender.RenderFacade").Methods.Single(m=>m.Name=="ChainUpdate");
        if(facade.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && r.Name=="UpdatePreviewCreature")!=1)throw new InvalidOperationException("Missing preview visual update hook.");
        foreach(var owner in new[]{"NGame2.NRender.NRenderFacade.Action","NGame2.NRender.NRenderFacade.AnimationData"})
            if(!facade.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.FullName==owner && r.Name=="ChainUpdate"))throw new InvalidOperationException("Native preview action/animation clock was removed.");
        foreach(var type in new[]{"NGame2.NUI.NRender3D.HeroPreview","NGame2.NUI.NRender3D.ItemPreview"})if(module.GetType(type).Methods.Single(m=>m.Name=="Update").Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && r.Name=="ShouldUpdatePreview"))throw new InvalidOperationException("Preview lifetime updates must remain native.");
        VerifyArenaResultStack(facade);
        if(!module.GetType("NGame2.NNodeTree.NObject.ParticleObject").Fields.Any(f=>f.Name=="_isDistanceEmit" && f.FieldType.FullName=="System.Boolean"))throw new InvalidOperationException("Unsupported distance-emitter metadata.");
        var assetRoot=module.GetType("NGameAsset.NEffect.ParticleInstance").Properties.Single(p=>p.Name=="InstanceGo");
        if(assetRoot.GetMethod==null || assetRoot.GetMethod.IsStatic || assetRoot.PropertyType.FullName!="UnityEngine.GameObject")throw new InvalidOperationException("Unsupported asset-particle root metadata.");
        foreach(var pair in new[]{("Open","ObserveParticles"),("FinalizeParticle","ReleaseParticles"),("OnReleaseDerived","ReleaseParticles")}) {
            var method=module.GetType("NGame2.NNodeTree.NObject.ParticleObject").Methods.Single(m=>m.Name==pair.Item1);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && r.Name==pair.Item2))throw new InvalidOperationException("Missing particle lifecycle hook: "+pair.Item1);
            VerifyArenaResultStack(method);
        }
        foreach(var pair in new[]{("Open","ObserveAssetParticles"),("Close","ReleaseAssetParticles"),("Release","ReleaseAssetParticles")}) {
            var method=module.GetType("NGameAsset.NEffect.ParticleInstance").Methods.Single(m=>m.Name==pair.Item1);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && r.Name==pair.Item2))throw new InvalidOperationException("Missing asset-particle lifecycle hook: "+pair.Item1);
            VerifyArenaResultStack(method);
        }
    }
    static readonly string[] ScaledPreviewTypes={"NGame2.NUI.NRender3D.HeroPreview","NGame2.NUI.NRender3D.ItemPreview","NGame2.NUI.NRender3D.RenderTargetCinema","NGame2.NUI.NRender3D.NCinema.Cinema"};
    static readonly (string Type,string Method)[] VisibilityPreviewMethods={
        ("NGame2.NUI.NRender3D.HeroPreview","LateUpdate"),("NGame2.NUI.NRender3D.RenderTargetCinema","LateUpdateDerived"),
        ("NGame2.NUI.NRender3D.NCinema.Cinema","LateUpdateDerived")};
    static void InstallGraphicsPerformanceHooks(ModuleDefinition module,TypeDefinition bridge) {
        // Repair staging builds from before visibility was narrowed to visual work.
        foreach(var type in new[]{"NGame2.NUI.NRender3D.HeroPreview","NGame2.NUI.NRender3D.ItemPreview"}) {
            var update=module.GetType(type).Methods.Single(m=>m.Name=="Update");var instructions=update.Body.Instructions;
            if(instructions.Count>4 && instructions[0].OpCode==OpCodes.Ldarg_0 && instructions[1].Operand is MethodReference previous && previous.DeclaringType.Name==bridge.Name && previous.Name=="ShouldUpdatePreview" && instructions[2].OpCode==OpCodes.Brtrue && instructions[3].OpCode==OpCodes.Ret)
                for(int i=0;i<4;i++)instructions.RemoveAt(0);
        }
        var facade=module.GetType("NGame2.NRender.RenderFacade").Methods.Single(m=>m.Name=="ChainUpdate");
        var visualCall=facade.Body.Instructions.SingleOrDefault(i=>i.Operand is MethodReference r && r.DeclaringType.FullName=="NGameAsset.ICreature" && r.Name=="ChainUpdate");
        if(visualCall!=null) {
            facade.Body.GetILProcessor().InsertBefore(visualCall,Instruction.Create(OpCodes.Ldarg_0));
            visualCall.OpCode=OpCodes.Call;visualCall.Operand=module.ImportReference(bridge.Methods.Single(m=>m.Name=="UpdatePreviewCreature"));
        }
        VerifyArenaResultStack(facade);
        foreach(var pair in VisibilityPreviewMethods) {
            var method=module.GetType(pair.Type).Methods.Single(m=>m.Name==pair.Method);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name==bridge.Name && r.Name=="ShouldUpdatePreview"))continue;
            var il=method.Body.GetILProcessor();var first=method.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="ShouldUpdatePreview"))));
            il.InsertBefore(first,il.Create(OpCodes.Brtrue,first));il.InsertBefore(first,il.Create(OpCodes.Ret));
            VerifyArenaResultStack(method);
        }
        foreach(var pair in new[]{("Open","ObserveParticles"),("FinalizeParticle","ReleaseParticles"),("OnReleaseDerived","ReleaseParticles")}) {
            var method=module.GetType("NGame2.NNodeTree.NObject.ParticleObject").Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name==bridge.Name && r.Name==pair.Item2))continue;
            var il=method.Body.GetILProcessor();var call=module.ImportReference(bridge.Methods.Single(m=>m.Name==pair.Item2));
            if(pair.Item1=="Open") {
                foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                    ret.OpCode=OpCodes.Ldarg_0;var hook=il.Create(OpCodes.Call,call);il.InsertAfter(ret,hook);il.InsertAfter(hook,il.Create(OpCodes.Ret));
                }
            } else {var first=method.Body.Instructions[0];il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));il.InsertBefore(first,il.Create(OpCodes.Call,call));}
            VerifyArenaResultStack(method);
        }
        foreach(var pair in new[]{("Open","ObserveAssetParticles"),("Close","ReleaseAssetParticles"),("Release","ReleaseAssetParticles")}) {
            var method=module.GetType("NGameAsset.NEffect.ParticleInstance").Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name==bridge.Name && r.Name==pair.Item2))continue;
            var il=method.Body.GetILProcessor();var call=module.ImportReference(bridge.Methods.Single(m=>m.Name==pair.Item2));
            if(pair.Item1=="Open") {
                foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                    // Forward the original success value through the helper.
                    ret.OpCode=OpCodes.Ldarg_0;var hook=il.Create(OpCodes.Call,call);il.InsertAfter(ret,hook);il.InsertAfter(hook,il.Create(OpCodes.Ret));
                }
            } else {var first=method.Body.Instructions[0];il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));il.InsertBefore(first,il.Create(OpCodes.Call,call));}
            VerifyArenaResultStack(method);
        }
    }
    static void VerifyGraphicsOptionsFile(string root,bool staged=false) {
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".graphics-options-staged":"")));
        VerifyGraphicsOptions(game.MainModule);
        using var post=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Unity.Postprocessing.Runtime.dll"));
        var modes=post.MainModule.GetType("UnityEngine.Rendering.PostProcessing.PostProcessLayer").NestedTypes.Single(t=>t.Name=="Antialiasing");
        foreach(var pair in new[]{("None",0),("FastApproximateAntialiasing",1),("SubpixelMorphologicalAntialiasing",2),("TemporalAntialiasing",3)})
            if(Convert.ToInt32(modes.Fields.Single(f=>f.Name==pair.Item1).Constant)!=pair.Item2)throw new InvalidOperationException("Unexpected native AA enum mapping.");
        var releaseHistory=post.MainModule.GetType("UnityEngine.Rendering.PostProcessing.TemporalAntialiasing").Methods.Single(m=>m.Name=="Release");
        if(releaseHistory.IsStatic || releaseHistory.Parameters.Count!=0 || releaseHistory.ReturnType.FullName!="System.Void")throw new InvalidOperationException("Unsupported native TAA history release.");
        using var automation=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","SprkAutomation.dll"+(staged?".staged":"")));
        var preferences=automation.MainModule.GetType("SprkTestPrefs");
        foreach(var name in new[]{"ClearGraphicsCache","get_GraphicsStorageReads","Invalidate"})if(!preferences.Methods.Any(m=>m.Name==name))throw new InvalidOperationException("Missing graphics preference cache integration: "+name);
        foreach(var name in new[]{"SetInt","SetString","SetFloat","DeleteKey"})if(!preferences.Methods.Single(m=>m.Name==name).Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="Invalidate"))throw new InvalidOperationException("Graphics cache setter does not invalidate: "+name);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","SprkGraphicsOptions.dll"+(staged?".staged":"")));
        var bridge=helper.MainModule.GetType("SprkGraphicsOptions");
        var fullscreenGuard=bridge.Methods.Single(m=>m.Name=="ApplyCustomResolution");
        var windowCheck=fullscreenGuard.Body.Instructions.FirstOrDefault(i=>i.Operand is MethodReference m && m.Name=="get_Graphic_WindowControl");
        var customCheck=fullscreenGuard.Body.Instructions.FirstOrDefault(i=>i.Operand is MethodReference m && m.Name=="get_CustomWindow");
        if(windowCheck==null || customCheck==null || windowCheck.Offset>=customCheck.Offset)throw new InvalidOperationException("Fullscreen must bypass fixed window resolution presets.");
        var custom=bridge.Methods.Single(m=>m.Name=="UpdateCustomWindow");
        foreach(var name in new[]{"FindGameWindow","GetWindowLong","SetWindowLong","SetWindowPos","SaveCustomSize"})
            if(!custom.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Missing custom resize integration: "+name);
        var setup=bridge.Methods.Single(m=>m.Name=="Setup");
        if(setup.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.Name=="Add" && m.DeclaringType.Name=="SprkGraphicsOptions")!=10)
            throw new InvalidOperationException("Graphics settings must expose AA, adjustments, world/preview scale, particle density and camera sensitivities.");
        foreach(var required in new[]{"Off","FXAA","SMAA","MSAA","TAA","Preset","Scale","FXAAQuality","SMAAQuality","MSAASamples","TAASharpness","LobbyCameraSensitivity","ModelCameraSensitivity","Anti-Aliasing","FXAA Quality","SMAA Quality","MSAA Samples","TAA Sharpness","Render Scale","Preview Scale","PreviewScale","Particle Density","ParticleDensity","Lobby Camera Sensitivity","Model Camera Sensitivity"})
            if(!setup.Body.Instructions.Any(i=>Equals(i.Operand,required)))throw new InvalidOperationException("Missing graphics preset control: "+required);
        var poll=bridge.Methods.Single(m=>m.Name=="PollComparisonKeys");
        if(poll.Body.Instructions.Any(i=>i.OpCode!=OpCodes.Ret && i.OpCode!=OpCodes.Nop))
            throw new InvalidOperationException("Temporary AA comparison hook must be inert.");
        if(bridge.Methods.Any(m=>m.Name=="OnGUI" || m.Name=="PollComparisonInput" || m.Name=="ComparisonPreset"))
            throw new InvalidOperationException("Temporary AA comparison input or popup remains.");
        if(Convert.ToInt32(game.MainModule.GetType("NShared.EFrameControl").Fields.Single(f=>f.Name=="Manual_144").Constant)!=144)
            throw new InvalidOperationException("Missing native 144 FPS mode.");
        foreach(var pair in new[]{("Refresh","ConfigureFrameRow"),("OnClickToggle","IsFrameRow")}) {
            var method=game.MainModule.GetType("NGame2.NUI.NComponent2.GameOptionToggle4Component").Methods.Single(m=>m.Name==pair.Item1);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && r.Name==pair.Item2))throw new InvalidOperationException("Missing FPS row hook.");
            VerifyArenaResultStack(method);
        }
        var displayCheck=game.MainModule.GetType("NGame2.NUI.NWindow.GameOptionWindow").Methods.Single(m=>m.Name=="CheckDeviceRefreshRate");
        if(displayCheck.Body.Instructions.Count!=1 || displayCheck.Body.Instructions[0].OpCode!=OpCodes.Ret)throw new InvalidOperationException("FPS choices must remain available at every refresh rate.");
        var graphicsInstructions=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).ToArray();
        if(!graphicsInstructions.Any(i=>i.Operand is MethodReference r && r.Name=="IsModelCamera") ||
           !graphicsInstructions.Any(i=>i.Operand is MethodReference r && r.Name=="IsLobbyBackground"))
            throw new InvalidOperationException("Missing lobby camera classification.");
        if(!graphicsInstructions.Any(i=>i.Operand is FieldReference f && f.Name=="onPreRender"))throw new InvalidOperationException("World presentation must run before the main UI camera.");
        if(!graphicsInstructions.Any(i=>i.Operand is FieldReference f && f.Name=="finalBlitToCameraTarget"))throw new InvalidOperationException("World effects must output to the shared texture.");
        if(!graphicsInstructions.Any(i=>i.Operand is MethodReference r && r.Name=="AddCommandBuffer"))throw new InvalidOperationException("World presentation must execute inside the UI camera render sequence.");
        if(helper.MainModule.GetType("SprkGraphicsPresenter")!=null)throw new InvalidOperationException("Removed trailing-camera presenter remains.");
        var add=bridge.Methods.Single(m=>m.Name=="Add");
        if(!add.Body.Instructions.Any(i=>i.Operand is GenericInstanceMethod m && m.Name=="AddGridItem" && m.GenericArguments.Any(t=>t.Name=="GameOptionToggle4Component")))throw new InvalidOperationException("Graphics choices must use the native toggle row prefab.");
        foreach(var call in new[]{"GetRenderTextureSupportedMSAASampleCount","Init","ResetHistory","Blit","get_supportsMotionVectors"})
            if(!bridge.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Any(i=>i.Operand is MethodReference m && m.Name==call))throw new InvalidOperationException("Missing graphics integration: "+call);
        var late=bridge.Methods.Single(m=>m.Name=="LateUpdate");
        if(late.Body.Instructions.Any(i=>i.Operand is MethodReference r && (r.DeclaringType.FullName=="System.Linq.Enumerable" || r.Name=="get_allCameras")))throw new InvalidOperationException("Graphics frame loop allocates camera collections.");
        var collect=bridge.Methods.Single(m=>m.Name=="CollectCameras");
        if(!collect.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="GetAllCameras"))throw new InvalidOperationException("Missing reusable camera buffer.");
        if(bridge.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Any(i=>i.Operand is MethodReference m && m.DeclaringType.FullName=="UnityEngine.PlayerPrefs"))throw new InvalidOperationException("Graphics preferences bypass test isolation.");
        Console.WriteLine("Verified graphics hooks, native AA modes, runtime integration and isolated preferences.");
    }
    static void InstallGraphicsOptions(string root,bool stageOnly) {
        // Graphics preference caching is owned by the existing isolated adapter.
        InstallAutomation(root,true);
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkGraphicsOptions.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.GraphicsOptions.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(Path.GetFileName(p)=="SprkAutomation.dll"?p+".staged":p));
        var compilation=CSharpCompilation.Create("SprkGraphicsOptions",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var module=game.MainModule;
        var bridge=helper.MainModule.GetType("SprkGraphicsOptions");
        var resolution=module.GetType("NShared.EResolutionControl");
        if(!resolution.Fields.Any(f=>f.Name=="Custom"))resolution.Fields.Add(new FieldDefinition("Custom",FieldAttributes.Public|FieldAttributes.Static|FieldAttributes.Literal|FieldAttributes.HasDefault,resolution){Constant=5});
        var applyResolution=module.GetType("NVespa.SystemOptionData").Methods.Single(m=>m.Name=="ApplyResoultionSetting");
        if(!applyResolution.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions" && m.Name=="ApplyCustomResolution")) {
            var il=applyResolution.Body.GetILProcessor();var first=applyResolution.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="ApplyCustomResolution"))));
            il.InsertBefore(first,il.Create(OpCodes.Brfalse,first));il.InsertBefore(first,il.Create(OpCodes.Ret));
        }
        // Change only the missing-preference default. LocalStorageBool.Init
        // continues to honor existing saved On/Off values.
        var outlineDefault=OutlineDefaultArgument(module);outlineDefault.OpCode=OpCodes.Ldc_I4_0;outlineDefault.Operand=null;
        foreach(var pair in new[]{("NGame2.NUI.NComponent2.GameOptionToggle3Component","SetToggle","CenterTextureChoices"),("UICamera","Awake","Ensure"),("NGame2.NUI.NWindow.GameOptionWindow","SetupOptionGrids","Setup"),("NGame2.NUI.NWindow.HeroFullSceneViewBase","UpdateDerived","PollComparisonKeys")}) {
            var method=module.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions"))continue;
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=pair.Item3=="Setup"||pair.Item3=="PollComparisonKeys"||pair.Item3=="CenterTextureChoices"?OpCodes.Ldarg_0:OpCodes.Nop;ret.Operand=null;
                var call=il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name==pair.Item3)));
                il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        // Keep the full native row available on 60 Hz displays too.
        var refreshRate=module.GetType("NGame2.NUI.NWindow.GameOptionWindow").Methods.Single(m=>m.Name=="CheckDeviceRefreshRate");
        refreshRate.Body.Instructions.Clear();refreshRate.Body.Variables.Clear();refreshRate.Body.ExceptionHandlers.Clear();
        refreshRate.Body.GetILProcessor().Emit(OpCodes.Ret);
        var toggleType=module.GetType("NGame2.NUI.NComponent2.GameOptionToggle4Component");
        foreach(var name in new[]{"SetToggle","SetValueByParent"}) {
            var method=toggleType.Methods.Single(m=>m.Name==name);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions" && m.Name=="RestoreResolutionChoices"))continue;
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
                var call=il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="RestoreResolutionChoices")));
                il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
            }
        }
        // Resolution now has five managed choices. Avoid the native four-choice
        // initializer firing stale callbacks while the settings menu refreshes.
        var setToggle=module.GetType("NGame2.NUI.NComponent2.GameOptionToggle4Component").Methods.Single(m=>m.Name=="SetToggle");
        if(!setToggle.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions" && m.Name=="IsResolutionRow")) {
            var il=setToggle.Body.GetILProcessor();var first=setToggle.Body.Instructions[0];
            var restore=setToggle.Body.Instructions.Last(i=>i.OpCode==OpCodes.Ret).Previous.Previous;
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="IsResolutionRow"))));
            il.InsertBefore(first,il.Create(OpCodes.Brtrue,restore));
        }
        foreach(var pair in new[]{("Refresh","ConfigureFrameRow"),("OnClickToggle","IsFrameRow")}) {
            var method=toggleType.Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkGraphicsOptions" && r.Name==pair.Item2))continue;
            var il=method.Body.GetILProcessor();var first=method.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name==pair.Item2))));
            il.InsertBefore(first,il.Create(OpCodes.Brfalse,first));
            il.InsertBefore(first,il.Create(OpCodes.Ret));
        }
        var mainTarget=module.GetType("NVespa.GlobalCameraManager").Methods.Single(m=>m.Name=="HasMainRenderTarget");
        mainTarget.Body.Instructions.Clear();mainTarget.Body.Variables.Clear();mainTarget.Body.ExceptionHandlers.Clear();
        var mainIl=mainTarget.Body.GetILProcessor();mainIl.Emit(OpCodes.Ldarg_0);
        mainIl.Emit(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="HasMainRenderTarget")));mainIl.Emit(OpCodes.Ret);
        var factory=module.GetType("NShader.FilterBase").Methods.Single(m=>m.Name=="CreateRenderTarget");
        factory.Body.Instructions.Clear();factory.Body.Variables.Clear();factory.Body.ExceptionHandlers.Clear();
        var factoryIl=factory.Body.GetILProcessor();factoryIl.Emit(OpCodes.Ldarg_0);factoryIl.Emit(OpCodes.Ldarg_1);factoryIl.Emit(OpCodes.Ldarg_2);
        factoryIl.Emit(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="CreateNativeTarget")));factoryIl.Emit(OpCodes.Ret);
        // Scale model previews explicitly. The shared factory also serves UI
        // effects, screen filters and captures that must retain native sizing.
        foreach(var type in ScaledPreviewTypes)foreach(var method in module.GetType(type).Methods.Where(m=>m.HasBody)) {
            foreach(var instruction in method.Body.Instructions)if(instruction.Operand is MethodReference call && call.DeclaringType.FullName=="NShader.FilterBase" && (call.Name=="CreateRenderTarget" || call.Name=="CreateScreenRenderTarget"))
                instruction.Operand=module.ImportReference(bridge.Methods.Single(m=>m.Name==(call.Name=="CreateRenderTarget"?"CreateTarget":"CreateScreenTarget")));
            if(!method.Body.HasExceptionHandlers)VerifyArenaResultStack(method);
        }
        InstallGraphicsPerformanceHooks(module,bridge);
        VerifyGraphicsOptions(game.MainModule);
        var staged=dll+".graphics-options-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        // Reapplying to the emitted assembly must not duplicate hooks.
        using(var repeated=AssemblyDefinition.ReadAssembly(staged)) {
            VerifyGraphicsOptions(repeated.MainModule);
            foreach(var method in repeated.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody))
                foreach(var name in new[]{"Ensure","Setup","CreateTarget"})
                    if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions" && m.Name==name)>method.Body.Instructions.Count(i=>i.OpCode==OpCodes.Ret))throw new InvalidOperationException("Duplicated graphics hook.");
        }
        VerifyGraphicsOptionsFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-graphics-options-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged,dll,true);File.Move(Path.Combine(managed,"SprkAutomation.dll.staged"),Path.Combine(managed,"SprkAutomation.dll"),true);File.Move(helperPath+".staged",helperPath,true);
        VerifyGraphicsOptionsFile(root);
    }
    static bool UsesClientFiles(System.Diagnostics.Process process,string root) {
        if(process.ProcessName!="King's Raid" && !process.ProcessName.StartsWith("SprkTest-"))return false;
        try {
            var directory=Path.GetDirectoryName(process.MainModule!.FileName)!;
            if(Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory)).Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)),StringComparison.OrdinalIgnoreCase))return true;
            foreach(var data in Directory.GetDirectories(directory,"*_Data")) {
                var resolvedData=new DirectoryInfo(data).ResolveLinkTarget(true)?.FullName??data;
                var managed=new DirectoryInfo(Path.Combine(resolvedData,"Managed"));
                if(!managed.Exists)continue;
                var resolved=managed.ResolveLinkTarget(true)?.FullName??managed.FullName;
                if(Path.GetFullPath(resolved).Equals(Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed")),StringComparison.OrdinalIgnoreCase))return true;
            }
            return false;
        } catch { return true; } // If ownership cannot be checked, retain the installation guard.
    }
}
