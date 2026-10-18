using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    const string ChatChannelWindow = "NGame2.NUI.NWindow.NChatting.ChattingChannelSelect1";

    static void PatchChatChannels(ModuleDefinition module, ModuleDefinition helper)
    {
        var bridge = helper.GetType("SprkChatChannels");
        MethodReference Hook(string name) => module.ImportReference(bridge.Methods.Single(m => m.Name == name));
        var window = module.GetType(ChatChannelWindow);
        var init = window.Methods.Single(m => m.Name == "Init");
        if (!init.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "Configure" && m.DeclaringType.Name == "SprkChatChannels"))
        {
            var il = init.Body.GetILProcessor();
            foreach (var ret in init.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0;
                var configure = il.Create(OpCodes.Call, Hook("Configure"));
                il.InsertAfter(ret, configure);
                il.InsertAfter(configure, il.Create(OpCodes.Ret));
            }
        }
        var open = window.Methods.Single(m => m.Name == "Open");
        var minimum = open.Body.Instructions.SingleOrDefault(i => i.OpCode == OpCodes.Ldflda && i.Operand is FieldReference f && f.Name == "minValue");
        if (minimum != null)
        {
            // Keep the current channel set by Init, rather than replacing it
            // with the old 1001 minimum after the open callback has run.
            var assignment = open.Body.Instructions.Skip(open.Body.Instructions.IndexOf(minimum) - 3).Take(6).ToArray();
            if (assignment[0].OpCode != OpCodes.Ldarg_0 || assignment[1].Operand is not FieldReference input || input.Name != "ChannelNoInput" ||
                assignment[2].OpCode != OpCodes.Ldarg_0 || assignment[4].Operand is not MethodReference convert || convert.Name != "ToString" ||
                assignment[5].Operand is not MethodReference set || set.Name != "set_value")
                throw new InvalidOperationException("Unexpected channel open assignment.");
            foreach (var instruction in assignment) { instruction.OpCode = OpCodes.Nop; instruction.Operand = null; }
        }
        var confirm = window.Methods.Single(m => m.Name == "OnConfirm");
        if (!confirm.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkChatChannels" && m.Name == "Normalize"))
        {
            var parse = confirm.Body.Instructions.First(i => i.Operand is MethodReference m && m.Name == "TOINT");
            confirm.Body.GetILProcessor().InsertAfter(parse, Instruction.Create(OpCodes.Call, Hook("Normalize")));
        }
        var onGuiType = module.GetType("UIInputOnGUI");
        var onGui = onGuiType.Methods.Single(m => m.Name == "OnGUI");
        if (!onGui.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "UsesKeyboardPolling"))
        {
            var first = onGui.Body.Instructions[0];
            var il = onGui.Body.GetILProcessor();
            foreach (var instruction in new[] {
                Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldfld, onGuiType.Fields.Single(f => f.Name == "mInput")),
                Instruction.Create(OpCodes.Call, Hook("UsesKeyboardPolling")), Instruction.Create(OpCodes.Brfalse, first), Instruction.Create(OpCodes.Ret)
            }) il.InsertBefore(first, instruction);
        }
        var addShare = module.GetType("NGame2.NUI.NComponent2.ItemLinkButtonComponent").Methods.Single(m => m.Name == "AddComponent");
        if (!addShare.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkChatChannels" && m.Name == "AddSendButton"))
        {
            var il = addShare.Body.GetILProcessor();
            foreach (var ret in addShare.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_1;
                var load = il.Create(OpCodes.Ldind_Ref);
                var attach = il.Create(OpCodes.Call, Hook("AddSendButton"));
                il.InsertAfter(ret, load); il.InsertAfter(load, attach); il.InsertAfter(attach, il.Create(OpCodes.Ret));
            }
        }
        VerifyChatChannels(module);
    }

    static void VerifyChatChannels(ModuleDefinition module)
    {
        var window = module.GetType(ChatChannelWindow);
        foreach (var (method, hook) in new[] { (window.Methods.Single(m => m.Name == "Init"), "Configure"),
            (window.Methods.Single(m => m.Name == "OnConfirm"), "Normalize"),
            (module.GetType("UIInputOnGUI").Methods.Single(m => m.Name == "OnGUI"), "UsesKeyboardPolling"),
            (module.GetType("NGame2.NUI.NComponent2.ItemLinkButtonComponent").Methods.Single(m => m.Name == "AddComponent"), "AddSendButton") })
        {
            if (method.Body.Instructions.Count(i => i.Operand is MethodReference m && m.DeclaringType.Name == "SprkChatChannels" && m.Name == hook) != 1)
                throw new InvalidOperationException("Missing or duplicated chat channel hook: " + hook);
            VerifyArenaResultStack(method);
        }
        var open = window.Methods.Single(m => m.Name == "Open");
        VerifyArenaResultStack(open);
        if (open.Body.Instructions.Any(i => i.Operand is FieldReference f && f.Name == "minValue"))
            throw new InvalidOperationException("Opening the channel dialog still overwrites the current channel.");
        if (!window.Methods.Single(m => m.Name == "OnConfirm").Body.Instructions.Any(i => i.Operand is FieldReference f && f.Name == "Toggle2"))
            throw new InvalidOperationException("Native language channel selection was removed.");
    }

    static void VerifyChatChannelsFile(string clientRoot, bool staged)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        using var assembly = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll" + (staged ? ".chat-channels-staged" : "")));
        VerifyChatChannels(assembly.MainModule);
        Console.WriteLine("Verified channel range, keyboard editing, Send button hook and IL control flow.");
    }

    static void InstallChatChannels(string clientRoot, bool stageOnly, string? sourcePath = null)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, "SprkChatChannels.dll");
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ChatChannels.cs.txt")!;
        using var reader = new StreamReader(resource);
        var references = Directory.GetFiles(managed, "*.dll").Where(p => Path.GetFileName(p) != "SprkChatChannels.dll")
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkChatChannels", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var bytes = new MemoryStream();
        var emitted = compilation.Emit(bytes);
        if (!emitted.Success) throw new InvalidOperationException(string.Join("\n", emitted.Diagnostics));
        bytes.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(bytes);
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        var staged = dll + ".chat-channels-staged";
        using (var assembly = AssemblyDefinition.ReadAssembly(sourcePath ?? dll, new ReaderParameters { AssemblyResolver = resolver }))
        {
            PatchChatChannels(assembly.MainModule, helper.MainModule);
            assembly.Write(staged);
        }
        using (var verified = AssemblyDefinition.ReadAssembly(staged, new ReaderParameters { AssemblyResolver = resolver }))
        {
            VerifyChatChannels(verified.MainModule);
            PatchChatChannels(verified.MainModule, helper.MainModule);
            using var repeated = new MemoryStream(); verified.Write(repeated);
            if (!repeated.ToArray().SequenceEqual(File.ReadAllBytes(staged)))
                throw new InvalidOperationException("Chat channel patch must be idempotent.");
        }
        File.WriteAllBytes(helperPath + ".chat-channels-staged", bytes.ToArray());
        if (!stageOnly)
        {
            var backup = Path.Combine("target", "chat-channel-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkChatChannels.dll"));
            File.Move(helperPath + ".chat-channels-staged", helperPath, true);
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified chat controls." : "Installed and verified chat controls. Restart the client.");
    }
}
