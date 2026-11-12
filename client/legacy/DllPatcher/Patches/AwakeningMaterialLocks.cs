using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static MethodDefinition AwakenMaterialEligibility(ModuleDefinition module) => module
        .GetType("NGame2.NContentsInfo.Item_Equip_MaxStar").Methods
        .Single(m => m.Name == "CanUse_as_AwakenMaterial" && m.IsStatic &&
            m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "NShared.EquipItemInfo");

    static void PatchAwakeningMaterialLocks(ModuleDefinition module)
    {
        var method = AwakenMaterialEligibility(module);
        if (method.Body.Instructions.Any(i => i.Operand is MethodReference { Name: "get_Locked" })) {
            VerifyAwakeningMaterialLocks(module);
            return;
        }
        var original = method.Body.Instructions.Select(i => (i.OpCode, i.Operand)).ToArray();
        var first = method.Body.Instructions[0];
        if (first.OpCode != OpCodes.Ldarg_0 || first.Next.OpCode != OpCodes.Brtrue_S ||
            first.Next.Next.OpCode != OpCodes.Ldc_I4_0 || first.Next.Next.Next.OpCode != OpCodes.Ret)
            throw new InvalidOperationException("Unexpected native awakening material null guard.");
        var locked = module.GetType("NShared.EquipItemInfo").Methods.Single(m => m.Name == "get_Locked");
        var il = method.Body.GetILProcessor();
        // Null still follows the native rejection path. Locking affects material
        // eligibility only: a locked item can still be the enhancement target.
        foreach (var instruction in new[] {
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Brfalse, first),
            Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Callvirt, locked),
            Instruction.Create(OpCodes.Brfalse, first), Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Ret)
        }) il.InsertBefore(first, instruction);
        if (!original.SequenceEqual(method.Body.Instructions.Skip(7).Select(i => (i.OpCode, i.Operand))))
            throw new InvalidOperationException("Awakening lock patch changed native eligibility checks.");
        VerifyAwakeningMaterialLocks(module);
    }

    static void VerifyAwakeningMaterialLocks(ModuleDefinition module)
    {
        var method = AwakenMaterialEligibility(module);
        var body = method.Body.Instructions;
        var expected = new[] { OpCodes.Ldarg_0, OpCodes.Brfalse, OpCodes.Ldarg_0, OpCodes.Callvirt,
            OpCodes.Brfalse, OpCodes.Ldc_I4_0, OpCodes.Ret };
        if (body.Count < 8 || !body.Take(7).Select(i => i.OpCode).SequenceEqual(expected) ||
            body[1].Operand != body[7] || body[4].Operand != body[7] ||
            body[3].Operand is not MethodReference getter || getter.Name != "get_Locked" ||
            getter.DeclaringType.FullName != "NShared.EquipItemInfo" || getter.ReturnType.FullName != "System.Byte")
            throw new InvalidOperationException("Awakening materials must reject every nonzero lock flag.");
        foreach (var name in new[] { "IsSoulWeapon", "get_ItemIndex", "get_ReqAwakeMaterialLevel", "get_Level" })
            if (!body.Skip(7).Any(i => i.Operand is MethodReference m && m.Name == name))
                throw new InvalidOperationException("Native awakening material check missing: " + name);
        var wrapper = module.GetType("__Extension_Method_ContentsInfo_Item_Equip_MaxStar__").Methods
            .Single(m => m.Name == "CLI_CanUse_as_AwakenMaterial");
        if (!wrapper.Body.Instructions.Any(i => i.Operand is MethodReference m && m.FullName == method.FullName))
            throw new InvalidOperationException("Client awakening eligibility bypasses the guarded native method.");
        var view = module.GetType("NGame2.NUI.NWindow.HeroEquipAwaken");
        foreach (var name in new[] { "OnClickEquipItem", "UpdateAwakeEquipItemListTask" })
            if (!view.Methods.Single(m => m.Name == name).Body.Instructions
                .Any(i => i.Operand is MethodReference m && m.FullName == wrapper.FullName))
                throw new InvalidOperationException("Awakening selection/visibility eligibility hook missing: " + name);
        VerifyArenaResultStack(method);
        TestAwakeningMaterialLockGuard(method);
    }

    static void TestAwakeningMaterialLockGuard(MethodDefinition method)
    {
        // Execute the installed guard's control flow, substituting only the
        // data-only lock getter and unchanged native eligibility boundary.
        foreach (var present in new[] { false, true })
        foreach (var locked in new[] { 0, 1, 2, 255 })
        foreach (var nativeEligible in new[] { false, true }) {
            var stack = new Stack<int>();
            var pc = method.Body.Instructions[0];
            bool? actual = null;
            while (actual == null) {
                if (pc == method.Body.Instructions[7]) {
                    if (stack.Count != 0) throw new InvalidOperationException("Unbalanced awakening guard.");
                    actual = present && nativeEligible;
                    break;
                }
                var next = pc.Next;
                switch (pc.OpCode.Code) {
                    case Code.Ldarg_0: stack.Push(present ? 1 : 0); break;
                    case Code.Ldc_I4_0: stack.Push(0); break;
                    case Code.Brfalse: if (stack.Pop() == 0) next = (Instruction)pc.Operand; break;
                    case Code.Callvirt:
                        if (stack.Pop() == 0) throw new InvalidOperationException("Null lock getter receiver.");
                        stack.Push(locked); break;
                    case Code.Ret: actual = stack.Pop() != 0; break;
                    default: throw new InvalidOperationException("Unexpected awakening lock guard IL: " + pc);
                }
                pc = next;
            }
            if (actual != (present && locked == 0 && nativeEligible) || stack.Count != 0)
                throw new InvalidOperationException($"Awakening lock regression: present={present}, locked={locked}, eligible={nativeEligible}");
        }
    }
}
