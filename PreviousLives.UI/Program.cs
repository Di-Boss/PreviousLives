using System;
using System.IO;
using System.Windows.Forms;
using SQLitePCL;            // for Batteries_V2
using PreviousLives.Data;

namespace PreviousLives.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable WinForms visual styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1) Initialize the SQLite native library from bundle_green
            Batteries_V2.Init();

            // 2) Pick a folder under %LocalAppData% for your .db
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbFolder = Path.Combine(appData, "PreviousLives");
            Directory.CreateDirectory(dbFolder);

            // 3) Initialize (or create) the SQLite database and tables
            //    Returns a full connection string you can hand off to Form1
            var connectionString = Database.Initialize(dbFolder);

            // 4) Launch the main form, passing in your connection string
            Application.Run(new Form1(connectionString));
        }
    }
}
