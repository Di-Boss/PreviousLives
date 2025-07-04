using System;
using System.Windows.Forms;

namespace PreviousLives.UI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles and text rendering
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run your custom form (make sure MainForm inherits from Form
            // and contains all your SetupUI() logic, no InitializeComponent())
            Application.Run(new Form1());
        }
    }
}
