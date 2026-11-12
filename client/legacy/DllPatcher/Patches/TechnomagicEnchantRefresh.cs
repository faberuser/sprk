using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;
using System.Runtime.CompilerServices;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace DllPatcher;
partial class Program {
    const string TmEnchantTarget = "SprkCurrentEnchantTarget";
    static void PatchTechnomagicEnchantRefresh(ModuleDefinition game) {
        var window=game.GetType("NGame2.NUI.NWindow.ValanceEnchantWindow");
        var icon=game.GetType("NGame2.NUI.NComponent2.ItemIconComponent");
        var info=game.GetType("NShared.EquipItemInfo");
        var helper=window.Methods.SingleOrDefault(m=>m.Name==TmEnchantTarget);
        if(helper==null) {
            helper=new MethodDefinition(TmEnchantTarget,Mono.Cecil.MethodAttributes.Private|Mono.Cecil.MethodAttributes.Static,info);
            helper.Parameters.Add(new ParameterDefinition(icon));window.Methods.Add(helper);
        }
        var response=window.Methods.Single(m=>m.Name=="OnEnchantResponse");
        var instance=response.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Single(m=>m.Name=="get_instance");
        var account=game.GetType("NGame2.NAccount.AccountManager");
        var manager=game.GetType("NGame2.NAccount.UserEquipItemManager");
        var apply=manager.Methods.Single(m=>m.Name=="Apply" && m.Parameters.Count==2 && m.Parameters[0].ParameterType.MetadataType==MetadataType.Int32);
        var get=apply.Body.Instructions.Select(i=>i.Operand).OfType<MethodReference>().Single(m=>m.Name=="Get");
        helper.Body=new MethodBody(helper){InitLocals=true};helper.Body.Variables.Add(new VariableDefinition(info));
        var il=helper.Body.GetILProcessor();var lookup=Instruction.Create(OpCodes.Call,instance);var done=Instruction.Create(OpCodes.Ret);
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Callvirt,icon.Methods.Single(m=>m.Name=="get_EquipItemInfo"));il.Emit(OpCodes.Stloc_0);
        il.Emit(OpCodes.Ldloc_0);il.Emit(OpCodes.Brtrue,lookup);il.Emit(OpCodes.Ldnull);il.Emit(OpCodes.Ret);
        il.Append(lookup);il.Emit(OpCodes.Callvirt,account.Methods.Single(m=>m.Name=="get_EquipItemManager"));
        il.Emit(OpCodes.Ldloc_0);il.Emit(OpCodes.Callvirt,info.Methods.Single(m=>m.Name=="get_SlotIndex"));il.Emit(OpCodes.Callvirt,get);
        il.Emit(OpCodes.Dup);il.Emit(OpCodes.Brtrue,done);il.Emit(OpCodes.Pop);il.Emit(OpCodes.Ldloc_0);il.Append(done);
        // Inventory snapshots can replace the stored object while the pooled
        // target icon still holds its old reference. Resolve by stable slot for
        // first rolls, subsequent comparisons, confirms and target refreshes.
        foreach(var method in window.Methods.Where(m=>m.HasBody && m!=helper))
            foreach(var call in method.Body.Instructions.Where(i=>i.Operand is MethodReference r && r.DeclaringType.FullName==icon.FullName && r.Name=="get_EquipItemInfo")) {
                call.OpCode=OpCodes.Call;call.Operand=helper;
            }
        VerifyTechnomagicEnchantRefresh(game);
    }
    static void VerifyTechnomagicEnchantRefresh(ModuleDefinition game) {
        var window=game.GetType("NGame2.NUI.NWindow.ValanceEnchantWindow");
        var helper=window.Methods.Single(m=>m.Name==TmEnchantTarget);
        foreach(var name in new[]{"OnClickEnchant","OnEnchantResponse","OnEnchantConfirmResponse","Refresh"}) {
            var method=window.Methods.Single(m=>m.Name==name);
            if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name==TmEnchantTarget) ||
               method.Body.Instructions.Any(i=>i.Operand is MethodReference r && r.Name=="get_EquipItemInfo"))
                throw new InvalidOperationException("Stale TM enchant target remains in "+name);
            VerifyArenaResultStack(method);
        }
        VerifyArenaResultStack(helper);
        Console.WriteLine("Verified TM enchant rolls, comparisons, confirms and refresh use the current equipment slot.");
    }
    static void InstallTechnomagicEnchantRefresh(string root,bool stageOnly,bool verifyOnly=false) {
        var managed=Path.GetFullPath(Path.Combine(root,"King's Raid_Data","Managed"));var path=Path.Combine(managed,"Assembly-CSharp.dll");
        using var resolver=new DefaultAssemblyResolver();resolver.AddSearchDirectory(managed);
        using var game=AssemblyDefinition.ReadAssembly(path,new ReaderParameters{InMemory=true,AssemblyResolver=resolver});
        if(verifyOnly) {VerifyTechnomagicEnchantRefresh(game.MainModule);TestTechnomagicEnchantRefresh(game,managed);return;}
        PatchTechnomagicEnchantRefresh(game.MainModule);
        var count=game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count);
        PatchTechnomagicEnchantRefresh(game.MainModule);
        if(count!=game.MainModule.GetTypes().SelectMany(t=>t.Methods).Where(m=>m.HasBody).Sum(m=>m.Body.Instructions.Count))throw new InvalidOperationException("TM enchant patch is not idempotent");
        TestTechnomagicEnchantRefresh(game,managed);
        var staged=path+".tm-enchant-staged";game.Write(staged);
        using(var check=AssemblyDefinition.ReadAssembly(staged))VerifyTechnomagicEnchantRefresh(check.MainModule);
        if(stageOnly)return;
        foreach(var process in System.Diagnostics.Process.GetProcesses())using(process)
            if(UsesClientFiles(process,root))throw new InvalidOperationException("TM enchant fix staged. Close clients and shared workers before installation.");
        File.Copy(path,path+".before-tm-enchant-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));File.Move(staged,path,true);
        Console.WriteLine("Installed TM enchant target refresh. Restart the client.");
    }
}
