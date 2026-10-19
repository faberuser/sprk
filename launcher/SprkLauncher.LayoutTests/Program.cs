using SprkLauncher.Core;

namespace SprkLauncher;

static class LayoutTests
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        var config = new LauncherConfig();
        config.EnsureProfiles();
        foreach (var scale in new[] { 1f, 1.25f, 1.5f, 2f }) {
            using var launcher = new LauncherForm(Path.GetTempPath(), config);
            Prepare(launcher, new Size(600, 365), scale);
            VerifyLauncher(launcher);
            if (args.Length == 1 && scale == 2f) {
                Directory.CreateDirectory(args[0]);
                using var screenshot = new Bitmap(launcher.Width, launcher.Height);
                launcher.DrawToBitmap(screenshot, new Rectangle(Point.Empty, screenshot.Size));
                screenshot.Save(Path.Combine(args[0], "launcher-200-percent.png"));
            }
            using var manager = new ServerManagerForm(config);
            Prepare(manager, new Size(480, 330), scale);
            VerifyControls(manager);
            using var editor = new ServerEditorForm(config, null);
            Prepare(editor, new Size(650, 440), scale);
            VerifyControls(editor);
            Console.WriteLine($"PASS launcher and server dialogs at {scale:P0} layout scale (host DPI {launcher.DeviceDpi}).");
        }
    }

    static void Prepare(Form form, Size baseline, float scale)
    {
        // First verify the constructor on the host's real DPI. Missing design
        // dimensions previously left 96-DPI bounds underneath scaled fonts.
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(-32000, -32000);
        form.ShowInTaskbar = false;
        form.Opacity = 0;
        form.Show();
        form.PerformAutoScale();
        form.PerformLayout();
        var dpiFactor = form.DeviceDpi / 96f;
        Require(Math.Abs(form.ClientSize.Width - baseline.Width * dpiFactor) <= 2 &&
            Math.Abs(form.ClientSize.Height - baseline.Height * dpiFactor) <= 2,
            $"{form.Text}: window did not scale with host DPI: {form.ClientSize}, DPI={form.DeviceDpi}");
        // Exercise additional layout sizes without changing Windows settings.
        // Scale fonts as well as bounds to retain the same clipping pressure.
        var factor = scale / dpiFactor;
        var fonts = Descendants(form).Prepend(form).Select(c => (Control: c, Font: c.Font)).ToArray();
        form.SuspendLayout();
        form.Scale(new SizeF(factor, factor));
        foreach (var (control, font) in fonts)
            control.Font = new Font(font.FontFamily, font.Size * factor, font.Style);
        form.ResumeLayout(true);
        form.PerformLayout();
    }

    static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls) {
            yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }

    static void VerifyLauncher(Form form)
    {
        VerifyControls(form);
        var controls = form.Controls.Cast<Control>().ToArray();
        for (var i = 0; i < controls.Length; i++)
            for (var j = i + 1; j < controls.Length; j++)
                Require(!controls[i].Bounds.IntersectsWith(controls[j].Bounds),
                    $"{form.Text}: {controls[i].Text} overlaps {controls[j].Text}");
    }

    static void VerifyControls(Form form)
    {
        foreach (var control in Descendants(form)) {
            Require(control.Width > 0 && control.Height > 0, $"{form.Text}: empty control {control.Text}");
            Require(control.Left >= 0 && control.Top >= 0 &&
                control.Right <= control.Parent!.ClientSize.Width + 1 &&
                control.Bottom <= control.Parent.ClientSize.Height + 1,
                $"{form.Text}: control outside parent: {control.Text}, {control.Bounds}, parent={control.Parent!.ClientSize}");
            if (control is Label or Button or CheckBox && control.Text.Length > 0) {
                var text = TextRenderer.MeasureText(control.Text, control.Font, Size.Empty, TextFormatFlags.SingleLine);
                Require(control.Height >= text.Height,
                    $"{form.Text}: text clipped vertically: {control.Text}, height={control.Height}, text={text.Height}");
            }
        }
    }

    static void Require(bool value, string message)
    {
        if (!value) throw new Exception(message);
    }
}
