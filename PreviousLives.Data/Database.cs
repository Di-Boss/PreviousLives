// PreviousLives.Data/Database.cs
using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace PreviousLives.Data
{
    public static class Database
    {
        /// <summary>
        /// Ensures the database folder & file exist, applies any needed migrations,
        /// creates tables if needed, and returns a fully–configured connection string.
        /// </summary>
        public static string Initialize(string folder)
        {
            // 1) ensure the folder exists
            Directory.CreateDirectory(folder);

            // 2) build the full path to captures.db
            var dbPath = Path.Combine(folder, "captures.db");
            Console.WriteLine($"SQLite DB path: {dbPath}");

            // 3) build a proper connection string with ReadWriteCreate mode
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };
            var connString = builder.ToString();

            // 4) open & migrate schema
            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();

                // A) Create table if missing (includes Description)
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Captures (
                          Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                          Timestamp   INTEGER NOT NULL,
                          ImageData   BLOB    NOT NULL,
                          Description TEXT    NOT NULL DEFAULT ''
                        );";
                    cmd.ExecuteNonQuery();
                }

                // B) If table existed without Description, add that column
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info(Captures);";
                    bool hasDescription = false;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 'name' is at index 1
                            if (reader.GetString(1).Equals("Description", StringComparison.OrdinalIgnoreCase))
                            {
                                hasDescription = true;
                                break;
                            }
                        }
                    }

                    if (!hasDescription)
                    {
                        using var alter = conn.CreateCommand();
                        alter.CommandText = @"
                            ALTER TABLE Captures
                            ADD COLUMN Description TEXT NOT NULL DEFAULT '';
                        ";
                        alter.ExecuteNonQuery();
                    }
                }
            }

            return connString;
        }
    }
}
