using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static MethodDefinition ArenaResultMethod(ModuleDefinition module) => module
        .GetType("NGame2.NUI.NManager.NBattle.ArenaManagement")
        .Methods.Single(m => m.Name == "OpenPvpResult_Normal" && m.Parameters.Count == 1);

    static void PatchArenaRematchPrompt(ModuleDefinition module)
    {
        var method = ArenaResultMethod(module);
        foreach (var call in method.Body.Instructions.Where(i => i.Operand is MethodReference m &&
            m.DeclaringType.FullName == "NGame2.NMatch.RematchManager" && m.Name == "TryOpenRematchPopup").ToArray())
        {
            var argument = call.Previous;
            var singleton = argument?.Previous;
            var target = (MethodReference)call.Operand;
            var discard = target.ReturnType.MetadataType == MetadataType.Void ? null : call.Next;
            if (argument?.OpCode != OpCodes.Ldarg_1 || singleton?.OpCode != OpCodes.Call ||
                singleton.Operand is not MethodReference getter || getter.Name != "get_instance" ||
                !getter.DeclaringType.FullName.Contains("NGame2.NMatch.RematchManager") ||
                (target.ReturnType.MetadataType != MetadataType.Void && discard?.OpCode != OpCodes.Pop))
                throw new InvalidOperationException("Unrecognized arena rematch prompt call; refusing to alter results.");
            // Remove only the optional automatic rematch prompt. Keep result
            // display, battle-end events, rewards and tier-change handling.
            foreach (var instruction in new[] { singleton, argument, call })
            {
                instruction.OpCode = OpCodes.Nop;
                instruction.Operand = null;
            }
            if (discard != null) { discard.OpCode = OpCodes.Nop; discard.Operand = null; }
        }
        // Repair the earlier three-NOP patch: TryOpenRematchPopup returns bool,
        // so removing its call must also remove the following result discard.
        foreach (var discard in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Pop &&
            i.Next?.OpCode == OpCodes.Ret && i.Previous?.OpCode == OpCodes.Nop &&
            i.Previous.Previous?.OpCode == OpCodes.Nop && i.Previous.Previous.Previous?.OpCode == OpCodes.Nop).ToArray())
            discard.OpCode = OpCodes.Nop;
        VerifyArenaRematchPrompt(module);
    }

    static void VerifyArenaRematchPrompt(ModuleDefinition module)
    {
        var method = ArenaResultMethod(module);
        VerifyArenaResultStack(method);
        var calls = method.Body.Instructions
            .Select(i => i.Operand).OfType<MethodReference>().ToArray();
        if (calls.Any(m => m.Name == "TryOpenRematchPopup") ||
            !calls.Any(m => m.Name == "Open_Pvp") || !calls.Any(m => m.Name == "ShowTierChangePopup"))
            throw new InvalidOperationException("Arena must show its normal results without the automatic rematch prompt.");
    }

    static void VerifyArenaResultStack(MethodDefinition method)
    {
        var pending = new Queue<(Instruction Instruction, int Depth)>();
        var depths = new Dictionary<Instruction, int>();
        pending.Enqueue((method.Body.Instructions[0], 0));
        while (pending.TryDequeue(out var entry))
        {
            var (instruction, depth) = entry;
            if (depths.TryGetValue(instruction, out var prior))
            {
                if (prior != depth) throw new InvalidOperationException("Inconsistent arena result stack at " + instruction);
                continue;
            }
            depths[instruction] = depth;
            var pop = instruction.OpCode.StackBehaviourPop;
            int consumed;
            if (pop == StackBehaviour.Varpop)
            {
                consumed = instruction.OpCode == OpCodes.Ret ? (method.ReturnType.MetadataType == MetadataType.Void ? 0 : 1) :
                    instruction.Operand is MethodReference called ? called.Parameters.Count +
                        (called.HasThis && instruction.OpCode != OpCodes.Newobj ? 1 : 0) :
                    throw new InvalidOperationException("Unsupported arena result instruction: " + instruction);
            }
            else consumed = pop == StackBehaviour.Pop0 ? 0 : pop.ToString().Split('_').Length;
            if (depth < consumed) throw new InvalidOperationException("Arena result stack underflow at " + instruction);
            depth -= consumed;
            var push = instruction.OpCode.StackBehaviourPush;
            if (push == StackBehaviour.Varpush)
            {
                if (instruction.OpCode == OpCodes.Newobj || instruction.Operand is MethodReference called && called.ReturnType.MetadataType != MetadataType.Void) depth++;
            }
            else if (push != StackBehaviour.Push0) depth += push.ToString().Split('_').Length;
            if (instruction.OpCode.FlowControl is FlowControl.Return or FlowControl.Throw)
            {
                if (depth != 0) throw new InvalidOperationException("Unbalanced arena result exit at " + instruction);
                continue;
            }
            if (instruction.Operand is Instruction branch) pending.Enqueue((branch, depth));
            if (instruction.Operand is Instruction[] branches) foreach (var target in branches) pending.Enqueue((target, depth));
            if (instruction.OpCode.FlowControl != FlowControl.Branch && instruction.Next != null) pending.Enqueue((instruction.Next, depth));
        }
    }

    static void VerifyArenaRematchFile(string clientRoot)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        VerifyArenaRematchPrompt(assembly.MainModule);
        Console.WriteLine("Verified arena results open without the Duel again prompt.");
    }

    static void InstallArenaRematchRemoval(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".arena-rematch-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchArenaRematchPrompt(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var verified = AssemblyDefinition.ReadAssembly(staged)) VerifyArenaRematchPrompt(verified.MainModule);
        if (!stageOnly)
        {
            var backup = Path.Combine("target", "arena-rematch-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified arena rematch prompt removal." : "Installed arena rematch prompt removal. Restart the client.");
    }
}
