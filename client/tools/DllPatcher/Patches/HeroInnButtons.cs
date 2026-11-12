using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static bool PatchHeroInnButtons(ModuleDefinition module, TypeDefinition? componentType, TypeDefinition? gameObjectType)
    {
        var group = module.GetType("NGame2.NUI.NWindow.HeroInnViewButtonGroup");
        var listener = group.Methods.Single(m => m.Name == "SetListener");
        // September 2022 SetListener only stores the listener. ActionType and
        // visibility belong to each prefab group, not to a shared 1/2/3 override.
        listener.Body = new MethodBody(listener);
        var il = listener.Body.GetILProcessor();
        il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, group.Fields.Single(f => f.Name == "_listener"));
        il.Emit(OpCodes.Ret);
        foreach (var legacy in group.Methods.Where(m => m.Name == "HideUnsupportedButtons").ToArray())
            group.Methods.Remove(legacy);
        Console.WriteLine("Restored native Inn button listeners and prefab action types.");
        return true;
    }
}
