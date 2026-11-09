using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static readonly string[] SelectorAcquireTypes = {
        "NGame2.NUI.NWindow.WeaponUniqueSelect",
        "NGame2.NUI.NWindow.SoulStoneSelect"
    };
    const string SelectorAcquireStageSuffix = ".selector-acquire-state-staged";

    static (MethodDefinition Method, Instruction Gray)[] SelectorAcquireTargets(ModuleDefinition module)
    {
        return SelectorAcquireTypes.Select(name => {
            var type = module.GetType(name) ?? throw new InvalidOperationException("Missing selector type: " + name);
            var methods = type.Methods.Where(m => m.Name == "SetItemDescription" && !m.IsStatic &&
                m.ReturnType.FullName == "System.Void" && m.Parameters.Count == 1 &&
                m.Parameters[0].ParameterType.FullName == "NShared.ItemData" && m.HasBody).ToArray();
            if (methods.Length != 1)
                throw new InvalidOperationException("Expected one native SetItemDescription(ItemData): " + name);
            var method = methods[0];
            var calls = method.Body.Instructions.Where(i => i.Operand is MethodReference m &&
                m.DeclaringType.FullName == "NGame2.NUI.UIHelper" && m.Name == "EnableButton").ToArray();
            if (calls.Length != 1)
                throw new InvalidOperationException("Expected one native selector EnableButton call: " + name);
            var call = calls[0];
            var enable = (MethodReference)call.Operand;
            var forceGray = call.Previous;
            var gray = forceGray?.Previous;
            var enabled = gray?.Previous;
            var button = enabled?.Previous;
            var receiver = button?.Previous;
            if (call.OpCode != OpCodes.Call || enable.HasThis || enable.ReturnType.FullName != "System.Void" ||
                enable.Parameters.Count != 4 || enable.Parameters[0].ParameterType.FullName != "UIButton" ||
                enable.Parameters.Skip(1).Any(p => p.ParameterType.FullName != "System.Boolean") ||
                receiver?.OpCode != OpCodes.Ldarg_0 || button?.OpCode != OpCodes.Ldfld ||
                button.Operand is not FieldReference field || field.Name != "Button_Buy" ||
                field.DeclaringType.FullName != SelectorAcquireTypes[0] || field.FieldType.FullName != "UIButton" ||
                enabled?.OpCode != OpCodes.Ldc_I4_1 || forceGray?.OpCode != OpCodes.Ldc_I4_0 ||
                gray == null || (gray.OpCode != OpCodes.Ldc_I4_0 && gray.OpCode != OpCodes.Ldc_I4_1) ||
                call.Next?.OpCode != OpCodes.Ret)
                throw new InvalidOperationException("Unexpected selector Acquire argument shape: " + name);

            // No alternate entry may bypass part of the validated argument load.
            var arguments = new[] { button, enabled, gray, forceGray, call };
            if (method.Body.Instructions.Any(i => i.Operand is Instruction target && arguments.Contains(target) ||
                i.Operand is Instruction[] targets && targets.Any(arguments.Contains)))
                throw new InvalidOperationException("Branch into selector Acquire arguments: " + name);
            VerifyArenaResultStack(method);
            return (method, gray);
        }).ToArray();
    }

    static void PatchSelectorAcquireState(ModuleDefinition module)
    {
        // Validate both overrides before mutating either. Keep every instruction,
        // branch and native empty/null path; only opt into grayscale refresh.
        foreach (var target in SelectorAcquireTargets(module))
            target.Gray.OpCode = OpCodes.Ldc_I4_1;
    }

    static void VerifySelectorAcquireState(ModuleDefinition module)
    {
        var targets = SelectorAcquireTargets(module);
        if (targets.Any(t => t.Gray.OpCode != OpCodes.Ldc_I4_1))
            throw new InvalidOperationException("Selector Acquire must enable with grayscale refresh: (true, true, false).");
        var before = targets.SelectMany(t => t.Method.Body.Instructions.Select(i => (i.OpCode, i.Operand))).ToArray();
        PatchSelectorAcquireState(module);
        var after = targets.SelectMany(t => t.Method.Body.Instructions.Select(i => (i.OpCode, i.Operand))).ToArray();
        if (!before.SequenceEqual(after))
            throw new InvalidOperationException("Selector Acquire state patch is not idempotent.");
    }

    static void VerifySelectorAcquireStateFile(string root, bool staged = false)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll") +
            (staged ? SelectorAcquireStageSuffix : ""), new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        VerifySelectorAcquireState(game.MainModule);
        Console.WriteLine("PASS: both selector Acquire calls use (true, true, false); IL stack and idempotence verified.");
    }

    static void InstallSelectorAcquireState(string root, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + SelectorAcquireStageSuffix;
        using (var resolver = new DefaultAssemblyResolver())
        {
            resolver.AddSearchDirectory(managed);
            // Always start from the current DLL, never a baseline or backup, so
            // focused installation retains all previously installed patches.
            using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
            PatchSelectorAcquireState(game.MainModule);
            VerifySelectorAcquireState(game.MainModule);
            game.Write(staged);
        }
        VerifySelectorAcquireStateFile(root, true);
        if (stageOnly)
        {
            Console.WriteLine("Staged selector Acquire state fix: " + staged);
            return;
        }
        foreach (var process in System.Diagnostics.Process.GetProcesses())
            using (process)
                if (UsesClientFiles(process, root))
                    throw new InvalidOperationException("Selector Acquire fix staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll, dll + ".before-selector-acquire-state-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff"));
        File.Move(staged, dll, true);
        VerifySelectorAcquireStateFile(root);
        Console.WriteLine("Installed selector Acquire state fix. Restart the client.");
    }
}
