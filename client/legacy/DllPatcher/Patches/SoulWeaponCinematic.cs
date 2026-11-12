using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    const string SoulCinematicType = "NGame2.NUI.NWindow.NSoulWeapon.SoulWeaponLiberationResultWindow";

    static void VerifySoulWeaponCinematic(ModuleDefinition module)
    {
        var type = module.GetType(SoulCinematicType);
        var play = type.Methods.Single(m => m.Name == "SetNodeTreePlay");
        var calls = play.Body.Instructions.Select(i => i.Operand).OfType<MethodReference>().ToArray();
        foreach (var name in new[] { "AddSoulStone", "PlayOnce", "PlaySequence", "PlaySoulWeaponNodeTree", "Close" })
            if (!calls.Any(m => m.Name == name))
                throw new InvalidOperationException("Native Soul Weapon cinematic step missing: " + name);
        if (!play.Body.Instructions.Any(i => i.OpCode == OpCodes.Switch))
            throw new InvalidOperationException("Soul Weapon cinematic state machine is missing.");
        var methods = type.Methods.Concat(type.NestedTypes.SelectMany(t => t.Methods)).Where(m => m.HasBody).ToArray();
        var callbacks = methods.SelectMany(m => m.Body.Instructions).ToArray();
        foreach (var name in new[] { "PlayFadeOutEffect", "SetAssetBgmVolume", "OnOffTouchRoot", "OnOffDescRoot", "SetupDescSide" })
            if (!callbacks.Any(i => i.Operand is MethodReference call && call.Name == name))
                throw new InvalidOperationException("Native cinematic completion/description callback missing: " + name);
        if (!callbacks.Any(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference { Name: "IsWaitingResult" } && i.Previous.OpCode == OpCodes.Ldc_I4_0))
            throw new InvalidOperationException("Cinematic completion cannot enable dismissal.");
    }

    static void VerifySoulWeaponCinematicFile(string clientRoot, bool staged)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed",
            "Assembly-CSharp.dll" + (staged ? ".soul-cinematic-staged" : "")));
        VerifySoulWeaponCinematic(assembly.MainModule);
        Console.WriteLine("Verified full native Soul Weapon cinematic, fade/audio callbacks, descriptions and dismissal.");
    }

    static void RestoreSoulWeaponCinematic(string clientRoot, string sourcePath, bool stageOnly)
    {
        var dll = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        using var source = AssemblyDefinition.ReadAssembly(sourcePath, new ReaderParameters { AssemblyResolver = resolver });
        VerifySoulWeaponCinematic(source.MainModule);
        var original = source.MainModule.GetType(SoulCinematicType).Methods.Single(m => m.Name == "SetNodeTreePlay");
        var staged = dll + ".soul-cinematic-staged";
        using (var target = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver })) {
            var module = target.MainModule;
            var method = module.GetType(SoulCinematicType).Methods.Single(m => m.Name == original.Name);
            method.Body = new MethodBody(method) { InitLocals = original.Body.InitLocals, MaxStackSize = original.Body.MaxStackSize };
            foreach (var variable in original.Body.Variables)
                method.Body.Variables.Add(new VariableDefinition(module.ImportReference(variable.VariableType)));
            var copies = original.Body.Instructions.ToDictionary(i => i, _ => Instruction.Create(OpCodes.Nop));
            foreach (var instruction in original.Body.Instructions) {
                var copy = copies[instruction]; copy.OpCode = instruction.OpCode;
                copy.Operand = instruction.Operand switch {
                    Instruction branch => copies[branch], Instruction[] branches => branches.Select(b => copies[b]).ToArray(),
                    VariableDefinition variable => method.Body.Variables[variable.Index],
                    ParameterDefinition parameter => method.Parameters[parameter.Index],
                    MethodReference call when call.DeclaringType.Scope == source.MainModule =>
                        (MethodReference?)module.GetTypes().SelectMany(t => t.Methods).FirstOrDefault(m => m.FullName == call.FullName) ?? module.ImportReference(call),
                    FieldReference field when field.DeclaringType.Scope == source.MainModule =>
                        module.GetTypes().SelectMany(t => t.Fields).Single(f => f.FullName == field.FullName),
                    MethodReference call => module.ImportReference(call), FieldReference field => module.ImportReference(field),
                    TypeReference type => module.ImportReference(type), var value => value
                };
                method.Body.Instructions.Add(copy);
            }
            if (original.Body.HasExceptionHandlers) throw new InvalidOperationException("Unexpected cinematic exception handlers.");
            VerifySoulWeaponCinematic(module); target.Write(staged);
        }
        VerifySoulWeaponCinematicFile(clientRoot, true);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcessesByName("King's Raid").Length != 0)
            throw new InvalidOperationException("Native cinematic staged. Close King's Raid before installing.");
        var backup = Path.Combine("target", "soul-cinematic-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup); File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        File.Move(staged, dll, true); VerifySoulWeaponCinematicFile(clientRoot, false);
    }
}
