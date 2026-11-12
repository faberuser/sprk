using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static bool PatchHeroInnRecruiting(TypeDefinition view)
    {
        var method = view.Methods.Single(m => m.Name == "InitUIBtnGroup");
        var have = method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().Single(m => m.Name == "IsHaveHero");
        var selected = method.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().Single(m => m.Name == "IsSelectedHero");
        var set = view.Methods.Single(m => m.Name == "SetButtonGroup" && m.Parameters.Count == 2);
        var group = view.Module.GetType("NGame2.NUI.NWindow.HeroInnViewButtonGroup");
        method.Body = new MethodBody(method) { InitLocals = true };
        method.Body.Variables.Add(new VariableDefinition(view.NestedTypes.Single(t => t.Name == "EGroupButtonType")));
        var il = method.Body.GetILProcessor();
        var owned = Instruction.Create(OpCodes.Ldc_I4_2);
        var store = Instruction.Create(OpCodes.Stloc_0);
        // Pre-doomsday: owned -> Closeness; selected recruit -> Friendly;
        // unselected recruit -> None. Affinity never follows ownership alone.
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, have); il.Emit(OpCodes.Brtrue, owned);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Call, selected); il.Emit(OpCodes.Br, store);
        il.Append(owned); il.Append(store);
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldloc_0); il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Call, set);
        foreach (var field in new[] { "FriendlyBtnGroup", "ClosenessBtnGroup", "ConnectBtnGroup" })
        {
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld, view.Fields.Single(f => f.Name == field));
            il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Callvirt, group.Methods.Single(m => m.Name == "SetListener"));
        }
        il.Emit(OpCodes.Ret);
        Console.WriteLine("Restored pre-doomsday Inn group selection.");
        return true;
    }
}
