using System.Diagnostics;
using System.Text.Json;
using SprkLauncher.Core;

namespace SprkLauncher;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        var root = AppContext.BaseDirectory;
        var configPath = Path.Combine(root, "sprk-launcher.json");
        var headless = args.Contains("--update-only", StringComparer.OrdinalIgnoreCase);
        try
        {
            if (!File.Exists(configPath))
                File.WriteAllText(configPath, JsonSerializer.Serialize(new LauncherConfig(), new JsonSerializerOptions { WriteIndented = true }));
            var config = LauncherConfig.Load(configPath);
            if (headless)
            {
                using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
                var engine = new UpdateEngine(root, config, http, () => EnsureGameClosed(config.GameExecutable));
                using var updateLock = engine.AcquireLock();
                engine.UpdateAsync().GetAwaiter().GetResult();
                return;
            }
            Application.Run(new LauncherForm(root, config));
        }
        catch (Exception error)
        {
            if (headless)
            {
                Environment.ExitCode = 1;
                try { File.WriteAllText(Path.Combine(root, "sprk-launcher-error.log"), error.ToString()); } catch { }
            }
            else MessageBox.Show(error.Message, "SPRK Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static void EnsureGameClosed(string executable)
    {
        foreach (var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executable)))
        {
            using (process)
            {
                if (process.HasExited) continue;
                // Conservatively block another instance even if its executable
                // location cannot be read (for example, an elevated process).
                throw new IOException("Close King's Raid before updating, then click Retry.");
            }
        }
    }
}

sealed class LauncherForm : Form
{
    readonly string root;
    readonly LauncherConfig config;
    readonly Label status = new() { AutoSize = false };
    readonly Label detail = new() { AutoSize = false };
    readonly Label version = new() { AutoSize = false };
    readonly ProgressBar progress = new();
    readonly Button action = new();
    readonly CancellationTokenSource cancellation = new();
    bool busy;
    bool closing;

    public LauncherForm(string root, LauncherConfig config)
    {
        this.root = root;
        this.config = config;
        Text = "SPRK Launcher";
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        ClientSize = new Size(600, 345);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(18, 22, 31);
        ForeColor = Color.FromArgb(234, 238, 245);
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        var title = new Label { Text = "SPRK", Location = new Point(30, 24), Size = new Size(350, 50), Font = new Font("Segoe UI", 28, FontStyle.Bold) };
        var subtitle = new Label { Text = "KING’S RAID", Location = new Point(33, 81), Size = new Size(350, 28), ForeColor = Color.FromArgb(156, 172, 195) };
        version.SetBounds(33, 120, 535, 25);
        version.ForeColor = Color.FromArgb(156, 172, 195);
        status.SetBounds(33, 156, 535, 44);
        status.Text = "Getting ready…";
        progress.SetBounds(33, 208, 534, 10);
        detail.SetBounds(33, 228, 360, 55);
        detail.ForeColor = Color.FromArgb(156, 172, 195);
        action.SetBounds(413, 268, 154, 43);
        action.Text = "Checking…";
        action.BackColor = Color.FromArgb(75, 109, 224);
        action.ForeColor = Color.White;
        action.FlatStyle = FlatStyle.Flat;
        action.FlatAppearance.BorderSize = 0;
        action.Enabled = false;
        action.Click += async (_, _) => await RunUpdate(launch: true);
        Controls.AddRange([title, subtitle, version, status, progress, detail, action]);
        Shown += async (_, _) => await RunUpdate();
        FormClosing += (_, e) =>
        {
            if (!busy) return;
            e.Cancel = true;
            closing = true;
            cancellation.Cancel();
            status.Text = "Finishing safely…";
        };
    }

    async Task RunUpdate(bool launch = false)
    {
        if (busy) return;
        busy = true;
        action.Enabled = false;
        action.Text = "Updating…";
        detail.Text = "";
        try
        {
            using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromMinutes(30) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("SprkLauncher/1.0");
            var reporter = new Progress<UpdateProgress>(value =>
            {
                if (closing) return;
                status.Text = value.Message;
                progress.Style = value.Percent < 0 ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                if (value.Percent >= 0) progress.Value = Math.Clamp(value.Percent, 0, 100);
                if (value.Total > 0) detail.Text = $"{value.Downloaded / 1048576.0:0.0} / {value.Total / 1048576.0:0.0} MB";
            });
            var engine = new UpdateEngine(root, config, http, () => Program.EnsureGameClosed(config.GameExecutable), reporter);
            version.Text = $"Installed: {engine.InstalledVersion}";
            using var updateLock = engine.AcquireLock();
            var result = await Task.Run(() => engine.UpdateAsync(cancellation.Token));
            version.Text = $"Installed: {result.Version}";
            status.Text = "Your client is up to date.";
            detail.Text = result.DownloadedFiles == 0 ? "No downloads needed." : $"Updated {result.DownloadedFiles} files.";
            action.Text = "Play";
            // The installation lock stays held until the game has started.
            if ((launch || config.AutoLaunch) && !closing) LaunchGame();
        }
        catch (OperationCanceledException) when (closing) { }
        catch (Exception error)
        {
            status.Text = "Update could not finish.";
            detail.Text = error.Message;
            progress.Style = ProgressBarStyle.Continuous;
            progress.Value = 0;
            action.Text = "Retry";
            var logs = Path.Combine(root, ".sprk-launcher");
            try { Directory.CreateDirectory(logs); File.AppendAllText(Path.Combine(logs, "launcher.log"), $"{DateTimeOffset.UtcNow:O} {error}\n"); } catch { }
        }
        finally
        {
            busy = false;
            action.Enabled = true;
            if (closing) Close();
        }
    }

    void LaunchGame()
    {
        try
        {
            Program.EnsureGameClosed(config.GameExecutable);
            var game = Path.GetFullPath(Path.Combine(root, config.GameExecutable));
            if (!File.Exists(game)) throw new FileNotFoundException("King's Raid.exe is missing. Put the launcher beside the full game client.");
            Process.Start(new ProcessStartInfo(game) { WorkingDirectory = root, UseShellExecute = true });
            // Allow the form to close after RunUpdate releases its handles.
            closing = true;
            if (!busy) Close();
        }
        catch (Exception error) { status.Text = "Unable to start the game."; detail.Text = error.Message; }
    }
}
