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

        Main(new[] { clientRoot, "--stage-only" });
        if (!File.Exists(staged))
            throw new InvalidOperationException("The full patcher did not produce a staged assembly.");
        // Stage-only also emits updated companion DLLs. Promote them before
        // compiling further helpers, otherwise CI would ship baseline helpers.
        foreach (var helper in Directory.GetFiles(Path.GetDirectoryName(dll)!, "Sprk*.dll.staged"))
            File.Move(helper, helper[..^7], true);
        File.Copy(staged, dll, true);

        // Companion helpers and patches that historically had separate CLI modes.
        InstallCrafting(clientRoot, false);
        InstallConquest(clientRoot, false);
        VerifyMissionWorker(clientRoot);
        VerifyArcdimWorker(clientRoot);
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
        InstallMailUpdates(clientRoot, false);
        InstallCinematicSkip(clientRoot, false);
        InstallRoomShortcut(clientRoot, false);
        VerifyRoomShortcutFile(clientRoot);
        InstallPetPresentation(clientRoot, false);
        VerifyPetPresentationFile(clientRoot);
        InstallRaidInfoScroll(clientRoot, false);
        InstallMotionCostumes(clientRoot, false);
        VerifyMotionCostumesFile(clientRoot);
        InstallIllustrationZoom(clientRoot, false);
        VerifyIllustrationZoomFile(clientRoot);
        TestIllustrationZoom(clientRoot);
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
        VerifyRaidInfoScrollFile(clientRoot);

        InstallMissionHistory(clientRoot, false);
        VerifyMissionHistoryFile(clientRoot);
        TestMissionHistory();
        VerifyNoTelemetry(clientRoot, null);
        VerifyFriendRecommendationsFile(clientRoot, false);
        VerifyFriendPresenceFile(clientRoot);
        VerifyHonorFile(clientRoot, false);
        VerifyRuneCategories(clientRoot, false);
        VerifyWebSocketLifecycle(clientRoot);
        VerifyImageRecoveryFile(clientRoot);
        VerifyMailUpdates(clientRoot);
        VerifySettingsPersistenceFile(clientRoot);
        VerifyAccountSession(clientRoot);
        VerifyLoginSettingsFile(clientRoot);
        VerifyShopPurchaseLimitsFile(clientRoot, false);
        VerifyPetFeedingFile(clientRoot);
        TestPetFeeding(clientRoot);
        TestShopPurchaseLimits(clientRoot);
        VerifySelectorCountLimitFile(clientRoot, false);
        TestSelectorCountLimit(clientRoot);
        VerifySoulWeaponCinematicFile(clientRoot, false);
        VerifySoulLiberationCostFile(clientRoot);
        VerifyStoryAutoFile(clientRoot);
        TestStoryAutoCancellation();
        TestCampaignSurvivors();
        TestMailUpdates();
        InstallLobbyAvatarStability(clientRoot, false);
        VerifyLobbyAvatarStabilityFile(clientRoot);
        TestLobbyAvatarStability(clientRoot);
        InstallSoulUpgradePlayback(clientRoot, false);
        VerifySoulUpgradePlaybackFile(clientRoot);
        TestSoulUpgradePlayback(clientRoot);
        InstallCostumeTickets(clientRoot, false, false);
        InstallCostumeTickets(clientRoot, false, true);
        // Run last: helpers compile against the baseline API, then currency
        // signatures and every compiled consumer are migrated together.
        InstallCurrency64(clientRoot, false);
        InstallAwakeningMaterials(clientRoot, false);
        VerifyAwakeningMaterialsFile(clientRoot);
        InstallTechnomagicRefresh(clientRoot, false);
        InstallTechnomagicRefresh(clientRoot, false, true);
        VerifyCurrency64File(clientRoot);
        VerifyContinuousBattleSerializationFile(clientRoot);
        InstallRaidCoopLimits(clientRoot, false);
        InstallRaidCoopLimits(clientRoot, false, true);
        Console.WriteLine("Complete client release patching and verification passed.");
    }
}
