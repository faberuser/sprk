using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program {
    static void PatchDirectAccessorySales(ModuleDefinition module) {
        // Apply after native table/getter restoration. The Dressing Room should
        // list and sell accessories that were previously reward/ownership-only.
        const string type="NShared.AccessoryCostumeData";
        PatchBooleanGetter(module,type,"get_IsOpen",true);
        PatchBooleanGetter(module,type,"get_IsBuy",true);
        PatchBooleanGetter(module,type,"get_PreviewableWhenOwned",false);
        // Read archived backing fields: the public availability getters are patched.
        var accessory=module.GetType(type);
        var getter=accessory.Methods.Single(m=>m.Name=="get_ReqBuyGem");
        FieldDefinition Field(string name)=>accessory.Fields.Single(f=>f.Name=="<"+name+">k__BackingField");
        getter.Body=new MethodBody(getter);var il=getter.Body.GetILProcessor();
        var original=Instruction.Create(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,Field("IsBuy"));il.Emit(OpCodes.Brtrue,original);
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,Field("PreviewableWhenOwned"));il.Emit(OpCodes.Brfalse,original);
        il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,Field("ReqBuyGem"));il.Emit(OpCodes.Ldc_I4,10000);il.Emit(OpCodes.Bne_Un,original);
        foreach(var (part,price) in new[]{(1,1000),(2,500),(3,500),(4,4000)}) {
            var next=Instruction.Create(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,Field("PartType"));il.Emit(OpCodes.Ldc_I4,part);il.Emit(OpCodes.Bne_Un,next);
            il.Emit(OpCodes.Ldc_I4,price);il.Emit(OpCodes.Ret);il.Append(next);
        }
        il.Append(original);il.Emit(OpCodes.Ldfld,Field("ReqBuyGem"));il.Emit(OpCodes.Ret);
    }
}
