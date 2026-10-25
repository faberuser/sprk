using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void VerifyRaidInfoScroll(ModuleDefinition module) {
        var method=module.GetType("NGame2.NUI.NWindow.RaidInfoBase").Methods.Single(m=>m.Name=="UpdateButtonState");
        var calls=method.Body.Instructions.Where(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkRaidInfoScroll" && m.Name=="Refresh").ToArray();
        if(calls.Length!=1 || calls[0].Previous.OpCode!=OpCodes.Ldarg_0 || calls[0].Next.OpCode!=OpCodes.Ret)
            throw new InvalidOperationException("Raid information scroll hook missing or duplicated.");
    }
    static void VerifyRaidInfoScrollFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".raid-scroll-staged":"")));
        VerifyRaidInfoScroll(game.MainModule);
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkRaidInfoScroll.dll"+(staged?".staged":"")));
        var calls=helper.MainModule.GetType("SprkRaidInfoScroll").Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().Select(m=>m.Name).ToArray();
        foreach(var name in new[]{"set_overflowMethod","set_clipping","InvalidateBounds","ResetPosition"})
            if(!calls.Contains(name))throw new InvalidOperationException("Incomplete Characteristics scrolling: "+name);
        if(!helper.MainModule.GetType("SprkRaidInfoScroll").Methods.Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Any(i=>i.OpCode==OpCodes.Stfld && i.Operand is FieldReference f && f.Name=="scrollView"))
            throw new InvalidOperationException("Characteristics drag target missing.");
        Console.WriteLine("Verified Characteristics viewport, full text sizing, drag binding and reset hook.");
    }
    static void InstallRaidInfoScroll(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkRaidInfoScroll.dll");
        using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.RaidInfoScroll.cs.txt")!;
        using var reader=new StreamReader(resource);
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkRaidInfoScroll",new[]{CSharpSyntaxTree.ParseText(reader.ReadToEnd())},refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();
        var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;
        using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var method=game.MainModule.GetType("NGame2.NUI.NWindow.RaidInfoBase").Methods.Single(m=>m.Name=="UpdateButtonState");
        if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkRaidInfoScroll")) {
            var ret=method.Body.Instructions.Single(i=>i.OpCode==OpCodes.Ret);
            var il=method.Body.GetILProcessor();
            ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;
            il.Append(il.Create(OpCodes.Call,game.MainModule.ImportReference(helper.MainModule.GetType("SprkRaidInfoScroll").Methods.Single(m=>m.Name=="Refresh"))));
            il.Append(il.Create(OpCodes.Ret));
        }
        VerifyRaidInfoScroll(game.MainModule);
        var staged=dll+".raid-scroll-staged";
        game.Write(staged);File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyRaidInfoScrollFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-raid-scroll-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        File.Move(staged,dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyRaidInfoScrollFile(root);
    }
}
