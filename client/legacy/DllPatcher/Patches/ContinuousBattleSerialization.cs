using Mono.Cecil;

namespace DllPatcher;
partial class Program {
    const string ContinuousPrefabCount = "sprkPrefabBoosterCount";
    static void PatchContinuousBattleSerialization(ModuleDefinition module) {
        var type=module.GetType("NGame2.NUI.NWindow.NBattle.AutoNextBattle");
        var runtime=type.Fields.Single(f=>f.Name=="ItemBoosterCount");
        if(runtime.FieldType.MetadataType!=MetadataType.Int64)throw new InvalidOperationException("Continuous Battle repair requires the currency64 API.");
        // The shipped prefab stores a 32-bit zero at this position, followed by
        // AutoCampaignDelay. Keep that slot while runtime counts retain Int64.
        // OpenGlobal always assigns the requested count before opening the UI.
        runtime.IsNotSerialized=true;
        if(!type.Fields.Any(f=>f.Name==ContinuousPrefabCount)) {
            var slot=new FieldDefinition(ContinuousPrefabCount,FieldAttributes.Private,module.TypeSystem.Int32);
            var serialize=module.GetTypes().SelectMany(t=>t.Fields).SelectMany(f=>f.CustomAttributes)
                .First(a=>a.AttributeType.FullName=="UnityEngine.SerializeField");
            slot.CustomAttributes.Add(new CustomAttribute(module.ImportReference(serialize.Constructor)));
            type.Fields.Insert(type.Fields.IndexOf(runtime),slot);
        }
        VerifyContinuousBattleSerialization(module);
    }
    static void VerifyContinuousBattleSerialization(ModuleDefinition module) {
        var type=module.GetType("NGame2.NUI.NWindow.NBattle.AutoNextBattle");
        var runtime=type.Fields.Single(f=>f.Name=="ItemBoosterCount");
        var slot=type.Fields.SingleOrDefault(f=>f.Name==ContinuousPrefabCount);
        if(runtime.FieldType.MetadataType!=MetadataType.Int64 || !runtime.IsNotSerialized || slot==null ||
            slot.FieldType.MetadataType!=MetadataType.Int32 || !slot.CustomAttributes.Any(a=>a.AttributeType.FullName=="UnityEngine.SerializeField") ||
            type.Fields.IndexOf(slot)+1!=type.Fields.IndexOf(runtime))
            throw new InvalidOperationException("Continuous Battle must retain the native Int32 prefab slot and nonserialized Int64 runtime count.");
        if(module.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).SelectMany(m=>m.Body.Instructions)
            .Any(i=>i.Operand is FieldReference f && f.Name==ContinuousPrefabCount && f.DeclaringType.FullName==type.FullName))
            throw new InvalidOperationException("Runtime code must not consume the prefab compatibility slot.");
        Console.WriteLine("Verified native Continuous Battle prefab layout and 64-bit runtime booster count.");
    }
    static void VerifyContinuousBattleSerializationFile(string root,bool staged=false) {
        using var a=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".continuous-battle-staged":"")));
        VerifyContinuousBattleSerialization(a.MainModule);
    }
    static void InstallContinuousBattleSerialization(string root,bool stageOnly) {
        var dll=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"));
        var staged=dll+".continuous-battle-staged";
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(Path.GetDirectoryName(dll));
        using(var a=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver})){PatchContinuousBattleSerialization(a.MainModule);a.Write(staged);}
        VerifyContinuousBattleSerializationFile(root,true);
        if(!stageOnly) {
            if(System.Diagnostics.Process.GetProcesses().Any(p=>p.ProcessName=="King's Raid" || p.ProcessName.StartsWith("SprkTest-")))throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
            var backup=Path.Combine("target","continuous-battle-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));File.Move(staged,dll,true);
            VerifyContinuousBattleSerializationFile(root);
        }
        Console.WriteLine(stageOnly?"Staged Continuous Battle prefab repair.":"Installed Continuous Battle prefab repair. Restart the client.");
    }
}
