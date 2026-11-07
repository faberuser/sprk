using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

// League of Honor (Sword2) tickets recharge with Rubies like League of Victory
// (Sword). Honor holds 20 tickets to Victory's 10, so one recharge costs twice
// Victory's price. The native recharge popup is driven by StaminaTable, and the
// server's ServicesSupport Stamina row must carry the same values.
partial class Program
{
    const int VictoryTicketType = 2, HonorTicketType = 13;
    // StaminaTable row fields (see ServicesSupport.json "Stamina").
    const int StaminaTypeField = 0, RechargeCostTypeField = 10, RechargeGemValueField = 12, PopupStringKeyField = 13;

    static byte[] MsgPackInt(long value) => value switch
    {
        >= 0 and <= 0x7f => new[] { (byte)value },
        >= 0 and <= 0xff => new byte[] { 0xcc, (byte)value },
        >= 0 and <= 0xffff => new byte[] { 0xcd, (byte)(value >> 8), (byte)value },
        _ => throw new InvalidOperationException("Unexpected JIT string index.")
    };

    // Returns the table bytes with Honor configured, and whether they changed.
    static (byte[] Output, bool Changed) HonorRechargeStaminaTable(byte[] original)
    {
        var jit = new RuneTicketJit(original);
        // Each row container is one MessagePack array of 17 fields.
        (int Index, List<byte[]> Fields) Row(int type)
        {
            var found = new List<(int, List<byte[]>)>();
            for (var i = 0; i < jit.Payloads.Count - 1; i++)
            {
                var container = jit.Payloads[i];
                if (container.Length == 0 || container[0] != 0xdc) continue;
                var fields = RuneTicketJit.Array(container);
                if (fields.Count == 17 && RuneTicketJit.Integer(fields[StaminaTypeField]) == type)
                    found.Add((i, fields));
            }
            if (found.Count != 1) throw new InvalidOperationException($"Expected one StaminaTable row for type {type}, found {found.Count}.");
            return found[0];
        }
        string Text(byte[] field) => jit.Strings[checked((int)RuneTicketJit.Integer(field))];
        var victory = Row(VictoryTicketType);
        var honor = Row(HonorTicketType);
        if (Text(victory.Fields[2]) != "Sword" || Text(honor.Fields[2]) != "Sword2" || Text(honor.Fields[7]) != "20" || Text(victory.Fields[7]) != "10")
            throw new InvalidOperationException("Unexpected arena ticket StaminaTable rows.");
        if (RuneTicketJit.Integer(victory.Fields[RechargeCostTypeField]) != 1)
            throw new InvalidOperationException("League of Victory tickets are no longer Ruby-rechargeable.");
        var victoryGems = RuneTicketJit.Array(victory.Fields[RechargeGemValueField]);
        if (victoryGems.Count != 1) throw new InvalidOperationException("Unexpected League of Victory Ruby price.");
        var price = (long.Parse(Text(victoryGems[0])) * 2).ToString();
        var priceIndex = Array.IndexOf(jit.Strings, price);
        if (priceIndex < 0) throw new InvalidOperationException($"StaminaTable has no '{price}' string for the Honor price.");

        var fields = honor.Fields.ToList();
        fields[RechargeCostTypeField] = MsgPackInt(1);
        fields[RechargeGemValueField] = new byte[] { 0x91 }.Concat(MsgPackInt(priceIndex)).ToArray();
        fields[PopupStringKeyField] = victory.Fields[PopupStringKeyField];
        if (fields.Zip(honor.Fields).All(p => p.First.SequenceEqual(p.Second))) return (original, false);
        jit.ReplaceRow(honor.Index, fields);
        var output = jit.Encoded();
        // Re-read: the change must be stable and touch only the Honor row.
        var reread = new RuneTicketJit(output);
        var before = new RuneTicketJit(original);
        if (reread.Payloads.Count != before.Payloads.Count) throw new InvalidOperationException("Honor recharge changed the StaminaTable layout.");
        for (var i = 0; i < before.Payloads.Count; i++)
            if (i != honor.Index && !reread.Payloads[i].SequenceEqual(before.Payloads[i]))
                throw new InvalidOperationException("Honor recharge changed an unrelated StaminaTable container.");
        if (HonorRechargeStaminaTable(output).Changed) throw new InvalidOperationException("Honor recharge table edit is not idempotent.");
        return (output, true);
    }

    static MethodDefinition HonorRegisterFailure(ModuleDefinition module) => module
        .GetType("NGame2.NMatch.MatchManager").Methods.Single(m => m.Name == "OnFailRegistered_Pvp");

    // NotEnoughSword after registering always offered Victory's (Sword) recharge.
    // Use Honor's Sword2 for non-Normal arenas, matching native RetryMatch_Pvp.
    static void PatchHonorRegisterFailure(ModuleDefinition module)
    {
        var method = HonorRegisterFailure(module);
        var recover = method.Body.Instructions.Where(i => i.Operand is MethodReference m && m.Name == "ResolveRecoverStamina").ToArray();
        if (recover.Length != 1) throw new InvalidOperationException("Unexpected arena registration recharge calls.");
        if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "get_ArenaType")) return;
        // Arguments: (true, null, StaminaType, 0, null, isSkip=true).
        var type = recover[0].Previous.Previous.Previous.Previous;
        if (type.OpCode != OpCodes.Ldc_I4_2 || type.Previous.OpCode != OpCodes.Ldnull || type.Next.OpCode != OpCodes.Ldc_I4_0)
            throw new InvalidOperationException("Unexpected arena registration recharge arguments.");
        var arenaType = module.GetType("NGame2.NMatch.MatchManager").Methods.Single(m => m.Name == "get_ArenaType");
        var il = method.Body.GetILProcessor();
        // StaminaType = 2 + 11 * (ArenaType != Normal)  ->  Sword (2) or Sword2 (13)
        il.InsertBefore(type, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(type, il.Create(OpCodes.Call, arenaType));
        il.InsertBefore(type, il.Create(OpCodes.Ldc_I4_0));
        il.InsertBefore(type, il.Create(OpCodes.Cgt_Un));
        il.InsertBefore(type, il.Create(OpCodes.Ldc_I4_S, (sbyte)(HonorTicketType - VictoryTicketType)));
        il.InsertBefore(type, il.Create(OpCodes.Mul));
        il.InsertBefore(type, il.Create(OpCodes.Ldc_I4_2));
        type.OpCode = OpCodes.Add;
        // Inserted code lengthens the method; widen short branches that skip it.
        foreach (var i in method.Body.Instructions)
            if (i.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                i.OpCode = i.OpCode.Code switch
                {
                    Code.Beq_S => OpCodes.Beq, Code.Bne_Un_S => OpCodes.Bne_Un, Code.Brfalse_S => OpCodes.Brfalse,
                    Code.Brtrue_S => OpCodes.Brtrue, Code.Br_S => OpCodes.Br,
                    _ => throw new InvalidOperationException("Unexpected short branch in arena registration handler.")
                };
        VerifyHonorRegisterFailure(module);
    }

    static void VerifyHonorRegisterFailure(ModuleDefinition module)
    {
        var instructions = HonorRegisterFailure(module).Body.Instructions;
        var call = instructions.Single(i => i.Operand is MethodReference m && m.Name == "ResolveRecoverStamina");
        // Evaluate the StaminaType expression for each arena type.
        var start = instructions.Single(i => i.Operand is MethodReference m && m.Name == "get_ArenaType").Previous;
        foreach (var (arena, expected) in new[] { (0, VictoryTicketType), (1, HonorTicketType) })
        {
            var stack = new Stack<int>();
            for (var pc = start; pc != call.Previous.Previous.Previous; pc = pc.Next)
            {
                if (pc.OpCode == OpCodes.Ldarg_0) continue;
                else if (pc.OpCode == OpCodes.Call) stack.Push(arena);
                else if (pc.OpCode == OpCodes.Ldc_I4_0) stack.Push(0);
                else if (pc.OpCode == OpCodes.Ldc_I4_2) stack.Push(2);
                else if (pc.OpCode == OpCodes.Ldc_I4_S) stack.Push((sbyte)pc.Operand);
                else if (pc.OpCode == OpCodes.Cgt_Un) { var b = stack.Pop(); var a = stack.Pop(); stack.Push((uint)a > (uint)b ? 1 : 0); }
                else if (pc.OpCode == OpCodes.Mul) stack.Push(stack.Pop() * stack.Pop());
                else if (pc.OpCode == OpCodes.Add) stack.Push(stack.Pop() + stack.Pop());
                else throw new InvalidOperationException("Unexpected arena recharge type instruction: " + pc.OpCode);
            }
            if (stack.Count != 1 || stack.Pop() != expected)
                throw new InvalidOperationException($"Arena type {arena} must recharge stamina type {expected}.");
        }
        if (instructions.Any(i => i.OpCode.OperandType == OperandType.ShortInlineBrTarget))
            throw new InvalidOperationException("Arena registration handler kept a short branch.");
    }

    static void InstallHonorTicketRecharge(string clientRoot, bool verifyOnly)
    {
        clientRoot = Path.GetFullPath(clientRoot);
        static string Hash(byte[] data) => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
        var root = Path.Combine(clientRoot, "King's Raid_Data/Documents/Patch/StandaloneWindows");
        const string relative = "TableJit/StaminaTable.jit";
        var tablePath = Path.Combine(root, relative);
        var metadataPath = Path.Combine(root, "SPRK-NativeData.json");
        var table = File.ReadAllBytes(tablePath);
        var metadataBytes = File.ReadAllBytes(metadataPath);
        var metadata = JsonNode.Parse(metadataBytes)!;
        var (output, changed) = HonorRechargeStaminaTable(table);

        var dll = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(dll)!);
        if (verifyOnly)
        {
            if (changed) throw new InvalidOperationException("League of Honor tickets are not Ruby-rechargeable in StaminaTable.");
            if (metadata["files"]?[relative]?["sha256"]?.GetValue<string>() != Hash(table))
                throw new InvalidOperationException("Native data integrity mismatch: " + relative);
            using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
                VerifyHonorRegisterFailure(assembly.MainModule);
            resolver.Dispose();
            Console.WriteLine("Verified League of Honor Ruby recharge table, integrity hash and registration prompt.");
            return;
        }

        foreach (var process in System.Diagnostics.Process.GetProcesses())
            using (process)
                if ((process.ProcessName == "King's Raid" || process.ProcessName.StartsWith("SprkTest-"))
                    && process.MainModule?.FileName is string executable
                    && executable.StartsWith(clientRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Close game clients before installing Honor ticket recharge.");

        var backup = Path.GetFullPath(Path.Combine("target", "honor-ticket-recharge-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")));
        Directory.CreateDirectory(backup);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchHonorRegisterFailure(assembly.MainModule);
            PatchHonorRegisterFailure(assembly.MainModule);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            assembly.Write(dll + ".honor-recharge-staged");
        }
        resolver.Dispose();
        File.Move(dll + ".honor-recharge-staged", dll, true);

        var entry = metadata["files"]![relative];
        if (entry == null) metadata["files"]![relative] = entry = new JsonObject { ["originalSha256"] = Hash(table) };
        entry["sha256"] = Hash(output);
        var newMetadata = Encoding.UTF8.GetBytes(metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        foreach (var (path, before, after) in new[] { (tablePath, table, output), (metadataPath, metadataBytes, newMetadata) })
        {
            if (before.SequenceEqual(after)) continue;
            var destination = Path.Combine(backup, Path.GetFileName(path));
            File.WriteAllBytes(destination, before);
            File.WriteAllBytes(path + ".staged", after);
            File.Move(path + ".staged", path, true);
        }
        InstallHonorTicketRecharge(clientRoot, true);
    }
}
