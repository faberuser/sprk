namespace DllPatcher;

partial class Program
{
    // One patcher-owned entry point for local release validation and CI.
    // Add gameplay patches to the full patch path in Program.cs/their owning
    // installer. The build service never maintains a second list of patch modes.
    static void BuildClientRelease(string clientRoot)
    {
        var dll = Path.Combine(clientRoot, "King's Raid_Data", "Managed", "Assembly-CSharp.dll");
        var staged = dll + ".patched";
        if (File.Exists(staged))
            throw new InvalidOperationException("Release builds require a fresh client workspace without a staged DLL.");

        TestLegendaryRuneTickets();
        InstallPunishmentRewards(clientRoot, false);
        InstallPunishmentRewards(clientRoot, true);
        RestoreLegendaryRuneTickets(clientRoot, false);
        Main(new[] { clientRoot, "--stage-only" });
        if (!File.Exists(staged))
            throw new InvalidOperationException("The full patcher did not produce a staged assembly.");
        // Stage-only also emits updated companion DLLs. Promote them before
        // compiling further helpers, otherwise CI would ship baseline helpers.
        foreach (var helper in Directory.GetFiles(Path.GetDirectoryName(dll)!, "Sprk*.dll.staged"))
            File.Move(helper, helper[..^7], true);
        File.Copy(staged, dll, true);
        using(var purify=Mono.Cecil.AssemblyDefinition.ReadAssembly(dll)) VerifyPurifyHudReuse(purify.MainModule);
        VerifyCutsceneCostumesFile(clientRoot);

        // Companion helpers and patches that historically had separate CLI modes.
        InstallCrafting(clientRoot, false);
        InstallConquest(clientRoot, false);
        VerifyMissionWorker(clientRoot);
        VerifyDispatchPacing(clientRoot);
        VerifyArcdimWorker(clientRoot);
        VerifyArenaLatency(clientRoot);
        InstallHonorBuff(clientRoot, false);
        VerifyHonorBuffFile(clientRoot);
        TestHonorBuff();
        InstallDispatch(clientRoot, false);
        VerifyDispatchFile(clientRoot);
        TestDispatch();
        InstallNetworkPerformance(clientRoot, false);
        VerifyNetworkPerformanceFile(clientRoot);
        TestNetworkPerformance();
        InstallAutomation(clientRoot);
        InstallFreeCamera(clientRoot, false);
        VerifyFreeCameraFile(clientRoot);
        InstallGraphicsOptions(clientRoot, false);
        VerifyGraphicsOptionsFile(clientRoot);
        InstallClientStorage(clientRoot, false);
        VerifyClientStorage(clientRoot);
        TestClientStorage();
        InstallPlayerLogging(clientRoot, false);
        VerifyPlayerLogging(clientRoot, false);
        InstallMailUpdates(clientRoot, false);
        InstallCinematicSkip(clientRoot, false);
        InstallRoomShortcut(clientRoot, false);
        VerifyRoomShortcutFile(clientRoot);
        InstallPetPresentation(clientRoot, false);
        VerifyPetPresentationFile(clientRoot);
        InstallRaidInfoScroll(clientRoot, false);
        InstallMotionCostumes(clientRoot, false);
        VerifyMotionCostumesFile(clientRoot);
        InstallArenaPractice(clientRoot, false);
        InstallArenaPractice(clientRoot, false, true);
        InstallIllustrationZoom(clientRoot, false);
        VerifyIllustrationZoomFile(clientRoot);
        TestIllustrationZoom(clientRoot);
        InstallIllustrationUpscale(clientRoot, false);
        VerifyIllustrationUpscaleFile(clientRoot);
        TestIllustrationUpscale();
        InstallWorldBossKeys(clientRoot, false);
        VerifyWorldBossKeysFile(clientRoot);
        InstallInventoryExpansion(clientRoot, false);
        VerifyInventoryExpansionFile(clientRoot);
        InstallSpecialShop(clientRoot, false);
        VerifySpecialShopFile(clientRoot);
        InstallShopTags(clientRoot, false);
        VerifyShopTagsFile(clientRoot);
        InstallArtifactSelectors(clientRoot, false);
        VerifyArtifactSelectorsFile(clientRoot);
        InstallSkillDescriptionFormula(clientRoot, false);
        VerifySkillDescriptionFormulaFile(clientRoot);
        InstallArtifactSourceFilter(clientRoot, false);
        VerifyArtifactSourceFilterFile(clientRoot);
        InstallTicketSearch(clientRoot, false);
        VerifyTicketSearchFile(clientRoot);
        InstallSelectorAcquireState(clientRoot, false);
        VerifySelectorAcquireStateFile(clientRoot);
        InstallSoulstoneOwnership(clientRoot, false);
        InstallSoulstoneOwnership(clientRoot, false, true);
        InstallTeamExpRefresh(clientRoot, false);
        VerifyTeamExpRefreshFile(clientRoot);
        VerifyRaidInfoScrollFile(clientRoot);

        InstallMissionHistory(clientRoot, false);
        VerifyMissionHistoryFile(clientRoot);
        TestMissionHistory();
        VerifyNoTelemetry(clientRoot, null);
        VerifyFriendRecommendationsFile(clientRoot, false);
        VerifyFriendPresenceFile(clientRoot);
        VerifyHonorFile(clientRoot, false);
        VerifyRuneCategories(clientRoot, false);
        InstallHeroRuneFilter(clientRoot, false);
        VerifyHeroRuneFilterFile(clientRoot);
        RestoreLegendaryRuneTickets(clientRoot, true);
        VerifyWebSocketLifecycle(clientRoot);
        VerifyImageRecoveryFile(clientRoot);
        VerifyMailUpdates(clientRoot);
        VerifySettingsPersistenceFile(clientRoot);
        VerifyAccountSession(clientRoot);
        VerifyLoginSettingsFile(clientRoot);
        RemoveCenterLoginLogos(clientRoot);
        VerifyLoginPresentationAssets(clientRoot);
        VerifyShopPurchaseLimitsFile(clientRoot, false);
        VerifyPetFeedingFile(clientRoot);
        TestPetFeeding(clientRoot);
        TestShopPurchaseLimits(clientRoot);
        VerifyPunishmentLevelSelectionFile(clientRoot, false);
        TestPunishmentLevelSelection(clientRoot);
        VerifySelectorCountLimitFile(clientRoot, false);
        TestSelectorCountLimit(clientRoot);
        VerifySoulWeaponCinematicFile(clientRoot, false);
        InstallSoulUpgradeFlash(clientRoot, false, false);
        InstallSoulUpgradeFlash(clientRoot, false, true);
        VerifySoulLiberationCostFile(clientRoot);
        VerifyStoryAutoFile(clientRoot);
        TestStoryAutoRecovery();
        TestStoryAutoCancellation();
        TestStoryAutoSkills();
        TestCampaignSurvivors();
        TestMailUpdates();
        InstallLobbyAvatarStability(clientRoot, false);
        VerifyLobbyAvatarStabilityFile(clientRoot);
        TestLobbyAvatarStability(clientRoot);
        InstallEnchantEmptyOption(clientRoot, false);
        InstallEnchantEmptyOption(clientRoot, false, true);
        InstallSoulUpgradePlayback(clientRoot, false);
        VerifySoulUpgradePlaybackFile(clientRoot);
        TestSoulUpgradePlayback(clientRoot);
        InstallCostumeTickets(clientRoot, false, false);
        InstallCostumeTickets(clientRoot, false, true);
        InstallClassWeaponSelectors(clientRoot, false);
        InstallClassWeaponSelectors(clientRoot, true);
        // Run last: helpers compile against the baseline API, then currency
        // signatures and every compiled consumer are migrated together.
        InstallHeroMaterialTickets(clientRoot, false);
        VerifyHeroMaterialTicketsFile(clientRoot);
        InstallBattleResultFlask(clientRoot, false);
        VerifyBattleResultFlaskFile(clientRoot);
        // Reads Gold, so it must compile before the Currency64 migration.
        InstallTechnomagicAwakenTarget(clientRoot, false);
        InstallSkillUpgradeAll(clientRoot, false);
        InstallCurrency64(clientRoot, false);
        InstallGearPresetPortraits(clientRoot, false);
        InstallGearPresetPortraits(clientRoot, true);
        InstallPresetDisposal(clientRoot, false);
        InstallPresetDisposal(clientRoot, true);
        InstallAwakeningMaterials(clientRoot, false);
        VerifyAwakeningMaterialsFile(clientRoot);
        InstallTechnomagicRefresh(clientRoot, false);
        InstallTechnomagicRefresh(clientRoot, false, true);
        InstallTechnomagicEnchantRefresh(clientRoot, false);
        InstallTechnomagicEnchantRefresh(clientRoot, false, true);
        VerifyCurrency64File(clientRoot);
        VerifyContinuousBattleSerializationFile(clientRoot);
        InstallSoloRetry(clientRoot, false);
        InstallSoloRetry(clientRoot, false, true);
        InstallPartyRetry(clientRoot, false);
        InstallPartyRetry(clientRoot, false, true);
        InstallGodkingRechargePrompt(clientRoot, false);
        InstallGodkingRechargePrompt(clientRoot, true);
        VerifyGodkingBackground(clientRoot);
        InstallValanceSkillSpeed(clientRoot, false);
        InstallValanceSkillSpeed(clientRoot, true);
        InstallHonorTicketRecharge(clientRoot, false);
        InstallHonorTicketRecharge(clientRoot, true);
        InstallTeamLevelBuffReset(clientRoot, false);
        InstallTeamLevelBuffReset(clientRoot, true);
        InstallGearFilterOptions(clientRoot, false);
        InstallGearFilterOptions(clientRoot, true);
        InstallFriendlyDuelMenu(clientRoot, false);
        InstallFriendlyDuelMenu(clientRoot, true);
        InstallHonorDuel(clientRoot, false);
        InstallHonorDuel(clientRoot, false, true);
        InstallDispatchSelection(clientRoot, false);
        InstallDispatchSelection(clientRoot, true);
        VerifyOrdealUnlockFile(clientRoot, false);
        InstallMariaLegendBird(clientRoot);
        InstallMariaLegendBird(clientRoot,true);
        InstallLegacyBannerRemoval(clientRoot, false);
        InstallBatchGrind(clientRoot, false);
        InstallBatchGrind(clientRoot, false, true);
        InstallHeroExpLevel(clientRoot, false);
        InstallHeroExpLevel(clientRoot, false, true);
        InstallHeroInnPresentation(clientRoot, false);
        InstallHeroInnPresentation(clientRoot, false, true);
        InstallLinePets(clientRoot, false);
        InstallLinePets(clientRoot, false, true);
        RestoreLinePetCurrency(clientRoot, false);
        RestoreLinePetCurrency(clientRoot, true);
        InstallVoidRuneShopLimits(clientRoot, false);
        InstallVoidRuneShopLimits(clientRoot, true);
        RestoreConquestDungeonIcons(clientRoot, false);
        RestoreConquestDungeonIcons(clientRoot, true);
        InstallHeroDeathVoices(clientRoot, false);
        InstallHeroDeathVoices(clientRoot, false, true);
        InstallAccessories(clientRoot, false);
        InstallAccessories(clientRoot, false, true);
        // Conquest label and raid table validation load the game assembly into
        // this process and keep it locked on Windows. Finish all DLL
        // replacements first.
        InstallConquestRoundLabel(clientRoot, false);
        InstallRaidCoopLimits(clientRoot, false);
        InstallRaidCoopLimits(clientRoot, false, true);
        InstallLavrilCircle(clientRoot, false);
        InstallLavrilCircle(clientRoot, false, true);
        Console.WriteLine("Complete client release patching and verification passed.");
    }
}
