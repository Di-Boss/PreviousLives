// PreviousLives.Data/Database.cs
using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace PreviousLives.Data
{
    public static class Database
    {
        /// <summary>
        /// Ensures the database folder & file exist, creates tables if needed,
        /// and returns a fully–configured connection string.
        /// </summary>
        public static string Initialize(string folder)
        {
            // 1) make sure the folder exists
            Directory.CreateDirectory(folder);

            // 2) build the full path to captures.db
            var dbPath = Path.Combine(folder, "captures.db");

            // (for debugging) print/log the path so you can confirm it
            Console.WriteLine($"SQLite DB path: {dbPath}");

            // 3) build a proper connection string with ReadWriteCreate mode
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };
            var connString = builder.ToString();

            // 4) open & create the table if missing
            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Captures (
                          Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                          Timestamp  INTEGER NOT NULL,
                          ImageData  BLOB    NOT NULL
                        );";
                    cmd.ExecuteNonQuery();
                }
            }

            return connString;
        }
    }
}
