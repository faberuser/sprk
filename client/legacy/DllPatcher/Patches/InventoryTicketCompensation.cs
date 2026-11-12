using Mono.Cecil;
using Mono.Cecil.Cil;
namespace DllPatcher;
partial class Program {
    static void PatchInventoryTicketCompensation(ModuleDefinition module,ModuleDefinition helper) {
        var type=helper.GetType("SprkInventoryTickets");
        var method=module.GetType("NGame2.NUI.NManager.InventoryManagement").Methods.Single(m=>m.Name=="OnUseExtendEquip");
        if(!method.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name==type.Name)) {
            var first=method.Body.Instructions[0];var il=method.Body.GetILProcessor();
            foreach(var i in new[]{Instruction.Create(OpCodes.Ldarg_1),Instruction.Create(OpCodes.Call,module.ImportReference(type.Methods.Single(m=>m.Name=="TryUse"))),Instruction.Create(OpCodes.Brfalse,first),Instruction.Create(OpCodes.Ret)})il.InsertBefore(first,i);
        }
        var encoder=module.GetType("JM_NShared_UsePotionItem_Request").Methods.Single(m=>m.Name=="EncodeJsonObject");
        if(!encoder.Body.Instructions.Any(i=>i.Operand is MethodReference m&&m.DeclaringType.Name==type.Name)) {
            var ret=encoder.Body.Instructions.Last(i=>i.OpCode==OpCodes.Ret);var il=encoder.Body.GetILProcessor();
            ret.OpCode=OpCodes.Ldarg_0;
            il.Append(il.Create(OpCodes.Call,module.ImportReference(type.Methods.Single(m=>m.Name=="AddParameters"))));
            il.Append(il.Create(OpCodes.Ret));
        }
    }
    static void VerifyInventoryTicketHooks(ModuleDefinition module) {
        foreach(var pair in new[]{("NGame2.NUI.NManager.InventoryManagement","OnUseExtendEquip","TryUse"),("JM_NShared_UsePotionItem_Request","EncodeJsonObject","AddParameters")}) {
            var method=module.GetType(pair.Item1).Methods.Single(m=>m.Name==pair.Item2);
            if(method.Body.Instructions.Count(i=>i.Operand is MethodReference m&&m.DeclaringType.Name=="SprkInventoryTickets"&&m.Name==pair.Item3)!=1)
                throw new InvalidOperationException("Missing/duplicated inventory ticket hook: "+pair.Item2);
        }
        var entry=module.GetType("NGame2.NUI.NManager.InventoryManagement").Methods.Single(m=>m.Name=="OnUseExtendEquip").Body.Instructions;
        if(entry[0].OpCode!=OpCodes.Ldarg_1 || entry[2].OpCode!=OpCodes.Brfalse || entry[3].OpCode!=OpCodes.Ret)
            throw new InvalidOperationException("Ordinary inventory ticket behavior must retain its native fallback.");
        VerifyArenaResultStack(module.GetType("JM_NShared_UsePotionItem_Request").Methods.Single(m=>m.Name=="EncodeJsonObject"));
    }
}
