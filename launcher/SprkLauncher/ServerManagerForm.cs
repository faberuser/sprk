using System.Text.Json;
using SprkLauncher.Core;

namespace SprkLauncher;

sealed class ServerManagerForm : Form
{
    public LauncherConfig Configuration { get; }
    readonly ListBox servers = new() { Dock = DockStyle.Fill };
    readonly Button edit = new() { Text = "Edit", AutoSize = true };
    readonly Button remove = new() { Text = "Remove", AutoSize = true };

    public ServerManagerForm(LauncherConfig config)
    {
        Configuration = JsonSerializer.Deserialize<LauncherConfig>(JsonSerializer.Serialize(config))!;
        Text = "Manage Servers";
        ClientSize = new Size(480, 330);
        Font = new Font("Segoe UI", 10);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), RowCount = 3, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(servers, 0, 0);
        var commands = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 8) };
        var add = new Button { Text = "Add", AutoSize = true };
        commands.Controls.AddRange([add, edit, remove]);
        layout.Controls.Add(commands, 0, 1);
        var footer = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var done = new Button { Text = "Save", AutoSize = true, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        footer.Controls.AddRange([done, cancel]);
        layout.Controls.Add(footer, 0, 2);
        Controls.Add(layout);
        AcceptButton = done;
        CancelButton = cancel;
        add.Click += (_, _) => EditServer(null);
        edit.Click += (_, _) => EditServer(servers.SelectedItem as ServerProfile);
        servers.DoubleClick += (_, _) => { if (servers.SelectedItem is ServerProfile profile) EditServer(profile); };
        remove.Click += (_, _) => {
            if (servers.SelectedItem is not ServerProfile profile || profile.Id == "local") return;
            if (MessageBox.Show(this, $"Remove {profile.Name} from this launcher?", Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            Configuration.RemoveProfile(profile.Id);
            RefreshServers(Configuration.SelectedProfile);
        };
        servers.SelectedIndexChanged += (_, _) => {
            if (servers.SelectedItem is ServerProfile selected) Configuration.SelectedProfile = selected.Id;
            edit.Enabled = servers.SelectedItem is ServerProfile;
            remove.Enabled = servers.SelectedItem is ServerProfile profile && profile.Id != "local";
        };
        RefreshServers(config.SelectedProfile);
    }

    void EditServer(ServerProfile? profile)
    {
        using var dialog = new ServerEditorForm(Configuration, profile);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        RefreshServers(dialog.SavedProfile!.Id);
    }

    void RefreshServers(string selected)
    {
        servers.Items.Clear();
        servers.Items.AddRange(Configuration.Profiles.Cast<object>().ToArray());
        servers.SelectedItem = Configuration.Profiles.FirstOrDefault(p => p.Id == selected) ?? Configuration.CurrentProfile;
    }
}

sealed class ServerEditorForm : Form
{
    public ServerProfile? SavedProfile { get; private set; }
    readonly TextBox name = new() { Dock = DockStyle.Fill };
    readonly TextBox host = new() { Dock = DockStyle.Fill, PlaceholderText = "https://your-server.example/host.json" };
    readonly TextBox manifest = new() { Dock = DockStyle.Fill, PlaceholderText = "https://your-updates.example/updates/stable/manifest.json" };
    readonly TextBox publicKey = new() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, WordWrap = false };
    readonly CheckBox inheritKey = new() { Text = "Use the launcher's default public key", AutoSize = true };

    public ServerEditorForm(LauncherConfig config, ServerProfile? profile)
    {
        Text = profile == null ? "Add Server" : "Edit Server";
        ClientSize = new Size(650, 440);
        Font = new Font("Segoe UI", 10);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        name.Text = profile?.Name ?? "";
        host.Text = profile?.HostUrl ?? "";
        manifest.Text = profile?.ManifestUrl ?? "";
        publicKey.Text = profile?.ManifestPublicKeyPem ?? config.ManifestPublicKeyPem;
        inheritKey.Checked = profile?.ManifestPublicKeyPem == null;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 10 };
        for (var row = 0; row < 10; row++) layout.RowStyles.Add(new RowStyle(row == 8 ? SizeType.Percent : SizeType.AutoSize, row == 8 ? 100 : 0));
        layout.Controls.Add(new Label { Text = "Server name", AutoSize = true }, 0, 0);
        layout.Controls.Add(name, 0, 1);
        layout.Controls.Add(new Label { Text = "Game URL", AutoSize = true }, 0, 2);
        layout.Controls.Add(host, 0, 3);
        layout.Controls.Add(new Label { Text = "Update manifest URL", AutoSize = true }, 0, 4);
        layout.Controls.Add(manifest, 0, 5);
        layout.Controls.Add(new Label { Text = "Update public key (PEM)", AutoSize = true }, 0, 6);
        var keyOptions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        var import = new Button { Text = "Load public key…", AutoSize = true };
        keyOptions.Controls.AddRange([inheritKey, import]);
        layout.Controls.Add(keyOptions, 0, 7);
        layout.Controls.Add(publicKey, 0, 8);
        var footer = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
        var save = new Button { Text = "Save", AutoSize = true };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        footer.Controls.AddRange([save, cancel]);
        layout.Controls.Add(footer, 0, 9);
        Controls.Add(layout);
        AcceptButton = save;
        CancelButton = cancel;
        publicKey.Enabled = !inheritKey.Checked;
        inheritKey.CheckedChanged += (_, _) => { publicKey.Enabled = !inheritKey.Checked; };
        import.Click += (_, _) => {
            using var file = new OpenFileDialog { Title = "Choose the update public key", Filter = "PEM public key (*.pem)|*.pem|All files (*.*)|*.*" };
            if (file.ShowDialog(this) != DialogResult.OK) return;
            try { publicKey.Text = File.ReadAllText(file.FileName); inheritKey.Checked = false; }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { ShowError(error); }
        };
        save.Click += (_, _) => {
            try
            {
                SavedProfile = config.StoreProfile(new() {
                    Id = profile?.Id ?? "", Name = name.Text, HostUrl = host.Text, ManifestUrl = manifest.Text,
                    ManifestPublicKeyPem = inheritKey.Checked ? null : publicKey.Text
                });
                DialogResult = DialogResult.OK;
            }
            catch (Exception error) when (error is InvalidDataException or IOException or ArgumentException) { ShowError(error); }
        };
    }

    void ShowError(Exception error) => MessageBox.Show(this, error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
}
