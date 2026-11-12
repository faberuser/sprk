using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;
partial class Program
{
    static void PatchArenaPractice(ModuleDefinition game, ModuleDefinition helper)
    {
        MethodReference Hook(string name) => game.ImportReference(helper.GetType("SprkArenaPractice").Methods.Single(m => m.Name == name));
        void AtReturns(MethodDefinition method, string hook, bool loadThis = false)
        {
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkArenaPractice" && r.Name == hook)) return;
            var il = method.Body.GetILProcessor();
            foreach (var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                // Keep original branch targets on the first hook instruction.
                ret.OpCode = loadThis ? OpCodes.Ldarg_0 : OpCodes.Call;
                ret.Operand = loadThis ? null : Hook(hook);
                var last = ret;
                if (loadThis) { var call = il.Create(OpCodes.Call, Hook(hook)); il.InsertAfter(last, call); last = call; }
                il.InsertAfter(last, il.Create(OpCodes.Ret));
            }
        }
        var select = game.GetType("NGame2.NUI.NWindow.MatchSelect");
        var slots = select.Methods.Single(m => m.Name == "SetArena");
        slots.Body = new MethodBody(slots);
        var slotIl = slots.Body.GetILProcessor();
        slotIl.Emit(OpCodes.Ldarg_0); slotIl.Emit(OpCodes.Ldarg_1);
        slotIl.Emit(OpCodes.Call, Hook("SetSlots")); slotIl.Emit(OpCodes.Ret);
        // Also covers a reused screen where no new slot filtering occurs.
        AtReturns(select.Methods.Single(m => m.Name == "InitBtn"), "AddCard", true);
        var dungeon = game.GetType("NShared.BattleInstance").Methods.Single(m => m.Name == "SetCampaignDungeon" && m.Parameters.Count == 3);
        if (!dungeon.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "CombatDungeon"))
        {
            var il = dungeon.Body.GetILProcessor(); var first = dungeon.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_1)); il.InsertBefore(first, il.Create(OpCodes.Call, Hook("CombatDungeon")));
            il.InsertBefore(first, il.Create(OpCodes.Starg, dungeon.Parameters[0]));
        }
        var custom = game.GetType("NGame2.NBattleContext.CustomCampaignContext");
        var create = game.GetType("NShared.BattleCreatureContainer").Methods.Single(m => m.Name == "Create");
        foreach (var pair in new[] { ("set_CreatureData", "CombatTarget"), ("set_CreateInfo", "ProtectTarget") })
        {
            if (create.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == pair.Item2 && r.DeclaringType.Name == "SprkArenaPractice")) continue;
            var setter = create.Body.Instructions.Single(i => i.Operand is MethodReference r && r.Name == pair.Item1);
            create.Body.GetILProcessor().InsertBefore(setter, Instruction.Create(OpCodes.Call, Hook(pair.Item2)));
        }
        AtReturns(custom.Methods.Single(m => m.Name.EndsWith(".GetBattleMenuType")), "Menu");
        AtReturns(custom.Methods.Single(m => m.Name.EndsWith(".GetBattleDefine")), "DisplayDefine");
        var management = game.GetType("NGame2.NUI.NManager.NBattle.BattleManagement");
        AtReturns(management.Methods.Single(m => m.Name == "GetGameSpeedRatio"), "Speed");
        AtReturns(management.Methods.Single(m => m.Name == "GetCampaignDungeonData"), "CombatDungeon");
        foreach (var typeName in new[] { "NGame2.NBattleView.Creature", "NGame2.NBattleView.NDamageOn3d.Manager" })
            foreach (var method in game.GetType(typeName).Methods.Where(m => m.HasBody))
                foreach (var instruction in method.Body.Instructions.Where(i => i.Operand is MethodReference r && r.DeclaringType.FullName == "NVespa.SystemOptionData" && r.Name == "get_Render_DamageText"))
                {
                    instruction.OpCode = OpCodes.Call; instruction.Operand = Hook("DamageText");
                }
        var damage = game.GetType("NGame2.NBattleView.NUIOn3d2.Damage").Methods.Single(m => m.Name == "Open" && m.Parameters.Count == 2);
        if (!damage.Body.Instructions.Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkArenaPractice" && r.Name == "ShowDamage"))
        {
            var visible = damage.Body.Instructions.Single(i => i.Operand is FieldReference r && r.Name == "_visibleDamage");
            damage.Body.GetILProcessor().InsertAfter(visible, Instruction.Create(OpCodes.Call, Hook("ShowDamage")));
        }
        var init = custom.Methods.Single(m => m.Name == "Init");
        if (!init.Body.Instructions.Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkArenaPractice"))
            init.Body.GetILProcessor().InsertBefore(init.Body.Instructions[0], Instruction.Create(OpCodes.Call, Hook("Clear")));
        VerifyArenaPractice(game);
    }
    static void VerifyArenaPractice(ModuleDefinition game)
    {
        foreach (var row in new[] {
            ("NGame2.NUI.NWindow.MatchSelect", "SetArena", "SetSlots"),
            ("NGame2.NUI.NWindow.MatchSelect", "InitBtn", "AddCard"),
            ("NShared.BattleInstance", "SetCampaignDungeon", "CombatDungeon"),
            ("NShared.BattleCreatureContainer", "Create", "CombatTarget"),
            ("NShared.BattleCreatureContainer", "Create", "ProtectTarget"),
            ("NGame2.NBattleContext.CustomCampaignContext", "NGame2.NBattleContext.IDataProvider.GetBattleMenuType", "Menu"),
            ("NGame2.NBattleContext.CustomCampaignContext", "NGame2.NBattleContext.IDataProvider.GetBattleDefine", "DisplayDefine"),
            ("NGame2.NUI.NManager.NBattle.BattleManagement", "GetGameSpeedRatio", "Speed"),
            ("NGame2.NUI.NManager.NBattle.BattleManagement", "GetCampaignDungeonData", "CombatDungeon"),
            ("NGame2.NBattleView.NUIOn3d2.Damage", "Open", "ShowDamage"),
            ("NGame2.NBattleContext.CustomCampaignContext", "Init", "Clear") })
        {
            var method = game.GetType(row.Item1).Methods.Single(m =>
                (m.Name == row.Item2 || (row.Item3 == "Menu" && m.Name.EndsWith(".GetBattleMenuType")) || (row.Item3 == "DisplayDefine" && m.Name.EndsWith(".GetBattleDefine")))
                && (row.Item1 != "NShared.BattleInstance" || m.Parameters.Count == 3)
                && (row.Item3 != "ShowDamage" || m.Parameters.Count == 2));
            var hooks = method.Body.Instructions.Count(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkArenaPractice" && r.Name == row.Item3);
            if (hooks < 1 || (row.Item3 != "Menu" && row.Item3 != "Speed" && row.Item3 != "DisplayDefine" && hooks != 1))
                throw new Exception("Missing or duplicated Arena Practice hook: " + row);
            VerifyArenaResultStack(method);
        }
        foreach (var typeName in new[] { "NGame2.NBattleView.Creature", "NGame2.NBattleView.NDamageOn3d.Manager" })
        {
            var methods = game.GetType(typeName).Methods.Where(m => m.HasBody).ToArray();
            if (methods.SelectMany(m => m.Body.Instructions).Any(i => i.Operand is MethodReference r && r.DeclaringType.FullName == "NVespa.SystemOptionData" && r.Name == "get_Render_DamageText") ||
                !methods.SelectMany(m => m.Body.Instructions).Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkArenaPractice" && r.Name == "DamageText"))
                throw new Exception("Arena Practice damage text override is missing: " + typeName);
            foreach (var method in methods.Where(m => m.Body.Instructions.Any(i => i.Operand is MethodReference r && r.DeclaringType.Name == "SprkArenaPractice" && r.Name == "DamageText"))) VerifyArenaResultStack(method);
        }
        // Practice must retain the local custom-battle callback and waves.
        var custom = game.GetType("NGame2.NBattleContext.CustomCampaignContext");
        foreach (var method in custom.Methods.Where(m => m.HasBody))
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == "RequestOfflineArenaResult"))
                throw new Exception("Practice must not submit ranked results.");
    }
    static void InstallArenaPractice(string root, bool stageOnly, bool verifyOnly = false)
    {
        var managed = Path.GetFullPath(Path.Combine(root, "King's Raid_Data", "Managed"));
        var dll = Path.Combine(managed, "Assembly-CSharp.dll"); var helperPath = Path.Combine(managed, "SprkArenaPractice.dll");
        var staged = dll + ".arena-practice-staged";
        using var resolver = new DefaultAssemblyResolver(); resolver.AddSearchDirectory(managed);
        using var game = AssemblyDefinition.ReadAssembly(verifyOnly && stageOnly ? staged : dll, new ReaderParameters { AssemblyResolver = resolver, InMemory = true });
        if (verifyOnly)
        {
            VerifyArenaPractice(game.MainModule);
            VerifyArenaPracticeHelper(helperPath + (stageOnly ? ".staged" : ""));
            Console.WriteLine("Verified Arena Practice card, combat/presentation rules, x1 speed, scoped damage text, local exit and IL stacks."); return;
        }
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ArenaPractice.cs.txt")!;
        using var reader = new StreamReader(stream);
        var refs = Directory.GetFiles(managed, "*.dll").Where(p => p != helperPath).Select(p => MetadataReference.CreateFromFile(p));
        using var testStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ArenaPracticeProbe.cs.txt")!;
        using var testReader = new StreamReader(testStream);
        var compilation = CSharpCompilation.Create("SprkArenaPractice", new[] { CSharpSyntaxTree.ParseText(reader.ReadToEnd()), CSharpSyntaxTree.ParseText(testReader.ReadToEnd()) }, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, deterministic: true));
        using var bytes = new MemoryStream(); var result = compilation.Emit(bytes);
        if (!result.Success) throw new Exception(string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)));
        bytes.Position = 0; using var helper = AssemblyDefinition.ReadAssembly(bytes);
        PatchArenaPractice(game.MainModule, helper.MainModule);
        var count = game.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count);
        PatchArenaPractice(game.MainModule, helper.MainModule);
        if (count != game.MainModule.GetTypes().SelectMany(t => t.Methods).Where(m => m.HasBody).Sum(m => m.Body.Instructions.Count)) throw new Exception("Arena Practice is not idempotent.");
        game.Write(staged); File.WriteAllBytes(helperPath + ".staged", bytes.ToArray());
        using (var check = AssemblyDefinition.ReadAssembly(staged)) VerifyArenaPractice(check.MainModule);
        VerifyArenaPracticeHelper(helperPath + ".staged");
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root))) throw new Exception("Close client/shared workers before installing Arena Practice.");
        var backup = Path.Combine("target", "arena-practice-backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")); Directory.CreateDirectory(backup);
        File.Copy(dll, Path.Combine(backup, "Assembly-CSharp.dll"));
        if (File.Exists(helperPath)) File.Copy(helperPath, Path.Combine(backup, "SprkArenaPractice.dll"));
        File.Move(helperPath + ".staged", helperPath, true); File.Move(staged, dll, true);
        Console.WriteLine("Installed Arena Practice. Restart the client.");
    }

    static void VerifyArenaPracticeHelper(string path)
    {
        using var assembly = AssemblyDefinition.ReadAssembly(path);
        var helper = assembly.MainModule.GetType("SprkArenaPractice");
        bool Calls(string method, string call) => helper.Methods.Single(m => m.Name == method).Body.Instructions.Any(i => i.Operand is MethodReference r && r.Name == call);
        if (!Calls("CreateParty", "CreateMyEnterPlayerBattleLog") || Calls("CreateParty", "MakeFullSkillLevel") ||
            !Calls("CombatDungeon", "Clone") || !Calls("CombatTarget", "Clone") ||
            !Calls("CombatTarget", "set_Race") || !Calls("ProtectTarget", "set_ForceMinHp") ||
            !Calls("OpenParty", "Open") || !Calls("SetSlots", "AddCard") ||
            !Calls("DamageText", "get_Render_DamageText") || !Calls("DisplayDefine", "GetBattleDefine"))
            throw new Exception("Arena Practice helper is missing its native party, scoped PvP target or card contracts.");
        var forbidden = new[] { "RegisterMatch_Pvp", "RequestOfflineArenaResult", "RequestBeginCampaign", "RequestEndCampaign", "MakeFullSkillLevel" };
        if (helper.NestedTypes.Append(helper).SelectMany(t => t.Methods).Where(m => m.HasBody)
            .SelectMany(m => m.Body.Instructions).Any(i => i.Operand is MethodReference r && forbidden.Contains(r.Name)))
            throw new Exception("Practice must not change hero skill levels or send ranked/reward requests.");
    }
}
