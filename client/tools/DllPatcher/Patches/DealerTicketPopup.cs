using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchDealerTicketPopup(ModuleDefinition module)
    {
        var popupData = module.GetType("NShared.PurchaseMarketingPopupData");
        var marketingType = module.GetType("NShared.MarketingType");
        var ticketTypes = marketingType.Fields
            .Where(f => f.HasConstant && f.Name.StartsWith("Recommend", StringComparison.Ordinal))
            .Select(f => Convert.ToInt32(f.Constant)).OrderBy(value => value).ToArray();
        if (ticketTypes.Length == 0)
            throw new InvalidOperationException("Recommended ticket promotion types were not found.");
        var getter = popupData.Methods.Single(m => m.Name == "get_Type");
        var manager = module.GetType("NGame2.PurchaseMarketingManager");
        var window = module.GetType("NGame2.NUI.NWindow.PurchaseMarketingPopup");
        var methods = new[]
        {
            manager.Methods.Single(m => m.Name == "IsOpen"),
            manager.Methods.Single(m => m.Name == "OpenRecommendHeroPopup"),
            window.Methods.Single(m => m.Name == "Open" && m.IsStatic && m.Parameters.Count == 1 &&
                m.Parameters[0].ParameterType.FullName == popupData.FullName)
        };
        foreach (var method in methods)
        {
            var first = method.Body.Instructions.Take(5).ToArray();
            if (first.Length == 5 && first[0].OpCode == OpCodes.Ldarg_0 && first[1].OpCode == OpCodes.Brfalse
                && first[3].Operand is MethodReference call && call.FullName == getter.FullName && first[4].OpCode == OpCodes.Stloc)
            {
                // Extend earlier promotion guards, retaining the original method body.
                var originalStart = (Instruction)first[1].Operand;
                var existing = method.Body.Instructions.TakeWhile(i => i != originalStart).ToArray();
                if (existing.Where(i => i.OpCode == OpCodes.Ldc_I4).Select(i => (int)i.Operand).SequenceEqual(ticketTypes)) continue;
                if (existing.Length == 0 || existing.Last().OpCode != OpCodes.Ret)
                    throw new InvalidOperationException("Unrecognized existing promotion guard: " + method.FullName);
                foreach (var instruction in existing) method.Body.GetILProcessor().Remove(instruction);
            }
            var original = method.Body.Instructions[0];
            var type = new VariableDefinition(module.TypeSystem.Int32);
            method.Body.Variables.Add(type);
            method.Body.InitLocals = true;
            var blocked = method.ReturnType.MetadataType == MetadataType.Void
                ? Instruction.Create(OpCodes.Ret) : Instruction.Create(OpCodes.Ldc_I4_0);
            var guard = new List<Instruction>
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Brfalse, original),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Callvirt, getter),
                Instruction.Create(OpCodes.Stloc, type)
            };
            foreach (var ticketType in ticketTypes)
            {
                guard.Add(Instruction.Create(OpCodes.Ldloc, type));
                guard.Add(Instruction.Create(OpCodes.Ldc_I4, ticketType));
                guard.Add(Instruction.Create(OpCodes.Beq, blocked));
            }
            guard.Add(Instruction.Create(OpCodes.Br, original));
            guard.Add(blocked);
            if (method.ReturnType.MetadataType != MetadataType.Void) guard.Add(Instruction.Create(OpCodes.Ret));
            var il = method.Body.GetILProcessor();
            foreach (var instruction in guard) il.InsertBefore(original, instruction);
        }
        Console.WriteLine($"Removed all {ticketTypes.Length} Recommended ticket promotion types from popup triggers and direct opening.");
    }
}
