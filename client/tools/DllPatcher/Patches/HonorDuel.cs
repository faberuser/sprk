using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

// Friend menu: "Friendly Duel" becomes "Victory Duel" and a new "Honor Duel"
// starts a League of Honor friendly duel (SprkHonorDuel). The league travels in
// the native GroupId (accountId + 2^40 for Honor); the server applies the same
// rule (src/api/community/duel.rs HONOR_DUEL_GROUP_OFFSET).
partial class Program
{
    const string HonorDuelHelper = "SprkHonorDuel";
    const long HonorDuelGroupOffset = 1L << 40;

    static MethodDefinition DuelStateMethod(ModuleDefinition module, string state, string name) =>
        module.GetType("NGame2.NMatch.NDuelState." + state).Methods.Single(m => m.Name == name);

    static IEnumerable<MethodDefinition> HonorDuelMenuMethods(ModuleDefinition module)
    {
        var menu = module.GetType(FriendlyDuelMenuType);
        yield return menu.Methods.Single(m => m.Name == "OpenInternal");
        yield return menu.Methods.Single(m => m.Name == "<RequestCanDuel>b__25_0");
    }

    static bool CallsHelper(MethodDefinition method, string name) =>
        method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == HonorDuelHelper && m.Name == name);

    static void WidenShortBranches(MethodDefinition method)
    {
        foreach (var i in method.Body.Instructions)
            if (i.OpCode.OperandType == OperandType.ShortInlineBrTarget)
                i.OpCode = i.OpCode.Code switch
                {
                    Code.Br_S => OpCodes.Br, Code.Brtrue_S => OpCodes.Brtrue, Code.Brfalse_S => OpCodes.Brfalse,
                    Code.Beq_S => OpCodes.Beq, Code.Bne_Un_S => OpCodes.Bne_Un, Code.Blt_S => OpCodes.Blt, Code.Bge_S => OpCodes.Bge,
                    Code.Bgt_S => OpCodes.Bgt, Code.Ble_S => OpCodes.Ble, Code.Blt_Un_S => OpCodes.Blt_Un, Code.Bge_Un_S => OpCodes.Bge_Un,
                    Code.Bgt_Un_S => OpCodes.Bgt_Un, Code.Ble_Un_S => OpCodes.Ble_Un, Code.Leave_S => OpCodes.Leave,
                    _ => throw new InvalidOperationException("Unexpected short branch in " + method.FullName)
                };
    }

    static IEnumerable<(MethodDefinition Method, bool Submit)> HonorDuelPartyGates(ModuleDefinition module)
    {
        yield return (DuelStateMethod(module, "BattlePartySetting", "OnBegin"), false);
        yield return (DuelStateMethod(module, "BattlePartySetting", "ChainUpdate"), true);
        yield return (DuelStateMethod(module, "CompleteVsPlayerSetting", "ChainUpdate"), true);
    }

    static void PatchHonorDuel(ModuleDefinition module, ModuleDefinition helper)
    {
        MethodReference Helper(string name) => module.ImportReference(helper.GetType(HonorDuelHelper).Methods.Single(m => m.Name == name));
        var groupId = module.GetType("NGame2.NMatch.NDuelState.StateBase").Methods.Single(m => m.Name == "get_GroupId");

        // 1. Menu: native RefreshButton -> rename/add the duel buttons, then RefreshButton.
        foreach (var method in HonorDuelMenuMethods(module))
        {
            if (CallsHelper(method, "SetupAndRefresh")) continue;
            var refresh = method.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "RefreshButton");
            if (refresh.Previous.OpCode != OpCodes.Ldarg_0) throw new InvalidOperationException("Unexpected friend menu layout call.");
            refresh.OpCode = OpCodes.Call; refresh.Operand = Helper("SetupAndRefresh");
        }

        // 2. Honor has no four-hero party screen: do not open it; submit the
        //    native eligible collection instead (once per duel state).
        //    The inviter submits from BattlePartySetting, the acceptor after the
        //    inviter's submission (CompleteVsPlayerSetting); both skip the native
        //    "party window closed" cancellation.
        //    A gate is skipped for Honor and for invitations withdrawn before
        //    acceptance (SkipPartyScreen); earlier installs gated on IsHonor only.
        foreach (var (method, submit) in HonorDuelPartyGates(module))
        {
            if (CallsHelper(method, "SkipPartyScreen")) continue;
            if (CallsHelper(method, "IsHonor"))
            {
                var old = method.Body.Instructions;
                if (old[1].Operand is not MethodReference { Name: "get_GroupId" } || old[2].Operand is not MethodReference { Name: "IsHonor" })
                    throw new InvalidOperationException("Unexpected Honor duel party gate: " + method.FullName);
                old[1].OpCode = OpCodes.Nop; old[1].Operand = null; old[2].Operand = Helper("SkipPartyScreen");
                continue;
            }
            {
                var il = method.Body.GetILProcessor();
                var native = method.Body.Instructions[0];
                var code = new List<Instruction> { il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Nop),
                    il.Create(OpCodes.Call, Helper("SkipPartyScreen")), il.Create(OpCodes.Brfalse, native) };
                if (submit) { code.Add(il.Create(OpCodes.Ldarg_0)); code.Add(il.Create(OpCodes.Call, Helper("SubmitParty"))); }
                code.Add(il.Create(OpCodes.Ret));
                foreach (var instruction in code) il.InsertBefore(native, instruction);
                WidenShortBranches(method);
            }
        }

        // 3. Register in the duel's league: MatchPvpType from GroupId instead of 0.
        var receive = DuelStateMethod(module, "BeginDuel", "OnReceive");
        if (!CallsHelper(receive, "PvpType"))
        {
            var pvp = receive.Body.Instructions[1];
            var register = receive.Body.Instructions.Single(i => i.Operand is MethodReference m && m.Name == "RegisterMatch_Pvp");
            if (pvp.OpCode != OpCodes.Ldc_I4_0 || receive.Body.Instructions.IndexOf(register) != 10)
                throw new InvalidOperationException("Unexpected duel registration arguments.");
            var il = receive.Body.GetILProcessor();
            pvp.OpCode = OpCodes.Ldarg_0; pvp.Operand = null;
            il.InsertAfter(pvp, il.Create(OpCodes.Call, Helper("PvpType")));
            il.InsertAfter(pvp, il.Create(OpCodes.Call, groupId));
            WidenShortBranches(receive);
        }

        // 4. The receiver's invitation names the league below the inviter's name.
        var invite = module.GetType("NGame2.NUI.NWindow.DuelRequestNotify").Methods.Single(m => m.Name == "OpenInternal");
        if (!invite.Body.Instructions.Any(i => i.Operand is MethodReference { Name: "SetInviteMessage" }))
        {
            var rets = invite.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray();
            if (rets.Length != 1 || rets[0] != invite.Body.Instructions[^1]) throw new InvalidOperationException("Unexpected duel invitation layout.");
            var il = invite.Body.GetILProcessor();
            var end = rets[0];
            var first = il.Create(OpCodes.Ldarg_0);
            il.InsertBefore(end, first);
            il.InsertBefore(end, il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(end, il.Create(OpCodes.Call, Helper("SetInviteMessage")));
            // Anything that jumped to the native ret now runs the label update first.
            foreach (var i in invite.Body.Instructions) if (i.Operand == end && i != end) i.Operand = first;
            WidenShortBranches(invite);
        }

        // 5. Friendly Honor duels skip the ranked matching screen.
        var registered = module.GetType("NGame2.NMatch.MatchManager").Methods.Single(m => m.Name == "OnRegistered_Pvp");
        var waiting = registered.Body.Instructions.SingleOrDefault(i => i.Operand is MethodReference { Name: "OpenMatchWaiting_BanPickOnly" });
        if (waiting != null) { waiting.OpCode = OpCodes.Call; waiting.Operand = Helper("OpenHonorWaiting"); }

        // 6. Friendly Honor results hide score/ranking and Retry, like Victory duels.
        var result = module.GetType("NGame2.NUI.NWindow.NBattle.PvpResult").Methods.Single(m => m.Name == "Init_BanPickOnly");
        if (!result.Body.Instructions.Any(i => i.Operand is MethodReference { Name: "FriendlyResult" }))
        {
            var rets = result.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray();
            if (rets.Length != 1 || rets[0] != result.Body.Instructions[^1]) throw new InvalidOperationException("Unexpected Honor result layout.");
            var il = result.Body.GetILProcessor();
            var first = il.Create(OpCodes.Ldarg_0);
            il.InsertBefore(rets[0], first);
            il.InsertBefore(rets[0], il.Create(OpCodes.Call, Helper("FriendlyResult")));
            foreach (var i in result.Body.Instructions) if (i.Operand == rets[0]) i.Operand = first;
            WidenShortBranches(result);
        }

        // 7. Withdrawn invitations: remember cancels received while idle; a new
        //    invitation from that friend forgets them (DuelManager dispatchers).
        var manager = module.GetType("NGame2.NMatch.DuelManager");
        foreach (var (protocol, hook, args) in new[] { ("CancelDuel", "NoteCancel", 2), ("RequestDuel", "ForgetCancel", 1) })
        {
            var dispatch = manager.Methods.Single(m => m.Name == "OnReceive" && m.Parameters[1].ParameterType.Name == protocol);
            if (CallsHelper(dispatch, hook)) continue;
            var il = dispatch.Body.GetILProcessor();
            var native = dispatch.Body.Instructions[0];
            il.InsertBefore(native, il.Create(OpCodes.Ldarg_1));
            if (args == 2) il.InsertBefore(native, il.Create(OpCodes.Ldarg_2));
            il.InsertBefore(native, il.Create(OpCodes.Call, Helper(hook)));
        }

        // 8. Friendly Honor duels poll matchmaking like Victory (0.5 s).
        var poll = module.GetType("NGame2.NMatch.MatchManager").Methods.Single(m => m.Name == "UpdateRequestWaitMatch_Pvp");
        if (!CallsHelper(poll, "ConfiguredWaitInterval"))
        {
            var interval = poll.Body.Instructions.Single(i => i.Operand is MethodReference { Name: "get_IntervalTime_for_RequestWaitMatch" });
            var arena = interval.Next.Next;
            if (interval.Next.OpCode != OpCodes.Ldarg_0 || arena.Operand is not MethodReference { Name: "get_ArenaType" }
                || arena.Next.OpCode.FlowControl != FlowControl.Cond_Branch || arena.Next.Next.OpCode != OpCodes.Pop)
                throw new InvalidOperationException("Unexpected matchmaking poll interval.");
            arena.Operand = Helper("ConfiguredWaitInterval");
        }

        // 10. Friendly Honor duels open the draft without the 5 s reveal delay.
        var start = module.GetType("NGame2.NBattleContext.ArenaContext").Methods.Single(m => m.Name == "OnBattleBanPickStartCallback");
        if (!CallsHelper(start, "BanPickRevealDelay"))
        {
            var delays = start.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldc_R4 && (float)i.Operand == 5f).ToArray();
            if (delays.Length != 1 || !start.Body.Instructions.Skip(start.Body.Instructions.IndexOf(delays[0])).Take(7)
                    .Any(i => i.Operand is MethodReference { Name: "DelayRun" }))
                throw new InvalidOperationException("Unexpected Honor draft reveal delay.");
            delays[0].OpCode = OpCodes.Call; delays[0].Operand = Helper("BanPickRevealDelay");
        }

        // 9. Friendly results enlarge the win/lose effect before it plays (both leagues).
        foreach (var init in new[] { "Init_Pvp", "Init_BanPickOnly" })
        {
            var method = module.GetType("NGame2.NUI.NWindow.NBattle.PvpResult").Methods.Single(m => m.Name == init);
            if (CallsHelper(method, "PrepareResultEffect")) continue;
            var il = method.Body.GetILProcessor();
            var native = method.Body.Instructions[0];
            il.InsertBefore(native, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(native, il.Create(OpCodes.Call, Helper("PrepareResultEffect")));
            WidenShortBranches(method);
        }
        VerifyHonorDuel(module);
    }

    static void VerifyHonorDuel(ModuleDefinition module)
    {
        foreach (var method in HonorDuelMenuMethods(module))
            if (!CallsHelper(method, "SetupAndRefresh") || method.Body.Instructions.Any(i => i.Operand is MethodReference { Name: "RefreshButton" }))
                throw new InvalidOperationException("Friend menu must lay out the Victory and Honor duel buttons: " + method.Name);
        foreach (var (method, submit) in HonorDuelPartyGates(module))
        {
            var name = method.FullName;
            var code = method.Body.Instructions;
            int ret = submit ? 6 : 4;
            if (code[0].OpCode != OpCodes.Ldarg_0 || code[1].OpCode != OpCodes.Nop
                || code[2].Operand is not MethodReference { Name: "SkipPartyScreen" } || code[3].OpCode != OpCodes.Brfalse
                || (submit && (code[4].OpCode != OpCodes.Ldarg_0 || code[5].Operand is not MethodReference { Name: "SubmitParty" }))
                || code[ret].OpCode != OpCodes.Ret || code[3].Operand != code[ret + 1])
                throw new InvalidOperationException("Honor duels must skip the party screen and submit the eligible collection: " + name);
        }
        if (!DuelStateMethod(module, "BattlePartySetting", "OnBegin").Body.Instructions.Any(i => i.Operand is MethodReference { Name: "OpenBattlePartySetting" }))
            throw new InvalidOperationException("Victory duels must keep the native party screen.");
        var receive = DuelStateMethod(module, "BeginDuel", "OnReceive").Body.Instructions;
        if (receive[1].OpCode != OpCodes.Ldarg_0 || receive[2].Operand is not MethodReference { Name: "get_GroupId" }
            || receive[3].Operand is not MethodReference { Name: "PvpType" } || receive[12].Operand is not MethodReference { Name: "RegisterMatch_Pvp" })
            throw new InvalidOperationException("Friendly duels must register in the league named by their GroupId.");
        var invite = module.GetType("NGame2.NUI.NWindow.DuelRequestNotify").Methods.Single(m => m.Name == "OpenInternal").Body.Instructions;
        if (invite.Count(i => i.Operand is MethodReference { Name: "SetInviteMessage" }) != 1 || invite[^2].Operand is not MethodReference { Name: "SetInviteMessage" }
            || invite[^3].OpCode != OpCodes.Ldarg_1 || invite[^4].OpCode != OpCodes.Ldarg_0)
            throw new InvalidOperationException("Duel invitations must name their league in the message line.");
        var registered = module.GetType("NGame2.NMatch.MatchManager").Methods.Single(m => m.Name == "OnRegistered_Pvp").Body.Instructions;
        if (registered.Any(i => i.Operand is MethodReference { Name: "OpenMatchWaiting_BanPickOnly" })
            || registered.Count(i => i.Operand is MethodReference { Name: "OpenHonorWaiting" } m && m.DeclaringType.Name == HonorDuelHelper) != 1)
            throw new InvalidOperationException("Ranked Honor must keep its matching screen; friendly Honor duels skip it.");
        var result = module.GetType("NGame2.NUI.NWindow.NBattle.PvpResult").Methods.Single(m => m.Name == "Init_BanPickOnly").Body.Instructions;
        if (result[^2].Operand is not MethodReference { Name: "FriendlyResult" } || result[^3].OpCode != OpCodes.Ldarg_0)
            throw new InvalidOperationException("Friendly Honor results must hide score and ranking.");
        var manager = module.GetType("NGame2.NMatch.DuelManager");
        foreach (var (protocol, hook) in new[] { ("CancelDuel", "NoteCancel"), ("RequestDuel", "ForgetCancel") })
        {
            var dispatch = manager.Methods.Single(m => m.Name == "OnReceive" && m.Parameters[1].ParameterType.Name == protocol).Body.Instructions;
            var call = dispatch.Single(i => i.Operand is MethodReference m && m.Name == hook && m.DeclaringType.Name == HonorDuelHelper);
            if (dispatch.IndexOf(call) > 2 || !dispatch.Skip(dispatch.IndexOf(call)).Any(i => i.Operand is MethodReference { Name: "OnReceive" }))
                throw new InvalidOperationException("Withdrawn invitations must be noted before the native " + protocol + " dispatch.");
        }
        var poll = module.GetType("NGame2.NMatch.MatchManager").Methods.Single(m => m.Name == "UpdateRequestWaitMatch_Pvp").Body.Instructions;
        var gate = poll.Single(i => i.Operand is MethodReference { Name: "ConfiguredWaitInterval" });
        if (gate.Previous.OpCode != OpCodes.Ldarg_0 || gate.Previous.Previous.Operand is not MethodReference { Name: "get_IntervalTime_for_RequestWaitMatch" })
            throw new InvalidOperationException("Friendly Honor duels must poll matchmaking like Victory.");
        var reveal = module.GetType("NGame2.NBattleContext.ArenaContext").Methods.Single(m => m.Name == "OnBattleBanPickStartCallback").Body.Instructions;
        var delay = reveal.SingleOrDefault(i => i.Operand is MethodReference { Name: "BanPickRevealDelay" });
        if (delay == null || reveal.Any(i => i.OpCode == OpCodes.Ldc_R4 && (float)i.Operand == 5f)
            || !reveal.Skip(reveal.IndexOf(delay)).Take(7).Any(i => i.Operand is MethodReference { Name: "DelayRun" }))
            throw new InvalidOperationException("Friendly Honor duels must open the draft without the ranked reveal delay.");
        foreach (var init in new[] { "Init_Pvp", "Init_BanPickOnly" })
        {
            var code = module.GetType("NGame2.NUI.NWindow.NBattle.PvpResult").Methods.Single(m => m.Name == init).Body.Instructions;
            if (code[0].OpCode != OpCodes.Ldarg_0 || code[1].Operand is not MethodReference { Name: "PrepareResultEffect" })
                throw new InvalidOperationException("Friendly results must enlarge the win/lose effect before it plays: " + init);
        }
        VerifyArenaResultStack(DuelStateMethod(module, "BeginDuel", "OnReceive"));
        foreach (var (method, _) in HonorDuelPartyGates(module)) VerifyArenaResultStack(method);
    }

    static void TestHonorDuel(byte[] helperBytes, string managed)
    {
        ResolveEventHandler resolve = (_, e) => {
            var path = Path.Combine(managed, new System.Reflection.AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? System.Reflection.Assembly.LoadFrom(path) : null;
        };
        AppDomain.CurrentDomain.AssemblyResolve += resolve;
        try
        {
            var type = System.Reflection.Assembly.Load(helperBytes).GetType(HonorDuelHelper)!;
            var offset = (long)type.GetField("GroupOffset")!.GetValue(null)!;
            bool Honor(long g) => (bool)type.GetMethod("IsHonor")!.Invoke(null, new object[] { g })!;
            int Pvp(long g) => (int)type.GetMethod("PvpType")!.Invoke(null, new object[] { g })!;
            if (offset != HonorDuelGroupOffset)
                throw new InvalidOperationException("Honor duel GroupId offset must match the server (1 << 40).");
            foreach (var account in new long[] { 1, 42, 999_999_999 })
                if (Honor(account) || Pvp(account) != 0 || !Honor(account + offset) || Pvp(account + offset) != 1)
                    throw new InvalidOperationException("Victory/Honor duel GroupId mapping is wrong for account " + account);
            string Message(long g) => (string)type.GetMethod("InviteMessage")!.Invoke(null, new object[] { g })!;
            bool Submit(string state, long g, bool inviter) => (bool)type.GetMethod("ShouldSubmit")!.Invoke(null, new object[] { state, g, inviter })!;
            if (Message(7) != "Victory Duel Request sent." || Message(7 + offset) != "Honor Duel Request sent.")
                throw new InvalidOperationException("Duel invitation must name its league.");
            // Inviter first, acceptor only after the inviter's submission; Victory never auto-submits.
            if (!Submit("BattlePartySetting", 7 + offset, true) || Submit("BattlePartySetting", 7 + offset, false)
                || !Submit("CompleteVsPlayerSetting", 7 + offset, false) || Submit("CompleteVsPlayerSetting", 7, false)
                || Submit("BattlePartySetting", 7, true) || Submit("RequestDuel", 7 + offset, true))
                throw new InvalidOperationException("Honor party submission order must avoid the acceptance race.");
            Console.WriteLine("PASS: duel GroupIds map to their leagues with the server's offset; invitation text and race-free Honor submission order.");
        }
        finally { AppDomain.CurrentDomain.AssemblyResolve -= resolve; }
    }

    static void InstallHonorDuel(string root, bool stageOnly, bool verify = false)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data/Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, "SprkHonorDuel.dll");
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        if (verify)
        {
            VerifyFriendlyDuelMenu(game.MainModule);
            VerifyHonorDuel(game.MainModule);
            TestHonorDuel(File.ReadAllBytes(helperPath), managed);
            Console.WriteLine("Verified Victory/Honor friendly duel menu, Honor party submission and league registration.");
            return;
        }
        using var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.HonorDuel.cs.txt")!);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => p != helperPath).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create(HonorDuelHelper, new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var compiled = new MemoryStream(); var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        TestHonorDuel(compiled.ToArray(), managed);
        compiled.Position = 0; using var helper = AssemblyDefinition.ReadAssembly(compiled);
        // The native duel menu restoration (visibility, CanDuel handshake) comes first.
        PatchFriendlyDuelMenu(game.MainModule);
        PatchHonorDuel(game.MainModule, helper.MainModule);
        int[] Sizes() => new[] { DuelStateMethod(game.MainModule, "BattlePartySetting", "ChainUpdate"), DuelStateMethod(game.MainModule, "CompleteVsPlayerSetting", "ChainUpdate"), DuelStateMethod(game.MainModule, "BeginDuel", "OnReceive") }
            .Select(m => m.Body.Instructions.Count).ToArray();
        var sizes = Sizes();
        PatchHonorDuel(game.MainModule, helper.MainModule);
        if (!sizes.SequenceEqual(Sizes())) throw new InvalidOperationException("Honor duel patch is not idempotent.");
        var staged = dll + ".honor-duel-staged"; game.Write(staged);
        File.WriteAllBytes(helperPath + ".staged", compiled.ToArray());
        using (var check = AssemblyDefinition.ReadAssembly(staged, new ReaderParameters { AssemblyResolver = resolver })) { VerifyFriendlyDuelMenu(check.MainModule); VerifyHonorDuel(check.MainModule); }
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root))) throw new InvalidOperationException("Close game clients before installing Honor friendly duels.");
        var backup = Path.Combine("target/honor-duel-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff")); Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkHonorDuel.dll"));
        File.Move(staged, dll, true); File.Move(helperPath + ".staged", helperPath, true);
        Console.WriteLine("Installed Victory/Honor friendly duels. Restart the game.");
    }
}
