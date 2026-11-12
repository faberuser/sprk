using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string FriendSlotType = "NGame2.NUI.NComponent2.FriendSlotComponent";
    const string FriendLogoutType = "NGame2.NMessageLog.NProtocol.LogoutProtocolHandler";

    static void PatchFriendPresence(ModuleDefinition module)
    {
        var slot = module.GetType(FriendSlotType);
        if (!slot.Fields.Any(f => f.Name == "_sprkNextPresenceRefresh"))
        {
            if (slot.Methods.Any(m => m.Name == "Update"))
                throw new InvalidOperationException("Friend slot already defines Update; inspect before patching.");
            var next = new FieldDefinition("_sprkNextPresenceRefresh", FieldAttributes.Private, module.TypeSystem.Single);
            slot.Fields.Add(next);
            var time = module.Types.SelectMany(t => t.Methods).Where(m => m.HasBody)
                .SelectMany(m => m.Body.Instructions).Select(i => i.Operand).OfType<MethodReference>()
                .First(m => m.DeclaringType.FullName == "UnityEngine.Time" && m.Name == "get_unscaledTime");
            var update = new MethodDefinition("Update", MethodAttributes.Private | MethodAttributes.HideBySig, module.TypeSystem.Void);
            slot.Methods.Add(update);
            var il = update.Body.GetILProcessor();
            var done = Instruction.Create(OpCodes.Ret);
            il.Append(Instruction.Create(OpCodes.Call, time));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Ldfld, next));
            il.Append(Instruction.Create(OpCodes.Blt, done));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Call, time));
            il.Append(Instruction.Create(OpCodes.Ldc_R4, 1f));
            il.Append(Instruction.Create(OpCodes.Add));
            il.Append(Instruction.Create(OpCodes.Stfld, next));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Call, slot.Methods.Single(m => m.Name == "get_FriendInfo")));
            il.Append(Instruction.Create(OpCodes.Brfalse, done));
            il.Append(Instruction.Create(OpCodes.Ldarg_0));
            il.Append(Instruction.Create(OpCodes.Call, slot.Methods.Single(m => m.Name == "UpdateOnlineButtonState")));
            il.Append(done);

            // ChangeFriendReq replays offline presence. A replay must not replace
            // the LastOnlineTime loaded from the server with the current time.
            var logout = module.GetType(FriendLogoutType).Methods.Single(m => m.Name == "Run");
            var firstTime = logout.Body.Instructions.First(i => i.Operand is MethodReference r && r.Name == "GetUTCNow");
            var processor = logout.Body.GetILProcessor();
            processor.InsertBefore(firstTime, Instruction.Create(OpCodes.Ldloc_0));
            processor.InsertBefore(firstTime, Instruction.Create(OpCodes.Callvirt,
                module.GetType("NShared.CachedUserInfo").Methods.Single(m => m.Name == "get_IsOnline")));
            processor.InsertBefore(firstTime, Instruction.Create(OpCodes.Brfalse, logout.Body.Instructions.Last()));
        }
        VerifyFriendPresence(module);
    }

    static void VerifyFriendPresence(ModuleDefinition module)
    {
        var slot = module.GetType(FriendSlotType);
        var update = slot.Methods.Single(m => m.Name == "Update");
        var logout = module.GetType(FriendLogoutType).Methods.Single(m => m.Name == "Run");
        var clock = logout.Body.Instructions.First(i => i.Operand is MethodReference r && r.Name == "GetUTCNow");
        var guard = clock.Previous;
        if (!update.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "get_unscaledTime")
            || !update.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "UpdateOnlineButtonState")
            || !update.Body.Instructions.Any(i => i.OpCode == OpCodes.Blt && i.Operand == update.Body.Instructions.Last())
            || !update.Body.Instructions.Any(i => i.OpCode == OpCodes.Ldc_R4 && Equals(i.Operand, 1f))
            || guard.OpCode != OpCodes.Brfalse || guard.Operand != logout.Body.Instructions.Last()
            || guard.Previous.Operand is not MethodReference read || read.Name != "get_IsOnline"
            || guard.Previous.Previous.OpCode != OpCodes.Ldloc_0)
            throw new InvalidOperationException("Friend presence requires a periodic refresh and a logout replay guard.");
        VerifyArenaResultStack(update);
        VerifyArenaResultStack(logout);
    }

    static void VerifyFriendPresenceFile(string root)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll"), new ReaderParameters { AssemblyResolver = resolver });
        VerifyFriendPresence(assembly.MainModule);
        Console.WriteLine("Verified friend offline timer refresh, logout replay guard and IL stacks.");
    }

    static void InstallFriendPresence(string root)
    {
        var dll = Path.Combine(root, "King's Raid_Data", "Managed", "Assembly-CSharp.dll");
        var staged = dll + ".friend-presence-staged";
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(dll))!);
        using (var assembly = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchFriendPresence(assembly.MainModule);
            using var first = new MemoryStream();
            assembly.Write(first);
            PatchFriendPresence(assembly.MainModule);
            using var second = new MemoryStream();
            assembly.Write(second);
            if (!first.ToArray().SequenceEqual(second.ToArray()))
                throw new InvalidOperationException("Friend presence patch must be idempotent.");
            File.WriteAllBytes(staged, second.ToArray());
        }
        if (System.Diagnostics.Process.GetProcessesByName("King's Raid").Any())
            throw new InvalidOperationException("Fix staged. Close King's Raid before installation.");
        var backup = Path.Combine("target", "friend-presence-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
        Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        File.Move(staged, dll, true);
        VerifyFriendPresenceFile(root);
    }
}
