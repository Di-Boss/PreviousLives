using System;
using System.Drawing;
using System.Windows.Forms;

namespace PreviousLives
{
    public partial class Form1 : Form
    {
        private Panel _headerPanel;
        private Panel _divider;        // ← divider line
        private Button _storiesButton;
        private LinkLabel _captureLink, _rememberLink, _discoverLink;
        private Label _titleLabel;

        public Form1()
        {
            InitializeComponent();
            BuildHeader();
        }

        private void BuildHeader()
        {
            // overall styling
            BackColor = ColorTranslator.FromHtml("#121212");

            // 1) Header panel, 20px left/right padding
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20, 0, 20, 0),
                BackColor = BackColor
            };
            Controls.Add(_headerPanel);

            // 2) Amber square for logo
            var logoContainer = new Panel
            {
                Size = new Size(40, 40),
                Location = new Point(_headerPanel.Padding.Left, (_headerPanel.Height - 40) / 2),
                BackColor = ColorTranslator.FromHtml("#FFB347")
            };
            _headerPanel.Controls.Add(logoContainer);

            // 3) Hourglass icon
            var picLogo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(32, 32),
                Location = new Point((logoContainer.Width - 32) / 2, (logoContainer.Height - 32) / 2),
                BackColor = Color.Transparent,
                Image = Properties.Resources.hourglasslogo
            };
            logoContainer.Controls.Add(picLogo);

            // 4) Title label (left group)
            _titleLabel = new Label
            {
                Text = "Previous Lives",
                UseCompatibleTextRendering = true,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            _titleLabel.Paint += (s, e) =>
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            _headerPanel.Controls.Add(_titleLabel);

            // 5) Right-group: STORIES pill
            _storiesButton = new Button
            {
                Text = "STORIES (0)",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = ColorTranslator.FromHtml("#FFB347"),
                BackColor = Color.Transparent,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(12, 6, 12, 6),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _storiesButton.FlatAppearance.BorderSize = 1;
            _storiesButton.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#FFB347");
            _storiesButton.Click += (s, e) => MessageBox.Show("Stories clicked (stub)");
            _headerPanel.Controls.Add(_storiesButton);

            // 6) Right-group: navigation links
            _captureLink = CreateNavLink("CAPTURE");
            _rememberLink = CreateNavLink("REMEMBER");
            _discoverLink = CreateNavLink("DISCOVER");
            _headerPanel.Controls.AddRange(new Control[]
            {
                _captureLink, _rememberLink, _discoverLink
            });

            // 7) Divider line
            _divider = new Panel
            {
                Height = 1,
                Dock = DockStyle.Bottom,
                BackColor = ColorTranslator.FromHtml("#333333")
            };
            _headerPanel.Controls.Add(_divider);

            // 8) Initial layout + resize handler
            LayoutHeader();
            _headerPanel.SizeChanged += (s, e) => LayoutHeader();
        }

        private void LayoutHeader()
        {
            const int gap = 40;

            // Left group: logoContainer at Padding.Left, then title
            _titleLabel.Location = new Point(
                _headerPanel.Padding.Left + 40 + 10,
                (_headerPanel.Height - _titleLabel.PreferredHeight) / 2
            );

            // Right-group anchored to the right edge:
            int rightX = _headerPanel.ClientSize.Width - _headerPanel.Padding.Right;
            int centerY = _headerPanel.Height / 2;

            // place DISCOVER
            _discoverLink.Location = new Point(
                rightX - _discoverLink.PreferredWidth,
                centerY - (_discoverLink.PreferredHeight / 2)
            );
            // then REMEMBER
            rightX -= _discoverLink.PreferredWidth + gap;
            _rememberLink.Location = new Point(
                rightX - _rememberLink.PreferredWidth,
                centerY - (_rememberLink.PreferredHeight / 2)
            );
            // then CAPTURE
            rightX -= _rememberLink.PreferredWidth + gap;
            _captureLink.Location = new Point(
                rightX - _captureLink.PreferredWidth,
                centerY - (_captureLink.PreferredHeight / 2)
            );
            // finally STORIES
            rightX -= _captureLink.PreferredWidth + gap;
            _storiesButton.Location = new Point(
                rightX - _storiesButton.PreferredSize.Width,
                centerY - (_storiesButton.PreferredSize.Height / 2)
            );
        }

        private LinkLabel CreateNavLink(string text)
        {
            var link = new LinkLabel
            {
                Text = text,
                LinkColor = Color.White,
                ActiveLinkColor = ColorTranslator.FromHtml("#FFB347"),
                LinkBehavior = LinkBehavior.HoverUnderline,
                AutoSize = true,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            link.Click += (s, e) => MessageBox.Show($"Clicked {text} (stub)");
            return link;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // no extra init required
        }
    }
}
