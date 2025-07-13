using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace PreviousLives
{
    public partial class Form2 : Form
    {
        // --- Header controls ---
        private Panel _headerPanel, _headerDivider;
        private Button _storiesButton;
        private LinkLabel _captureLink, _rememberLink, _discoverLink;
        private Label _titleLabel;

        // --- Content controls ---
        private Panel _pictureFrame;
        private PictureBox _pbEdited;       // <-- renamed to make clear this is the edited image
        private TextBox _tbDescription;

        // --- Footer controls ---
        private Panel _footerPanel, _footerDivider;
        private LinkLabel _footerLeftFacebook, _footerLeftYouTube, _footerLeftInstagram;
        private Label _footerCenterLabel, _footerRightLabel;

        private readonly string _connectionString;
        private readonly long _captureId;

        public Form2(string connectionString, long captureId)
            : this()
        {
            _connectionString = connectionString;
            _captureId = captureId;
        }

        public Form2()
        {
            InitializeComponent();
            BuildHeader();
            BuildContent();
            BuildFooter();
            Load += Form2_Load;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            // <-- pull EditedImage instead of ImageData
            cmd.CommandText = @"
                SELECT EditedImage, Description
                  FROM Captures
                 WHERE Id = $id
            ";
            cmd.Parameters.AddWithValue("$id", _captureId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                MessageBox.Show($"No capture found with ID {_captureId}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            // load the *edited* image
            var blob = (byte[])reader["EditedImage"];
            using var ms = new MemoryStream(blob);
            _pbEdited.Image = Image.FromStream(ms);

            // load description
            _tbDescription.TextAlign = HorizontalAlignment.Center;
            _tbDescription.Text = reader.GetString(reader.GetOrdinal("Description"));
        }

        private void BuildHeader()
        {
            BackColor = ColorTranslator.FromHtml("#121212");
            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20, 0, 20, 0),
                BackColor = BackColor
            };
            Controls.Add(_headerPanel);

            // Logo
            var logo = new Panel
            {
                Size = new Size(40, 40),
                Location = new Point(_headerPanel.Padding.Left, (_headerPanel.Height - 40) / 2),
                BackColor = ColorTranslator.FromHtml("#FFB347")
            };
            _headerPanel.Controls.Add(logo);
            logo.Controls.Add(new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = Properties.Resources.hourglasslogo
            });

            // Title
            _titleLabel = new Label
            {
                Text = "Previous Lives",
                UseCompatibleTextRendering = true,
                Font = new Font("Segoe UI Semibold", 18F),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            _titleLabel.Paint += (s, ea) =>
                ea.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            _headerPanel.Controls.Add(_titleLabel);

            // Nav links
            _storiesButton = MakePill("PRICING");
            _captureLink = MakeNav("CAPTURE");
            _rememberLink = MakeNav("REMEMBER");
            _discoverLink = MakeNav("DISCOVER");
            _headerPanel.Controls.AddRange(new Control[] {
                _storiesButton, _captureLink, _rememberLink, _discoverLink
            });

            // Divider
            _headerDivider = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = ColorTranslator.FromHtml("#333333")
            };
            _headerPanel.Controls.Add(_headerDivider);

            LayoutHeader();
            _headerPanel.SizeChanged += (s, e) => LayoutHeader();
        }

        private void LayoutHeader()
        {
            const int gap = 40;
            _titleLabel.Location = new Point(
                _headerPanel.Padding.Left + 50,
                (_headerPanel.Height - _titleLabel.PreferredHeight) / 2
            );

            int rx = _headerPanel.ClientSize.Width - _headerPanel.Padding.Right;
            int cy = _headerPanel.Height / 2;

            _discoverLink.Location = new Point(rx - _discoverLink.PreferredWidth, cy - _discoverLink.PreferredHeight / 2);
            rx -= _discoverLink.PreferredWidth + gap;
            _rememberLink.Location = new Point(rx - _rememberLink.PreferredWidth, cy - _rememberLink.PreferredHeight / 2);
            rx -= _rememberLink.PreferredWidth + gap;
            _captureLink.Location = new Point(rx - _captureLink.PreferredWidth, cy - _captureLink.PreferredHeight / 2);
            rx -= _captureLink.PreferredWidth + gap;
            _storiesButton.Location = new Point(rx - _storiesButton.PreferredSize.Width, cy - _storiesButton.PreferredSize.Height / 2);
        }

        private Button MakePill(string text) => new Button
        {
            Text = text,
            Font = new Font("Segoe UI", 9F),
            FlatStyle = FlatStyle.Flat,
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6),
            BackColor = Color.Transparent,
            ForeColor = ColorTranslator.FromHtml("#FFB347"),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatAppearance = { BorderSize = 1, BorderColor = ColorTranslator.FromHtml("#FFB347") }
        };

        private LinkLabel MakeNav(string text) => new LinkLabel
        {
            Text = text,
            LinkColor = Color.White,
            ActiveLinkColor = ColorTranslator.FromHtml("#FFB347"),
            LinkBehavior = LinkBehavior.HoverUnderline,
            AutoSize = true,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 9F),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        private void BuildContent()
        {
            // Picture frame
            _pictureFrame = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#FFB347"),
                Padding = new Padding(2)
            };
            Controls.Add(_pictureFrame);

            _pbEdited = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };
            _pictureFrame.Controls.Add(_pbEdited);

            // Description textbox
            _tbDescription = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.None,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Segoe UI", 8F),
                Dock = DockStyle.Bottom,
                Height = 120,
                BorderStyle = BorderStyle.None,
                BackColor = BackColor,
                ForeColor = Color.White
            };
            Controls.Add(_tbDescription);

            Shown += (s, e) =>
            {
                int margin = 40;
                int topY = _headerPanel.Bottom + margin / 2;
                int descrY = Height - _footerPanel.Height - margin / 2 - _tbDescription.Height;

                int maxH = descrY - topY;
                int maxW = ClientSize.Width - margin * 2;

                if (_pbEdited.Image != null)
                {
                    double ratio = (double)_pbEdited.Image.Width / _pbEdited.Image.Height;
                    int frameH = maxH;
                    int frameW = (int)(frameH * ratio);
                    if (frameW > maxW)
                    {
                        frameW = maxW;
                        frameH = (int)(frameW / ratio);
                    }
                    int frameX = (ClientSize.Width - frameW) / 2;
                    _pictureFrame.SetBounds(frameX, topY, frameW, frameH);
                }
                else
                {
                    _pictureFrame.SetBounds(margin, topY, maxW, maxH);
                }

                _tbDescription.SetBounds(margin, descrY + 10, maxW, _tbDescription.Height);
            };
        }

        private void BuildFooter()
        {
            _footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                Padding = new Padding(20, 0, 20, 0),
                BackColor = BackColor
            };
            Controls.Add(_footerPanel);

            _footerDivider = new Panel
            {
                Height = 1,
                BackColor = ColorTranslator.FromHtml("#333333")
            };
            _footerPanel.Controls.Add(_footerDivider);

            _footerLeftFacebook = MakeFooterLink("Facebook");
            _footerLeftYouTube = MakeFooterLink("YouTube");
            _footerLeftInstagram = MakeFooterLink("Instagram");
            _footerPanel.Controls.AddRange(new Control[] {
                _footerLeftFacebook, _footerLeftYouTube, _footerLeftInstagram
            });

            _footerCenterLabel = new Label
            {
                Text = "© PreviousLives",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            _footerRightLabel = new Label
            {
                Text = "Powered by M.T.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            _footerPanel.Controls.AddRange(new Control[] { _footerCenterLabel, _footerRightLabel });

            void LayoutFooter()
            {
                _footerDivider.SetBounds(0, 40, _footerPanel.ClientSize.Width, 1);
                int y = 40 + (_footerCenterLabel.PreferredHeight / 2);

                _footerLeftFacebook.Location = new Point(_footerPanel.Padding.Left, y);
                _footerLeftYouTube.Location = new Point(_footerLeftFacebook.Right + 20, y);
                _footerLeftInstagram.Location = new Point(_footerLeftYouTube.Right + 20, y);

                _footerCenterLabel.Location = new Point(
                    (_footerPanel.ClientSize.Width - _footerCenterLabel.PreferredWidth) / 2, y);
                _footerRightLabel.Location = new Point(
                    _footerPanel.ClientSize.Width - _footerRightLabel.PreferredWidth - _footerPanel.Padding.Right, y);
            }

            _footerPanel.SizeChanged += (s, e) => LayoutFooter();
            LayoutFooter();
        }

        private LinkLabel MakeFooterLink(string text) => new LinkLabel
        {
            Text = text,
            LinkColor = Color.White,
            ActiveLinkColor = ColorTranslator.FromHtml("#FFB347"),
            LinkBehavior = LinkBehavior.HoverUnderline,
            AutoSize = true,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 9F),
            Cursor = Cursors.Hand
        };
    }
}
