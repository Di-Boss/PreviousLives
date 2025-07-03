using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PreviousLives
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.BackColor = ColorTranslator.FromHtml("#121212");

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,  // match your v0 spec
                BackColor = ColorTranslator.FromHtml("#121212"),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(headerPanel);

            // 2) Add the logo PictureBox
            var picLogo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Image = Properties.Resources.hourglasslogo  // your resource name
            };

            // Now set Size and Location in normal statements:
            // After creating picLogo and before adding to headerPanel:
            picLogo.Size = new Size(32, 32);
            picLogo.Location = new Point(20, (headerPanel.Height - picLogo.Height) / 2);


            // Finally add to the header panel:
            headerPanel.Controls.Add(picLogo);

            // 4) Add the title label
            var titleLabel = new Label
            {
                Text = "Previous Lives",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // position it 10px to the right of the logo
            titleLabel.Location = new Point(
                picLogo.Right + 10,
                (headerPanel.Height - titleLabel.PreferredHeight) / 2
            );

            headerPanel.Controls.Add(titleLabel);



        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
