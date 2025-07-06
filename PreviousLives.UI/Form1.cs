using System;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace PreviousLives
{
    public partial class Form1 : Form
    {
        // --- Header controls ---
        private Panel _headerPanel, _divider;
        private Button _storiesButton;
        private LinkLabel _captureLink, _rememberLink, _discoverLink;
        private Label _titleLabel;

        // --- Webcam preview ---
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _webcam;
        private PictureBox _pbPreview;
        private Button _btnCapture;

        public Form1()
        {
            InitializeComponent();

            BuildHeader();
            BuildWebcamPreview();
            InitializeWebcam();
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

            // Logo square
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
            _titleLabel.Paint += (s, e) =>
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            _headerPanel.Controls.Add(_titleLabel);

            // STORIES pill
            _storiesButton = MakePill("STORIES (0)");
            _headerPanel.Controls.Add(_storiesButton);

            // Nav links
            _captureLink = MakeNav("CAPTURE");
            _rememberLink = MakeNav("REMEMBER");
            _discoverLink = MakeNav("DISCOVER");
            _headerPanel.Controls.AddRange(new Control[] { _captureLink, _rememberLink, _discoverLink });

            // Divider
            _divider = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = ColorTranslator.FromHtml("#333333")
            };
            _headerPanel.Controls.Add(_divider);

            LayoutHeader();
            _headerPanel.SizeChanged += (s, e) => LayoutHeader();
        }

        private Button MakePill(string text)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Padding = new Padding(12, 6, 12, 6),
                BackColor = Color.Transparent,
                ForeColor = ColorTranslator.FromHtml("#FFB347"),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#FFB347");
            b.Click += (s, e) => MessageBox.Show("Stories clicked");
            return b;
        }

        private LinkLabel MakeNav(string text)
        {
            var l = new LinkLabel
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
            l.Click += (s, e) => MessageBox.Show($"{text} clicked");
            return l;
        }

        private void LayoutHeader()
        {
            const int gap = 40;

            // Title just right of logo
            _titleLabel.Location = new Point(
                _headerPanel.Padding.Left + 40 + 10,
                (_headerPanel.Height - _titleLabel.PreferredHeight) / 2
            );

            // Discover ← Remember ← Capture ← Stories
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

        private void BuildWebcamPreview()
        {
            _pbPreview = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Black
            };
            Controls.Add(_pbPreview);

            _btnCapture = new Button
            {
                Text = "Capture",
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#FFB347"),
                ForeColor = Color.Black,
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            _btnCapture.FlatAppearance.BorderSize = 0;
            _btnCapture.Click += (s, e) =>
            {
                if (_pbPreview.Image != null)
                    MessageBox.Show("Captured!");
            };
            Controls.Add(_btnCapture);

            this.Shown += (s, e) =>
            {
                const int sideMargin = 40;
                const int topSpacing = 30;  // ← was 10, now 30 for more room  
                const int footerSpace = 80;
                const int bottomMargin = sideMargin + footerSpace;

                int topY = _headerPanel.Bottom + topSpacing;
                int availH = ClientSize.Height - bottomMargin - topY - topSpacing;

                int previewW = ClientSize.Width - sideMargin * 2;
                int previewH = previewW * 9 / 16;
                if (previewH > availH)
                {
                    previewH = availH;
                    previewW = previewH * 16 / 9;
                }

                _pbPreview.SetBounds(
                    (ClientSize.Width - previewW) / 2,
                    topY,
                    previewW,
                    previewH
                );
                _pbPreview.BringToFront();

                _btnCapture.Location = new Point(
                    (ClientSize.Width - _btnCapture.Width) / 2,
                    _pbPreview.Bottom + topSpacing
                );
                _btnCapture.BringToFront();
            };
        }

        private void InitializeWebcam()
        {
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (_videoDevices.Count == 0)
            {
                MessageBox.Show("No webcam found.");
                return;
            }
            _webcam = new VideoCaptureDevice(_videoDevices[0].MonikerString);
            _webcam.NewFrame += (s, e) =>
            {
                var bmp = (Bitmap)e.Frame.Clone();
                _pbPreview?.Invoke(new Action(() =>
                {
                    _pbPreview.Image?.Dispose();
                    _pbPreview.Image = bmp;
                }));
            };
            _webcam.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_webcam?.IsRunning == true)
                _webcam.SignalToStop();
            base.OnFormClosing(e);
        }
    }
}
