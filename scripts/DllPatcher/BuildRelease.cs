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
        VerifyArcdimWorker(clientRoot);
        InstallDispatch(clientRoot, false);
        InstallAutomation(clientRoot);
        InstallMailUpdates(clientRoot, false);
        InstallCinematicSkip(clientRoot, false);
        InstallPetPresentation(clientRoot, false);
        VerifyPetPresentationFile(clientRoot);
        InstallRaidInfoScroll(clientRoot, false);
        InstallShopTags(clientRoot, false);
        VerifyShopTagsFile(clientRoot);
        VerifyRaidInfoScrollFile(clientRoot);

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
        Console.WriteLine("Complete client release patching and verification passed.");
    }
}
