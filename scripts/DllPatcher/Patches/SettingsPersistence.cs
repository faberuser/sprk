using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static bool PatchSettingsPersistence(ModuleDefinition module) {
        bool changed=false;
        var options=module.GetType("NVespa.SystemOptionData");
        var prefs=module.GetType("NVespa.LocalStorageInt").Methods.SelectMany(m=>m.HasBody?m.Body.Instructions:Enumerable.Empty<Instruction>())
            .Select(i=>i.Operand).OfType<MethodReference>().First(m=>m.Name=="GetInt" && m.Parameters.Count==2).DeclaringType;
        // Use the installed preference adapter so isolated test profiles stay isolated.
        var prefsType=prefs.Resolve();
        MethodReference Pref(string name)=>module.ImportReference(prefsType.Methods.Single(m=>m.Name==name && m.Parameters.Count==(name=="HasKey"?1:0)));
        var migrate=options.Methods.Single(m=>m.Name=="CheckExistFrameRateOption");
        if(!migrate.Body.Instructions.Any(i=>Equals(i.Operand,"KingsRaid_SystemOption_graphicFrameContorl"))) {
            changed=true;
            var il=migrate.Body.GetILProcessor();var first=migrate.Body.Instructions[0];
            il.InsertBefore(first,Instruction.Create(OpCodes.Ldstr,"KingsRaid_SystemOption_graphicFrameContorl"));
            il.InsertBefore(first,Instruction.Create(OpCodes.Call,Pref("HasKey")));
            il.InsertBefore(first,Instruction.Create(OpCodes.Brtrue,migrate.Body.Instructions.Last()));
        }
        var restore=options.Methods.SingleOrDefault(m=>m.Name=="SprkRestoreSavedSettings");
        if(restore==null) {
            changed=true;
            var done=new FieldDefinition("_sprkSettingsRestored",FieldAttributes.Private|FieldAttributes.Static,module.TypeSystem.Boolean);
            options.Fields.Add(done);
            restore=new MethodDefinition("SprkRestoreSavedSettings",MethodAttributes.Public|MethodAttributes.Static,module.TypeSystem.Void);
            options.Methods.Add(restore);var il=restore.Body.GetILProcessor();var ret=Instruction.Create(OpCodes.Ret);
            il.Emit(OpCodes.Ldsfld,done);il.Emit(OpCodes.Brtrue,ret);
            il.Emit(OpCodes.Call,options.Methods.Single(m=>m.Name=="Init"));
            il.Emit(OpCodes.Ldc_I4_1);il.Emit(OpCodes.Stsfld,done);il.Append(ret);
        }
        var awake=module.GetType("UICamera").Methods.Single(m=>m.Name=="Awake");
        if(!CallsSettings(awake,"SprkRestoreSavedSettings")) {
            changed=true;
            awake.Body.GetILProcessor().InsertBefore(awake.Body.Instructions[0],Instruction.Create(OpCodes.Call,restore));
        }
        // Flush options at the user change boundary, including texture reloads.
        foreach(var method in new[]{options.Methods.Single(m=>m.Name=="set_Spec"),
            module.GetType("NGame2.NUI.NComponent2.GameOptionBaseComponent").Methods.Single(m=>m.Name=="SetOptionValue" && m.Parameters.Count==5)}) {
            if(method.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name=="Save" && m.DeclaringType.FullName==prefs.FullName))continue;
            changed=true;
            var il=method.Body.GetILProcessor();
            foreach(var ret in method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).ToArray()) {
                ret.OpCode=OpCodes.Call;ret.Operand=Pref("Save");il.InsertAfter(ret,Instruction.Create(OpCodes.Ret));
            }
        }
        VerifySettingsPersistence(module);
        return changed;
    }
    static bool CallsSettings(MethodDefinition method,string name)=>method.HasBody && method.Body.Instructions.Any(i=>
        i.Operand is MethodReference m && m.DeclaringType.FullName=="NVespa.SystemOptionData" && m.Name==name);
    static void VerifySettingsPersistence(ModuleDefinition module) {
        var options=module.GetType("NVespa.SystemOptionData");
        if(!CallsSettings(module.GetType("UICamera").Methods.Single(m=>m.Name=="Awake"),"SprkRestoreSavedSettings") ||
           !CallsSettings(options.Methods.Single(m=>m.Name=="SprkRestoreSavedSettings"),"Init"))
            throw new InvalidOperationException("Saved settings must initialize at UI startup.");
        var migrate=options.Methods.Single(m=>m.Name=="CheckExistFrameRateOption").Body.Instructions;
        if(!Equals(migrate[0].Operand,"KingsRaid_SystemOption_graphicFrameContorl") ||
           migrate[1].Operand is not MethodReference check || check.Name!="HasKey" || migrate[2].OpCode!=OpCodes.Brtrue ||
           migrate[2].Operand!=migrate.Last())throw new InvalidOperationException("Legacy FPS migration must preserve an existing preference.");
        foreach(var method in new[]{options.Methods.Single(m=>m.Name=="set_Spec"),
            module.GetType("NGame2.NUI.NComponent2.GameOptionBaseComponent").Methods.Single(m=>m.Name=="SetOptionValue" && m.Parameters.Count==5)})
            if(method.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ret).Any(i=>i.Previous.Operand is not MethodReference m || m.Name!="Save"))
                throw new InvalidOperationException("User option changes must flush preferences: "+method.FullName);
        Console.WriteLine("Verified settings startup and existing FPS preference guard.");
    }
    static void VerifySettingsPersistenceFile(string clientRoot) {
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot,"King's Raid_Data","Managed","Assembly-CSharp.dll"));
        VerifySettingsPersistence(game.MainModule);
    }
    static void InstallSettingsPersistence(string clientRoot,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(clientRoot,"King's Raid_Data","Managed"));
        var path=Path.Combine(managed,"Assembly-CSharp.dll");var staged=path+".settings-staged";
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using(var game=AssemblyDefinition.ReadAssembly(path,new ReaderParameters{AssemblyResolver=resolver})) {
            if(PatchSettingsPersistence(game.MainModule))game.Write(staged);
            else File.Copy(path,staged,true);
        }
        using(var game=AssemblyDefinition.ReadAssembly(staged))VerifySettingsPersistence(game.MainModule);
        if(!stageOnly) {
            var backup=Path.Combine("target","settings-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);File.Copy(path,Path.Combine(backup,"Assembly-CSharp.dll"));File.Move(staged,path,true);
        }
        Console.WriteLine(stageOnly?"Staged saved settings restoration.":"Installed saved settings restoration. Restart the client.");
    }
    static void InspectSettings(string clientRoot) {
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot,"King's Raid_Data","Managed","Assembly-CSharp.dll"));
        foreach(var type in game.MainModule.GetTypes())
        foreach(var method in type.Methods.Where(m=>m.HasBody)) {
            if((type.FullName=="NVespa.SystemOptionData" && new[]{"SprkRestoreSavedSettings","Init","CheckExistFrameRateOption"}.Contains(method.Name)) ||
               method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.DeclaringType.FullName=="UnityEngine.Application" && r.Name=="set_targetFrameRate")) {
                Console.WriteLine(method.FullName);
                foreach(var i in method.Body.Instructions)Console.WriteLine(i);
            }
        }
    }
}
