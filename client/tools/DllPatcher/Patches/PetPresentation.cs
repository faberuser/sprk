using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void RestoreTeamBuffPrefabs(string root,bool staged=false,bool install=false) {
        var data=Path.GetFullPath(Path.Combine(root,"King's Raid_Data"));
        var stage=Path.GetFullPath(Path.Combine("target/team-buff-prefabs",Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(stage);
        var script=Path.Combine(stage,"restore_team_buff_prefabs.py");
        using(var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.restore_team_buff_prefabs.py")!)
        using(var output=File.Create(script))resource.CopyTo(output);
        var asset=Path.Combine(data,"resources.assets");
        var start=new System.Diagnostics.ProcessStartInfo(Environment.GetEnvironmentVariable("SPRK_PYTHON") ?? (OperatingSystem.IsWindows()?"python":"python3")) {UseShellExecute=false,CreateNoWindow=true};
        foreach(var arg in new[]{script,asset+(staged?".pet-presentation-staged":""),Path.Combine(data,"globalgamemanagers.assets")})start.ArgumentList.Add(arg);
        if(install)start.ArgumentList.Add(asset+".pet-presentation-staged");
        using var process=System.Diagnostics.Process.Start(start)!;
        process.WaitForExit();
        if(process.ExitCode!=0)throw new InvalidOperationException("Native Team Level buff prefab verification failed.");
    }
    static void VerifyPetPresentation(ModuleDefinition module) {
        var contents=module.GetType("NShared.PortalRenewalDataContainer").Methods.Single(m=>m.Name=="GetData");
        var routes=contents.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkPetPresentation" && m.Name=="Contents").ToArray();
        if(routes.Length!=1 || routes[0].Previous.OpCode!=OpCodes.Ldarg_1 || routes[0].Next.OpCode!=OpCodes.Ret)
            throw new InvalidOperationException("Missing or duplicated native Pet Portal destination repair.");
        VerifyArenaResultStack(contents);
        foreach(var pair in new[]{("NShared.PortalRenewalData","get_SubComponentShape","Shape"),("NGame2.NUI.NWindow.MyInfoWindow","SetupMyInfo","Setup")}) {
            var method=module.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2);
            var calls=method.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkPetPresentation" && m.Name==pair.Item3).ToArray();
            if(calls.Length!=1 || calls[0].Previous.OpCode!=OpCodes.Ldarg_0 || calls[0].Next.OpCode!=OpCodes.Ret)
                throw new InvalidOperationException("Pet presentation hook missing or duplicated: "+pair.Item2);
        }
    }
    static void VerifyPetPresentationFile(string root,bool staged=false) {
        RestoreTeamBuffPrefabs(root,staged);
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".pet-presentation-staged":"")));
        VerifyPetPresentation(game.MainModule);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkPetPresentation.dll"+(staged?".staged":"")));
        var routes=helper.MainModule.GetType("SprkPetPresentation").Methods.Single(m=>m.Name=="Contents");
        if(routes.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.Name=="PetDestination")!=4)
            throw new InvalidOperationException("Pet Portal must provide its menu, artwork and both button destinations.");
        var setup=helper.MainModule.GetType("SprkPetPresentation").Methods.Single(m=>m.Name=="Setup");
        foreach(var required in new[]{"SetActive","Set"})
            if(!setup.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==required))
                throw new InvalidOperationException("Missing native pet stats control: "+required);
        if(!setup.Body.Instructions.Any(i=>Equals(i.Operand,"Panel/Root/CategoryRoot/Button_Buff")))
            throw new InvalidOperationException("Original My Info buff tab is not targeted.");
        var choose=helper.MainModule.GetType("SprkPetPresentation").Methods.Single(m=>m.Name=="ShowBuffViews");
        foreach(var label in new[]{"Pet Buffs","Team Level Buffs"})
            if(!choose.Body.Instructions.Any(i=>Equals(i.Operand,label)))
                throw new InvalidOperationException("My Info buff source missing: "+label);
        if(!choose.Body.Instructions.Any(i=>i.Operand is MethodReference {Name:"Open",DeclaringType.Name:"MessagePopup3Button"}) ||
           !choose.Body.Instructions.Any(i=>i.Operand is MethodReference {Name:"OpenPopup",DeclaringType.Name:"PetTotalBuffPopup"}))
            throw new InvalidOperationException("My Info must expose both native buff sources with a cancel-safe selector.");
        var team=helper.MainModule.GetType("SprkPetPresentation").Methods.Single(m=>m.Name=="OpenTeamLevelBuffs");
        if(!team.Body.Instructions.Any(i=>i.Operand is MethodReference {Name:"OnClickTeamLevelBuff",DeclaringType.Name:"MyInfoWindow"}))
            throw new InvalidOperationException("Missing original My Info Team Level Buffs navigation.");
        var helperCalls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody)
            .SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var target in new[]{"ShowBuffViews","OpenTeamLevelBuffs"})
            if(!helperCalls.Any(m=>m.DeclaringType.Name=="SprkPetPresentation" && m.Name==target))
                throw new InvalidOperationException("My Info buff callback missing: "+target);
        if(!setup.Body.Instructions.Any(i=>Equals(i.Operand,"Panel/Root/WidgetAvatar/Sprite_TextureDeco")))
            throw new InvalidOperationException("My Info legacy portrait backdrop cleanup is missing.");
        if(!setup.Body.Instructions.Any(i=>Equals(i.Operand,"Panel/Root/Sprite_Frame")) ||
           !setup.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="CleanWindowFrame"))
            throw new InvalidOperationException("My Info outer window border cleanup is missing.");
        var awake=game.MainModule.GetType("UICamera").Methods.Single(m=>m.Name=="Awake");
        if(awake.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkPetPresentation" && m.Name=="EnsureProbe")!=1)
            throw new InvalidOperationException("Missing or duplicated My Info frame validation hook.");
        VerifyArenaResultStack(awake);
        var native=game.MainModule.GetType("NGame2.NUI.NWindow.PetTotalBuffPopup").Methods.Single(m=>m.Name=="Init");
        foreach(var name in new[]{"GetPetStatDataBaseList","GetPetContentsStatDataBaseList","GetPetBonusDataBaseList"})
            if(!native.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))
                throw new InvalidOperationException("Native pet summary missing: "+name);
        var teamPopup=game.MainModule.GetType("NGame2.NUI.NWindow.TeamLevelTotalBuffPopup").Methods.Single(m=>m.Name=="Init");
        foreach(var name in new[]{"GetTeamLevelBuffStatDataBaseList","GetTeamLevelBuffContentsStatDataBaseList","GetTeamLevelBonusDataBaseList"})
            if(!teamPopup.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))
                throw new InvalidOperationException("Native Team Level buff summary missing: "+name);
        Console.WriteLine("Verified My Info Pet and Team Level buff navigation, all native stat groups and frame preservation.");
    }
    static void InstallPetPresentation(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkPetPresentation.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.PetPresentation.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkPetPresentation",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var contents=game.MainModule.GetType("NShared.PortalRenewalDataContainer").Methods.Single(m=>m.Name=="GetData");
        if(!contents.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkPetPresentation" && m.Name=="Contents")) {
            var ret=contents.Body.Instructions.Single(i=>i.OpCode==OpCodes.Ret);
            ret.OpCode=OpCodes.Ldarg_1;ret.Operand=null;
            var il=contents.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Call,game.MainModule.ImportReference(helper.MainModule.GetType("SprkPetPresentation").Methods.Single(m=>m.Name=="Contents"))));
            il.Append(il.Create(OpCodes.Ret));
        }
        foreach(var pair in new[]{("NShared.PortalRenewalData","get_SubComponentShape","Shape"),("NGame2.NUI.NWindow.MyInfoWindow","SetupMyInfo","Setup")}) {
            var method=game.MainModule.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkPetPresentation"))continue;
            var ret=method.Body.Instructions.Single(i=>i.OpCode==OpCodes.Ret);
            var il=method.Body.GetILProcessor();
            ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
            il.Append(il.Create(OpCodes.Call,game.MainModule.ImportReference(helper.MainModule.GetType("SprkPetPresentation").Methods.Single(m=>m.Name==pair.Item3))));
            il.Append(il.Create(OpCodes.Ret));
        }
        var awake=game.MainModule.GetType("UICamera").Methods.Single(m=>m.Name=="Awake");
        if(!awake.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkPetPresentation" && m.Name=="EnsureProbe"))
            awake.Body.GetILProcessor().InsertBefore(awake.Body.Instructions[0],Instruction.Create(OpCodes.Call,game.MainModule.ImportReference(helper.MainModule.GetType("SprkPetPresentation").Methods.Single(m=>m.Name=="EnsureProbe"))));
        VerifyPetPresentation(game.MainModule);
        var staged=dll+".pet-presentation-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        RestoreTeamBuffPrefabs(root,install:true);
        VerifyPetPresentationFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-pet-presentation-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        var asset=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","resources.assets"));
        File.Copy(asset,asset+".before-pet-presentation-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(asset+".pet-presentation-staged",asset,true);
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyPetPresentationFile(root);
    }
}
