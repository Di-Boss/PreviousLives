using System;
using System.Drawing;
using System.Windows.Forms;

namespace PreviousLives
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // overall form styling
            this.BackColor = ColorTranslator.FromHtml("#121212");

            // 1) Header panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = this.BackColor
            };
            this.Controls.Add(headerPanel);

            // 2) Amber square behind the logo
            var logoContainer = new Panel
            {
                Size = new Size(40, 40),
                Location = new Point(20, (headerPanel.Height - 40) / 2),
                BackColor = ColorTranslator.FromHtml("#FFB347")
            };
            headerPanel.Controls.Add(logoContainer);

            // 3) Hourglass icon centered in that square
            var picLogo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(32, 32),
                Location = new Point((logoContainer.Width - 32) / 2,
                                      (logoContainer.Height - 32) / 2),
                BackColor = Color.Transparent,
                Image = Properties.Resources.hourglasslogo
            };
            logoContainer.Controls.Add(picLogo);

            // 4) Title label
            var titleLabel = new Label
            {
                Text = "Previous Lives",
                UseCompatibleTextRendering = true,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            titleLabel.Paint += (s, e) =>
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            titleLabel.Location = new Point(
                logoContainer.Right + 10,
                (headerPanel.Height - titleLabel.PreferredHeight) / 2
            );
            headerPanel.Controls.Add(titleLabel);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // any runtime initialization
        }
    }
}
