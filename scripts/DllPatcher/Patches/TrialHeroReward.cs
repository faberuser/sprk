using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchTrialHeroReward(ModuleDefinition module)
    {
        var manager = module.GetType("NGame2.NUI.NManager.NBattle.EndCampaign");
        var method = manager.Methods.Single(m => m.Name == "ReserveGetNewHeroPopup");
        var battleType = module.GetType("NShared.BattleType");
        int trial = Convert.ToInt32(battleType.Fields.Single(f => f.Name == "Trial").Constant);
        var stock = manager.Fields.Single(f => f.Name == "_rewardStock");
        var entry = stock.FieldType.Resolve().Fields.Single(f => f.Name == "EnterDungeonData");
        var dungeon = module.GetType("NGame2.NBattleContext.EnterDungeonData").Methods.Single(m => m.Name == "get_LinkedDungeonData");
        var type = module.GetType("NShared.CampaignDungeonData").Methods.Single(m => m.Name == "get_BattleType");
        if (method.Body.Instructions.Take(30).Any(i => i.Operand is MethodReference call && call.FullName == type.FullName)) return;
        var original = method.Body.Instructions[0];
        var il = method.Body.GetILProcessor();
        // Older servers send an existing trial hero in the acquisition field.
        // Keep account updates and purification rewards, but never reserve a recruitment popup for a trial.
        var guard = new[]
        {
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, stock), Instruction.Create(OpCodes.Brfalse, original),
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, stock), Instruction.Create(OpCodes.Ldfld, entry), Instruction.Create(OpCodes.Brfalse, original),
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, stock), Instruction.Create(OpCodes.Ldfld, entry), Instruction.Create(OpCodes.Callvirt, dungeon), Instruction.Create(OpCodes.Brfalse, original),
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, stock), Instruction.Create(OpCodes.Ldfld, entry), Instruction.Create(OpCodes.Callvirt, dungeon),
            Instruction.Create(OpCodes.Callvirt, type), Instruction.Create(OpCodes.Ldc_I4, trial), Instruction.Create(OpCodes.Bne_Un, original), Instruction.Create(OpCodes.Ret)
        };
        foreach (var instruction in guard) il.InsertBefore(original, instruction);
        Console.WriteLine("Room of Ordeals retains purification rewards without reserving a new-hero acquisition animation.");
    }
}
