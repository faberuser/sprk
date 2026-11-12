using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchVoiceLanguage(ModuleDefinition module)
    {
        var component = module.GetType("NGame2.NUI.NComponent2.GameOptionToggle4Component");
        var method = component.Methods.Single(m => m.Name == "SetValueByParent");
        if (method.Body.Instructions.Any(i => Equals(i.Operand, "Graphic_ResolutionControl"))) return;

        // The native 4K availability check runs for every four-choice setting.
        // Turning character voices off therefore hides Japanese (choice four)
        // on non-4K displays, and turning voices back on never restores it.
        var resolutions = method.Body.Instructions.Single(i => i.Operand is MethodReference m
            && m.DeclaringType.FullName == "UnityEngine.Screen" && m.Name == "get_resolutions");
        var end = method.Body.Instructions.Last();
        if (end.OpCode != OpCodes.Ret || method.Body.Instructions.Any(i =>
            ReferenceEquals(i.Operand, resolutions)))
            throw new InvalidOperationException("Unexpected four-choice settings control flow");
        var data = component.BaseType.Resolve().Fields.Single(f => f.Name == "gameOptionData");
        var fieldName = module.GetType("NShared.GameOptionData").Methods.Single(m => m.Name == "get_FieldName");
        var equals = (MethodReference)method.Body.Instructions.Single(i => i.Operand is MethodReference m
            && m.DeclaringType.FullName == "System.String" && m.Name == "op_Equality").Operand;
        var il = method.Body.GetILProcessor();
        foreach (var instruction in new[] {
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, data),
            Instruction.Create(OpCodes.Callvirt, fieldName),
            Instruction.Create(OpCodes.Ldstr, "Graphic_ResolutionControl"),
            Instruction.Create(OpCodes.Call, equals),
            Instruction.Create(OpCodes.Brfalse, end),
        }) il.InsertBefore(resolutions, instruction);
        Console.WriteLine("Restricted the 4K choice filter to screen resolution; Japanese voice remains available.");
    }

    static void InstallVoiceLanguage(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchVoiceLanguage(assembly.MainModule);
            assembly.Write(dll + ".voice-language-staged");
        }
        if (stageOnly) return;
        File.Copy(dll, dll + ".before-voice-language-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        File.Copy(dll + ".voice-language-staged", dll, true);
        Console.WriteLine("Installed voice language fix. Restart the game to apply it.");
    }
}
