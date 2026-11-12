using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void PatchHonorBuff(ModuleDefinition module,ModuleDefinition helper) {
        var method=module.GetType("NGame2.NUI.NManager.InventoryManagement").Methods.Single(m=>m.Name=="SelectBoosterItem");
        if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkHonorBuff"))return;
        var first=method.Body.Instructions[0];var il=method.Body.GetILProcessor();
        foreach(var instruction in new[]{Instruction.Create(OpCodes.Ldarg_1),Instruction.Create(OpCodes.Ldarg_2),Instruction.Create(OpCodes.Ldarg_3),
            Instruction.Create(OpCodes.Call,module.ImportReference(helper.GetType("SprkHonorBuff").Methods.Single(m=>m.Name=="TrySelect"))),Instruction.Create(OpCodes.Brfalse,first),Instruction.Create(OpCodes.Ret)})il.InsertBefore(first,instruction);
    }
    static void VerifyHonorBuffFile(string root) {
        var managed=Path.Combine(Path.GetFullPath(root),"King's Raid_Data","Managed");
        VerifyHonorBuffFiles(Path.Combine(managed,"Assembly-CSharp.dll"),Path.Combine(managed,"SprkHonorBuff.dll"));
    }
    static void VerifyHonorBuffFiles(string assemblyPath,string helperPath) {
        using var assembly=AssemblyDefinition.ReadAssembly(assemblyPath);using var helper=AssemblyDefinition.ReadAssembly(helperPath);
        var method=assembly.MainModule.GetType("NGame2.NUI.NManager.InventoryManagement").Methods.Single(m=>m.Name=="SelectBoosterItem");
        if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkHonorBuff" && m.Name=="TrySelect")!=1)
            throw new InvalidOperationException("Missing or duplicate Honor server buff hook.");
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>();
        foreach(var call in new[]{"Request","Apply","AddItem","Open"})if(!calls.Any(m=>m.Name==call))throw new InvalidOperationException("Incomplete Honor buff helper: "+call);
        if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="get_Duration"))throw new InvalidOperationException("Ordinary booster confirmation path was removed.");
        Console.WriteLine("Verified Honor token activation hook, item receipt application and ordinary booster fallback.");
    }
    static void InstallHonorBuff(string root,bool stageOnly) {
        root=Path.GetFullPath(root);var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var input=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.HonorBuff.cs.txt")!;using var reader=new StreamReader(input);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>Path.GetFileName(p)!="SprkHonorBuff.dll").Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkHonorBuff",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release));
        using var output=new MemoryStream();var emitted=compilation.Emit(output);if(!emitted.Success)throw new InvalidOperationException(string.Join("\n",emitted.Diagnostics));
        var stage=Path.GetFullPath("target/honor-buff/staged/King's Raid_Data/Managed");Directory.CreateDirectory(stage);
        var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using(var assembly=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"),new ReaderParameters{AssemblyResolver=resolver}))
        using(var helper=ModuleDefinition.ReadModule(new MemoryStream(output.ToArray()))) {
            PatchHonorBuff(assembly.MainModule,helper);
            var method=assembly.MainModule.GetType("NGame2.NUI.NManager.InventoryManagement").Methods.Single(m=>m.Name=="SelectBoosterItem");var count=method.Body.Instructions.Count;
            PatchHonorBuff(assembly.MainModule,helper);if(count!=method.Body.Instructions.Count)throw new InvalidOperationException("Honor buff patch not idempotent.");
            assembly.Write(Path.Combine(stage,"Assembly-CSharp.dll"));File.WriteAllBytes(Path.Combine(stage,"SprkHonorBuff.dll"),output.ToArray());
        }
        VerifyHonorBuffFiles(Path.Combine(stage,"Assembly-CSharp.dll"),Path.Combine(stage,"SprkHonorBuff.dll"));
        if(stageOnly)return;
        foreach(var process in System.Diagnostics.Process.GetProcesses())using(process)if(UsesClientFiles(process,root))throw new InvalidOperationException("Close the game and shared client workers before installing Honor buff changes.");
        var backup=Path.GetFullPath("target/honor-buff/backups/"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));Directory.CreateDirectory(backup);
        foreach(var name in new[]{"Assembly-CSharp.dll","SprkHonorBuff.dll"}) {
            var dest=Path.Combine(managed,name);if(File.Exists(dest))File.Copy(dest,Path.Combine(backup,name));File.Copy(Path.Combine(stage,name),dest,true);
        }
        VerifyHonorBuffFile(root);Console.WriteLine("Installed Honor server buff activation.");
    }
    static void TestHonorBuff() {
        string Resource(string name) {using var stream=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name)!;using var reader=new StreamReader(stream);return reader.ReadToEnd();}
        var refs=((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("HonorBuffTests-"+Guid.NewGuid().ToString("N"),new[]{CSharpSyntaxTree.ParseText(Resource("HonorBuff.cs.txt")),CSharpSyntaxTree.ParseText(Resource("HonorBuffTests.cs.txt"))},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var output=new MemoryStream();var result=compilation.Emit(output);if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics));
        System.Reflection.Assembly.Load(output.ToArray()).GetType("HonorBuffTestRunner")!.GetMethod("Run")!.Invoke(null,null);
        Console.WriteLine("PASS: Honor buff scope, quantity, confirmation, rates, request fields, duplicate callbacks, failure and session guards.");
    }

}
