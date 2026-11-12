using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static TypeDefinition RewardHero(ModuleDefinition module)=>module.GetType("NGame2.NUI.NComponent2.NBattle.HeroInRewardComponent");
    static MethodDefinition FlaskResultCompletion(ModuleDefinition module) {
        var setup=RewardHero(module).Methods.Single(m=>m.Name=="SetupUI");
        return setup.Body.Instructions.Where(i=>i.OpCode==OpCodes.Ldftn).Select(i=>(MethodReference)i.Operand)
            .Single(m=>m.Name.Contains("<SetupUI>") && m.Parameters.Count==0).Resolve();
    }
    static void PatchBattleResultFlask(ModuleDefinition module) {
        var callback=FlaskResultCompletion(module);
        if(callback.Body.Instructions.Count!=1 || callback.Body.Instructions[0].OpCode!=OpCodes.Ret) {
            var calls=callback.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Select(m=>m.Name).ToArray();
            if(!calls.Contains("InitHeroLevelExp") || !calls.Contains("coUpdateHeroLevelExp") || calls.Any(n=>n=="Apply"))
                throw new InvalidOperationException("Unknown flask animation completion callback");
            // These calls only replace the result card's presentation. Rewards and
            // IsLevelUp are already applied independently by the native result path.
            callback.Body=new Mono.Cecil.Cil.MethodBody(callback);callback.Body.GetILProcessor().Emit(OpCodes.Ret);
        }
        VerifyBattleResultFlask(module);
    }
    static void VerifyBattleResultFlask(ModuleDefinition module) {
        var callback=FlaskResultCompletion(module);
        if(callback.Body.Instructions.Count!=1 || callback.Body.Instructions[0].OpCode!=OpCodes.Ret)
            throw new InvalidOperationException("Flask result completion still switches to hero EXP");
        var type=RewardHero(module);var setup=type.Methods.Single(m=>m.Name=="SetupUI");
        foreach(var name in new[]{"InitFlaskLevelExp","coUpdateFlaskLevelExp","InitHeroLevelExp","coUpdateHeroLevelExp","set_IsLevelUp"})
            if(!setup.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Missing native result path: "+name);
        var animation=type.NestedTypes.Single(t=>t.Name.StartsWith("<coUpdateFlaskLevelExp>"));
        var move=animation.Methods.Single(m=>m.Name=="MoveNext");
        foreach(var name in new[]{"Lerp","SetFlaskExp","fire"})
            if(!move.Body.Instructions.Any(i=>i.Operand is MethodReference m && m.Name==name))throw new InvalidOperationException("Missing native flask animation: "+name);
        VerifyArenaResultStack(callback);
        Console.WriteLine("Verified persistent battle-result flask bar; native fill animation, level-up state, and non-flask hero EXP paths preserved.");
    }
    static void VerifyBattleResultFlaskFile(string root,bool staged=false) {
        using var game=AssemblyDefinition.ReadAssembly(Path.Combine(root,"King's Raid_Data","Managed","Assembly-CSharp.dll"+(staged?".battle-result-flask-staged":"")));
        VerifyBattleResultFlask(game.MainModule);
    }
    static void InstallBattleResultFlask(string root,bool stageOnly) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));var dll=Path.Combine(managed,"Assembly-CSharp.dll");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using(var game=AssemblyDefinition.ReadAssembly(dll,new ReaderParameters{AssemblyResolver=resolver,InMemory=true})) {
            PatchBattleResultFlask(game.MainModule);PatchBattleResultFlask(game.MainModule);game.Write(dll+".battle-result-flask-staged");
        }
        VerifyBattleResultFlaskFile(root,true);if(stageOnly)return;
        if(System.Diagnostics.Process.GetProcesses().Any(p=>UsesClientFiles(p,root)))throw new InvalidOperationException("Flask result fix staged. Close the game and shared-asset workers before installation.");
        var backup=Path.Combine("target","battle-result-flask-backups",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));Directory.CreateDirectory(backup);File.Copy(dll,Path.Combine(backup,"Assembly-CSharp.dll"));
        File.Move(dll+".battle-result-flask-staged",dll,true);VerifyBattleResultFlaskFile(root);
    }
}
