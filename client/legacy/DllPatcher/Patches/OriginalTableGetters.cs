using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    // Exact archive restoration must read archived flags, rather than the older
    // local patch that forced every costume open/buyable and removed previews.
    static void RestoreOriginalTableGetters(ModuleDefinition module)
    {
        foreach(string name in new[]{"CreatureData","CostumeData","HairCostumeData","WeaponCostumeData","AccessoryCostumeData"})
        {
            var type=module.GetType("NShared."+name);
            foreach(string property in new[]{"OpenType","IsOpen","IsBuy","IsShow","PreviewableWhenOwned","PreviewableWhenNeedCostumeOwned","PreviewableWhenNeedHairOwned","PreviewableWhenNeedWeaponOwned"})
            {
                var field=type.Fields.FirstOrDefault(f=>f.Name=="<"+property+">k__BackingField");
                var getter=type.Methods.FirstOrDefault(m=>m.Name=="get_"+property);
                if(field==null||getter==null)continue;
                getter.Body=new MethodBody(getter);
                var il=getter.Body.GetILProcessor();il.Emit(OpCodes.Ldarg_0);il.Emit(OpCodes.Ldfld,field);il.Emit(OpCodes.Ret);
            }
        }
        Console.WriteLine("Restored archived hero/costume flags through their table-backed getters.");
    }
}
