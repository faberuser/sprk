using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static MethodDefinition PresetRemoval(ModuleDefinition module) => module.GetType("NGame2.NAccount.UserEquipItemManager")
        .Methods.Single(m=>m.Name=="Remove" && m.Parameters.Count==1 && m.Parameters[0].ParameterType.MetadataType==MetadataType.Int32);
    static void PatchPresetDisposal(ModuleDefinition game, ModuleDefinition helper) {
        var method=PresetRemoval(game);
        if (!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkPresetDisposal")) {
            var il=method.Body.GetILProcessor();
            var end=method.Body.Instructions.Single(i=>i.OpCode==OpCodes.Ret);
            il.InsertBefore(end,il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(end,il.Create(OpCodes.Call,game.ImportReference(helper.GetType("SprkPresetDisposal").Methods.Single(m=>m.Name=="AfterRemove"))));
        }
        VerifyPresetDisposal(game);
    }
    static void VerifyPresetDisposal(ModuleDefinition game) {
        var method=PresetRemoval(game);
        var call=method.Body.Instructions.Single(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkPresetDisposal" && m.Name=="AfterRemove");
        if (call.Next?.OpCode!=OpCodes.Ret || call.Previous?.OpCode!=OpCodes.Ldarg_1 ||
            call.Previous.Previous?.Operand is not MethodReference remove || remove.Name!="Remove" || !remove.DeclaringType.Name.StartsWith("AccountAttrManager"))
            throw new InvalidOperationException("Preset cleanup must follow successful native equipment removal.");
        VerifyArenaResultStack(method);
    }
    static void TestPresetDisposal() {
        var trees=new[]{"PresetDisposal","PresetDisposalTests"}.Select(name=>CSharpSyntaxTree.ParseText(SoloRetrySource(name)));
        var refs=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("PresetDisposalTests",trees,refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var bytes=new MemoryStream();var result=compilation.Emit(bytes);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        System.Reflection.Assembly.Load(bytes.ToArray()).GetType("PresetDisposalTests")!.GetMethod("Run")!.Invoke(null,null);
        Console.WriteLine("PASS: successful-removal-only preset cleanup, UID/rune pages, all matching references, observers, repeat and login guards.");
    }
    static void InstallPresetDisposal(string root,bool verifyOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkPresetDisposal.dll");
        TestPresetDisposal();
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using (var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true})) {
            if(verifyOnly){VerifyPresetDisposal(game.MainModule);return;}
            var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
            var compilation=CSharpCompilation.Create("SprkPresetDisposal",new[]{CSharpSyntaxTree.ParseText(SoloRetrySource("PresetDisposal"))},refs,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
            using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
            if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
            compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
            PatchPresetDisposal(game.MainModule,helper.MainModule);PatchPresetDisposal(game.MainModule,helper.MainModule);
            game.Write(dll+".preset-disposal-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        }
        resolver.Dispose();
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Close the client before installing preset disposal.");
        var backup=Path.Combine("target","preset-disposal-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));
        File.Move(helperPath+".staged",helperPath,true);File.Move(dll+".preset-disposal-staged",dll,true);
        using var check=AssemblyDefinition.ReadAssembly(dll);VerifyPresetDisposal(check.MainModule);
        Console.WriteLine("Installed and verified immediate client preset cleanup after equipment removal.");
    }
}
