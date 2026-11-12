using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static MethodDefinition GearPortraitUpdate(ModuleDefinition module) => module.GetType("NGame2.NUI.NComponent2.EquipStorageSlotComponent")
        .Methods.Single(m=>m.Name=="UpdatePortraitTexture" && m.Parameters.Count==1);
    static void PatchGearPresetPortraits(ModuleDefinition game, ModuleDefinition helper) {
        var method=GearPortraitUpdate(game);
        if (!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkGearPresetPortraits")) {
            if (!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="IsEquippedItemExist"))
                throw new InvalidOperationException("Unexpected native gear portrait method; refusing to replace it.");
            method.Body=new MethodBody(method);
            var il=method.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Call,game.ImportReference(helper.GetType("SprkGearPresetPortraits").Methods.Single(m=>m.Name=="Update"))));
            il.Append(il.Create(OpCodes.Ret));
        }
        VerifyGearPresetPortraits(game);
    }
    static void VerifyGearPresetPortraits(ModuleDefinition game) {
        var method=GearPortraitUpdate(game);
        var instructions=method.Body.Instructions;
        if(instructions.Count!=4 || instructions[0].OpCode!=OpCodes.Ldarg_0 || instructions[1].OpCode!=OpCodes.Ldarg_1 ||
            instructions[2].OpCode!=OpCodes.Call || instructions[2].Operand is not MethodReference call ||
            call.DeclaringType.Name!="SprkGearPresetPortraits" || call.Name!="Update" || instructions[3].OpCode!=OpCodes.Ret)
            throw new InvalidOperationException("Gear slot portrait helper is not installed correctly.");
        VerifyArenaResultStack(method);
    }
    static void TestGearPresetPortraits() {
        var trees=new[]{"GearPresetPortraits","GearPresetPortraitsTests"}.Select(name=>CSharpSyntaxTree.ParseText(SoloRetrySource(name)));
        var refs=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("GearPresetPortraitsTests",trees,refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var bytes=new MemoryStream();var result=compilation.Emit(bytes);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        System.Reflection.Assembly.Load(bytes.ToArray()).GetType("GearPresetPortraitsTests")!.GetMethod("Run")!.Invoke(null,null);
        Console.WriteLine("PASS: gear preset portraits for other heroes, selected-hero priority, split sets, empty/deleted/chest gear and owner changes.");
    }
    static void InstallGearPresetPortraits(string root,bool verifyOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,"SprkGearPresetPortraits.dll");
        TestGearPresetPortraits();
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using (var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true})) {
            if(verifyOnly){VerifyGearPresetPortraits(game.MainModule);return;}
            var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
            var compilation=CSharpCompilation.Create("SprkGearPresetPortraits",new[]{CSharpSyntaxTree.ParseText(SoloRetrySource("GearPresetPortraits"))},refs,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
            using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
            if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
            compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
            PatchGearPresetPortraits(game.MainModule,helper.MainModule);PatchGearPresetPortraits(game.MainModule,helper.MainModule);
            game.Write(dll+".gear-preset-portraits-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        }
        resolver.Dispose();
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Close the client before installing gear preset portraits.");
        var backup=Path.Combine("target","gear-preset-portraits-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));
        File.Move(helperPath+".staged",helperPath,true);File.Move(dll+".gear-preset-portraits-staged",dll,true);
        using var check=AssemblyDefinition.ReadAssembly(dll);VerifyGearPresetPortraits(check.MainModule);
        Console.WriteLine("Installed and verified current-wearer portraits on all Gear Custom slots.");
    }
}
