using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string PartyRetryHelper = "SprkPartyRetry";

    static string PartyRetrySource(string name)
    {
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher." + name + ".cs.txt")!;
        using var reader = new StreamReader(resource);
        return reader.ReadToEnd();
    }

    static MethodDefinition PartyRetryMenu(ModuleDefinition game) => game.GetType("NGame2.NBattleContext.CampaignContext")
        .Methods.Single(m => m.Name == "GetBattleMenuType" && !m.HasParameters);
    static MethodDefinition PartyRetryExit(ModuleDefinition game) => game.GetType("NGame2.NUI.NManager.NBattle.BattleManagement")
        .Methods.Single(m => m.Name.EndsWith(".SystemButtonPartyExit.Listener.OnExit"));
    static MethodDefinition PartyRetryMessage(ModuleDefinition game) => game.GetType("NGame2.NBattleContext.OnlineBattleCallback")
        .Methods.Single(m => m.Name == "OnPacket" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "NShared.BattleMessageNot");

    static MethodDefinition PartyRetryPopupClose(ModuleDefinition game) => game.GetType("NGame2.NUI.NWindow.MessagePopup3Button")
        .Methods.Single(m => m.Name == "OnClickClose" && !m.HasParameters);

    static bool IsPartyRetryHook(Instruction i, string name) =>
        i.Operand is MethodReference m && m.DeclaringType.Name == PartyRetryHelper && m.Name == name;

    static void PatchPartyRetry(ModuleDefinition game, ModuleDefinition helper)
    {
        MethodReference Hook(string name) => game.ImportReference(helper.GetType(PartyRetryHelper).Methods.Single(m => m.Name == name));

        // Menu: every return becomes `ldarg.0; call Menu; ret`, keeping branch
        // targets on the rewritten return so all paths are filtered.
        var menu = PartyRetryMenu(game);
        if (!menu.Body.Instructions.Any(i => IsPartyRetryHook(i, "Menu")))
        {
            var il = menu.Body.GetILProcessor();
            foreach (var ret in menu.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = il.Create(OpCodes.Call, Hook("Menu"));
                il.InsertAfter(ret, call);
                il.InsertAfter(call, il.Create(OpCodes.Ret));
            }
        }

        // Exit button, battle messages and vote dismissal: `if (Hook(...)) return;`.
        void Prefix(MethodDefinition method, string hook, OpCode? load)
        {
            if (method.Body.Instructions.Any(i => IsPartyRetryHook(i, hook))) return;
            var il = method.Body.GetILProcessor();
            var original = method.Body.Instructions[0];
            var code = new List<Instruction>();
            if (load is OpCode argument) code.Add(il.Create(argument));
            code.Add(il.Create(OpCodes.Call, Hook(hook)));
            code.Add(il.Create(OpCodes.Brfalse, original));
            code.Add(il.Create(OpCodes.Ret));
            foreach (var instruction in code) il.InsertBefore(original, instruction);
        }
        Prefix(PartyRetryExit(game), "OnExit", null);
        Prefix(PartyRetryMessage(game), "OnMessage", OpCodes.Ldarg_1);
        Prefix(PartyRetryPopupClose(game), "KeepOpen", OpCodes.Ldarg_0);
        VerifyPartyRetry(game);
    }

    static void VerifyPartyRetry(ModuleDefinition game)
    {
        var menu = PartyRetryMenu(game).Body.Instructions;
        var returns = menu.Where(i => i.OpCode == OpCodes.Ret).ToArray();
        if (returns.Length == 0 || menu.Count(i => IsPartyRetryHook(i, "Menu")) != returns.Length
            || returns.Any(r => !IsPartyRetryHook(r.Previous, "Menu") || r.Previous.Previous.OpCode != OpCodes.Ldarg_0))
            throw new InvalidOperationException("Party Retry menu hook must filter every GetBattleMenuType return exactly once.");
        if (!menu.Any(i => i.Operand is MethodReference m && m.Name == "IsMultiplay")
            || !menu.Any(i => i.Operand is MethodReference m && m.Name == "IsPartyDungeon"))
            throw new InvalidOperationException("Native multiplayer menu policy changed.");

        void VerifyPrefix(MethodDefinition method, string hook, OpCode? load)
        {
            var code = method.Body.Instructions;
            int at = load is null ? 0 : 1;
            if (load is OpCode argument && code[0].OpCode != argument
                || !IsPartyRetryHook(code[at], hook) || code[at + 1].OpCode != OpCodes.Brfalse
                || code[at + 1].Operand != code[at + 3] || code[at + 2].OpCode != OpCodes.Ret
                || code.Count(i => IsPartyRetryHook(i, hook)) != 1)
                throw new InvalidOperationException("Party Retry " + hook + " must fall back to the native handler exactly once.");
        }
        var exit = PartyRetryExit(game);
        VerifyPrefix(exit, "OnExit", null);
        if (!exit.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "Open_BattlePopup"))
            throw new InvalidOperationException("Native party exit confirmation missing.");
        var message = PartyRetryMessage(game);
        VerifyPrefix(message, "OnMessage", OpCodes.Ldarg_1);
        var close = PartyRetryPopupClose(game);
        VerifyPrefix(close, "KeepOpen", OpCodes.Ldarg_0);
        if (!close.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "Close"))
            throw new InvalidOperationException("Native popup close missing.");
        // The countdown must still close the vote without the guarded handler.
        var update = game.GetType("NGame2.NUI.NWindow.MessagePopup3Button").Methods.Single(m => m.Name == "UpdateDerived");
        if (update.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "OnClickClose"))
            throw new InvalidOperationException("Popup countdown now routes through OnClickClose.");
        if (!message.Body.Instructions.Any(i => i.Operand is FieldReference f && f.Name == "OnBattleMessageCallback"))
            throw new InvalidOperationException("Native battle message dispatch missing.");
        // The helper restarts through these private native members by reflection.
        var party = game.GetType("NGame2.NParty.PartyManager");
        if (!party.Methods.Any(m => m.Name == "Handle" && m.Parameters.Count == 2 && m.Parameters[1].ParameterType.FullName == "NShared.NMessage.NProtocol.PartyBattleStart"))
            throw new InvalidOperationException("PartyManager.Handle(PartyBattleStart) missing.");
        var popup = game.GetType("NGame2.NUI.NWindow.MessagePopup3Button");
        if (!popup.Fields.Any(f => f.Name == "_remainedTime")
            || !popup.Methods.Any(m => m.Name == "OpenInternal" && m.Parameters.Count == 7 && m.Parameters[6].ParameterType.FullName == "System.Boolean"))
            throw new InvalidOperationException("Native battle popup members missing.");
        Console.WriteLine("Verified Party Retry menu, exit, battle message and vote dismissal hooks with native fallbacks.");
    }

    static void TestPartyRetry()
    {
        var trees = new[] { "PartyRetryPolicy", "PartyRetryTests" }.Select(name => CSharpSyntaxTree.ParseText(PartyRetrySource(name)));
        var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("PartyRetryTests", trees, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var bytes = new MemoryStream();
        var result = compilation.Emit(bytes);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        System.Reflection.Assembly.Load(bytes.ToArray()).GetType("PartyRetryTests")!.GetMethod("Run")!.Invoke(null, null);
        Console.WriteLine("Party Retry eligibility, competitive exclusions, wire format and notice tests passed.");
    }

    static void InstallPartyRetry(string root, bool stageOnly, bool verifyOnly = false)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, PartyRetryHelper + ".dll");
        var staged = dll + ".party-retry-staged";
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(verifyOnly && stageOnly ? staged : dll,
            new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        TestPartyRetry();
        if (verifyOnly) { VerifyPartyRetry(game.MainModule); return; }
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => p != helperPath).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create(PartyRetryHelper,
            new[] { "PartyRetryPolicy", "PartyRetry" }.Select(name => CSharpSyntaxTree.ParseText(PartyRetrySource(name))), refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var compiled = new MemoryStream(); var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        compiled.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(compiled);
        PatchPartyRetry(game.MainModule, helper.MainModule);
        var counts = game.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count);
        PatchPartyRetry(game.MainModule, helper.MainModule);
        if (counts != game.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count))
            throw new InvalidOperationException("Party Retry patch is not idempotent.");
        game.Write(staged); File.WriteAllBytes(helperPath + ".staged", compiled.ToArray());
        using (var check = AssemblyDefinition.ReadAssembly(staged)) VerifyPartyRetry(check.MainModule);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Patch staged. Close game clients and shared-asset workers before installation.");
        File.Copy(dll, dll + ".before-party-retry-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff"));
        File.Move(helperPath + ".staged", helperPath, true);
        File.Move(staged, dll, true);
        Console.WriteLine("Installed party Exit / vote Retry. Restart the client.");
    }
}
