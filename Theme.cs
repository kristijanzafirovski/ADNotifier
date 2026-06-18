namespace GimNotifier
{
    public static class Theme
    {
        // Navy sidebar + clean white content area
        public static readonly Color NavBg       = Color.FromArgb(22, 40, 70);
        public static readonly Color NavHover    = Color.FromArgb(35, 60, 100);
        public static readonly Color NavActive   = Color.FromArgb(0, 120, 212);
        public static readonly Color NavText     = Color.FromArgb(190, 210, 240);
        public static readonly Color NavTextActive = Color.White;

        public static readonly Color ContentBg   = Color.FromArgb(245, 247, 250);
        public static readonly Color CardBg      = Color.White;
        public static readonly Color Border      = Color.FromArgb(220, 225, 235);

        public static readonly Color AccentBlue  = Color.FromArgb(0, 120, 212);
        public static readonly Color AccentGreen = Color.FromArgb(16, 124, 65);
        public static readonly Color AccentRed   = Color.FromArgb(196, 43, 28);

        public static readonly Color TextPrimary   = Color.FromArgb(24, 28, 36);
        public static readonly Color TextSecondary = Color.FromArgb(100, 110, 130);

        public static readonly Font FontBase    = new("Segoe UI", 9.5f);
        public static readonly Font FontSmall   = new("Segoe UI", 8.5f);
        public static readonly Font FontBold    = new("Segoe UI Semibold", 9.5f);
        public static readonly Font FontTitle   = new("Segoe UI Semibold", 13f);
        public static readonly Font FontNav     = new("Segoe UI", 9.5f);
        public static readonly Font FontNavBold = new("Segoe UI Semibold", 9.5f);

        public static Button MakePrimaryButton(string text, int w = 120, int h = 34)
        {
            return new Button
            {
                Text      = text,
                Width     = w,
                Height    = h,
                BackColor = AccentBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = FontBold,
                Cursor    = Cursors.Hand
            };
        }

        public static Button MakeSecondaryButton(string text, int w = 100, int h = 34)
        {
            var btn = new Button
            {
                Text      = text,
                Width     = w,
                Height    = h,
                BackColor = CardBg,
                ForeColor = TextPrimary,
                FlatStyle = FlatStyle.Flat,
                Font      = FontBase,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Border;
            btn.FlatAppearance.BorderSize  = 1;
            return btn;
        }

        public static Button MakeDangerButton(string text, int w = 100, int h = 34)
        {
            return new Button
            {
                Text      = text,
                Width     = w,
                Height    = h,
                BackColor = AccentRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = FontBold,
                Cursor    = Cursors.Hand
            };
        }

        public static TextBox MakeTextBox(int w, int h = 30, bool multiline = false)
        {
            return new TextBox
            {
                Width     = w,
                Height    = h,
                Multiline = multiline,
                Font      = FontBase,
                BackColor = CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        public static Label MakeLabel(string text, bool bold = false)
        {
            return new Label
            {
                Text      = text,
                Font      = bold ? FontBold : FontBase,
                ForeColor = TextPrimary,
                AutoSize  = true
            };
        }

        public static Panel MakeCard(int x, int y, int w, int h)
        {
            var p = new Panel
            {
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                BackColor = CardBg,
                Padding   = new Padding(16)
            };
            p.Paint += (s, e) =>
            {
                using var pen = new Pen(Border, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            };
            return p;
        }
    }
}
