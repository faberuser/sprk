using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;

namespace DllPatcher;
partial class Program {
    static JsonDocument ArtifactSelectorPools() => JsonDocument.Parse(
        typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ArtifactSelectorPools.json")!);

    static void PatchArtifactSelectors(ModuleDefinition module) {
        var type=module.GetType("NShared.WeaponUniqueSelectItemData");
        var getter=type.Methods.Single(m=>m.Name=="get_ItemIndices");
        var backing=type.Fields.Single(f=>f.Name=="<ItemIndices>k__BackingField");
        var code=type.Fields.Single(f=>f.Name=="<ItemCode>k__BackingField");
        getter.Body=new MethodBody(getter);
        var il=getter.Body.GetILProcessor();
        var equal=new MethodReference("op_Equality",module.TypeSystem.Boolean,module.TypeSystem.String);
        equal.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        equal.Parameters.Add(new ParameterDefinition(module.TypeSystem.String));
        using var pools=ArtifactSelectorPools();
        foreach(var row in pools.RootElement.EnumerateArray()) {
            var next=il.Create(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,code);
            il.Emit(OpCodes.Ldstr,row.GetProperty("Code").GetString()!);
            il.Emit(OpCodes.Call,equal);il.Emit(OpCodes.Brfalse,next);
            var choices=row.GetProperty("Choices").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
            il.Emit(OpCodes.Ldc_I4,choices.Length);il.Emit(OpCodes.Newarr,module.TypeSystem.Int32);
            for(int i=0;i<choices.Length;i++) {
                il.Emit(OpCodes.Dup);il.Emit(OpCodes.Ldc_I4,i);
                il.Emit(OpCodes.Ldc_I4,choices[i]);il.Emit(OpCodes.Stelem_I4);
            }
            il.Emit(OpCodes.Ret);il.Append(next);
        }
        // All other tickets retain their baked arrays, including restricted pools.
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,backing);il.Emit(OpCodes.Ret);
        VerifyArenaResultStack(getter);
    }

    static void VerifyArtifactSelectorsFile(string root,bool staged=false) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        var context=new System.Runtime.Loader.AssemblyLoadContext("ArtifactSelectors-"+Guid.NewGuid(),true);
        context.Resolving+=(load,name)=> {
            var dependency=Path.Combine(managed,name.Name+".dll");
            return File.Exists(dependency)?load.LoadFromStream(new MemoryStream(File.ReadAllBytes(dependency))):null;
        };
        try {
            var path=Path.Combine(managed,"Assembly-CSharp.dll"+(staged?".artifact-selectors-staged":""));
            using var bytes=new MemoryStream(File.ReadAllBytes(path));
            var type=context.LoadFromStream(bytes).GetType("NShared.WeaponUniqueSelectItemData")!;
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var code=type.GetField("<ItemCode>k__BackingField",flags)!;
            var backing=type.GetField("<ItemIndices>k__BackingField",flags)!;
            var getter=type.GetProperty("ItemIndices")!;
            var row=Activator.CreateInstance(type)!;
            using var pools=ArtifactSelectorPools();
            foreach(var pool in pools.RootElement.EnumerateArray()) {
                code.SetValue(row,pool.GetProperty("Code").GetString());
                var expected=pool.GetProperty("Choices").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
                var actual=(int[])getter.GetValue(row)!;
                if(!actual.SequenceEqual(expected) || actual.Distinct().Count()!=actual.Length ||
                    new[]{150900,150901,150902}.Any(id=>!actual.Contains(id)))
                    throw new InvalidOperationException("Artifact selector choices are missing or duplicated.");
            }
            foreach(var restricted in new[]{"ARTIFACT_SELECT_ITEM","ARTIFACT_SELECT_ITEM_2","ARTIFACT_SELECT_ITEM_SPECIAL","ARTIFACT_SELECT_VELKAZAR","UNKNOWN",null}) {
                code.SetValue(row,restricted);
                var original=new[]{150001,150005};backing.SetValue(row,original);
                if(!ReferenceEquals(original,getter.GetValue(row)))
                    throw new InvalidOperationException("Unrelated artifact selector was changed: "+restricted);
            }
            Console.WriteLine("PASS: real client getters for Normal/Lua artifact pools, all three additions exactly once and six native fallbacks.");
        } finally {context.Unload();}
    }

    static void InstallArtifactSelectors(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));
        if(!NativeStaticData.ValidateInstalled(managed))
            throw new InvalidOperationException("Artifact selector overrides require verified baked native pools.");
        var path=Path.Combine(managed,"Assembly-CSharp.dll");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using(var game=AssemblyDefinition.ReadAssembly(path,new ReaderParameters{AssemblyResolver=resolver,InMemory=true})) {
            PatchArtifactSelectors(game.MainModule);
            var getter=game.MainModule.GetType("NShared.WeaponUniqueSelectItemData").Methods.Single(m=>m.Name=="get_ItemIndices");
            var before=string.Join("\n",getter.Body.Instructions);
            PatchArtifactSelectors(game.MainModule);
            if(before!=string.Join("\n",getter.Body.Instructions))throw new InvalidOperationException("Artifact selector patch is not idempotent.");
            game.Write(path+".artifact-selectors-staged");
        }
        VerifyArtifactSelectorsFile(root,true);
        if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))
            throw new InvalidOperationException("Artifact selectors staged. Close the game and shared-asset workers before installation.");
        File.Copy(path,path+".before-artifact-selectors-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        File.Move(path+".artifact-selectors-staged",path,true);
        VerifyArtifactSelectorsFile(root);
    }
}
