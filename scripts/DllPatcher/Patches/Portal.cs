using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DllPatcher;

partial class Program
{
    static void PatchPortal(ModuleDefinition module)
    {
        using var stream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.Portal.cs.txt")!;
        using var reader = new StreamReader(stream);
        using var raidStream = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.ReconstructedRaids.json")!;
        using var raidReader = new StreamReader(raidStream);
        var sourceText = reader.ReadToEnd().Replace("\"__RECONSTRUCTED_RAIDS__\"", System.Text.Json.JsonSerializer.Serialize(raidReader.ReadToEnd()));
        var references = Directory.GetFiles(Path.GetDirectoryName(module.FileName)!, "*.dll")
            .Select(path => MetadataReference.CreateFromFile(path));
        var compilation = CSharpCompilation.Create("PortalRestore",
            new[] { CSharpSyntaxTree.ParseText(sourceText) }, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var compiled = new MemoryStream();
        var result = compilation.Emit(compiled);
        if (!result.Success) throw new InvalidOperationException(string.Join("\n", result.Diagnostics));
        compiled.Position = 0;
        using var sourceAssembly = AssemblyDefinition.ReadAssembly(compiled);
        var sourceType = sourceAssembly.MainModule.Types.Single(t => t.Name == "RestoredPortal");
        var targetType = module.Types.SingleOrDefault(t => t.Name == sourceType.Name);
        if (targetType == null)
        {
            targetType = new TypeDefinition("", sourceType.Name, sourceType.Attributes, module.TypeSystem.Object);
            module.Types.Add(targetType);
        }
        foreach (var source in sourceType.Methods)
        {
            if (targetType.Methods.Any(m => m.Name == source.Name)) continue;
            var target = new MethodDefinition(source.Name, source.Attributes, module.ImportReference(source.ReturnType));
            foreach (var parameter in source.Parameters)
                target.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, module.ImportReference(parameter.ParameterType)));
            targetType.Methods.Add(target);
        }
        foreach (var source in sourceType.Methods)
        {
            var target = targetType.Methods.Single(m => m.Name == source.Name);
            var body = new MethodBody(target) { InitLocals = source.Body.InitLocals, MaxStackSize = source.Body.MaxStackSize };
            target.Body = body;
            foreach (var variable in source.Body.Variables)
                body.Variables.Add(new VariableDefinition(module.ImportReference(variable.VariableType)));
            var instructions = source.Body.Instructions.ToDictionary(i => i, i => Instruction.Create(OpCodes.Nop));
            foreach (var instruction in source.Body.Instructions)
            {
                var copy = instructions[instruction];
                copy.OpCode = instruction.OpCode;
                copy.Operand = instruction.Operand switch
                {
                    Instruction branch => instructions[branch],
                    Instruction[] branches => branches.Select(b => instructions[b]).ToArray(),
                    VariableDefinition variable => body.Variables[variable.Index],
                    ParameterDefinition parameter => target.Parameters[parameter.Index],
                    MethodReference method when method.DeclaringType.FullName == sourceType.FullName => targetType.Methods.Single(m => m.Name == method.Name),
                    MethodReference method => module.ImportReference(method),
                    FieldReference field => module.ImportReference(field),
                    TypeReference type => module.ImportReference(type),
                    var operand => operand
                };
                body.Instructions.Add(copy);
            }
            foreach (var handler in source.Body.ExceptionHandlers)
                body.ExceptionHandlers.Add(new ExceptionHandler(handler.HandlerType) {
                    TryStart = instructions[handler.TryStart], TryEnd = handler.TryEnd == null ? null : instructions[handler.TryEnd],
                    HandlerStart = instructions[handler.HandlerStart], HandlerEnd = handler.HandlerEnd == null ? null : instructions[handler.HandlerEnd],
                    CatchType = handler.CatchType == null ? null : module.ImportReference(handler.CatchType),
                    FilterStart = handler.FilterStart == null ? null : instructions[handler.FilterStart]
                });
        }
        // Carry the server's authoritative Raider balance alongside every stamina
        // result, and apply it on the same main-thread path as the stamina balance.
        var staminaType = module.Types.Single(t => t.FullName == "NShared.StaminaResultInfo");
        var expType = module.Types.Single(t => t.FullName == "NShared.ExpResultInfo");
        var expField = staminaType.Fields.SingleOrDefault(f => f.Name == "SprkRaiderExp");
        if (expField == null)
        {
            expField = new FieldDefinition("SprkRaiderExp", FieldAttributes.Public, expType);
            staminaType.Fields.Add(expField);
        }
        var parseStamina = module.Types.Single(t => t.Name == "JM_NShared_StaminaResultInfo").Methods
            .Single(m => m.Name == "Parse" && m.Parameters.Count == 2 && m.Parameters[0].ParameterType.FullName == "System.Collections.IDictionary");
        var parseExp = targetType.Methods.Single(m => m.Name == "ParseStaminaExp");
        if (!parseStamina.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == parseExp.Name))
        {
            var il = parseStamina.Body.GetILProcessor(); var first = parseStamina.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, parseExp));
            il.InsertBefore(first, Instruction.Create(OpCodes.Stfld, expField));
        }
        var userManager = module.Types.Single(t => t.FullName == "NGame2.NAccount.UserManager");
        var applyStamina = userManager.Methods.Single(m => m.Name == "Apply" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == staminaType.FullName);
        var applyExp = userManager.Methods.Single(m => m.Name == "Apply" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == expType.FullName);
        if (!applyStamina.Body.Instructions.Any(i => i.Operand is FieldReference f && f.Name == expField.Name))
        {
            var il = applyStamina.Body.GetILProcessor(); var first = applyStamina.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldfld, expField));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, applyExp));
        }
        foreach (var type in module.GetTypes().Where(t => t.FullName.Contains("MailManagement")))
        foreach (var method in type.Methods.Where(m => m.HasBody && m.ReturnType.FullName == "System.Void"))
        {
            var responseParameter = method.Parameters.FirstOrDefault(p => p.ParameterType.FullName is "NShared.ReceiveMail.Response" or "NShared.ReceiveAllMail.Response");
            if (responseParameter == null || method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == "ShowMailCapacityError")) continue;
            var il = method.Body.GetILProcessor(); var first = method.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg, responseParameter));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, targetType.Methods.Single(m => m.Name == "ShowMailCapacityError")));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var unlockNotice = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.NLobby.LobbyPopupQueue")
            .Methods.Single(m => m.Name == "ReservedContentsOpenConditionPopup" && m.Parameters.Count == 1);
        var unlockHelper = targetType.Methods.Single(m => m.Name == "ReserveContentsUnlockNotice");
        if (!unlockNotice.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == unlockHelper.Name))
        {
            var il = unlockNotice.Body.GetILProcessor();
            var first = unlockNotice.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg, unlockNotice.Parameters[0]));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, unlockHelper));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var emptyScene = module.Types.Single(t => t.FullName == "NGame2.NCutScene.CutSceneManager")
            .Methods.Single(m => m.Name == "LoadEmptyScript");
        var missingSceneHelper = targetType.Methods.Single(m => m.Name == "TempleMissingScene");
        if (!emptyScene.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == missingSceneHelper.Name))
        {
            var il = emptyScene.Body.GetILProcessor();
            foreach (var ret in emptyScene.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, missingSceneHelper);
                il.InsertAfter(ret, call); il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var shakmehManager = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.ShakmehDungeonManagement");
        foreach (var method in shakmehManager.Methods.Where(m => m.Name == "OpenShakmehDungeonWindow" && m.IsPublic))
        {
            var helper = targetType.Methods.Single(m => m.Name == "ShakmehAccess");
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == helper.Name)) continue;
            var il = method.Body.GetILProcessor(); var first = method.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, helper));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var soulManager = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.NLobby.SoulWeaponManagement");
        foreach (var method in soulManager.Methods.Where(m => m.Name is "OpenGodkingTrialDungeonList" or "OpenGodkingTrialDungeonInfo" or "OpenEclipseMainWithReqeust"))
        {
            var helper = targetType.Methods.Single(m => m.Name == "GodkingAccess");
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == helper.Name)) continue;
            var il = method.Body.GetILProcessor(); var first = method.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, helper));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var moveActivity = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.PortalRenewal")
            .Methods.Single(m => m.Name == "IsMoveActivityType");
        var closePortalHelper = targetType.Methods.Single(m => m.Name == "ClosePortalForActivity");
        if (!moveActivity.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == closePortalHelper.Name))
        {
            var il = moveActivity.Body.GetILProcessor();
            foreach (var ret in moveActivity.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg; ret.Operand = moveActivity.Parameters[0];
                var call = Instruction.Create(OpCodes.Call, closePortalHelper);
                il.InsertAfter(ret, call); il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var subStoryHidden = module.Types.Single(t => t.FullName == "NShared.CampaignNodeData")
            .Methods.Single(m => m.Name == "get_Hidden");
        var subStoryHelper = targetType.Methods.Single(m => m.Name == "ChapterFiveSubStoryHidden");
        if (!subStoryHidden.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == subStoryHelper.Name))
        {
            var il = subStoryHidden.Body.GetILProcessor();
            foreach (var ret in subStoryHidden.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, subStoryHelper);
                il.InsertAfter(ret, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var portalHidden = module.Types.Single(t => t.FullName == "NShared.PortalData")
            .Methods.Single(m => m.Name == "get_Hidden");
        var worldMapHelper = targetType.Methods.Single(m => m.Name == "WorldMapHidden");
        if (!portalHidden.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == worldMapHelper.Name))
        {
            var il = portalHidden.Body.GetILProcessor();
            foreach (var ret in portalHidden.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, worldMapHelper);
                il.InsertAfter(ret, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var mainChapters = module.Types.Single(t => t.FullName == "NShared.CampaignChapterDataContainer")
            .Methods.Single(m => m.Name == "GetMainChapterDataDic");
        var mainHelper = targetType.Methods.Single(m => m.Name == "MainChapters");
        if (!mainChapters.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == mainHelper.Name))
        {
            var il = mainChapters.Body.GetILProcessor();
            foreach (var ret in mainChapters.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Call; ret.Operand = mainHelper;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Ret));
            }
        }
        var chapterOpen = module.Types.Single(t => t.FullName == "NShared.CampaignChapterData")
            .Methods.Single(m => m.Name == "get_IsOpen");
        var chapterHelper = targetType.Methods.Single(m => m.Name == "RestoredCampaignChapterOpen");
        if (!chapterOpen.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == chapterHelper.Name))
        {
            var il = chapterOpen.Body.GetILProcessor();
            foreach (var ret in chapterOpen.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0; ret.Operand = null;
                var call = Instruction.Create(OpCodes.Call, chapterHelper);
                il.InsertAfter(ret, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var bagMax = module.Types.Single(t => t.FullName == "NShared.InventoryExtendDataContainer")
            .Methods.Single(m => m.Name == "GetExtendMaxCount");
        var bagHelper = targetType.Methods.Single(m => m.Name == "EquipmentBagMaxExtensions");
        if (!bagMax.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == bagHelper.Name))
        {
            var il = bagMax.Body.GetILProcessor();
            foreach (var ret in bagMax.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg;
                ret.Operand = bagMax.Parameters[0];
                var call = Instruction.Create(OpCodes.Call, bagHelper);
                il.InsertAfter(ret, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        var storyGetter = module.Types.Single(t => t.FullName == "NGame2.NTutorial.TutorialManager")
            .Methods.Single(m => m.Name == "get_IsTutorialEnabled");
        var storyHelper = targetType.Methods.Single(m => m.Name == "RequiredBattleStoryEnabled");
        if (!storyGetter.Body.Instructions.Any(i => i.Operand is MethodReference m && m.FullName == storyHelper.FullName))
        {
            var il = storyGetter.Body.GetILProcessor();
            foreach (var ret in storyGetter.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Call;
                ret.Operand = storyHelper;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Ret));
            }
        }
        var punishOpen = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.PunishManagment")
            .Methods.Single(m => m.Name == "OpenPunishMainWindow");
        var punishAccess = targetType.Methods.Single(m => m.Name == "ApocalypsionAccess");
        if (!punishOpen.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == punishAccess.Name))
        {
            var il = punishOpen.Body.GetILProcessor(); var first = punishOpen.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, punishAccess));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var trialWindow = module.Types.Single(t => t.Name == "ContentUIGodkingTrialDungeonList");
        foreach (var pair in new[] { ("Open", "GodkingWindowBackground"), ("OnClosing", "GodkingReturnToPortal") })
        {
            var method = trialWindow.Methods.Single(m => m.Name == pair.Item1);
            var helper = targetType.Methods.Single(m => m.Name == pair.Item2);
            if (method.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == helper.Name)) continue;
            var il = method.Body.GetILProcessor();
            foreach (var ret in method.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = pair.Item1 == "Open" ? OpCodes.Ldarg_0 : OpCodes.Nop;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, helper));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        var dispatchMenu = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.LobbyTopmostMenu")
            .Methods.Single(m => m.Name == "ShowDispatchBattleMenu");
        var dispatchVisible = targetType.Methods.Single(m => m.Name == "DispatchShortcutVisible");
        if (!dispatchMenu.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == dispatchVisible.Name))
        {
            var il = dispatchMenu.Body.GetILProcessor(); var first = dispatchMenu.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, dispatchVisible));
            il.InsertBefore(first, Instruction.Create(OpCodes.Starg, dispatchMenu.Parameters[0]));
        }
        var dispatchClick = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.LobbyTopmostMenu")
            .Methods.Single(m => m.Name == "OnClickDispatchBattleMenu");
        var dispatchEntries = targetType.Methods.Single(m => m.Name == "DispatchListHasEntries");
        if (!dispatchClick.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == dispatchEntries.Name))
        {
            var il = dispatchClick.Body.GetILProcessor(); var first = dispatchClick.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, dispatchEntries));
            il.InsertBefore(first, Instruction.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ret));
        }
        var heroDeck = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.HeroDeck2")
            .Methods.Single(m => m.Name == "CreateHeroItem" && m.Parameters.Count == 0);
        var dispatchHeroes = targetType.Methods.Single(m => m.Name == "RefreshHeroDeckDispatch");
        if (!heroDeck.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == dispatchHeroes.Name))
        {
            var il = heroDeck.Body.GetILProcessor();
            foreach (var ret in heroDeck.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Ldarg_0;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, dispatchHeroes));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        var cardType = module.Types.Single(t => t.FullName == "NGame2.NUI.NComponent2.PortalContentsComponent");
        var cardBackground = cardType.Methods.Single(m => m.Name == "SetBackgroundTexture");
        var curveHelper = targetType.Methods.Single(m => m.Name == "SetupCardCurve");
        if (!cardBackground.Body.Instructions.Any(i => i.Operand is MethodReference m && m.Name == curveHelper.Name))
        {
            var il = cardBackground.Body.GetILProcessor();
            var first = cardBackground.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, cardType.Methods.Single(m => m.Name == "get__portalRenewalData")));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, curveHelper));
        }
        var container = module.Types.Single(t => t.FullName == "NShared.PortalRenewalDataContainer");
        foreach (var pair in new[] { ("GetCategoryDatas", "Categories"), ("GetData", "Contents") })
        {
            var target = container.Methods.Single(m => m.Name == pair.Item1);
            var helper = targetType.Methods.Single(m => m.Name == pair.Item2);
            if (target.Body.Instructions.Any(i => i.Operand is MethodReference m && m.FullName == helper.FullName)) continue;
            // Mutate the return itself so existing branches to it also pass through the hook.
            foreach (var ret in target.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                var il = target.Body.GetILProcessor();
                ret.OpCode = pair.Item1 == "GetData" ? OpCodes.Ldarg_1 : OpCodes.Nop;
                il.InsertAfter(ret, Instruction.Create(OpCodes.Call, helper));
                il.InsertAfter(ret.Next, Instruction.Create(OpCodes.Ret));
            }
        }
        var raids = module.Types.Single(t => t.FullName == "NShared.RaidDataContainer");
        foreach (var pair in new[] { ("GetData", "Raid"), ("GetRaidIndices", "RaidIndices"), ("GetRaidLevels", "RaidLevels"), ("GetRaidDataListByType", "RaidList"), ("GetRaidDataByDungeon", "RaidDungeon"), ("GetMultiRaidIndexBySingleRaidIndex", "MultiRaid") })
        {
            var target = raids.Methods.Single(m => m.Name == pair.Item1);
            var helper = targetType.Methods.Single(m => m.Name == pair.Item2);
            if (target.Body.Instructions.Any(i => i.Operand is MethodReference m && m.FullName == helper.FullName)) continue;
            foreach (var ret in target.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToArray())
            {
                ret.OpCode = OpCodes.Nop;
                var il = target.Body.GetILProcessor();
                var cursor = ret;
                foreach (var parameter in target.Parameters)
                {
                    var load = Instruction.Create(OpCodes.Ldarg, parameter);
                    il.InsertAfter(cursor, load); cursor = load;
                }
                var call = Instruction.Create(OpCodes.Call, helper);
                il.InsertAfter(cursor, call);
                il.InsertAfter(call, Instruction.Create(OpCodes.Ret));
            }
        }
        foreach (var pair in new[] { ("OpenValanceCraft", "OpenValanceCraft"), ("OpenValanceIndentified", "OpenValanceIdentified"), ("OpenValanceManage", "OpenValanceManage") })
        {
            var action = module.Types.Single(t => t.FullName == "NGame2.NUI.NWindow.NEntryMenu.NAction." + pair.Item1);
            var run = action.Methods.Single(m => m.Name.EndsWith(".Run") && m.Parameters.Count == 3);
            run.Body = new MethodBody(run);
            var il = run.Body.GetILProcessor();
            il.Append(Instruction.Create(OpCodes.Ldarg_1));
            il.Append(Instruction.Create(OpCodes.Call, targetType.Methods.Single(m => m.Name == pair.Item2)));
            il.Append(Instruction.Create(OpCodes.Ret));
        }
        var matchSelect = module.Types.Single(t => t.FullName == "NGame2.NUI.NManager.MatchManagement")
            .Methods.Single(m => m.Name == "OpenMatchSelect" && m.Parameters.Count == 3);
        if (!matchSelect.Body.Instructions.Any(i => i.Operand is MethodReference m && m.DeclaringType.Name == "RestoredPortal" && m.Name == "ArenaMenu"))
        {
            var il = matchSelect.Body.GetILProcessor();
            var first = matchSelect.Body.Instructions[0];
            il.InsertBefore(first, Instruction.Create(OpCodes.Ldarg_3));
            il.InsertBefore(first, Instruction.Create(OpCodes.Call, targetType.Methods.Single(m => m.Name == "ArenaMenu")));
            il.InsertBefore(first, Instruction.Create(OpCodes.Starg, matchSelect.Parameters[2]));
        }
        if (module.AssemblyReferences.Any(r => r.Name == "PortalRestore"))
            throw new InvalidOperationException("Portal patch leaked a helper assembly reference");
        Console.WriteLine("Restored missing Portal categories and shared reconstructed raid definitions.");
    }
}

