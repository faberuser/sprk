using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static readonly string[] AwakenMaterialRefreshes={"UpdateAwakeEquipItemListTask","UpdatePublicAwakeItemListTask"};
    static void PatchAwakeningMaterials(ModuleDefinition module) {
        var view=module.GetType("NGame2.NUI.NWindow.HeroEquipAwaken");
        var icon=module.GetType("NGame2.NUI.NComponent2.ItemIconComponent");
        var gray=icon.Methods.Single(m=>m.Name=="SetIconGrayScale");
        var calls=module.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions).Select(i=>i.Operand).OfType<MethodReference>().ToArray();
        var visible=view.Methods.SingleOrDefault(m=>m.Name=="SprkSetMaterialVisibility");
        if(visible==null) {
            visible=new MethodDefinition("SprkSetMaterialVisibility",MethodAttributes.Private|MethodAttributes.Static,module.TypeSystem.Void);
            visible.Parameters.Add(new ParameterDefinition("icon",ParameterAttributes.None,icon));
            visible.Parameters.Add(new ParameterDefinition("gray",ParameterAttributes.None,module.TypeSystem.Boolean));view.Methods.Add(visible);
            var il=visible.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Callvirt,gray);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Callvirt,module.ImportReference(calls.First(m=>m.Name=="get_gameObject" && m.DeclaringType.FullName=="UnityEngine.Component")));
            il.Emit(OpCodes.Ldarg_1);il.Emit(OpCodes.Ldc_I4_0);il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Callvirt,module.ImportReference(calls.First(m=>m.Name=="SetActive" && m.DeclaringType.FullName=="UnityEngine.GameObject")));il.Emit(OpCodes.Ret);
        }
        var compact=view.Methods.SingleOrDefault(m=>m.Name=="SprkCompactMaterialGrid");
        if(compact==null) {
            compact=new MethodDefinition("SprkCompactMaterialGrid",MethodAttributes.Private,module.TypeSystem.Void);view.Methods.Add(compact);
            var grid=view.Fields.Single(f=>f.Name=="AwakeInvenItemListGrid");
            var inner=grid.FieldType.Resolve().Fields.Single(f=>f.Name=="Grid");
            var hide=module.GetType("UIGrid").Fields.Single(f=>f.Name=="hideInactive");
            var reposition=module.GetType("NGame2.NUI.UIHelper").Methods.Single(m=>m.Name=="RepositionGrid");
            var il=compact.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,grid);il.Emit(OpCodes.Ldfld,inner);il.Emit(OpCodes.Ldc_I4_1);il.Emit(OpCodes.Stfld,hide);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,grid);il.Emit(OpCodes.Call,reposition);il.Emit(OpCodes.Ret);
        }
        foreach(var name in AwakenMaterialRefreshes) {
            var method=view.Methods.Single(m=>m.Name==name);
            foreach(var i in method.Body.Instructions)
                if(i.Operand is MethodReference m && m.FullName==gray.FullName){i.OpCode=OpCodes.Call;i.Operand=visible;}
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==compact.Name))continue;
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Ldarg_0;ret.Operand=null;var il=method.Body.GetILProcessor();
                var call=Instruction.Create(OpCodes.Call,compact);il.InsertAfter(ret,call);il.InsertAfter(call,Instruction.Create(OpCodes.Ret));
            }
        }
        VerifyAwakeningMaterials(module);
    }
    static void VerifyAwakeningMaterials(ModuleDefinition module) {
        var view=module.GetType("NGame2.NUI.NWindow.HeroEquipAwaken");
        foreach(var name in AwakenMaterialRefreshes) {
            var method=view.Methods.Single(m=>m.Name==name);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="SprkSetMaterialVisibility") ||
                method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="SetIconGrayScale") ||
                method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).Any(i=>i.Previous.Operand is not MethodReference m || m.Name!="SprkCompactMaterialGrid"))
                throw new InvalidOperationException("Awakening material visibility/compaction hook missing: "+name);
            VerifyArenaResultStack(method);
        }
        foreach(var name in new[]{"SprkSetMaterialVisibility","SprkCompactMaterialGrid"})VerifyArenaResultStack(view.Methods.Single(m=>m.Name==name));
        Console.WriteLine("Verified awakening-only material visibility, native eligibility checks and compact grid refreshes.");
    }
    static void VerifyAwakeningMaterialsFile(string root,bool staged=false) {
        using var a=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".awakening-materials-staged":"")));
        VerifyAwakeningMaterials(a.MainModule);
    }
    static void InstallAwakeningMaterials(string root,bool stageOnly) {
        var dll=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"));
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(Path.GetDirectoryName(dll));
        var staged=dll+".awakening-materials-staged";
        using(var a=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver})){PatchAwakeningMaterials(a.MainModule);a.Write(staged);}
        VerifyAwakeningMaterialsFile(root,true);
        if(!stageOnly) {
            if(System.Diagnostics.Process.GetProcesses().Any(p=>p.ProcessName=="King's Raid" || p.ProcessName.StartsWith("SprkTest-")))throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
            var backup=Path.Combine("target","awakening-materials-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));File.Move(staged,dll,true);VerifyAwakeningMaterialsFile(root);
        }
        Console.WriteLine(stageOnly?"Staged awakening material filtering.":"Installed awakening material filtering. Restart the client.");
    }
}
