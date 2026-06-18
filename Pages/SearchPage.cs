using GimNotifier.Helpers;
using GimNotifier.Models;

namespace GimNotifier.Pages
{
    public class SearchPage : Panel
    {
        private TextBox txtQuery = null!;
        private ListView lvResults = null!;
        private TextBox txtMessage = null!;
        private Label lblCharCount = null!;
        private Button btnSearch = null!;
        private Button btnSend = null!;
        private RichTextBox rtbLog = null!;

        public SearchPage()
        {
            Dock      = DockStyle.Fill;
            BackColor = Theme.ContentBg;
            Padding   = new Padding(20);
            BuildUI();
        }

        private void BuildUI()
        {
            var title = Theme.MakeLabel("Search Computers", bold: true);
            title.Font     = Theme.FontTitle;
            title.Location = new Point(20, 20);
            Controls.Add(title);

            var sub = Theme.MakeLabel("Search for computers across the entire domain by name.");
            sub.ForeColor = Theme.TextSecondary;
            sub.Location  = new Point(20, 50);
            Controls.Add(sub);

            // ── Search bar ────────────────────────────────────────────────
            var searchCard = new Panel
            {
                Location  = new Point(20, 80),
                Size      = new Size(ClientSize.Width - 40, 60),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            searchCard.Paint += PaintBorder;
            Controls.Add(searchCard);

            txtQuery = Theme.MakeTextBox(400, 30);
            txtQuery.Location         = new Point(12, 14);
            txtQuery.PlaceholderText  = "Computer name (wildcards supported, e.g. PC-5*)";
            txtQuery.KeyDown         += (s, e) => { if (e.KeyCode == Keys.Enter) DoSearch(); };
            searchCard.Controls.Add(txtQuery);

            btnSearch = Theme.MakePrimaryButton("Search", 90, 30);
            btnSearch.Location = new Point(420, 14);
            btnSearch.Click   += (s, e) => DoSearch();
            searchCard.Controls.Add(btnSearch);

            // ── Results ───────────────────────────────────────────────────
            var resultsCard = new Panel
            {
                Location  = new Point(20, 152),
                Size      = new Size(ClientSize.Width - 40, 260),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            resultsCard.Paint += PaintBorder;
            Controls.Add(resultsCard);

            var resLabel = Theme.MakeLabel("Results", bold: true);
            resLabel.Location = new Point(12, 12);
            resultsCard.Controls.Add(resLabel);

            var btnSelAll = Theme.MakeSecondaryButton("Select All", 90, 26);
            btnSelAll.Location = new Point(resultsCard.Width - 200, 9);
            btnSelAll.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnSelAll.Click   += (s, e) => { foreach (ListViewItem i in lvResults.Items) i.Checked = true; };
            resultsCard.Controls.Add(btnSelAll);

            var btnSelNone = Theme.MakeSecondaryButton("Select None", 90, 26);
            btnSelNone.Location = new Point(resultsCard.Width - 106, 9);
            btnSelNone.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnSelNone.Click   += (s, e) => { foreach (ListViewItem i in lvResults.Items) i.Checked = false; };
            resultsCard.Controls.Add(btnSelNone);

            lvResults = new ListView
            {
                Location      = new Point(8, 40),
                Size          = new Size(resultsCard.Width - 16, 208),
                View          = View.Details,
                CheckBoxes    = true,
                FullRowSelect = true,
                GridLines     = true,
                Font          = Theme.FontBase,
                BackColor     = Theme.CardBg,
                BorderStyle   = BorderStyle.None,
                Anchor        = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lvResults.Columns.Add("Computer Name", 200);
            lvResults.Columns.Add("OU",             300);
            resultsCard.Controls.Add(lvResults);

            // ── Message & send ────────────────────────────────────────────
            var msgCard = new Panel
            {
                Location  = new Point(20, 424),
                Size      = new Size(ClientSize.Width - 40, 120),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            msgCard.Paint += PaintBorder;
            Controls.Add(msgCard);

            var msgLabel = Theme.MakeLabel("Message:", bold: true);
            msgLabel.Location = new Point(12, 14);
            msgCard.Controls.Add(msgLabel);

            txtMessage = new TextBox
            {
                Location    = new Point(90, 10),
                Size        = new Size(msgCard.Width - 270, 30),
                Font        = Theme.FontBase,
                BackColor   = Theme.CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            txtMessage.TextChanged += (s, e) =>
            {
                int len = txtMessage.Text.Length;
                lblCharCount.Text      = $"{len}/255";
                lblCharCount.ForeColor = len > 255 ? Theme.AccentRed : Theme.TextSecondary;
            };
            msgCard.Controls.Add(txtMessage);

            lblCharCount = Theme.MakeLabel("0/255");
            lblCharCount.ForeColor = Theme.TextSecondary;
            lblCharCount.Location  = new Point(msgCard.Width - 170, 15);
            lblCharCount.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
            msgCard.Controls.Add(lblCharCount);

            btnSend = Theme.MakePrimaryButton("Send →", 110, 34);
            btnSend.Location = new Point(msgCard.Width - 126, 10);
            btnSend.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnSend.Click   += BtnSend_Click;
            msgCard.Controls.Add(btnSend);

            // Log
            rtbLog = new RichTextBox
            {
                Location   = new Point(12, 52),
                Size       = new Size(msgCard.Width - 24, 56),
                Font       = new Font("Consolas", 8f),
                BackColor  = Theme.CardBg,
                BorderStyle= BorderStyle.None,
                ReadOnly   = true,
                Anchor     = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            msgCard.Controls.Add(rtbLog);
        }

        private void DoSearch()
        {
            string query = txtQuery.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            btnSearch.Enabled = false;
            btnSearch.Text    = "Searching...";
            lvResults.Items.Clear();

            var results = AdHelper.SearchComputers(query);

            foreach (var pc in results)
            {
                var item = new ListViewItem(pc.Name) { Tag = pc.Name, Checked = true };
                item.SubItems.Add(pc.OUName);
                lvResults.Items.Add(item);
            }

            btnSearch.Enabled = true;
            btnSearch.Text    = "Search";

            if (results.Count == 0)
                AppendLog("No computers found.", Theme.TextSecondary);
            else
                AppendLog($"Found {results.Count} computer(s).", Theme.AccentGreen);
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                MessageBox.Show("Enter a message.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var targets = lvResults.CheckedItems
                .Cast<ListViewItem>()
                .Select(i => i.Tag?.ToString() ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (targets.Count == 0)
            {
                MessageBox.Show("No computers selected.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnSend.Enabled = false;
            string msg = txtMessage.Text;
            int ok = 0, fail = 0;

            await Task.Run(() =>
            {
                foreach (var pc in targets)
                {
                    var (success, err) = MsgSender.Send(pc, msg);
                    string line = success ? $"✓ {pc}" : $"✗ {pc} — {err}";
                    Invoke(() => AppendLog(line, success ? Theme.AccentGreen : Theme.AccentRed));
                    MsgSender.AppendLog(line);
                    if (success) ok++; else fail++;
                }
            });

            AppendLog($"Done — {ok} sent, {fail} failed.", Theme.TextPrimary);
            btnSend.Enabled = true;
        }

        private void AppendLog(string text, Color color)
        {
            rtbLog.SelectionStart  = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor  = Theme.TextSecondary;
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] ");
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(text + Environment.NewLine);
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
    }
}
