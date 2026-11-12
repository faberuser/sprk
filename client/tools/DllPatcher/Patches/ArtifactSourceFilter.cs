using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void VerifyArtifactSourceFilterFile(string root,bool staged=false) {
        var managed=Path.Combine(root,"King's Raid_Data","Managed");
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".artifact-filter-staged":"")));
        var filter=game.MainModule.GetType("NGame2.NUI.NWindow.SelectFilter");
        var window=game.MainModule.GetType("NGame2.NUI.NWindow.WeaponUniqueSelect");
        foreach(var hook in new[]{(filter,"InitFilter","Reset"),(filter,"SetSubComboBox","Build"),(filter,"CheckAddItem","Matches"),(window,"AddEquipItemByFilter","BeforeList")}) {
            var method=hook.Item1.Methods.Single(m=>m.Name==hook.Item2);
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkArtifactSourceFilter" && m.Name==hook.Item3)!=1)
                throw new InvalidOperationException("Missing artifact source hook: "+hook.Item2);
            VerifyArenaResultStack(method);
        }
        var matcher=filter.Methods.Single(m=>m.Name=="CheckAddItem");
        foreach(var name in new[]{"CheckAddItemByUserItem","CheckAddEquipItemByUniqueWeapon"})
            if(!matcher.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Native ownership filter missing.");
        var guard=matcher.Body.Instructions;
        if(guard[2].Operand is not MethodReference {Name:"Matches"} || guard[3].OpCode!=OpCodes.Brtrue || guard[3].Operand!=guard[6]
            || guard[4].OpCode!=OpCodes.Ldc_I4_0 || guard[5].OpCode!=OpCodes.Ret)throw new InvalidOperationException("Artifact source filter must narrow native eligibility.");
        var rebuild=window.Methods.Single(m=>m.Name=="AddEquipItemByFilter");
        if(rebuild.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).Any(i=>i.Previous?.Operand is not MethodReference {Name:"AfterList"}))
            throw new InvalidOperationException("Artifact list must reset scrolling after every rebuild.");
        using var helper=AssemblyDefinition.ReadAssembly(Path.Combine(managed,"SprkArtifactSourceFilter.dll"+(staged?".staged":"")));
        var calls=helper.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        foreach(var name in new[]{"get_ItemGroupDataContainer","get_ItemIndices","SelectItem","ResetPosition","EnableButton"})
            if(!calls.Any(m=>m.Name==name))throw new InvalidOperationException("Incomplete artifact source behavior: "+name);
        Console.WriteLine("PASS: artifact source hooks, IL stack, native ownership and ticket-pool restriction.");
    }
    static void InstallArtifactSourceFilter(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        var helperPath=Path.Combine(managed,"SprkArtifactSourceFilter.dll");
        var trees=new[]{"ArtifactSourceFilter","ArtifactSourceFilterProbe"}.Select(name=> {
            using var resource=typeof(Program).Assembly.GetManifestResourceStream("DllPatcher."+name+".cs.txt")!;
            using var reader=new StreamReader(resource);return CSharpSyntaxTree.ParseText(reader.ReadToEnd());
        }).ToArray();
        var refs=Directory.GetFiles(managed,"*.dll").Where(p=>p!=helperPath).Select(p=>MetadataReference.CreateFromFile(p));
        var compilation=CSharpCompilation.Create("SprkArtifactSourceFilter",trees,refs,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,optimizationLevel:OptimizationLevel.Release,deterministic:true));
        using var compiled=new MemoryStream();var result=compilation.Emit(compiled);
        if(!result.Success)throw new InvalidOperationException(string.Join("\n",result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error)));
        compiled.Position=0;using var helper=AssemblyDefinition.ReadAssembly(compiled);
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true});
        var module=game.MainModule;
        var filter=module.GetType("NGame2.NUI.NWindow.SelectFilter");
        var window=module.GetType("NGame2.NUI.NWindow.WeaponUniqueSelect");
        MethodReference Call(string name)=>module.ImportReference(helper.MainModule.GetType("SprkArtifactSourceFilter").Methods.Single(m=>m.Name==name));
        bool Has(MethodDefinition method,string name)=>method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.DeclaringType.Name=="SprkArtifactSourceFilter" && m.Name==name);
        foreach(var name in new[]{"InitFilter","SetSubComboBox","CheckAddItem","AddEquipItemByFilter"}) {
            var method=(name=="AddEquipItemByFilter"?window:filter).Methods.Single(m=>m.Name==name);
            string hook=name switch {"InitFilter"=>"Reset","SetSubComboBox"=>"Build","CheckAddItem"=>"Matches",_=>"BeforeList"};
            if(Has(method,hook))continue;
            var il=method.Body.GetILProcessor();var first=method.Body.Instructions[0];
            il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
            if(name=="SetSubComboBox") {
                foreach(var field in new[]{"ComboBoxSub","_selectType","_callback"}) {
                    il.InsertBefore(first,il.Create(OpCodes.Ldarg_0));
                    il.InsertBefore(first,il.Create(OpCodes.Ldfld,filter.Fields.Single(f=>f.Name==field)));
                }
            }
            if(name=="CheckAddItem")il.InsertBefore(first,il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first,il.Create(OpCodes.Call,Call(hook)));
            if(name=="SetSubComboBox") {
                il.InsertBefore(first,il.Create(OpCodes.Brfalse,first));
                il.InsertBefore(first,il.Create(OpCodes.Ret));
            } else if(name=="CheckAddItem") {
                il.InsertBefore(first,il.Create(OpCodes.Brtrue,first));
                il.InsertBefore(first,il.Create(OpCodes.Ldc_I4_0));il.InsertBefore(first,il.Create(OpCodes.Ret));
            } else if(name=="AddEquipItemByFilter") {
                foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                    ret.OpCode=OpCodes.Ldarg_0;
                    var call=il.Create(OpCodes.Call,Call("AfterList"));il.InsertAfter(ret,call);il.InsertAfter(call,il.Create(OpCodes.Ret));
                }
            }
        }
        game.Write(dll+".artifact-filter-staged");File.WriteAllBytes(helperPath+".staged",compiled.ToArray());
        VerifyArtifactSourceFilterFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll,dll+".before-artifact-filter-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(dll+".artifact-filter-staged",dll,true);File.Move(helperPath+".staged",helperPath,true);
        VerifyArtifactSourceFilterFile(root);
    }
}
