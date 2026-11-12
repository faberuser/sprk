using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    const string SoulUpgradeType="NGame2.NUI.NWindow.NSoulWeapon.SoulWeaponUpgradeResultWindow";
    static void PatchSoulUpgradePlayback(ModuleDefinition game,ModuleDefinition helper) {
        var type=game.GetType(SoulUpgradeType);
        var hooks=helper.GetType("SprkSoulUpgradePlayback");
        MethodReference Hook(string name)=>game.ImportReference(hooks.Methods.Single(m=>m.Name==name));
        bool Has(MethodDefinition m,string name)=>m.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkSoulUpgradePlayback" && r.Name==name);
        var setup=type.Methods.Single(m=>m.Name=="Setup");
        if(!Has(setup,"Begin")) {var il=setup.Body.GetILProcessor();var first=setup.Body.Instructions[0];il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));il.InsertBefore(first,il.Create(OpCodes.Call,Hook("Begin")));}
        var factory=type.Methods.Single(m=>m.Name=="CoroutineEnableUpgradeInfo");
        if(!Has(factory,"Guard")) {
            var il=factory.Body.GetILProcessor();var ret=factory.Body.Instructions.Single(i=>i.OpCode==OpCodes.Ret);
            il.InsertBefore(ret,il.Create(OpCodes.Ldarg_0));il.InsertBefore(ret,il.Create(OpCodes.Call,Hook("Guard")));
        }
        var state=type.NestedTypes.Single(t=>t.Name.Contains("<CoroutineEnableUpgradeInfo>"));
        var move=state.Methods.Single(m=>m.Name=="MoveNext");
        var engine=game.AssemblyResolver.Resolve(game.AssemblyReferences.Single(r=>r.Name=="UnityEngine.CoreModule")).MainModule;
        var realCtor=engine.GetType("UnityEngine.WaitForSecondsRealtime").Methods.Single(m=>m.IsConstructor && m.Parameters.Count==1);
        foreach(var i in move.Body.Instructions.Where(i=>i.OpCode==OpCodes.Newobj && i.Operand is MethodReference r && r.DeclaringType.FullName=="UnityEngine.WaitForSeconds"))i.Operand=game.ImportReference(realCtor);
        var close=type.Methods.Single(m=>m.Name=="Close");
        if(!Has(close,"End")) {
            var call=close.Body.Instructions.Single(i=>i.Operand is MethodReference r && r.Name=="Close" && r.DeclaringType.Name=="BaseWindow");
            var il=close.Body.GetILProcessor();il.InsertBefore(call,il.Create(OpCodes.Ldarg_0));il.InsertBefore(call,il.Create(OpCodes.Call,Hook("End")));
        }
        var cinema=game.GetType("NGame2.NUI.NRender3D.RenderTargetCinema");
        foreach(var method in cinema.Methods.Where(m=>m.HasBody)) {
            foreach(var load in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ldsfld && i.Operand is FieldReference f && f.DeclaringType.Name=="GlobalTimer" && f.Name=="DefaultLocal").ToArray()) {
                if(load.Next.OpCode==OpCodes.Ldarg_0 && load.Next.Next.Operand is MethodReference r && r.DeclaringType.Name=="SprkSoulUpgradePlayback" && r.Name=="Timer")continue;
                var il=method.Body.GetILProcessor();var receiver=il.Create(OpCodes.Ldarg_0);il.InsertAfter(load,receiver);il.InsertAfter(receiver,il.Create(OpCodes.Call,Hook("Timer")));
            }
        }
        VerifySoulUpgradePlayback(game);
    }
    static void VerifySoulUpgradePlayback(ModuleDefinition game) {
        var type=game.GetType(SoulUpgradeType);
        foreach(var pair in new[]{("Setup","Begin"),("CoroutineEnableUpgradeInfo","Guard"),("Close","End")}) {
            var method=type.Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkSoulUpgradePlayback" && r.Name==pair.Item2)!=1)throw new Exception("Missing/duplicated advancement hook: "+pair);
            VerifyArenaResultStack(method);
        }
        var move=type.NestedTypes.Single(t=>t.Name.Contains("<CoroutineEnableUpgradeInfo>")).Methods.Single(m=>m.Name=="MoveNext");
        if(move.Body.Instructions.Count(i=>i.OpCode==OpCodes.Newobj && i.Operand is MethodReference r && r.DeclaringType.Name=="WaitForSecondsRealtime")!=2 || move.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="WaitForSeconds"))throw new Exception("Advancement result still uses scaled waits");
        foreach(float seconds in new[]{4.5f,1f})if(!move.Body.Instructions.Any(i=>i.OpCode==OpCodes.Ldc_R4 && Equals(i.Operand,seconds)))throw new Exception("Original result duration changed");
        var cinema=game.GetType("NGame2.NUI.NRender3D.RenderTargetCinema");
        var timerCalls=cinema.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Count(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="SprkSoulUpgradePlayback" && r.Name=="Timer");
        if(timerCalls!=2)throw new Exception("Expected two scoped cinematic timer hooks, got "+timerCalls);
        var setup=type.Methods.Single(m=>m.Name=="Setup");
        foreach(var name in new[]{"SetSoulWeaponNodeTree","AddTouchDisabler"})if(!setup.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name==name))throw new Exception("Native result setup removed: "+name);
        var close=type.Methods.Single(m=>m.Name=="Close");
        if(!close.Body.Instructions.Any(i=>i.OpCode==OpCodes.Ldfld && i.Operand is FieldReference f && f.Name=="_isWaitingResult") || !close.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.Name=="NodeTreeUIPlayer" && r.Name=="Close"))throw new Exception("Native result close/cleanup removed");
    }
    static void VerifySoulUpgradePlaybackFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".soul-upgrade-playback-staged":"")));
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkSoulUpgradePlayback.dll"+(staged?".staged":"")));
        VerifySoulUpgradePlayback(game.MainModule);
        var calls=helper.MainModule.Types.SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"Close","set_updateMode","set_speed","RemoveTouchDisabler","Guard","IsUpgrade"})if(!calls.Any(r=>r.Name==name))throw new Exception("Incomplete advancement safeguard "+name);
        if(calls.Any(r=>r.Name=="set_timeScale"))throw new Exception("Advancement patch must not change global time scale");
        Console.WriteLine("Verified real-time advancement waits, scoped cinematic clock, animation restoration, lifecycle guard and native cleanup.");
    }
    static void TestSoulUpgradePlayback(string root,bool staged=false) {
        var path=Path.Combine(root,"King's Raid_Data","Managed","SprkSoulUpgradePlayback.dll"+(staged?".staged":""));
        var asm=System.Reflection.Assembly.Load(File.ReadAllBytes(path));var t=asm.GetType("SprkSoulUpgradeSession")!;
        bool Scope(string? pack,string? tree)=>(bool)t.GetMethod("IsUpgrade")!.Invoke(null,new object?[]{pack,tree})!;
        if(!Scope("Common/Cinema","SoulWeapon_RankUP_1") || !Scope("Common/Cinema","SoulWeapon_RankUP_2") || Scope("Other","SoulWeapon_RankUP_1") || Scope("Common/Cinema","SoulWeapon_Upgrade_0") || Scope(null,null))throw new Exception("Cinematic scope test failed");
        var s=Activator.CreateInstance(t)!;
        void Begin()=>t.GetMethod("Begin")!.Invoke(s,null);void End()=>t.GetMethod("End")!.Invoke(s,null);
        int Generation()=>(int)t.GetProperty("Generation")!.GetValue(s)!;
        System.Collections.IEnumerator Wrap(System.Collections.IEnumerator source,int token,Func<bool> active)=>(System.Collections.IEnumerator)t.GetMethod("Guard")!.Invoke(s,new object[]{source,token,active})!;
        int callbacks=0,disposed=0;
        System.Collections.IEnumerator Routine(){try{yield return null;callbacks++;yield return null;callbacks++;}finally{disposed++;}}
        Begin();var old=Wrap(Routine(),Generation(),()=>true);if(!old.MoveNext())throw new Exception("Routine did not start");
        End();Begin();if(old.MoveNext() || callbacks!=0 || disposed!=1)throw new Exception("Stale result callback executed after close/reopen");
        bool visible=true;var hidden=Wrap(Routine(),Generation(),()=>visible);hidden.MoveNext();visible=false;if(hidden.MoveNext()||callbacks!=0||disposed!=2)throw new Exception("Hidden-window callback executed");
        var current=Wrap(Routine(),Generation(),()=>true);while(current.MoveNext()){}if(callbacks!=2||disposed!=3)throw new Exception("Current result callbacks lost or repeated");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(Path.Combine(root,"King's Raid_Data","Managed"));
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".soul-upgrade-playback-staged":"")),new ReaderParameters{AssemblyResolver=resolver});
        using var helper=AssemblyDefinition.ReadAssembly(path);
        int before=game.MainModule.GetTypes().SelectMany(x=>x.Methods).Where(x=>x.HasBody).Sum(x=>x.Body.Instructions.Count);
        PatchSoulUpgradePlayback(game.MainModule,helper.MainModule);
        if(before!=game.MainModule.GetTypes().SelectMany(x=>x.Methods).Where(x=>x.HasBody).Sum(x=>x.Body.Instructions.Count))throw new Exception("Advancement patch is not idempotent");
        Console.WriteLine("PASS: A1/A2-only clock, unrelated cinema fallback, close/reopen cancellation, hidden window, single completion, disposal and idempotence.");
    }
    static void InstallSoulUpgradePlayback(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkSoulUpgradePlayback.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.SoulUpgradePlayback.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkSoulUpgradePlayback",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        PatchSoulUpgradePlayback(game.MainModule,helper.MainModule);
        var staged=dll+".soul-upgrade-playback-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifySoulUpgradePlaybackFile(root,true);TestSoulUpgradePlayback(root,true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        var backup=dll+".before-soul-upgrade-playback-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        File.Copy(dll,backup);
        if (File.Exists(helperPath)) File.Copy(helperPath,helperPath+".before-soul-upgrade-playback-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifySoulUpgradePlaybackFile(root);
        Console.WriteLine("Installed Soul Weapon advancement playback safeguards. Restart the client.");
    }
}
