using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void InstallStoryAuto(string clientRoot, bool stageOnly, string? sourcePath = null)
    {
        var managed = Path.GetFullPath(Path.Combine(clientRoot, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll");
        var helperPath = Path.Combine(managed, "SprkStoryAuto.dll");
        using var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.StoryAuto.cs.txt")!;
        using var reader = new StreamReader(resource);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => Path.GetFileName(p) != "SprkStoryAuto.dll")
            .Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SprkStoryAuto", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var compiled = new MemoryStream();
        var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics));
        compiled.Position = 0;
        using var helper = AssemblyDefinition.ReadAssembly(compiled);
        using var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managed);
        using var assembly = AssemblyDefinition.ReadAssembly(sourcePath ?? dll, new ReaderParameters { AssemblyResolver = resolver });
        var module = assembly.MainModule;
        var worker = helper.MainModule.GetType("SprkStoryAuto");
        MethodReference Hook(string name) => module.ImportReference(worker.Methods.Single(m => m.Name == name));
        MethodDefinition Method(string type, string name) => module.GetType(type).Methods.Single(m => m.Name == name);
        bool Hooked(MethodDefinition m, string name) => m.Body.Instructions.Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkStoryAuto" && r.Name == name);
        void Prefix(MethodDefinition m, params Instruction[] instructions)
        {
            var first = m.Body.Instructions[0];
            foreach (var instruction in instructions) m.Body.GetILProcessor().InsertBefore(first, instruction);
        }

        var choice = Method("NGame2.NUI.NManager.NLobby.LobbyCampaignDungeon", "NGame2.NUI.NWindow.BattlePartySetting.Listener.OpenAutoNextBattleSelect");
        if (!Hooked(choice, "ShowChoice")) {
            Prefix(choice, Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, choice.DeclaringType.Fields.Single(f => f.Name == "_enterDungeon")),
                Instruction.Create(OpCodes.Call, Hook("ShowChoice")), Instruction.Create(OpCodes.Brfalse, choice.Body.Instructions[0]),
                Instruction.Create(OpCodes.Ldc_I4_1), Instruction.Create(OpCodes.Ret));
        }
        var start = Method("NGame2.NUI.NWindow.BattlePartySetting", "TryStartBattle");
        var startClick = Method("NGame2.NUI.NWindow.BattlePartySetting", "OnClickStartBattle");
        var oldChoice = startClick.Body.Instructions.FirstOrDefault(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkStoryAuto" && r.Name == "ChooseForParty");
        if (oldChoice != null && startClick.Body.Instructions.IndexOf(oldChoice) == 4) {
            // Upgrade the initial development hook while preserving native entry
            // count/party checks before presenting our choice.
            var prefix = startClick.Body.Instructions.Take(7).ToArray();
            if (prefix[6].OpCode != OpCodes.Ret) throw new InvalidOperationException("Unexpected story choice prefix");
            foreach (var i in prefix) startClick.Body.Instructions.Remove(i);
        }
        if (!Hooked(startClick, "ChooseForParty")) {
            var menu = startClick.Body.Instructions.Single(i => i.Operand is MethodReference r && r.Name == "IsMultipleMenu");
            var anchor = menu.Previous.Previous;
            if (anchor.OpCode != OpCodes.Ldarg_0 || menu.Previous.Operand is not FieldReference field || field.Name != "_dataProvider")
                throw new InvalidOperationException("Unexpected battle selection call shape");
            foreach (var i in new[] { Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, start.DeclaringType.Fields.Single(f => f.Name == "_dataProvider")),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, start.DeclaringType.Fields.Single(f => f.Name == "_listener")),
                Instruction.Create(OpCodes.Call, Hook("ChooseForParty")), Instruction.Create(OpCodes.Brfalse, anchor),
                Instruction.Create(OpCodes.Ret) }) startClick.Body.GetILProcessor().InsertBefore(anchor, i);
            // The no-booster branch originally jumped directly to menu selection.
            foreach (var i in startClick.Body.Instructions)
                if (ReferenceEquals(i.Operand, anchor) && i.Previous?.Operand is not MethodReference { Name: "ChooseForParty" })
                    i.Operand = anchor.Previous.Previous.Previous.Previous.Previous.Previous.Previous;
        }
        if (!Hooked(start, "BeforeStart")) {
            Prefix(start, Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, start.DeclaringType.Fields.Single(f => f.Name == "_dataProvider")),
                Instruction.Create(OpCodes.Call, Hook("BeforeStart")));
            var eventCall = start.Body.Instructions.Single(i => i.Operand is MethodReference r && r.Name == "RaiseEvent");
            start.Body.GetILProcessor().InsertAfter(eventCall, Instruction.Create(OpCodes.Call, Hook("EventRaised")));
            var entryCall = start.Body.Instructions.Single(i => i.Operand is MethodReference r && r.Name == "CheckBattleEnter");
            start.Body.GetILProcessor().InsertAfter(entryCall, Instruction.Create(OpCodes.Call, Hook("EntryChecked")));
            foreach (var call in start.Body.Instructions.Where(i => i.Operand is MethodReference r && r.Name == "OnClickStop").ToArray()) {
                call.OpCode = OpCodes.Call; call.Operand = Hook("StopForEntryEvent");
            }
        }
        var canNext = Method("NGame2.NUI.NWindow.NBattle.EndBattleReward", "CanNextDungeon");
        if (!Hooked(canNext, "CanNext")) {
            Prefix(canNext, Instruction.Create(OpCodes.Call, Hook("Active")), Instruction.Create(OpCodes.Brfalse, canNext.Body.Instructions[0]),
                Instruction.Create(OpCodes.Call, Hook("CanNext")), Instruction.Create(OpCodes.Ret));
        }
        var checkNext = Method("NGame2.NUI.NManager.NBattle.EndCampaign", "CheckNextDungeon");
        var advance = Method("NGame2.NUI.NManager.NBattle.EndCampaign", "NGame2.NUI.NWindow.NBattle.EndBattleReward.Listener.ReturnToLobbyAndOpenNextDungeon");
        if (!Hooked(advance, "Advance"))
            Prefix(advance, Instruction.Create(OpCodes.Ldarg_0), Instruction.Create(OpCodes.Call, Hook("Advance")),
                Instruction.Create(OpCodes.Brfalse, advance.Body.Instructions[0]), Instruction.Create(OpCodes.Ret));
        foreach (var name in new[] { "OnClickStop", "OnBattleLose", "OnClickStopWithoutMessage" }) {
            var stop = Method("NGame2.NUI.NWindow.NBattle.AutoNextBattle", name);
            if (!Hooked(stop, "Cancelled")) Prefix(stop, Instruction.Create(OpCodes.Call, Hook("Cancelled")));
        }
        if (!Hooked(checkNext, "LowerDifficultyCleared")) {
            var call = checkNext.Body.Instructions.Single(i => i.Operand is MethodReference r && r.Name == "IsCompleted");
            call.OpCode = OpCodes.Call; call.Operand = Hook("LowerDifficultyCleared");
        }
        if (!Hooked(checkNext, "NextDifficulty")) {
            var difficulty = checkNext.Body.Instructions.Single(i => i.Operand is FieldReference f && f.Name == "DungeonDifficulty");
            var loadTarget = Instruction.Create(OpCodes.Ldarg_1);
            checkNext.Body.GetILProcessor().InsertAfter(difficulty, loadTarget);
            checkNext.Body.GetILProcessor().InsertAfter(loadTarget, Instruction.Create(OpCodes.Call, Hook("NextDifficulty")));
        }
        var update = module.Types.Single(t => t.Name == "GlobalUpdateBehaviour").Methods.Single(m => m.Name == "Update");
        if (!Hooked(update, "Tick")) Prefix(update, Instruction.Create(OpCodes.Call, Hook("Tick")));
        var rewardUpdate = Method("NGame2.NUI.NWindow.NBattle.EndBattleReward", "UpdateAutoNextCampaign");
        var stopped = rewardUpdate.Body.Instructions.Single(i => i.Operand is MethodReference r && r.Name == "OnClickStop");
        // Native code falls through after destroying AutoNextBattle, recreating
        // it with default chapter/difficulty and potentially scheduling a move.
        if (stopped.Next.OpCode != OpCodes.Ret)
            rewardUpdate.Body.GetILProcessor().InsertAfter(stopped, Instruction.Create(OpCodes.Ret));
        var beginNext = Method("NGame2.NUI.NWindow.NBattle.EndBattleReward", "StartAutoNextBattle");
        var enabled = module.GetType("NGame2.NUI.NWindow.NBattle.AutoNextBattle").Methods.Single(m => m.Name == "IsEnabled");
        if (beginNext.Body.Instructions[0].Operand is not MethodReference firstCall || firstCall.FullName != enabled.FullName)
            Prefix(beginNext, Instruction.Create(OpCodes.Call, enabled), Instruction.Create(OpCodes.Brtrue, beginNext.Body.Instructions[0]), Instruction.Create(OpCodes.Ret));
        foreach (var type in new[] { "SystemButtonAuto", "SystemButtonAutoFastContinue", "SystemButtonAutoSimple" }) {
            var getter = Method("NGame2.NUI.NWindow.NBattle." + type, "get_SkillReserveAuto");
            if (!Hooked(getter, "AutoSkills")) {
                var flag = getter.Body.Instructions.Single(i => i.Operand is FieldReference f && f.Name == "EditorToggleAutoSkill");
                getter.Body.GetILProcessor().InsertAfter(flag, Instruction.Create(OpCodes.Call, Hook("AutoSkills")));
            }
        }

        // Entry failures must disarm immediately. Keep the original error handling.
        var failure = module.GetTypes().SelectMany(t => t.Methods).Single(m =>
            m.DeclaringType.FullName.StartsWith("NGame2.NUI.NManager.NLobby.LobbyCampaignDungeon/") &&
            m.Name.Contains("<RequestBeginCampaign>") && m.HasBody &&
            m.Body.Instructions.Any(i => i.OpCode == OpCodes.Stfld && i.Operand is FieldReference f && f.Name == "_battleRequestedTime"));
        if (!Hooked(failure, "Stop")) Prefix(failure, Instruction.Create(OpCodes.Call, Hook("Stop")));

        var entered = module.GetTypes().SelectMany(t => t.Methods).Single(m =>
            m.DeclaringType.FullName.StartsWith("NGame2.NUI.NManager.NLobby.LobbyCampaignDungeon/") &&
            m.Name.Contains("<RequestBeginCampaign>") && m.HasBody &&
            m.Body.Instructions.Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "AutoNextBattle" && r.Name == "OpenGlobal"));
        if (!Hooked(entered, "ContinueType")) {
            var type = entered.Body.Instructions.Single(i => i.Operand is MethodReference r && r.Name == "get_BattleType");
            entered.Body.GetILProcessor().InsertAfter(type, Instruction.Create(OpCodes.Call, Hook("ContinueType")));
        }
        if (!Hooked(entered, "SaveStoryPosition")) {
            var save = entered.Body.Instructions.Single(i => i.Operand is MethodReference r && r.Name == "SaveLatelyBattleNodePos");
            save.OpCode = OpCodes.Call; save.Operand = Hook("SaveStoryPosition");
        }
        var request = Method("NGame2.NUI.NManager.NLobby.LobbyCampaignDungeon", "RequestBeginCampaign");
        if (!Hooked(request, "PrepareStoryRequest"))
            Prefix(request, Instruction.Create(OpCodes.Ldarg_1), Instruction.Create(OpCodes.Call, Hook("PrepareStoryRequest")));

        var provider = Method("NGame2.NUI.NManager.NLobby.LobbyCampaignDungeon", "GetDataProvider");
        if (!Hooked(provider, "StoryProvider")) {
            var fallback = Instruction.Create(OpCodes.Pop);
            Prefix(provider, Instruction.Create(OpCodes.Ldarg_1), Instruction.Create(OpCodes.Call, Hook("StoryProvider")),
                Instruction.Create(OpCodes.Dup), Instruction.Create(OpCodes.Brfalse, fallback), Instruction.Create(OpCodes.Ret), fallback);
        }

        // Cecil does not expand existing short branches after inserting instructions.
        foreach (var method in new[] { choice, start, startClick, canNext, checkNext, advance, update, failure, entered, rewardUpdate, beginNext })
            foreach (var i in method.Body.Instructions) {
                if (i.OpCode == OpCodes.Br_S) i.OpCode = OpCodes.Br;
                else if (i.OpCode == OpCodes.Brtrue_S) i.OpCode = OpCodes.Brtrue;
                else if (i.OpCode == OpCodes.Brfalse_S) i.OpCode = OpCodes.Brfalse;
                else if (i.OpCode == OpCodes.Bne_Un_S) i.OpCode = OpCodes.Bne_Un;
                else if (i.OpCode == OpCodes.Beq_S) i.OpCode = OpCodes.Beq;
                else if (i.OpCode == OpCodes.Bge_S) i.OpCode = OpCodes.Bge;
                else if (i.OpCode == OpCodes.Bgt_S) i.OpCode = OpCodes.Bgt;
                else if (i.OpCode == OpCodes.Ble_S) i.OpCode = OpCodes.Ble;
                else if (i.OpCode == OpCodes.Blt_S) i.OpCode = OpCodes.Blt;
                else if (i.OpCode == OpCodes.Leave_S) i.OpCode = OpCodes.Leave;
            }
        var staged = dll + ".story-auto-staged";
        assembly.Write(staged);
        File.WriteAllBytes(helperPath + ".staged", compiled.ToArray());
        using (var verified = AssemblyDefinition.ReadAssembly(staged)) VerifyStoryAuto(verified.MainModule);
        if (!stageOnly) {
            var backup = Path.Combine("target", "story-auto-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
            Directory.CreateDirectory(backup);
            File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
            if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkStoryAuto.dll"));
            assembly.Dispose();
            File.Move(helperPath + ".staged", helperPath, true);
            File.Move(staged, dll, true);
        }
        Console.WriteLine(stageOnly ? "Staged and verified Continuous Story patch." : "Installed and verified Continuous Story. Restart the client.");
    }

    static void VerifyStoryAuto(ModuleDefinition module)
    {
        foreach (var name in new[] { "ShowChoice", "ChooseForParty", "BeforeStart", "EventRaised", "EntryChecked", "CanNext", "Advance", "LowerDifficultyCleared", "NextDifficulty", "Tick", "ContinueType", "SaveStoryPosition", "PrepareStoryRequest", "StoryProvider" }) {
            var calls = module.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody)
                .SelectMany(m => m.Body.Instructions).Count(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkStoryAuto" && r.Name == name);
            if (calls != 1) throw new InvalidOperationException($"Expected one {name} hook; found {calls}.");
        }
        var next = module.GetType("NGame2.NUI.NManager.NBattle.EndCampaign").Methods.Single(m => m.Name == "CheckNextDungeon");
        foreach (var check in new[] { "IsMaxEquipItemCount", "CanGoToChapter", "CanVisit", "GetMaxAvailableDifficulty" })
            if (!next.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == check))
                throw new InvalidOperationException("Missing native safety check: " + check);
        var lose = module.GetType("NGame2.NUI.NWindow.NBattle.AutoNextBattle").Methods.Single(m => m.Name == "OnBattleLose");
        if (!lose.Body.Instructions.Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "AutoBattle" && r.Name == "OnBattleLose"))
            throw new InvalidOperationException("Missing native stop-on-defeat path.");
        Console.WriteLine("Verified unique hooks, native entry/inventory/difficulty checks, and stop-on-defeat path.");
    }
}
