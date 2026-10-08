using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
namespace DllPatcher;
partial class Program {
    static void InstallCrafting(string clientRoot,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(clientRoot,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        using var source=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Crafting.cs.txt")!;
        using var reader=new StreamReader(source);
        using var data=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RestoredCrafting.json")!;
        using var json=JsonDocument.Parse(data);
        var literal=JsonSerializer.Serialize(json.RootElement.GetRawText());
        var code=reader.ReadToEnd().Replace("\"__RESTORED_CRAFTING__\"",literal);
        var references=Directory.GetFiles(managed,"*.dll").Where(path=>!Path.GetFileName(path).StartsWith("SprkCrafting"))
            .Select(path=>MetadataReference.CreateFromFile(path));
        var compilation=CSharpCompilation.Create("SprkCrafting",new[]{CSharpSyntaxTree.ParseText(code)},references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release));
        using var bytes=new MemoryStream();var emitted=compilation.Emit(bytes);
        if(!emitted.Success)throw new InvalidOperationException(string.Join("\n",emitted.Diagnostics));
        bytes.Position=0;using var helper=AssemblyDefinition.ReadAssembly(bytes);
        var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var assembly=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver});
        var module=assembly.MainModule;var type=helper.MainModule.Types.Single(t=>t.Name=="SprkCrafting");int count=0;
        foreach(var method in module.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody))
        foreach(var instruction in method.Body.Instructions) {
            if(instruction.Operand is not MethodReference call || !call.DeclaringType.FullName.Contains("NShared.CraftItemData"))continue;
            var name=call.Name switch {"GetData" when call.Parameters.Count==1=>"Get","GetAllData" when call.Parameters.Count==0=>"All","GetCategoryData" when call.Parameters.Count==1=>"Category",_=>null};
            if(name==null)continue;
            instruction.OpCode=OpCodes.Call;instruction.Operand=module.ImportReference(type.Methods.Single(m=>m.Name==name));count++;
        }
        File.WriteAllBytes(Path.Combine(managed,"SprkCrafting.dll.staged"),bytes.ToArray());
        var staged=dll+".craft-staged";assembly.Write(staged);
        if(!stageOnly) {
            File.Copy(dll,dll+".before-crafting-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));assembly.Dispose();
            File.Move(staged,dll,true);File.Move(Path.Combine(managed,"SprkCrafting.dll.staged"),Path.Combine(managed,"SprkCrafting.dll"),true);
        }
        Console.WriteLine($"{(stageOnly?"Staged":"Installed")} archive crafting restoration; {count} list, recipe and shortcut calls patched");
    }
}
