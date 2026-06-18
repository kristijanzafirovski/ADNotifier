using GimNotifier.Helpers;
using GimNotifier.Models;

namespace GimNotifier.Pages
{
    public class TemplatesPage : Panel
    {
        private List<MessageTemplate> _templates = new();
        private ListBox lstTemplates = null!;
        private TextBox txtName = null!;
        private TextBox txtBody = null!;
        private Button btnSave = null!;
        private Button btnDelete = null!;
        private Button btnNew = null!;
        private string? _editingId = null;

        public event Action? TemplatesChanged;

        public TemplatesPage()
        {
            Dock      = DockStyle.Fill;
            BackColor = Theme.ContentBg;
            Padding   = new Padding(20);
            BuildUI();
            LoadTemplates();
        }

        private void BuildUI()
        {
            var title = Theme.MakeLabel("Message Templates", bold: true);
            title.Font     = Theme.FontTitle;
            title.Location = new Point(20, 20);
            Controls.Add(title);

            var sub = Theme.MakeLabel("Save frequently used messages for quick access on the Send page.");
            sub.ForeColor = Theme.TextSecondary;
            sub.Location  = new Point(20, 50);
            Controls.Add(sub);

            // ── Left: template list ───────────────────────────────────────
            var listCard = new Panel
            {
                Location  = new Point(20, 80),
                Size      = new Size(280, 420),
                BackColor = Theme.CardBg
            };
            listCard.Paint += PaintBorder;
            Controls.Add(listCard);

            var listLabel = Theme.MakeLabel("Saved Templates", bold: true);
            listLabel.Location = new Point(12, 12);
            listCard.Controls.Add(listLabel);

            btnNew = Theme.MakePrimaryButton("+ New", 80, 28);
            btnNew.Location = new Point(listCard.Width - 96, 8);
            btnNew.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnNew.Click   += BtnNew_Click;
            listCard.Controls.Add(btnNew);

            lstTemplates = new ListBox
            {
                Location    = new Point(8, 44),
                Size        = new Size(264, 364),
                Font        = Theme.FontBase,
                BackColor   = Theme.CardBg,
                BorderStyle = BorderStyle.None,
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right
            };
            lstTemplates.SelectedIndexChanged += LstTemplates_SelectedIndexChanged;
            listCard.Controls.Add(lstTemplates);

            // ── Right: editor ─────────────────────────────────────────────
            var editorCard = new Panel
            {
                Location  = new Point(312, 80),
                Size      = new Size(ClientSize.Width - 340, 420),
                BackColor = Theme.CardBg,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            editorCard.Paint += PaintBorder;
            Controls.Add(editorCard);

            var editorLabel = Theme.MakeLabel("Template Editor", bold: true);
            editorLabel.Location = new Point(12, 12);
            editorCard.Controls.Add(editorLabel);

            var nameLabel = Theme.MakeLabel("Name:");
            nameLabel.Location = new Point(12, 50);
            editorCard.Controls.Add(nameLabel);

            txtName = Theme.MakeTextBox(editorCard.Width - 28);
            txtName.Location = new Point(12, 70);
            txtName.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.PlaceholderText = "e.g. Maintenance Warning";
            editorCard.Controls.Add(txtName);

            var bodyLabel = Theme.MakeLabel("Message Body:");
            bodyLabel.Location = new Point(12, 112);
            editorCard.Controls.Add(bodyLabel);

            txtBody = new TextBox
            {
                Location    = new Point(12, 132),
                Size        = new Size(editorCard.Width - 28, 200),
                Multiline   = true,
                Font        = Theme.FontBase,
                BackColor   = Theme.CardBg,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars  = ScrollBars.Vertical,
                Anchor      = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            editorCard.Controls.Add(txtBody);

            btnSave = Theme.MakePrimaryButton("Save Template", 140, 34);
            btnSave.Location = new Point(editorCard.Width - 156, editorCard.Height - 50);
            btnSave.Anchor   = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.Click   += BtnSave_Click;
            editorCard.Controls.Add(btnSave);

            btnDelete = Theme.MakeDangerButton("Delete", 90, 34);
            btnDelete.Location = new Point(editorCard.Width - 256, editorCard.Height - 50);
            btnDelete.Anchor   = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDelete.Enabled  = false;
            btnDelete.Click   += BtnDelete_Click;
            editorCard.Controls.Add(btnDelete);
        }

        private void LoadTemplates()
        {
            _templates = TemplateManager.Load();
            RefreshList();
        }

        private void RefreshList()
        {
            lstTemplates.Items.Clear();
            foreach (var t in _templates)
                lstTemplates.Items.Add(t);
            lstTemplates.DisplayMember = "Name";
        }

        private void LstTemplates_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstTemplates.SelectedItem is MessageTemplate t)
            {
                _editingId   = t.Id;
                txtName.Text = t.Name;
                txtBody.Text = t.Body;
                btnDelete.Enabled = true;
            }
        }

        private void BtnNew_Click(object? sender, EventArgs e)
        {
            _editingId        = null;
            txtName.Text      = "";
            txtBody.Text      = "";
            btnDelete.Enabled = false;
            lstTemplates.ClearSelected();
            txtName.Focus();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtBody.Text))
            {
                MessageBox.Show("Name and body are required.", "GIM Notifier",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_editingId != null)
            {
                var existing = _templates.FirstOrDefault(t => t.Id == _editingId);
                if (existing != null)
                {
                    existing.Name = txtName.Text.Trim();
                    existing.Body = txtBody.Text.Trim();
                }
            }
            else
            {
                _templates.Add(new MessageTemplate
                {
                    Name = txtName.Text.Trim(),
                    Body = txtBody.Text.Trim()
                });
            }

            TemplateManager.Save(_templates);
            RefreshList();
            TemplatesChanged?.Invoke();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_editingId == null) return;

            var result = MessageBox.Show("Delete this template?", "GIM Notifier",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            _templates.RemoveAll(t => t.Id == _editingId);
            TemplateManager.Save(_templates);
            RefreshList();
            BtnNew_Click(null, EventArgs.Empty);
            TemplatesChanged?.Invoke();
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
