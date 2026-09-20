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
            if (args.Contains("--self-test-survivors")) { TestCampaignSurvivors(); return; }
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
            bool stageOnly = args.Contains("--stage-only");
            string sourcePath = chatOnly || campaignOnly || shopOnly ? dllPath : File.Exists(backupPath) ? backupPath : dllPath;

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

                if (shopOnly)
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
                    Console.WriteLine($"\nSaving modified assembly to: {patchedPath}");
                    assembly.Write(patchedPath);
                }
            }

            if (stageOnly)
            {
                Console.WriteLine($"Staged patched assembly: {patchedPath}");
                return;
            }

            Console.WriteLine($"Copying to: {dllPath}");
            File.Copy(patchedPath, dllPath, true);
            Console.WriteLine("Done! The DLL has been patched successfully.");
            if (chatOnly || campaignOnly || shopOnly) return;

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
