using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    const string FormulaHelper="SprkSkillDescriptionFormula";
    static void VerifySkillDescriptionFormulaFile(string root,bool staged=false) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".skill-formula-staged":"")),new ReaderParameters{AssemblyResolver=resolver});
        var method=game.MainModule.GetType("NGame2.NUI.NWindow.HeroSkillDesc").Methods.Single(m=>m.Name=="SetupCurSkillLevelDesc");
        if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name==FormulaHelper && m.Name=="Setup")!=1)
            throw new InvalidOperationException("Missing or duplicated skill formula card hook");
        if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference{Name:"GetByHeroInfo"}))throw new InvalidOperationException("Native calculated skill description removed");
        VerifyArenaResultStack(method);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,FormulaHelper+".dll"+(staged?".staged":"")),new ReaderParameters{AssemblyResolver=resolver});
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"GetOperationDataUnsafe","GetByHeroInfo","GetObjectValue","ResetPosition"})
            if(!calls.Any(m=>m.Name==name))throw new InvalidOperationException("Incomplete formula helper: "+name);
        if(calls.Any(m=>m.Name=="SetOperationData" || m.Name=="SendRequest"))throw new InvalidOperationException("Formula helper must be presentation-only");
        Console.WriteLine("PASS: formula toggle hook, native calculated path and IL stacks.");
    }
    static void InstallSkillDescriptionFormula(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");var helperPath=Path.Combine(managed,FormulaHelper+".dll");
        var trees=new[]{"SkillDescriptionFormula","SkillDescriptionFormulaWords","SkillDescriptionFormulaProbe"}.Select(name=>{
            using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name+".cs.txt")!;
            using var reader=new StreamReader(resource);return CSharpSyntaxTree.ParseText(reader.ReadToEnd());
        });
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create(FormulaHelper,trees,refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var module=game.MainModule;var method=module.GetType("NGame2.NUI.NWindow.HeroSkillDesc").Methods.Single(m=>m.Name=="SetupCurSkillLevelDesc");
        if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name==FormulaHelper && m.Name=="Setup")) {
            var call=module.ImportReference(helper.MainModule.GetType(FormulaHelper).Methods.Single(m=>m.Name=="Setup"));
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;var arg=il.Create(OpCodes.Ldarg_1);il.InsertAfter(ret,arg);
                var invoke=il.Create(OpCodes.Call,call);il.InsertAfter(arg,invoke);il.InsertAfter(invoke,il.Create(OpCodes.Ret));
            }
        }
        game.Write(dll+".skill-formula-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifySkillDescriptionFormulaFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Formula patch staged. Close client before installation.");
        File.Copy(dll,dll+".before-skill-formula-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(dll+".skill-formula-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifySkillDescriptionFormulaFile(root);
    }
}
