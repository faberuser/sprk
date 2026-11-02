using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void VerifyHeroRuneFilterFile(string root, bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".rune-filter-staged":"")));
        var panel=game.MainModule.GetType("NGame2.NUI.NComponent2.HeroRuneListPanel");
        foreach(var pair in new[]{("Open","Open"),("Selector","Matches"),("Close","Close")}) {
            var method=panel.Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkHeroRuneFilter" && m.Name==pair.Item2)!=1)
                throw new InvalidOperationException("Missing rune filter hook: "+pair.Item1);
            VerifyArenaResultStack(method);
        }
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkHeroRuneFilter.dll"+(staged?".staged":"")));
        var type=helper.MainModule.GetType("SprkHeroRuneFilter")??throw new InvalidOperationException("Missing rune filter helper.");
        var calls=type.Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"GetRuneStat","GetInstance","AddInstance","Open","Close","Refresh","ResetScroll"})
            if(!calls.Any(m=>m.Name==name))throw new InvalidOperationException("Incomplete rune filter lifecycle: "+name);
        var selector=panel.Methods.Single(m=>m.Name=="Selector");
        foreach(var name in new[]{"get_IsEquipped","GetActivedRuneSlotTypes","GetRuneSlotTypes"})
            if(!selector.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))
                throw new InvalidOperationException("Native rune eligibility removed: "+name);
        var guard=selector.Body.Instructions;
        if(guard[2].Operand is not MethodReference {Name:"Matches"} || guard[3].OpCode!=OpCodes.Brtrue || guard[3].Operand!=guard[6]
            || guard[4].OpCode!=OpCodes.Ldc_I4_0 || guard[5].OpCode!=OpCodes.Ret)
            throw new InvalidOperationException("Rune filter must narrow the original selector.");
        Console.WriteLine("PASS: hero rune filter hooks, IL stack, native eligibility and popup lifecycle.");
    }
    static void InstallHeroRuneFilter(string root, bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkHeroRuneFilter.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.HeroRuneFilter.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkHeroRuneFilter",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var panel=game.MainModule.GetType("NGame2.NUI.NComponent2.HeroRuneListPanel");
        foreach(var pair in new[]{("Open","Open"),("Selector","Matches"),("Close","Close")}) {
            var method=panel.Methods.Single(m=>m.Name==pair.Item1);
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkHeroRuneFilter"))continue;
            var first=method.Body.Instructions[0];var il=method.Body.GetILProcessor();
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            if(pair.Item1=="Selector")il.InsertBefore(first,il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first,il.Create(OpCodes.Call,game.MainModule.ImportReference(helper.MainModule.GetType("SprkHeroRuneFilter").Methods.Single(m=>m.Name==pair.Item2))));
            if(pair.Item1=="Selector") {
                il.InsertBefore(first,il.Create(OpCodes.Brtrue,first));
                il.InsertBefore(first,il.Create(OpCodes.Ldc_I4_0));
                il.InsertBefore(first,il.Create(OpCodes.Ret));
            }
        }
        game.Write(dll+".rune-filter-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyHeroRuneFilterFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-rune-filter-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(dll+".rune-filter-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyHeroRuneFilterFile(root);
    }
}
