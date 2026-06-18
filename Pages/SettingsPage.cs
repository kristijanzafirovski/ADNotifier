using GimNotifier.Helpers;
using GimNotifier.Models;

namespace GimNotifier.Pages
{
    public class SettingsPage : Panel
    {
        private TextBox txtDomain = null!;
        private TextBox txtDomainDN = null!;
        private TextBox txtLogPath = null!;
        private TextBox txtTimeout = null!;
        private CheckBox chkSaveLogs = null!;
        private Label lblStatus = null!;

        public event Action? SettingsSaved;

        public SettingsPage()
        {
            Dock      = DockStyle.Fill;
            BackColor = Theme.ContentBg;
            Padding   = new Padding(20);
            BuildUI();
            LoadSettings();
        }

        private void BuildUI()
        {
            var title = Theme.MakeLabel("Settings", bold: true);
            title.Font     = Theme.FontTitle;
            title.Location = new Point(20, 20);
            Controls.Add(title);

            var sub = Theme.MakeLabel("Configure domain connection, logging, and send behaviour.");
            sub.ForeColor = Theme.TextSecondary;
            sub.Location  = new Point(20, 50);
            Controls.Add(sub);

            // ── Domain settings ───────────────────────────────────────────
            var domCard = MakeSection("Domain", 80, 180);

            AddRow(domCard, "Domain name:",     ref txtDomain,   "gim.local",          12, 50);
            AddRow(domCard, "Domain DN:",       ref txtDomainDN, "DC=gim,DC=local",    12, 100);

            // ── Logging settings ──────────────────────────────────────────
            var logCard = MakeSection("Logging", 280, 180);

            chkSaveLogs = new CheckBox
            {
                Text     = "Save activity log to disk",
                Font     = Theme.FontBase,
                Location = new Point(12, 50),
                AutoSize = true,
                Checked  = true
            };
            logCard.Controls.Add(chkSaveLogs);

            var logLabel = Theme.MakeLabel("Log folder:");
            logLabel.Location = new Point(12, 86);
            logCard.Controls.Add(logLabel);

            txtLogPath = Theme.MakeTextBox(logCard.Width - 160);
            txtLogPath.Location = new Point(100, 82);
            txtLogPath.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            logCard.Controls.Add(txtLogPath);

            var btnBrowse = Theme.MakeSecondaryButton("Browse...", 80, 28);
            btnBrowse.Location = new Point(logCard.Width - 96, 82);
            btnBrowse.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Click   += BtnBrowse_Click;
            logCard.Controls.Add(btnBrowse);

            var openLogLabel = Theme.MakeLabel("Open log folder:");
            openLogLabel.Location = new Point(12, 124);
            logCard.Controls.Add(openLogLabel);

            var btnOpenLog = Theme.MakeSecondaryButton("Open →", 80, 28);
            btnOpenLog.Location = new Point(120, 120);
            btnOpenLog.Click   += (s, e) =>
            {
                var path = txtLogPath.Text.Trim();
                if (Directory.Exists(path))
                    System.Diagnostics.Process.Start("explorer.exe", path);
                else
                    MessageBox.Show("Log folder does not exist yet.", "GIM Notifier",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            logCard.Controls.Add(btnOpenLog);

            // ── Send settings ─────────────────────────────────────────────
            var sendCard = MakeSection("Send Behaviour", 480, 130);

            var timeoutLabel = Theme.MakeLabel("msg.exe timeout (ms):");
            timeoutLabel.Location = new Point(12, 50);
            sendCard.Controls.Add(timeoutLabel);

            txtTimeout = Theme.MakeTextBox(80);
            txtTimeout.Location = new Point(180, 46);
            sendCard.Controls.Add(txtTimeout);

            var noteLabel = Theme.MakeLabel("Increase if computers are slow to respond on the network.");
            noteLabel.ForeColor = Theme.TextSecondary;
            noteLabel.Location  = new Point(272, 50);
            sendCard.Controls.Add(noteLabel);

            // ── Save button ───────────────────────────────────────────────
            var btnSave = Theme.MakePrimaryButton("Save Settings", 140, 36);
            btnSave.Location = new Point(20, 630);
            btnSave.Anchor   = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSave.Click   += BtnSave_Click;
            Controls.Add(btnSave);

            lblStatus = new Label
            {
                Text      = "",
                Font      = Theme.FontBase,
                ForeColor = Theme.AccentGreen,
                Location  = new Point(172, 638),
                AutoSize  = true,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left
            };
            Controls.Add(lblStatus);
        }

        private Panel MakeSection(string heading, int y, int h)
        {
            var card = new Panel
            {
                Location  = new Point(20, y),
                Size      = new Size(ClientSize.Width - 40, h),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Paint += PaintBorder;
            Controls.Add(card);

            var label = Theme.MakeLabel(heading, bold: true);
            label.Location = new Point(12, 12);
            card.Controls.Add(label);

            return card;
        }

        private static void AddRow(Panel parent, string label, ref TextBox field,
                                   string placeholder, int x, int y)
        {
            var lbl = Theme.MakeLabel(label);
            lbl.Location = new Point(x, y + 4);
            parent.Controls.Add(lbl);

            field = Theme.MakeTextBox(300);
            field.Location         = new Point(x + 140, y);
            field.PlaceholderText  = placeholder;
            parent.Controls.Add(field);
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description         = "Select log folder",
                UseDescriptionForTitle = true
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtLogPath.Text = dlg.SelectedPath;
        }

        private void LoadSettings()
        {
            var s = SettingsManager.Load();
            txtDomain.Text   = s.Domain;
            txtDomainDN.Text = s.DomainDN;
            txtLogPath.Text  = s.LogPath;
            txtTimeout.Text  = s.MsgTimeoutMs.ToString();
            chkSaveLogs.Checked = s.SaveLogs;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!int.TryParse(txtTimeout.Text, out int timeout) || timeout < 1000)
            {
                MessageBox.Show("Timeout must be a number ≥ 1000.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var s = new AppSettings
            {
                Domain       = txtDomain.Text.Trim(),
                DomainDN     = txtDomainDN.Text.Trim(),
                LogPath      = txtLogPath.Text.Trim(),
                MsgTimeoutMs = timeout,
                SaveLogs     = chkSaveLogs.Checked
            };

            SettingsManager.Save(s);

            lblStatus.Text = "✓ Settings saved";
            var t = new System.Windows.Forms.Timer { Interval = 2500 };
            t.Tick += (s, e) => { lblStatus.Text = ""; t.Stop(); };
            t.Start();

            SettingsSaved?.Invoke();
        }

        private static void PaintBorder(object? sender, PaintEventArgs e)
        {
            if (sender is Panel p)
            {
                using var pen = new Pen(Theme.Border, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            }
        }
    }
}
