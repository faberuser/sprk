using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static TypeDefinition ArenaContextType(ModuleDefinition module) => module.GetType("NGame2.NBattleContext.ArenaContext");

    static void PatchArenaResponsiveness(ModuleDefinition module)
    {
        var context = ArenaContextType(module);
        var result = context.Methods.Single(m => m.Name == "OnBattleResult");
        var wait = context.Methods.Single(m => m.Name == "WaitBattleEnd");
        var initialDelay = result.Body.Instructions.Single(i => i.OpCode == OpCodes.Ldc_R4);
        if ((float)initialDelay.Operand != 0.5f && (float)initialDelay.Operand != 0.05f)
            throw new InvalidOperationException("Unrecognized arena result delay.");
        // Leave a frame for the Quit acknowledgement/UI callback to drain.
        initialDelay.Operand = 0.05f;

        if (!wait.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "get_Dropped"))
        {
            // Dropped is set for both players on a server-confirmed forfeit or
            // cancellation. Combat cannot end naturally after its worker stops.
            // Ordinary victories still wait for the ordered replay to finish.
            var first = wait.Body.Instructions[0];
            var il = wait.Body.GetILProcessor();
            var dropped = module.GetType("NShared.BattleResultNot").Methods.Single(m => m.Name == "get_Dropped");
            foreach (var i in new[] {
                Instruction.Create(OpCodes.Ldarg_1), Instruction.Create(OpCodes.Callvirt, dropped),
                Instruction.Create(OpCodes.Brfalse, first), Instruction.Create(OpCodes.Ldc_I4_0),
                Instruction.Create(OpCodes.Starg, wait.Parameters[1])
            }) il.InsertBefore(first, i);
        }
        var delays = wait.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldc_R4).ToArray();
        if (delays.Length != 2 || delays.Any(i => (float)i.Operand != 0.5f && (float)i.Operand != 0.05f && (float)i.Operand != 0.1f))
            throw new InvalidOperationException("Unrecognized arena replay/result delays.");
        delays[0].Operand = 0.05f; // after socket close, before fetching persisted rewards
        delays[1].Operand = 0.1f;  // observe replay completion more frequently
        var callback = context.NestedTypes.SelectMany(t => t.Methods).Single(m => m.HasBody &&
            m.Name.Contains("<OnBattleResult>") && m.Body.Instructions.Any(i => i.Operand is MethodReference called && called.Name == "WaitBattleEnd"));
        var count = callback.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "WaitBattleEnd").Previous;
        if (count.OpCode != OpCodes.Ldc_I4_5 && !(count.OpCode == OpCodes.Ldc_I4 && (int)count.Operand == 25))
            throw new InvalidOperationException("Unrecognized arena replay retry count.");
        count.OpCode = OpCodes.Ldc_I4;
        count.Operand = 25; // retain the native 2.5-second replay grace period

        var poll = module.GetType("NGame2.NMatch.MatchManager").Methods.Single(m => m.Name == "UpdateRequestWaitMatch_Pvp");
        var interval = poll.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "get_IntervalTime_for_RequestWaitMatch");
        if (interval.Next.OpCode != OpCodes.Ldarg_0)
        {
            var next = interval.Next;
            var il = poll.Body.GetILProcessor();
            var arenaType = poll.DeclaringType.Methods.Single(m => m.Name == "get_ArenaType");
            // Normal (Victory) only; preserve other modes and the independent
            // offline-fallback timer. The existing sentinel prevents overlapping
            // HTTP requests even when responses take longer than this interval.
            foreach (var i in new[] {
                Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Call, arenaType),
                Instruction.Create(OpCodes.Brtrue, next), Instruction.Create(OpCodes.Pop),
                Instruction.Create(OpCodes.Ldc_R4, 0.5f)
            }) il.InsertBefore(next, i);
        }
        VerifyArenaResponsiveness(module);
    }

    static void VerifyArenaResponsiveness(ModuleDefinition module)
    {
        var context = ArenaContextType(module);
        var result = context.Methods.Single(m => m.Name == "OnBattleResult");
        var wait = context.Methods.Single(m => m.Name == "WaitBattleEnd");
        var callback = context.NestedTypes.SelectMany(t => t.Methods).Single(m => m.Name.Contains("<OnBattleResult>") && m.HasBody);
        var poll = module.GetType("NGame2.NMatch.MatchManager").Methods.Single(m => m.Name == "UpdateRequestWaitMatch_Pvp");
        foreach (var method in new[] { result, wait, callback, poll }) VerifyArenaResultStack(method);
        var prefix = wait.Body.Instructions.Take(5).ToArray();
        if (prefix[0].OpCode != OpCodes.Ldarg_1 || prefix[1].Operand is not MethodReference dropped || dropped.Name != "get_Dropped" ||
            prefix[2].OpCode != OpCodes.Brfalse || prefix[2].Operand != wait.Body.Instructions[5] ||
            prefix[3].OpCode != OpCodes.Ldc_I4_0 || prefix[4].OpCode != OpCodes.Starg || prefix[4].Operand != wait.Parameters[1])
            throw new InvalidOperationException("Arena forfeits must bypass the replay timeout only after the authoritative result.");
        if (!result.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_R4 && (float)i.Operand == 0.05f) ||
            !wait.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldc_R4).Select(i => (float)i.Operand).SequenceEqual(new[] { 0.05f, 0.1f }) ||
            !callback.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_I4 && (int)i.Operand == 25) ||
            !wait.Body.Instructions.Any(i => i.Operand is FieldReference f && f.Name == "IsBattleEnded") ||
            !wait.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "ForceStopBattle"))
            throw new InvalidOperationException("Arena must retain bounded replay completion with shorter result transitions.");
        var interval = poll.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "get_IntervalTime_for_RequestWaitMatch");
        var gate = poll.Body.Instructions.Skip(poll.Body.Instructions.IndexOf(interval) + 1).Take(5).ToArray();
        if (gate[0].OpCode != OpCodes.Ldarg_0 || gate[1].Operand is not MethodReference type || type.Name != "get_ArenaType" ||
            gate[2].OpCode != OpCodes.Brtrue || gate[2].Operand != gate[4].Next || gate[3].OpCode != OpCodes.Pop ||
            gate[4].OpCode != OpCodes.Ldc_R4 || (float)gate[4].Operand != 0.5f ||
            !poll.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "get_WaitingTime_for_RequestOfflineMatch") ||
            !poll.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_R4 && (float)i.Operand == 9999999f))
            throw new InvalidOperationException("Victory polling must preserve offline fallback and the in-flight request guard.");
    }

    static void VerifyArenaResponsivenessFile(string clientRoot)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        VerifyArenaResponsiveness(assembly.MainModule);
        Console.WriteLine("Verified responsive arena forfeits, results and Victory matchmaking; normal replay grace retained.");
    }

    static void InstallArenaResponsiveness(string clientRoot, bool stageOnly)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var staged = dll + ".arena-responsive-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchArenaResponsiveness(assembly.MainModule);
            assembly.Write(staged);
        }
        using (var verified = AssemblyDefinition.ReadAssembly(staged)) VerifyArenaResponsiveness(verified.MainModule);
        if (!stageOnly)
        {
            var backup = Path.Combine("target", "arena-responsive-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified responsive arena transitions." : "Installed responsive arena transitions. Restart the client.");
    }
}
