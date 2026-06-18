using GimNotifier.Pages;

namespace GimNotifier
{
    public class MainForm : Form
    {
        private Panel pnlNav = null!;
        private Panel pnlContent = null!;
        private Panel? _activePage;
        private Button? _activeNavBtn;

        private SendPage sendPage = null!;
        private TemplatesPage templatesPage = null!;
        private SchedulePage schedulePage = null!;
        private SearchPage searchPage = null!;
        private SendPage sendPage1;
        private SettingsPage settingsPage = null!;

        public MainForm()
        {
            InitForm();
            BuildNav();
            BuildPages();
            Navigate(sendPage, _navButtons[0]);
        }

        private void InitForm()
        {
            Text = "GIM IT нотификации";
            Size = new Size(1080, 720);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.NavBg;
            Font = Theme.FontBase;
            Icon = SystemIcons.Application;
        }

        // ── Nav ────────────────────────────────────────────────────────────
        private readonly List<Button> _navButtons = new();

        private void BuildNav()
        {
            pnlNav = new Panel
            {
                Dock = DockStyle.Left,
                Width = 180,
                BackColor = Theme.NavBg,
                Padding = new Padding(0, 8, 0, 8)
            };
            Controls.Add(pnlNav);

            // Logo / app name
            var logo = new Label
            {
                Text = "GIM IT нотификации",
                Font = new Font("Segoe UI Semibold", 13f),
                ForeColor = Color.White,
                Location = new Point(0, 20),
                Width = 180,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlNav.Controls.Add(logo);

            var divider = new Panel
            {
                Location = new Point(16, 62),
                Size = new Size(148, 1),
                BackColor = Color.FromArgb(50, 80, 120)
            };
            pnlNav.Controls.Add(divider);

            // Nav items: label, emoji, y-offset
            var items = new (string label, string icon)[]
            {
                ("Прати",      "📨"),
                ("Шаблони", "📋"),
                ("Распоред",  "🕐"),
                ("Пребарување",    "🔍"),
                ("Поставки",  "⚙"),
            };

            int y = 80;
            foreach (var (label, icon) in items)
            {
                var btn = new Button
                {
                    Text = $"  {icon}  {label}",
                    Location = new Point(0, y),
                    Size = new Size(180, 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Theme.NavBg,
                    ForeColor = Theme.NavText,
                    Font = Theme.FontNav,
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 0, 0)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Theme.NavHover;
                btn.FlatAppearance.MouseDownBackColor = Theme.NavActive;

                _navButtons.Add(btn);
                pnlNav.Controls.Add(btn);
                y += 44;
            }

            var ver = new Label
            {
                Text = "v1.0  •  Кристијан Зафировски",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(90, 120, 160),
                Location = new Point(0, pnlNav.Height - 30),
                Width = 180,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            pnlNav.Controls.Add(ver);

            // Content area
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.ContentBg
            };
            Controls.Add(pnlContent);
            Controls.Add(pnlNav);
        }

        private void BuildPages()
        {
            sendPage = new SendPage();
            templatesPage = new TemplatesPage();
            schedulePage = new SchedulePage();
            searchPage = new SearchPage();
            settingsPage = new SettingsPage();

            // Wire up cross-page events
            templatesPage.TemplatesChanged += () => sendPage.LoadTemplates();
            settingsPage.SettingsSaved += () => sendPage.Reload();

            // Wire nav buttons to pages
            var pages = new Panel[] { sendPage, templatesPage, schedulePage, searchPage, settingsPage };
            for (int i = 0; i < _navButtons.Count; i++)
            {
                var page = pages[i];
                var btn = _navButtons[i];
                btn.Click += (s, e) => Navigate(page, btn);
            }

            // Add all to content (hidden by default)
           foreach (var page in pages)
{
    page.Dock = DockStyle.Fill;
    page.Visible = false;

    pnlContent.Controls.Add(page);
}
        }

        private void Navigate(Panel page, Button btn)
        {
            // Deactivate old
            if (_activePage != null) _activePage.Visible = false;
            if (_activeNavBtn != null)
            {
                _activeNavBtn.BackColor = Theme.NavBg;
                _activeNavBtn.ForeColor = Theme.NavText;
                _activeNavBtn.Font = Theme.FontNav;
            }

            // Activate new
            page.Visible = true;
            page.BringToFront();
            _activePage = page;

            btn.BackColor = Theme.NavActive;
            btn.ForeColor = Theme.NavTextActive;
            btn.Font = Theme.FontNavBold;
            _activeNavBtn = btn;
        }

        private void InitializeComponent()
        {
            sendPage1 = new SendPage();
            SuspendLayout();
            // 
            // sendPage1
            // 
            sendPage1.BackColor = Color.FromArgb(245, 247, 250);
            sendPage1.Dock = DockStyle.Fill;
            sendPage1.Location = new Point(0, 0);
            sendPage1.Name = "sendPage1";
            sendPage1.Padding = new Padding(5);
            sendPage1.Size = new Size(861, 482);
            sendPage1.TabIndex = 0;
            // 
            // MainForm
            // 
            ClientSize = new Size(861, 482);
            Controls.Add(sendPage1);
            Name = "MainForm";
            ResumeLayout(false);

        }
    }
}
