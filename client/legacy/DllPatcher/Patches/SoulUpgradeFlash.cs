using System.Diagnostics;

namespace DllPatcher;

partial class Program
{
    static void InstallSoulUpgradeFlash(string root, bool stageOnly, bool verifyOnly)
    {
        root = Path.GetFullPath(root);
        var asset = Path.Combine(root, "King's Raid_Data/Documents/Patch/StandaloneWindows/Assetbundle/Effect_UI_Ascension_AdvanceEffect.unity3d");
        var work = Path.GetFullPath(Path.Combine("target/soul-upgrade-flash", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(work);
        var script = Path.Combine(work, "SoftenSoulUpgradeFlash.py");
        using (var input = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.SoftenSoulUpgradeFlash.py")!)
        using (var output = File.Create(script)) input.CopyTo(output);
        var staged = asset + ".soul-upgrade-flash-staged";
        void Run(string input, string? output)
        {
            var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("SPRK_PYTHON") ?? (OperatingSystem.IsWindows() ? "python" : "python3")) {
                UseShellExecute = false, CreateNoWindow = true
            };
            start.ArgumentList.Add(script); start.ArgumentList.Add(input);
            if (output != null) { start.ArgumentList.Add("--output"); start.ArgumentList.Add(output); }
            using var process = Process.Start(start)!; process.WaitForExit();
            if (process.ExitCode != 0) throw new InvalidOperationException("Soul Weapon advancement flash verification failed.");
        }
        if (verifyOnly) { Run(stageOnly ? staged : asset, null); return; }
        Run(asset, staged); Run(staged, null);
        if (stageOnly) return;
        if (System.Diagnostics.Process.GetProcesses().Any(p => UsesClientFiles(p, root)))
            throw new InvalidOperationException("Flash patch staged. Close game clients before installation.");
        var backup = Path.Combine(work, "Effect_UI_Ascension_AdvanceEffect.unity3d.before");
        File.Copy(asset, backup); File.Move(staged, asset, true); Run(asset, null);
        Console.WriteLine("Installed 0.35-second, unscaled Soul Weapon advancement flash; native cinematic and gameplay timing preserved.");
    }
}
