using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void TestShopTags(byte[] bytes) {
        var method=System.Reflection.Assembly.Load(bytes).GetType("SprkShopTags")!.GetMethod("Matches")!;
        bool Matches(int tag,string name,string category,int type)=>(bool)method.Invoke(null,new object[]{tag,name,category,type})!;
        if(!Matches(7,"Beginner Ruby Package 1","Beginner",4) || !Matches(7,"Ruby offer","Ruby",2)
            || Matches(7,"Unique Weapon Package","Equipment",4) || !Matches(4,"Mini Unique Weapon Random Package","Event",4)
            || !Matches(34,"Technomagic Gear","Technomagic",4) || Matches(1001,"Hero Package","Event",4))
            throw new InvalidOperationException("Shop tag classification regression.");
        Console.WriteLine("Shop tag classification checks passed.");
    }
    static void VerifyShopTags(ModuleDefinition module) {
        var method=module.GetType("NShared.NewPayShopTagDataContainer").Methods.Single(m=>m.Name=="GetTags" && m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="System.String");
        var calls=method.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkShopTags" && m.Name=="Refresh").ToArray();
        if(calls.Length!=1 || calls[0].Previous.OpCode!=OpCodes.Ldarg_0)
            throw new InvalidOperationException("Shop tag refresh hook missing or duplicated.");
    }
    static void VerifyShopTagsFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".shop-tags-staged":"")));
        VerifyShopTags(game.MainModule);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkShopTags.dll"+(staged?".staged":"")));
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().Select(m=>m.Name).ToArray();
        foreach(var name in new[]{"GetValues","GetAllData","Matches","set_Indices","OrderBy","ThenBy"})
            if(!calls.Contains(name))throw new InvalidOperationException("Incomplete active shop tags: "+name);
        Console.WriteLine("Verified active-catalog shop tag hook and helper.");
    }
    static void InstallShopTags(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkShopTags.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ShopTags.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkShopTags",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        TestShopTags(compiled.ToArray());
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var method=game.MainModule.GetType("NShared.NewPayShopTagDataContainer").Methods.Single(m=>m.Name=="GetTags" && m.Parameters.Count==1 && m.Parameters[0].ParameterType.FullName=="System.String");
        if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkShopTags")) {
            var first=method.Body.Instructions[0];
            var il=method.Body.GetILProcessor();
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first,il.Create(OpCodes.Call,game.MainModule.ImportReference(helper.MainModule.GetType("SprkShopTags").Methods.Single(m=>m.Name=="Refresh"))));
        }
        VerifyShopTags(game.MainModule);
        var staged=dll+".shop-tags-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyShopTagsFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>p.ProcessName=="King's Raid" || p.ProcessName.StartsWith("SprkTest-")))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-shop-tags-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyShopTagsFile(root);
    }
}
