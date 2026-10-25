using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void VerifyPetPresentation(ModuleDefinition module) {
        foreach(var pair in new[]{("NShared.PortalRenewalData","get_SubComponentShape","Shape"),("NGame2.NUI.NWindow.MyInfoWindow","SetupMyInfo","Setup")}) {
            var method=module.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2);
            var calls=method.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkPetPresentation" && m.Name==pair.Item3).ToArray();
            if(calls.Length!=1 || calls[0].Previous.OpCode!=OpCodes.Ldarg_0 || calls[0].Next.OpCode!=OpCodes.Ret)
                throw new InvalidOperationException("Pet presentation hook missing or duplicated: "+pair.Item2);
        }
    }
    static void VerifyPetPresentationFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".pet-presentation-staged":"")));
        VerifyPetPresentation(game.MainModule);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkPetPresentation.dll"+(staged?".staged":"")));
        var setup=helper.MainModule.GetType("SprkPetPresentation").Methods.Single(m=>m.Name=="Setup");
        foreach(var required in new[]{"SetActive","Set","OpenPopup"})
            if(!setup.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==required))
                throw new InvalidOperationException("Missing native pet stats control: "+required);
        if(!setup.Body.Instructions.Any(i=>Equals(i.Operand,"Panel/Root/CategoryRoot/Button_Buff")))
            throw new InvalidOperationException("Original My Info buff tab is not targeted.");
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
        Console.WriteLine("Verified Pet House Window layout and original My Info buff button with native stat groups.");
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
        VerifyPetPresentationFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-pet-presentation-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyPetPresentationFile(root);
    }
}
