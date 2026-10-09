using Mono.Cecil;

namespace DllPatcher;
partial class Program {
    static void PatchDirectAccessorySales(ModuleDefinition module) {
        // Apply after native table/getter restoration. The Dressing Room should
        // list and sell accessories that were previously reward/ownership-only.
        const string type="NShared.AccessoryCostumeData";
        PatchBooleanGetter(module,type,"get_IsOpen",true);
        PatchBooleanGetter(module,type,"get_IsBuy",true);
        PatchBooleanGetter(module,type,"get_PreviewableWhenOwned",false);
        // Preserve ReqBuyGem, compatibility and the normal per-hero ownership checks.
    }
}
