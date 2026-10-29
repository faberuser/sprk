namespace DllPatcher;

partial class Program
{
    static void RestoreLegendaryRuneTickets(string clientRoot, bool verify)
    {
        if (!verify && System.Diagnostics.Process.GetProcesses().Any(p => p.ProcessName == "King's Raid" || p.ProcessName.StartsWith("SprkTest-")))
            throw new InvalidOperationException("Close game clients before installing rune ticket tables.");
        var directory = Path.GetFullPath(Path.Combine("target", "legendary-rune-tickets"));
        Directory.CreateDirectory(directory);
        var script = Path.Combine(directory, "restore_legendary_rune_tickets.py");
        using (var resource = typeof(Program).Assembly.GetManifestResourceStream("DllPatcher.restore_legendary_rune_tickets.py")!)
        using (var reader = new StreamReader(resource)) File.WriteAllText(script, reader.ReadToEnd());
        var start = new System.Diagnostics.ProcessStartInfo(Environment.GetEnvironmentVariable("SPRK_PYTHON") ?? (OperatingSystem.IsWindows() ? "python" : "python3"))
            { UseShellExecute = false, CreateNoWindow = true };
        start.ArgumentList.Add(script);
        start.ArgumentList.Add(Path.GetFullPath(clientRoot));
        if (verify) start.ArgumentList.Add("--verify");
        using var process = System.Diagnostics.Process.Start(start)!;
        process.WaitForExit();
        if (process.ExitCode != 0) throw new InvalidOperationException("Legendary rune ticket table restoration/verification failed.");
    }
}
