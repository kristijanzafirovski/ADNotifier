using System.DirectoryServices;
using GimNotifier.Helpers;
using GimNotifier.Models;

namespace GimNotifier.Pages
{
    public class SendPage : Panel
    {
        // ── State ──────────────────────────────────────────────────────────
        private readonly List<ComputerEntry> _computers = new();
        private List<MessageTemplate> _templates = new();

        // ── Left: OU Tree ──────────────────────────────────────────────────
        private TreeView treeOU = null!;

        // ── Right: Computer list ───────────────────────────────────────────
        private CheckedListBox clbComputers = null!;
        private TextBox txtSearch = null!;
        private Label lblComputerCount = null!;

        // ── Bottom: Message area ───────────────────────────────────────────
        private TextBox txtMessage = null!;
        private Label lblCharCount = null!;
        private ComboBox cmbTemplate = null!;
        private Button btnSend = null!;
        private RichTextBox rtbLog = null!;

        public SendPage()
        {
            Dock      = DockStyle.Fill;
            BackColor = Theme.ContentBg;
            Padding   = new Padding(20);
            BuildUI();
            LoadTemplates();
            LoadOUTree();
        }

        // ── UI Construction ────────────────────────────────────────────────
        private void BuildUI()
        {
            // Page title
            var title = Theme.MakeLabel("Прати порака", bold: true);
            title.Font     = Theme.FontTitle;
            title.Location = new Point(20, 20);
            Controls.Add(title);

            // ── Top split: tree | computers ──────────────────────────────
            var topPanel = new Panel
            {
                Location  = new Point(20, 60),
                Size      = new Size(ClientSize.Width - 40, 320),
                BackColor = Theme.ContentBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(topPanel);

            // OU Tree card
            var treeCard = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(280, 320),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };
            treeCard.Paint += PaintBorder;
            topPanel.Controls.Add(treeCard);

            var ouLabel = Theme.MakeLabel("Organisational Units", bold: true);
            ouLabel.Location = new Point(12, 12);
            treeCard.Controls.Add(ouLabel);

            treeOU = new TreeView
            {
                Location     = new Point(8, 36),
                Size         = new Size(264, 272),
                Font         = Theme.FontBase,
                BackColor    = Theme.CardBg,
                BorderStyle  = BorderStyle.None,
                CheckBoxes   = true,
                ShowLines    = true,
                ShowPlusMinus= true,
                Anchor       = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
            };
            treeOU.BeforeExpand  += TreeOU_BeforeExpand;
            treeOU.AfterCheck    += TreeOU_AfterCheck;
            treeCard.Controls.Add(treeOU);

            // Computer list card
            var compCard = new Panel
            {
                Location  = new Point(292, 0),
                Size      = new Size(topPanel.Width - 292, 320),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            compCard.Paint += PaintBorder;
            topPanel.Controls.Add(compCard);

            var compLabel = Theme.MakeLabel("Компјутери", bold: true);
            compLabel.Location = new Point(12, 12);
            compCard.Controls.Add(compLabel);

            // Search box
            txtSearch = Theme.MakeTextBox(180);
            txtSearch.Location    = new Point(12, 38);
            txtSearch.PlaceholderText = "Филтрирај...";
            txtSearch.TextChanged += TxtSearch_TextChanged;
            compCard.Controls.Add(txtSearch);

            lblComputerCount = Theme.MakeLabel("0 компјутери");
            lblComputerCount.ForeColor = Theme.TextSecondary;
            lblComputerCount.Location  = new Point(200, 42);
            compCard.Controls.Add(lblComputerCount);

            // Select All / None
            var btnAll = Theme.MakeSecondaryButton("Сите", 50, 26);
            btnAll.Location = new Point(compCard.Width - 110, 38);
            btnAll.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnAll.Click   += (s, e) => SetAllChecked(true);
            compCard.Controls.Add(btnAll);

            var btnNone = Theme.MakeSecondaryButton("Ниту еден", 50, 26);
            btnNone.Location = new Point(compCard.Width - 56, 38);
            btnNone.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnNone.Click   += (s, e) => SetAllChecked(false);
            compCard.Controls.Add(btnNone);

            clbComputers = new CheckedListBox
            {
                Location      = new Point(8, 72),
                Size          = new Size(compCard.Width - 16, 236),
                Font          = Theme.FontBase,
                BackColor     = Theme.CardBg,
                BorderStyle   = BorderStyle.None,
                CheckOnClick  = true,
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            compCard.Controls.Add(clbComputers);

            // ── Bottom: message + log ─────────────────────────────────────
            var bottomPanel = new Panel
            {
                Location  = new Point(20, 392),
                Size      = new Size(ClientSize.Width - 40, 280),
                BackColor = Theme.ContentBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            Controls.Add(bottomPanel);

            // Message card
            var msgCard = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(bottomPanel.Width - 320, 280),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            msgCard.Paint += PaintBorder;
            bottomPanel.Controls.Add(msgCard);

            var msgLabel = Theme.MakeLabel("Порака", bold: true);
            msgLabel.Location = new Point(12, 12);
            msgCard.Controls.Add(msgLabel);

            // Template picker
            var tmplLabel = Theme.MakeLabel("Шаблон:");
            tmplLabel.Location = new Point(12, 42);
            msgCard.Controls.Add(tmplLabel);

            cmbTemplate = new ComboBox
            {
                Location     = new Point(80, 39),
                Width        = 200,
                Font         = Theme.FontBase,
                DropDownStyle= ComboBoxStyle.DropDownList
            };
            cmbTemplate.SelectedIndexChanged += CmbTemplate_SelectedIndexChanged;
            msgCard.Controls.Add(cmbTemplate);

            txtMessage = new TextBox
            {
                Location    = new Point(12, 70),
                Size        = new Size(msgCard.Width - 24, 140),
                Multiline   = true,
                Font        = Theme.FontBase,
                BackColor   = Theme.CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars  = ScrollBars.Vertical,
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            txtMessage.TextChanged += TxtMessage_TextChanged;
            msgCard.Controls.Add(txtMessage);

            lblCharCount = new Label
            {
                Text      = "0 / 255",
                Font      = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                Location  = new Point(12, 218),
                AutoSize  = true,
                Anchor    = AnchorStyles.Bottom | AnchorStyles.Left
            };
            msgCard.Controls.Add(lblCharCount);

            btnSend = Theme.MakePrimaryButton("Прати →", 120, 36);
            btnSend.Location = new Point(msgCard.Width - 136, 230);
            btnSend.Anchor   = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSend.Click   += BtnSend_Click;
            msgCard.Controls.Add(btnSend);

            // Log card
            var logCard = new Panel
            {
                Location  = new Point(msgCard.Width + 12, 0),
                Size      = new Size(300, 280),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };
            logCard.Paint += PaintBorder;
            bottomPanel.Controls.Add(logCard);

            var logLabel = Theme.MakeLabel("Log", bold: true);
            logLabel.Location = new Point(12, 12);
            logCard.Controls.Add(logLabel);

            rtbLog = new RichTextBox
            {
                Location   = new Point(8, 36),
                Size       = new Size(284, 232),
                Font       = new Font("Consolas", 8.5f),
                BackColor  = Theme.CardBg,
                BorderStyle= BorderStyle.None,
                ReadOnly   = true,
                Anchor     = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            logCard.Controls.Add(rtbLog);
        }

        // ── OU Tree ────────────────────────────────────────────────────────
        private void LoadOUTree()
        {
            treeOU.Nodes.Clear();
            var settings = SettingsManager.Load();

            var root = new TreeNode(settings.Domain)
            {
                Tag = settings.DomainDN
            };
            treeOU.Nodes.Add(root);
            AdHelper.LoadChildOUs(root, settings.DomainDN);
            root.Expand();
        }

        private void TreeOU_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            var node = e.Node;
            if (node == null) return;

            if (node.Nodes.Count == 1 && node.Nodes[0].Tag?.ToString() == "__dummy__")
            {
                node.Nodes.Clear();
                AdHelper.LoadChildOUs(node, node.Tag!.ToString()!);
            }
        }

        private void TreeOU_AfterCheck(object? sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Unknown) return;
            var node = e.Node;
            if (node == null) return;

            string dn     = node.Tag?.ToString() ?? "";
            string ouName = node.Text;

            if (node.Checked)
            {
                // Load computers from this OU if not already loaded
                bool alreadyLoaded = _computers.Any(c => c.OUDN == dn);
                if (!alreadyLoaded)
                {
                    var pcs = AdHelper.GetComputers(dn, ouName);
                    _computers.AddRange(pcs);
                }
            }
            else
            {
                // Remove computers from this OU
                _computers.RemoveAll(c => c.OUDN == dn);
            }

            RefreshComputerList();
        }

        // ── Computer list ──────────────────────────────────────────────────
        private void RefreshComputerList(string filter = "")
        {
            clbComputers.Items.Clear();

            var grouped = _computers
                .GroupBy(c => c.OUName)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                // Group header (disabled item styled differently)
                string header = $"── {group.Key} ──";
                clbComputers.Items.Add(header, false);

                foreach (var pc in group.OrderBy(p => p.Name))
                {
                    if (!string.IsNullOrEmpty(filter) &&
                        !pc.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    clbComputers.Items.Add(pc, pc.Checked);
                }
            }

            int total    = _computers.Count;
            int checked_ = _computers.Count(c => c.Checked);
            lblComputerCount.Text = $"{checked_} / {total} избрани";
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            RefreshComputerList(txtSearch.Text.Trim());
        }

        private void SetAllChecked(bool value)
        {
            for (int i = 0; i < clbComputers.Items.Count; i++)
            {
                if (clbComputers.Items[i] is ComputerEntry pc)
                {
                    pc.Checked = value;
                    clbComputers.SetItemChecked(i, value);
                }
            }
            UpdateComputerCount();
        }

        private void UpdateComputerCount()
        {
            int total    = _computers.Count;
            int checked_ = _computers.Count(c => c.Checked);
            lblComputerCount.Text = $"{checked_} / {total} избрани";
        }

        // ── Templates ──────────────────────────────────────────────────────
        public void LoadTemplates()
        {
            _templates = TemplateManager.Load();
            cmbTemplate.Items.Clear();
            cmbTemplate.Items.Add("— избери шаблон —");
            foreach (var t in _templates)
                cmbTemplate.Items.Add(t);
            cmbTemplate.DisplayMember = "Name";
            cmbTemplate.SelectedIndex = 0;
        }

        private void CmbTemplate_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbTemplate.SelectedItem is MessageTemplate t)
                txtMessage.Text = t.Body;
        }

        // ── Char counter ───────────────────────────────────────────────────
        private void TxtMessage_TextChanged(object? sender, EventArgs e)
        {
            int len = txtMessage.Text.Length;
            lblCharCount.Text      = $"{len} / 255";
            lblCharCount.ForeColor = len > 255 ? Theme.AccentRed
                                   : len > 220 ? Color.DarkOrange
                                   : Theme.TextSecondary;
        }

        // ── Send ───────────────────────────────────────────────────────────
        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                MessageBox.Show("Внесете порака.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtMessage.Text.Length > 255)
            {
                MessageBox.Show("Пораката е над 255 карактери.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var targets = new List<string>();
            foreach (var item in clbComputers.CheckedItems)
            {
                if (item is ComputerEntry pc) targets.Add(pc.Name);
            }

            if (targets.Count == 0)
            {
                MessageBox.Show("Не се избрани компјутери.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSend.Enabled = false;
            btnSend.Text    = "Се испраќа...";

            string msg   = txtMessage.Text;
            int ok = 0, fail = 0;

            await Task.Run(() =>
            {
                foreach (var pc in targets)
                {
                    var (success, err) = MsgSender.Send(pc, msg);
                    string line = success ? $"✓ {pc}" : $"✗ {pc} — {err}";
                    MsgSender.AppendLog(line);

                    Invoke(() =>
                    {
                        AppendLog(line, success ? Theme.AccentGreen : Theme.AccentRed);
                    });

                    if (success) ok++; else fail++;
                }
            });

            string summary = $"Завршено — {ok} испратени, {fail} неуспешни.";
            AppendLog(summary, Theme.TextPrimary);
            MsgSender.AppendLog(summary);

            btnSend.Enabled = true;
            btnSend.Text    = "Прати →";
        }

        // ── Log helper ─────────────────────────────────────────────────────
        private void AppendLog(string text, Color color)
        {
            rtbLog.SelectionStart  = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor  = Theme.TextSecondary;
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ");
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(text + Environment.NewLine);
            rtbLog.SelectionColor = rtbLog.ForeColor;
            rtbLog.ScrollToCaret();
        }

        private static void PaintBorder(object? sender, PaintEventArgs e)
        {
            if (sender is Panel p)
            {
                using var pen = new Pen(Theme.Border, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            }
        }

        // ── Public: reload tree after settings change ──────────────────────
        public void Reload()
        {
            _computers.Clear();
            clbComputers.Items.Clear();
            LoadOUTree();
            LoadTemplates();
        }
    }
}
