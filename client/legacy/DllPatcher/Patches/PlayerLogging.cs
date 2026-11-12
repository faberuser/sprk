namespace DllPatcher;

partial class Program
{
    // The complete baseline/isolated payload is checked by FindFrameTimingSettings.
    // usePlayerLog is at 410/402; enableFrameTimingStats is at 648/640.
    const int PlayerLogToFrameTiming = 238;
    const string PlayerLoggingIdentity = "sprk\nKing's Raid\n";

    static void VerifyPlayerLogging(string root, bool staged)
    {
        var path = Path.Combine(Path.GetFullPath(root), "King's Raid_Data", "globalgamemanagers")
            + (staged ? ".player-logging-staged" : "");
        var bytes = File.ReadAllBytes(path);
        var timing = FindFrameTimingSettings(bytes, out _);
        if (bytes[timing - PlayerLogToFrameTiming] != 1)
            throw new InvalidOperationException("Native Unity player logging is disabled.");
        var identity = Path.Combine(Path.GetFullPath(root), "King's Raid_Data", "app.info")
            + (staged ? ".player-logging-staged" : "");
        if (File.ReadAllText(identity).Replace("\r\n", "\n") != PlayerLoggingIdentity)
            throw new InvalidOperationException("Unity startup logging identity must be sprk/King's Raid.");
        Console.WriteLine("Verified native Unity player logging is enabled.");
    }

    static void InstallPlayerLogging(string root, bool stageOnly)
    {
        root = Path.GetFullPath(root);
        var path = Path.Combine(root, "King's Raid_Data", "globalgamemanagers");
        var bytes = File.ReadAllBytes(path);
        var timing = FindFrameTimingSettings(bytes, out _);
        var logging = timing - PlayerLogToFrameTiming;
        var staged = path + ".player-logging-staged";
        var identity = Path.Combine(root, "King's Raid_Data", "app.info");
        var oldIdentity = File.Exists(identity)
            ? File.ReadAllText(identity).Replace("\r\n", "\n").TrimEnd('\n') + "\n" : null;
        if (oldIdentity != null && oldIdentity != PlayerLoggingIdentity && oldIdentity != "masangsoft\nKing's Raid\n")
            throw new InvalidOperationException("Unsupported Unity startup identity.");
        bytes[logging] = 1;
        File.WriteAllBytes(staged, bytes);
        File.WriteAllText(identity + ".player-logging-staged", PlayerLoggingIdentity);
        VerifyPlayerLogging(root, true);
        if (stageOnly) return;
        foreach (var process in System.Diagnostics.Process.GetProcesses())
            using (process)
                if (UsesClientFiles(process, root))
                    throw new InvalidOperationException("Player logging staged. Close this game client before installation.");
        if (!File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes) || oldIdentity != PlayerLoggingIdentity)
        {
            var backup = Path.GetFullPath(Path.Combine("target/player-logging", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(backup);
            File.Copy(path, Path.Combine(backup, "globalgamemanagers"));
            if (File.Exists(identity)) File.Copy(identity, Path.Combine(backup, "app.info"));
            File.Copy(staged, path, true);
            File.Copy(identity + ".player-logging-staged", identity, true);
        }
        File.Delete(staged);
        File.Delete(identity + ".player-logging-staged");
        VerifyPlayerLogging(root, false);
        Console.WriteLine("Enabled Player.log for normal client launches.");
    }
}
