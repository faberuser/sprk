using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchPetContentsAvatarLookup(ModuleDefinition module)
    {
        PatchPetAvatarLookup(module, "GetPetContentsStatDataBaseList", "GetPetContentsStatDataBases", true);
        PatchPetAvatarLookup(module, "GetPetStatDataBaseList", "GetPetStatDataBases", false);
    }

    static void PatchPetAvatarLookup(ModuleDefinition module, string methodName, string cacheLookup, bool contents)
    {
        var method = module.GetType("NGame2.NPet.PetManager").Methods
            .Single(m => m.Name == methodName);
        var avatar = module.GetType("NGame2.NAccount.UserPetManager").Methods
            .Single(m => m.Name == "get_MainPetAvatarIndex");
        if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.FullName == avatar.FullName))
            return;

        // Owning pets does not imply that an avatar has been selected. The
        // original buff display dereferences a missing PetMiscInfo and aborts
        // battle setup. Reuse the native getter, which returns 0 for no avatar
        // and safely parses the stored index, preserving selected-pet bonuses.
        var lookup = method.Body.Instructions.Single(i => i.Operand is MethodReference m
            && m.Name == "GetPetMiscInfo");
        var start = lookup.Previous.Previous.Previous;
        var end = method.Body.Instructions.Single(i => i.Operand is MethodReference m
            && m.Name == cacheLookup);
        var span = new List<Instruction>();
        for (var i = start; i != end && i != null; i = i.Next) span.Add(i);
        if (span.Count != 9 || start.Operand is not MethodReference instance || instance.Name != "get_instance"
            || span[1].Operand is not MethodReference pets || pets.Name != "get_PetManager"
            || span[2].OpCode != OpCodes.Ldc_I4_1 || span[4].OpCode != (contents ? OpCodes.Stloc_2 : OpCodes.Stloc_1)
            || span[5].OpCode != OpCodes.Ldarg_0 || span[6].OpCode != (contents ? OpCodes.Ldloc_2 : OpCodes.Ldloc_1)
            || span[7].Operand is not MethodReference value || value.Name != "get_MiscValue"
            || span[8].Operand is not MethodReference parse || parse.FullName != "System.Int32 System.Int32::Parse(System.String)")
            throw new InvalidOperationException("Unexpected pet contents avatar lookup; client left unchanged.");

        foreach (var i in span) { i.OpCode = OpCodes.Nop; i.Operand = null; }
        span[0].OpCode = OpCodes.Ldarg_0;
        span[1].OpCode = OpCodes.Call; span[1].Operand = instance;
        span[2].OpCode = OpCodes.Callvirt; span[2].Operand = pets;
        span[3].OpCode = OpCodes.Callvirt; span[3].Operand = avatar;
        Console.WriteLine($"{methodName}: handle an unselected Lil' Raider avatar.");
    }
}
