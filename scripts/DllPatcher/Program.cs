using System;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace DllPatcher
{
    partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("--verify-currency64")) { VerifyCurrency64File(args[0]); TestCurrency64(args[0]); return; }
            if (args.Contains("--currency64-only")) {
                var report=Array.IndexOf(args,"--currency64-report");
                InstallCurrency64(args[0],args.Contains("--stage-only"),report<0?null:args[report+1]); return;
            }
            if (args.Contains("--mission-history-only")) { InstallMissionHistory(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-mission-history")) { VerifyMissionHistoryFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-mission-history")) { TestMissionHistory(); return; }
            if (args.Contains("--artifact-selectors-only")) { InstallArtifactSelectors(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-artifact-selectors")) { VerifyArtifactSelectorsFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--world-boss-keys-only")) { InstallWorldBossKeys(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-world-boss-keys")) { VerifyWorldBossKeysFile(args[0]); return; }
            if (args.Contains("--network-performance-only")) { InstallNetworkPerformance(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-network-performance")) { VerifyNetworkPerformanceFile(args[0]); return; }
            if (args.Contains("--self-test-network-performance")) { TestNetworkPerformance(); return; }
            if (args.Contains("--network-runtime-probe")) { InstallNetworkRuntimeProbe(args[0]); return; }
            if (args.Contains("--self-test-client-storage")) { TestClientStorage(); return; }
            if (args.Contains("--client-storage-only")) { InstallClientStorage(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-client-storage")) { VerifyClientStorage(args[0]); return; }
            if (args.Contains("--verify-login-settings")) { VerifyLoginSettingsFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--free-camera-only")) { InstallFreeCamera(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-free-camera")) { VerifyFreeCameraFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--graphics-options-only")) { InstallGraphicsOptions(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-graphics-options")) { VerifyGraphicsOptionsFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--room-shortcut-only")) { InstallRoomShortcut(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-room-shortcut")) { VerifyRoomShortcutFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--pet-presentation-only")) { InstallPetPresentation(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-pet-presentation")) { VerifyPetPresentationFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--inventory-expansion-only")) { InstallInventoryExpansion(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-inventory-expansion")) { VerifyInventoryExpansionFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--special-shop-only")) { InstallSpecialShop(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-special-shop")) { VerifySpecialShopFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--shop-tags-only")) { InstallShopTags(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-shop-tags")) { VerifyShopTagsFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--raid-info-scroll-only")) { InstallRaidInfoScroll(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-raid-info-scroll")) { VerifyRaidInfoScrollFile(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--friend-presence-only")) { InstallFriendPresence(args[0]); return; }
            if (args.Contains("--verify-friend-presence")) { VerifyFriendPresenceFile(args[0]); return; }
            if (args.Contains("--craft-shortcut-only")) { InstallCraftShortcut(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-craft-shortcut")) { VerifyCraftShortcutFile(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-craft-shortcut")) { TestCraftShortcut(args[0]); return; }
            if (args.Contains("--soul-liberation-cost-only")) { InstallSoulLiberationCost(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-soul-liberation-cost")) { VerifySoulLiberationCostFile(args[0]); return; }
            if (args.Contains("--build-release")) { BuildClientRelease(args[0]); return; }
            if (args.Contains("--pet-feeding-only")) { InstallPetFeeding(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-pet-feeding")) { VerifyPetFeedingFile(args[0]); return; }
            if (args.Contains("--self-test-pet-feeding")) { TestPetFeeding(args[0]); return; }
            if (args.Contains("--restore-soul-weapon-cinematic")) {
                var source = Array.IndexOf(args, "--source");
                if (source < 0 || source + 1 >= args.Length) throw new ArgumentException("Cinematic restoration requires --source <original-assembly>");
                RestoreSoulWeaponCinematic(args[0], args[source + 1], args.Contains("--stage-only")); return;
            }
            if (args.Contains("--verify-soul-weapon-cinematic")) { VerifySoulWeaponCinematicFile(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--selector-count-limit-only")) { InstallSelectorCountLimit(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-selector-count-limit")) { VerifySelectorCountLimitFile(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-selector-count-limit")) { TestSelectorCountLimit(args[0]); return; }
            if (args.Contains("--shop-purchase-limits-only")) { InstallShopPurchaseLimits(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-shop-purchase-limits")) { VerifyShopPurchaseLimitsFile(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-shop-purchase-limits")) { TestShopPurchaseLimits(args[0]); return; }
            if (args.Contains("--inspect-settings")) { InspectSettings(args[0]); return; }
            if (args.Contains("--settings-persistence-only")) { InstallSettingsPersistence(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-settings-persistence")) { VerifySettingsPersistenceFile(args[0]); return; }
            if (args.Contains("--verify-account-session")) { VerifyAccountSession(args[0]); return; }
            if (args.Contains("--friend-recommendations-only")) { InstallFriendRecommendations(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-friend-recommendations")) { VerifyFriendRecommendationsFile(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-friend-recommendations")) { TestFriendRecommendations(args[0]); return; }
            if (args.Contains("--ordeal-unlock-only")) { InstallOrdealUnlock(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-ordeal-unlock")) { VerifyOrdealUnlockFile(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--mission-claim-all-only")) { InstallMissionClaimAll(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-mission-claim-all")) { VerifyMissionClaimAllFile(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--chat-channels-only") || args.Contains("--chat-send-only")) { InstallChatChannels(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-chat-channels") || args.Contains("--verify-chat-send")) { VerifyChatChannelsFile(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--honor-only")) { InstallHonor(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-honor")) { VerifyHonorFile(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--runes-only")) { InstallRuneCategories(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-runes")) { VerifyRuneCategories(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--hero-stats-only")) { InstallHeroStats(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-hero-stats")) { TestHeroStats(args[0]); return; }
            if (args.Contains("--orvel-visuals-only")) { InstallOrvelVisuals(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-orvel-visuals")) { VerifyOrvelVisualsFile(Path.Combine(args[0],"King's Raid_Data","Managed","Assembly-CSharp.dll"+(args.Contains("--stage-only")?".orvel-visuals-staged":""))); return; }
            if (args.Contains("--verify-conquest-session-cache")) { VerifyConquestSessionCache(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-conquest-party-cleanup")) { VerifyConquestPartyCleanup(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--prepare-loading-background-probe")) { PrepareLoadingBackgroundProbe(args[0]); return; }
            if (args.Contains("--image-recovery-only")) { InstallImageRecovery(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-image-recovery")) { VerifyImageRecoveryFile(args[0]); return; }
            if (args.Contains("--cinematic-skip-only")) { InstallCinematicSkip(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-cinematic-skip")) { VerifyCinematicSkipFile(args[0]); return; }
            if (args.Contains("--raid-coop-limits-only")) { InstallRaidCoopLimits(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--arena-responsive-only")) { InstallArenaResponsiveness(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-arena-responsive")) { VerifyArenaResponsivenessFile(args[0]); return; }
            if (args.Contains("--arena-rematch-only")) { InstallArenaRematchRemoval(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-arena-rematch")) { VerifyArenaRematchFile(args[0]); return; }
            if (args.Contains("--verify-arena-worker")) { VerifyArenaWorker(args[0]); return; }
            if (args.Contains("--verify-arcdim-worker")) { VerifyArcdimWorker(args[0]); return; }
            if (args.Contains("--party-join-unlock-only")) { InstallPartyJoinUnlock(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-party-join-unlock")) { VerifyPartyJoinUnlockFile(args[0]); return; }
            if (args.Contains("--self-test-party-join-unlock")) { TestPartyJoinUnlock(); return; }
            if (args.Contains("--verify-websocket-lifecycle")) { VerifyWebSocketLifecycle(args[0]); return; }
            if (args.Contains("--raid-social-only")) { InstallRaidSocial(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-raid-social")) { VerifyRaidSocialFile(args[0]); return; }
            if (args.Contains("--verify-conquest-replay")) { VerifyConquestReplay(args[0]); return; }
            if (args.Contains("--awaken-delay-only")) { InstallAwakenDelay(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--completion-banners-only")) { InstallCompletionBanners(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--mission-thumbnails-only")) { InstallMissionThumbnails(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--accessory-list-only")) { InstallAccessoryShopDuplicates(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--shop-catalog-only")) { InstallShopCatalog(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--voice-language-only")) { InstallVoiceLanguage(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--battle-entry-only")) { InstallBattleEntry(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-battle-entry")) { TestBattleEntry(args[0]); return; }
            if (args.Contains("--story-auto-only")) { InstallStoryAuto(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-story-auto-cancel")) { TestStoryAutoCancellation(); return; }
            if (args.Contains("--verify-story-auto")) { VerifyStoryAutoFile(args[0]); return; }
            if (args.Contains("--remove-app-review-only")) { InstallAppReviewRemoval(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--background-chat-only")) { InstallBackgroundChat(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--combat-only")) { InstallCombat(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-combat")) { TestCombat(args[0]); return; }
            if (args.Contains("--self-test-dragon-unlock")) { TestDragonUnlock(args[0]); return; }
            if (args.Contains("--websocket-only")) { InstallWebSockets(args[0], args.Contains("--stage-only")); return; }
            if(args.Contains("--remove-payments")) {
                int output=Array.IndexOf(args,"--output");
                if(output<0 || output+1>=args.Length)throw new ArgumentException("--remove-payments requires --output <staging-dir>");
                StagePaymentRemoval(args[0],args[output+1]);return;
            }
            if(args.Contains("--remove-login-providers")) {
                int output=Array.IndexOf(args,"--output");
                if(output<0 || output+1>=args.Length)throw new ArgumentException("--remove-login-providers requires --output <staging-dir>");
                StageLoginProviderRemoval(args[0],args[output+1]);return;
            }
            if (args.Contains("--verify-no-telemetry"))
            {
                int overlay = Array.IndexOf(args, "--overlay");
                VerifyNoTelemetry(args[0], overlay >= 0 ? args[overlay + 1] : null);
                return;
            }
            if (args.Contains("--remove-telemetry"))
            {
                int output = Array.IndexOf(args, "--output");
                if (output < 0 || output + 1 >= args.Length) throw new ArgumentException("--remove-telemetry requires --output <staging-dir>");
                RemoveClientTelemetry(args[0], args.Contains("--stage-only"), args[output + 1]);
                return;
            }
            if (args.Contains("--accounts-only")) { InstallAccounts(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--craft-only")) { InstallCrafting(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--conquest-only")) { InstallConquest(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-mission-worker")) { VerifyMissionWorker(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-dispatch")) { TestDispatch(); return; }
            if (args.Contains("--verify-dispatch")) { VerifyDispatchFile(args[0]); return; }
            if (args.Contains("--self-test-honor-buff")) { TestHonorBuff(); return; }
            if (args.Contains("--honor-buff-only")) { InstallHonorBuff(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-honor-buff")) { VerifyHonorBuffFile(args[0]); return; }
            if (args.Contains("--dispatch-only")) { InstallDispatch(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--automation-only")) { InstallAutomation(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--self-test-survivors")) { TestCampaignSurvivors(); return; }
            if (args.Contains("--self-test-selectors")) { TestSelectorPools(); return; }
            if (args.Contains("--mail-updates-only")) { InstallMailUpdates(args[0], args.Contains("--stage-only")); return; }
            if (args.Contains("--verify-mail-updates")) { VerifyMailUpdates(args[0]); return; }
            if (args.Contains("--self-test-mail-updates")) { TestMailUpdates(); return; }
            // Determine client path: first CLI arg, or prompt, or default
            string clientRoot = args.Length > 0
                ? args[0].TrimEnd('\\', '/')
                : PromptForClientPath();

            string managedDir = Path.Combine(clientRoot, "King's Raid_Data", "Managed");
            string dllPath = Path.Combine(managedDir, "Assembly-CSharp.dll");
            string backupPath = dllPath + ".backup_before_patch";
            string unityCorePath = Path.Combine(managedDir, "UnityEngine.CoreModule.dll");
            string unityEnginePath = Path.Combine(managedDir, "UnityEngine.dll");
            string patchedPath = dllPath + ".patched";

            // Use the backup as source if it exists
            bool chatOnly = args.Contains("--chat-only");
            bool campaignOnly = args.Contains("--campaign-only");
            bool shopOnly = args.Contains("--shop-only");
            bool dealerPopupOnly = args.Contains("--dealer-popup-only");
            bool adventureUnlocksOnly = args.Contains("--adventure-unlocks-only");
            bool portalOnly = args.Contains("--portal-only");
            bool stageOnly = args.Contains("--stage-only");
            bool selectorsOnly = args.Contains("--selectors-only");
            bool originalTablesOnly = args.Contains("--original-tables-only");
            string sourcePath = chatOnly || campaignOnly || shopOnly || dealerPopupOnly || adventureUnlocksOnly || portalOnly || selectorsOnly || originalTablesOnly ? dllPath : File.Exists(backupPath) ? backupPath : dllPath;

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"DLL not found: {sourcePath}");
                return;
            }

            // Create backup if not exists
            if (!File.Exists(backupPath))
            {
                Console.WriteLine($"Creating backup: {backupPath}");
                File.Copy(dllPath, backupPath);
            }

            Console.WriteLine($"Loading assembly from: {sourcePath}");

            // Set up resolver to find Unity DLLs
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(dllPath)!);

            var readerParams = new ReaderParameters {
                ReadWrite = false, // Don't lock the file
                AssemblyResolver = resolver
            };

            using (var assembly = AssemblyDefinition.ReadAssembly(sourcePath, readerParams))
            {
                var module = assembly.MainModule;

                // Full patching can read an old recovery DLL; keep removed telemetry out of every output.
                RemoveTelemetryFromModule(module);
                PatchDamageReduction(module);
                PatchLegacyCombat(module);
                PatchLegacyHeroStats(module);
                PatchRuneCategories(module);
                PatchCachedRuneOptions(module);
                PatchDealerTicketPopup(module);
                PatchAppReviewPopup(module);
                PatchOrvelVisuals(module);
                PatchArenaRematchPrompt(module);
                PatchArenaResponsiveness(module);
                PatchHonor(module);
                PatchAdventureUnlocks(module);
                PatchTrialHeroReward(module);
                PatchVoiceLanguage(module);
                PatchShopFeaturedCatalog(module);
                PatchShopPurchaseLimits(module);
                PatchPetFeeding(module);
                PatchSelectorCountLimit(module);
                PatchCraftShortcut(module);
                VerifySoulWeaponCinematic(module);
                PatchAccessoryShopDuplicates(module);
                PatchMissionThumbnails(module);
                PatchBattleEntry(module);
                PatchAwakenDelay(module);
                PatchFriendRecommendations(module);
                PatchFriendPresence(module);
                PatchCompletionBanners(module);
                PatchRaidSocial(module);
                PatchPartyJoinUnlock(module);

                if (dealerPopupOnly || adventureUnlocksOnly)
                {
                    assembly.Write(patchedPath);
                }
                else if (originalTablesOnly)
                {
                    PatchPortal(module);
                    RestoreOriginalTableGetters(module);
                    PatchSelectorPools(module);
                    assembly.Write(patchedPath);
                }
                else if (selectorsOnly)
                {
                    PatchSelectorPools(module);
                    assembly.Write(patchedPath);
                }
                else if (portalOnly)
                {
                    PatchPortal(module);
                    assembly.Write(patchedPath);
                }
                else if (shopOnly)
                {
                    PatchShopShortcut(module);
                    assembly.Write(patchedPath);
                }
                else if (campaignOnly)
                {
                    PatchCampaignSurvivors(module);
                    PatchCinematicSkip(module);
                    assembly.Write(patchedPath);
                }
                else if (chatOnly)
                {
                    PatchChatSession(module);
                    PatchChatBackground(module);
                    assembly.Write(patchedPath);
                }
                else
                {
                    if (!PatchHeroInn(module, unityCorePath, unityEnginePath, resolver)) return;
                    PatchPaymentErrorPopup(module);
                    PatchHeroAndCostumeVisibility(module);
                    PatchSoulWeaponLimitBreak(module);
                    PatchChatSession(module);
                    PatchChatBackground(module);
                    PatchCampaignSurvivors(module);
                    PatchShopShortcut(module);
                    PatchPortal(module);
                    PatchSelectorPools(module);
                    RestoreOriginalTableGetters(module);
                    Console.WriteLine($"\nSaving modified assembly to: {patchedPath}");
                    assembly.Write(patchedPath);
                }
            }

            if (stageOnly)
            {
                InstallAccounts(clientRoot, true, patchedPath);
                File.Move(dllPath + ".accounts-staged", patchedPath, true);
                InstallWebSockets(clientRoot, true, patchedPath);
                File.Move(dllPath + ".websocket-staged", patchedPath, true);
                InstallStoryAuto(clientRoot, true, patchedPath);
                File.Move(dllPath + ".story-auto-staged", patchedPath, true);
                InstallImageRecovery(clientRoot, true, patchedPath);
                File.Move(dllPath + ".image-recovery-staged", patchedPath, true);
                InstallChatChannels(clientRoot, true, patchedPath);
                File.Move(dllPath + ".chat-channels-staged", patchedPath, true);
                InstallMissionClaimAll(clientRoot, true, patchedPath);
                File.Move(dllPath + ".mission-claim-all-staged", patchedPath, true);
                InstallSoulLiberationCost(clientRoot, true, patchedPath);
                File.Move(dllPath + ".soul-cost-staged", patchedPath, true);
                Console.WriteLine($"Staged patched assembly: {patchedPath}");
                return;
            }

            Console.WriteLine($"Copying to: {dllPath}");
            File.Copy(patchedPath, dllPath, true);
            InstallAccounts(clientRoot, false);
            InstallWebSockets(clientRoot, false);
            InstallStoryAuto(clientRoot, false);
            InstallImageRecovery(clientRoot, false);
            InstallChatChannels(clientRoot, false);
            InstallMissionClaimAll(clientRoot, false);
            InstallSoulLiberationCost(clientRoot, false);
            Console.WriteLine("Done! The DLL has been patched successfully.");
            if (chatOnly || campaignOnly || shopOnly || dealerPopupOnly || adventureUnlocksOnly || portalOnly || selectorsOnly) return;

            // Print summary
            Console.WriteLine("\n=== PATCH SUMMARY ===");
            Console.WriteLine("1. HeroInnView.InitUIBtnGroup: Changed None -> Friendly (always show recruiting buttons)");
            Console.WriteLine("2. HeroInnView.InitHeroPortraits: Added bounds check to prevent IndexOutOfRangeException");
            Console.WriteLine("3. HeroInnViewButtonGroup: Added HideUnsupportedButtons (fix ActionTypes, hide extra buttons)");
            Console.WriteLine("4. StateBase_InAppBilling: Skip payment error popup (Steam initialization)");
            Console.WriteLine("5. SoulWeaponLimitBreak: Disabled Limit Break stars 6-15 (TryGetMaxStar, GetAfterDataByCurrent, GetDataByResultStar patched)");
            Console.WriteLine("6. Hero/Costume visibility: Hero OpenType None->Opened only for index 1..102 and 111 (Valance), costume visibility and priced purchases enabled, native ownership checks preserved");
        }

        /// <summary>
        /// Prompts the user to enter the game client root directory if not provided via CLI.
        /// </summary>
        static string PromptForClientPath()
        {
            string defaultPath = @"D:\client";
            Console.Write($"Enter game client path [default: {defaultPath}]: ");
            string? input = Console.ReadLine()?.Trim().TrimEnd('\\', '/');
            if (string.IsNullOrEmpty(input))
            {
                input = defaultPath;
            }
            Console.WriteLine($"Using client path: {input}");
            return input;
        }
    }
}
