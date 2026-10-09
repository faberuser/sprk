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
            if (args.Contains("--dispatch-only")) { InstallDispatch(args[0],args.Contains("--stage-only")); return; }
            if (args.Contains("--automation-only")) { InstallAutomation(args[0]); return; }
            if (args.Contains("--self-test-survivors")) { TestCampaignSurvivors(); return; }
            if (args.Contains("--self-test-selectors")) { TestSelectorPools(); return; }
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
                PatchDealerTicketPopup(module);
                PatchAdventureUnlocks(module);
                PatchTrialHeroReward(module);

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
                Console.WriteLine($"Staged patched assembly: {patchedPath}");
                return;
            }

            Console.WriteLine($"Copying to: {dllPath}");
            File.Copy(patchedPath, dllPath, true);
            InstallAccounts(clientRoot, false);
            InstallWebSockets(clientRoot, false);
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
