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
        foreach(var pair in new[]{("NGame2.NUI.NComponent2.GameOptionToggle3Component","SetToggle","CenterTextureChoices"),("UICamera","Awake","Ensure"),("NGame2.NUI.NWindow.GameOptionWindow","SetupOptionGrids","Setup"),("NShader.FilterBase","CreateRenderTarget","CreateTarget"),("NVespa.GlobalCameraManager","HasMainRenderTarget","HasMainRenderTarget"),("NGame2.NUI.NWindow.HeroFullSceneViewBase","UpdateDerived","PollComparisonKeys")}) {
            var method=module.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGraphicsOptions" && m.Name==pair.Item3))throw new InvalidOperationException("Missing graphics hook: "+pair.Item3);
            if(!method.Body.HasExceptionHandlers)VerifyArenaResultStack(method);
        }
    }
    static void VerifyGraphicsOptionsFile(string root,bool staged=false) {
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".graphics-options-staged":"")));
        VerifyGraphicsOptions(game.MainModule);
        using var post=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Unity.Postprocessing.Runtime.dll"));
        var modes=post.MainModule.GetType("UnityEngine.Rendering.PostProcessing.PostProcessLayer").NestedTypes.Single(t=>t.Name=="Antialiasing");
        foreach(var pair in new[]{("None",0),("FastApproximateAntialiasing",1),("SubpixelMorphologicalAntialiasing",2),("TemporalAntialiasing",3)})
            if(Convert.ToInt32(modes.Fields.Single(f=>f.Name==pair.Item1).Constant)!=pair.Item2)throw new InvalidOperationException("Unexpected native AA enum mapping.");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","SprkGraphicsOptions.dll"+(staged?".staged":"")));
        var bridge=helper.MainModule.GetType("SprkGraphicsOptions");
        var custom=bridge.Methods.Single(m=>m.Name=="UpdateCustomWindow");
        foreach(var name in new[]{"FindGameWindow","GetWindowLong","SetWindowLong","SetWindowPos","SaveCustomSize"})
            if(!custom.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Missing custom resize integration: "+name);
        var setup=bridge.Methods.Single(m=>m.Name=="Setup");
        if(setup.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.Name=="Add" && m.DeclaringType.Name=="SprkGraphicsOptions")!=8)
            throw new InvalidOperationException("Graphics settings must expose method, four conditional adjustments, render scale and two independent camera sensitivities.");
        foreach(var required in new[]{"Off","FXAA","SMAA","MSAA","TAA","Preset","Scale","FXAAQuality","SMAAQuality","MSAASamples","TAASharpness","LobbyCameraSensitivity","ModelCameraSensitivity","Anti-Aliasing","FXAA Quality","SMAA Quality","MSAA Samples","TAA Sharpness","Render Scale","Lobby Camera Sensitivity","Model Camera Sensitivity"})
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
        if(bridge.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Any(i=>i.Operand is MethodReference m && m.DeclaringType.FullName=="UnityEngine.PlayerPrefs"))throw new InvalidOperationException("Graphics preferences bypass test isolation.");
        Console.WriteLine("Verified graphics hooks, native AA modes, runtime integration and isolated preferences.");
    }
    static void InstallGraphicsOptions(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkGraphicsOptions.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.GraphicsOptions.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
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
        factoryIl.Emit(OpCodes.Call,module.ImportReference(bridge.Methods.Single(m=>m.Name=="CreateTarget")));factoryIl.Emit(OpCodes.Ret);
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
        if(System.Diagnostics.Process.GetProcesses().Any(p=>p.ProcessName=="King's Raid" || p.ProcessName.StartsWith("SprkTest-")))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-graphics-options-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyGraphicsOptionsFile(root);
    }
}
