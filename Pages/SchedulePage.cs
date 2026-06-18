using GimNotifier.Helpers;
using GimNotifier.Models;

namespace GimNotifier.Pages
{
    public class SchedulePage : Panel
    {
        private List<ScheduledMessage> _scheduled = new();
        private System.Windows.Forms.Timer _ticker = null!;
        private ListView lvSchedule = null!;

        // New schedule form fields
        private DateTimePicker dtpDate = null!;
        private DateTimePicker dtpTime = null!;
        private TextBox txtComputers = null!;
        private TextBox txtMessage = null!;
        private Label lblCharCount = null!;

        public SchedulePage()
        {
            Dock      = DockStyle.Fill;
            BackColor = Theme.ContentBg;
            Padding   = new Padding(20);
            BuildUI();
            LoadSchedule();
            StartTicker();
        }

        private void BuildUI()
        {
            var title = Theme.MakeLabel("Scheduled Sends", bold: true);
            title.Font     = Theme.FontTitle;
            title.Location = new Point(20, 20);
            Controls.Add(title);

            var sub = Theme.MakeLabel("Schedule messages to be sent automatically at a specified date and time.");
            sub.ForeColor = Theme.TextSecondary;
            sub.Location  = new Point(20, 50);
            Controls.Add(sub);

            // ── Upcoming list ─────────────────────────────────────────────
            var listCard = new Panel
            {
                Location  = new Point(20, 80),
                Size      = new Size(ClientSize.Width - 40, 220),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            listCard.Paint += PaintBorder;
            Controls.Add(listCard);

            var listLabel = Theme.MakeLabel("Upcoming & Past", bold: true);
            listLabel.Location = new Point(12, 12);
            listCard.Controls.Add(listLabel);

            lvSchedule = new ListView
            {
                Location     = new Point(8, 36),
                Size         = new Size(listCard.Width - 16, 172),
                View         = View.Details,
                FullRowSelect= true,
                GridLines    = true,
                Font         = Theme.FontBase,
                BackColor    = Theme.CardBg,
                BorderStyle  = BorderStyle.None,
                Anchor       = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            lvSchedule.Columns.Add("Status",   70);
            lvSchedule.Columns.Add("Send At",  140);
            lvSchedule.Columns.Add("Targets",  100);
            lvSchedule.Columns.Add("Message",  400);
            listCard.Controls.Add(lvSchedule);

            // Cancel button
            var btnCancel = Theme.MakeDangerButton("Cancel Selected", 140, 28);
            btnCancel.Location = new Point(listCard.Width - 156, 10);
            btnCancel.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Click   += BtnCancel_Click;
            listCard.Controls.Add(btnCancel);

            // ── New schedule form ─────────────────────────────────────────
            var formCard = new Panel
            {
                Location  = new Point(20, 316),
                Size      = new Size(ClientSize.Width - 40, 230),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            formCard.Paint += PaintBorder;
            Controls.Add(formCard);

            var formLabel = Theme.MakeLabel("New Scheduled Send", bold: true);
            formLabel.Location = new Point(12, 12);
            formCard.Controls.Add(formLabel);

            // Date
            var dateLabel = Theme.MakeLabel("Date:");
            dateLabel.Location = new Point(12, 48);
            formCard.Controls.Add(dateLabel);

            dtpDate = new DateTimePicker
            {
                Location = new Point(80, 44),
                Width    = 140,
                Font     = Theme.FontBase,
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Now
            };
            formCard.Controls.Add(dtpDate);

            // Time
            var timeLabel = Theme.MakeLabel("Time:");
            timeLabel.Location = new Point(240, 48);
            formCard.Controls.Add(timeLabel);

            dtpTime = new DateTimePicker
            {
                Location     = new Point(290, 44),
                Width        = 120,
                Font         = Theme.FontBase,
                Format       = DateTimePickerFormat.Time,
                ShowUpDown   = true,
                Value        = DateTime.Now.AddHours(1)
            };
            formCard.Controls.Add(dtpTime);

            // Computers
            var compLabel = Theme.MakeLabel("Computers (comma-separated):");
            compLabel.Location = new Point(12, 86);
            formCard.Controls.Add(compLabel);

            txtComputers = Theme.MakeTextBox(formCard.Width - 28);
            txtComputers.Location     = new Point(12, 106);
            txtComputers.PlaceholderText = "PC-01, PC-02, PC-03";
            txtComputers.Anchor       = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            formCard.Controls.Add(txtComputers);

            // Message
            var msgLabel = Theme.MakeLabel("Message:");
            msgLabel.Location = new Point(12, 142);
            formCard.Controls.Add(msgLabel);

            txtMessage = new TextBox
            {
                Location    = new Point(80, 138),
                Size        = new Size(formCard.Width - 220, 30),
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
            formCard.Controls.Add(txtMessage);

            lblCharCount = Theme.MakeLabel("0/255");
            lblCharCount.ForeColor = Theme.TextSecondary;
            lblCharCount.Location  = new Point(formCard.Width - 130, 144);
            lblCharCount.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
            formCard.Controls.Add(lblCharCount);

            var btnSchedule = Theme.MakePrimaryButton("Schedule →", 120, 34);
            btnSchedule.Location = new Point(formCard.Width - 136, 182);
            btnSchedule.Anchor   = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSchedule.Click   += BtnSchedule_Click;
            formCard.Controls.Add(btnSchedule);
        }

        private void LoadSchedule()
        {
            _scheduled = ScheduleManager.Load();
            RefreshList();
        }

        private void RefreshList()
        {
            lvSchedule.Items.Clear();

            foreach (var s in _scheduled.OrderBy(x => x.SendAt))
            {
                string status = s.Cancelled ? "🚫 Cancelled"
                              : s.Sent      ? "✓ Sent"
                              : s.SendAt < DateTime.Now ? "⚠ Missed"
                              : "🕐 Pending";

                var item = new ListViewItem(status) { Tag = s.Id };
                item.SubItems.Add(s.SendAt.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add($"{s.Computers.Count} PCs");
                item.SubItems.Add(s.Message);

                if (s.Sent)      item.ForeColor = Theme.AccentGreen;
                if (s.Cancelled) item.ForeColor = Theme.TextSecondary;
                if (!s.Sent && !s.Cancelled && s.SendAt < DateTime.Now)
                    item.ForeColor = Color.DarkOrange;

                lvSchedule.Items.Add(item);
            }
        }

        private void BtnSchedule_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtComputers.Text) ||
                string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                MessageBox.Show("Computers and message are required.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtMessage.Text.Length > 255)
            {
                MessageBox.Show("Message exceeds 255 characters.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sendAt = dtpDate.Value.Date + dtpTime.Value.TimeOfDay;
            if (sendAt <= DateTime.Now)
            {
                MessageBox.Show("Scheduled time must be in the future.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var computers = txtComputers.Text
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            _scheduled.Add(new ScheduledMessage
            {
                SendAt    = sendAt,
                Message   = txtMessage.Text.Trim(),
                Computers = computers,
                OULabel   = "Manual"
            });

            ScheduleManager.Save(_scheduled);
            RefreshList();

            txtComputers.Text = "";
            txtMessage.Text   = "";
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            if (lvSchedule.SelectedItems.Count == 0) return;

            string id = lvSchedule.SelectedItems[0].Tag?.ToString() ?? "";
            var item  = _scheduled.FirstOrDefault(s => s.Id == id);

            if (item == null || item.Sent) return;

            item.Cancelled = true;
            ScheduleManager.Save(_scheduled);
            RefreshList();
        }

        private void StartTicker()
        {
            _ticker          = new System.Windows.Forms.Timer();
            _ticker.Interval = 30000; // check every 30 seconds
            _ticker.Tick    += Ticker_Tick;
            _ticker.Start();
        }

        private async void Ticker_Tick(object? sender, EventArgs e)
        {
            var due = _scheduled
                .Where(s => !s.Sent && !s.Cancelled && s.SendAt <= DateTime.Now)
                .ToList();

            if (due.Count == 0) return;

            foreach (var job in due)
            {
                await Task.Run(() =>
                {
                    foreach (var pc in job.Computers)
                        MsgSender.Send(pc, job.Message);
                });

                job.Sent = true;
                MsgSender.AppendLog($"[SCHEDULED] Sent to {job.Computers.Count} PCs: {job.Message}");
            }

            ScheduleManager.Save(_scheduled);
            Invoke(RefreshList);
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
