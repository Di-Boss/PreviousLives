using AForge.Video;
using AForge.Video.DirectShow;
using Microsoft.Data.Sqlite;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using PreviousLives.Data;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreviousLives
{
    public partial class Form1 : Form
    {
        // --- Header controls ---
        private Panel _headerPanel, _headerDivider;
        private Button _storiesButton;
        private LinkLabel _captureLink, _rememberLink, _discoverLink;
        private Label _titleLabel;

        // --- Footer controls ---
        private Panel _footerPanel, _footerDivider;
        private LinkLabel _footerLeftFacebook, _footerLeftYouTube, _footerLeftInstagram;
        private Label _footerCenterLabel, _footerRightLabel;

        // --- Webcam preview & capture ---
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _webcam;
        private PictureBox _pbPreview;
        private Button _btnCapture;

        // your SQLite connection string
        private readonly string _connectionString;

        // OpenAI service (unused in C# but retained)
        private readonly OpenAIService _openAi;

        // 25 Past-life professions
        private static readonly string[] Professions = new[]
        {
            "African scammer","Homeless","Opera Singer","Oil Billionaire","F1 Racer",
            "Club Bouncer","Drug Addict","YouTuber","WWE Wrestler","Whiskey Bar Owner",
            "President of India","Pickpocketer","Prisoner","SEAL Team 6 Operator",
            "Car Salesman","Viking","Hunter","Bear Wrestler","Dictator","Model",
            "Rockstar","Rapper","Pilot","Burglar","Rocker Biker"
        };
        private readonly Random _rng = new Random();

        public Form1(string connectionString) : this()
        {
            _connectionString = connectionString;
        }

        public Form1()
        {
            InitializeComponent();
            BuildHeader();
            BuildWebcamPreview();
            BuildFooter();

            // Initialize or migrate the database on startup
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // Ensure PreviousLives folder exists and initialize the DB
            _connectionString = Database.Initialize(Path.Combine(localAppData, "PreviousLives"));

            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                         ?? throw new InvalidOperationException("Please set OPENAI_API_KEY");
            _openAi = new OpenAIService(new OpenAiOptions { ApiKey = apiKey });

            // wire up capture
            _btnCapture.Click += CaptureAndSave;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeWebcam();
        }

        // ------------------------
        //     BUILD HEADER
        // ------------------------
        private void OpenUrl(string url)
        {
            // On .NET Framework and .NET Core 3.1+ you need UseShellExecute = true
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
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

            _storiesButton = MakePill("PRICING");
            _captureLink = MakeNav("CAPTURE");
            _rememberLink = MakeNav("REMEMBER");
            _discoverLink = MakeNav("DISCOVER");
            _headerPanel.Controls.AddRange(new Control[] {
                _storiesButton, _captureLink, _rememberLink, _discoverLink
            });


            _storiesButton.Click += (s, e) => OpenUrl("https://google.com");
            _captureLink.LinkClicked += (s, e) => OpenUrl("https://your.site/capture");
            _rememberLink.LinkClicked += (s, e) => OpenUrl("https://your.site/remember");
            _discoverLink.LinkClicked += (s, e) => OpenUrl("https://your.site/discover");



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

        // ------------------------
        //     BUILD FOOTER
        // ------------------------
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
            _footerPanel.Controls.AddRange(new Control[] {
                _footerCenterLabel, _footerRightLabel
            });

            LayoutFooter();
            _footerPanel.SizeChanged += (s, e) => LayoutFooter();
        }

        private void LayoutFooter()
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

        // ------------------------
        //   BUILD WEBCAM + BUTTON
        // ------------------------
        private void BuildWebcamPreview()
        {
            var previewContainer = new Panel
            {
                BackColor = ColorTranslator.FromHtml("#FFB347"),
                Padding = new Padding(2)
            };
            Controls.Add(previewContainer);

            _pbPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            previewContainer.Controls.Add(_pbPreview);

            _btnCapture = new Button
            {
                Text = "CAPTURE",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = ColorTranslator.FromHtml("#FFB347"),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnCapture.FlatAppearance.BorderSize = 2;
            _btnCapture.FlatAppearance.BorderColor = Color.White;
            Controls.Add(_btnCapture);

            Shown += (s, e) =>
            {
                const int side = 40, topSp = 30, footSp = 80;
                int bottomM = side + footSp,
                    topY = _headerPanel.Bottom + topSp;
                int availH = ClientSize.Height - bottomM - topY - topSp,
                    availW = ClientSize.Width - side * 2;
                int w = availW, h = w * 9 / 16;
                if (h > availH) { h = availH; w = h * 16 / 9; }

                previewContainer.SetBounds((ClientSize.Width - w) / 2, topY, w, h);
                previewContainer.BringToFront();

                var sz = _btnCapture.PreferredSize;
                _btnCapture.Size = sz;
                _btnCapture.Location = new Point((ClientSize.Width - sz.Width) / 2, previewContainer.Bottom + topSp);
                _btnCapture.BringToFront();
            };
        }

        // ------------------------
        //  CAPTURE → Python img2img → DB → Show Form2
        // ------------------------
        private async void CaptureAndSave(object sender, EventArgs e)
        {
            try
            {
                // STEP 1
                if (_pbPreview.Image == null)
                {
                    MessageBox.Show("No image in preview!", "Error");
                    return;
                }

                // STEP 2: save the capture
                var exeDir = Application.StartupPath;
                var imagePath = Path.Combine(exeDir, "upload.png");
                _pbPreview.Image.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

                // STEP 3: pick profession & age
                var profession = Professions[_rng.Next(Professions.Length)];
                var age = _rng.Next(20, 81);

                // STEP 4: locate the Python script
                var scriptPath = Path.Combine(exeDir, "img2img.py");
                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show($"Python script not found at:\n{scriptPath}", "Error");
                    return;
                }

                // STEP 5: compute the same DB path used by Initialize
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dbPath = Path.Combine(localAppData, "PreviousLives", "captures.db");

                // STEP 6: launch Python against that DB
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments =
                        $"\"{scriptPath}\" --image \"{imagePath}\"" +
                        $" --profession \"{profession}\" --age {age}" +
                        $" --db \"{dbPath}\"",
                    WorkingDirectory = exeDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);

                var stdout = await proc.StandardOutput.ReadToEndAsync();
                var stderr = await proc.StandardError.ReadToEndAsync();
                proc.WaitForExit();

                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    MessageBox.Show(stderr, "Python error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // STEP 7: parse ID
                var successLine = stdout
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(l => l.Contains("Saved capture"));
                if (successLine == null)
                {
                    MessageBox.Show("No success line from Python.", "Error");
                    return;
                }

                var parts = successLine.Split('#');
                if (!long.TryParse(parts.Last().Trim(), out var newId))
                {
                    MessageBox.Show("Failed to parse new capture ID.", "Error");
                    return;
                }

                // STEP 8: show Form2
                var viewer = new Form2(_connectionString, newId);
                viewer.Show();
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------
        //   WEBCAM INIT & STOP
        // ------------------------
        private void InitializeWebcam()
        {
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (_videoDevices.Count == 0)
            {
                MessageBox.Show("No webcam found.", "Error");
                return;
            }
            _webcam = new VideoCaptureDevice(_videoDevices[0].MonikerString);
            _webcam.NewFrame += (s, ea) =>
            {
                var bmp = (Bitmap)ea.Frame.Clone();
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

