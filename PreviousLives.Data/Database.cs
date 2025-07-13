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

                // A) Create table if missing (includes Description & EditedImage)
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Captures (
                          Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                          Timestamp    INTEGER NOT NULL,
                          ImageData    BLOB    NOT NULL,
                          Description  TEXT    NOT NULL DEFAULT '',
                          EditedImage  BLOB    NOT NULL
                        );";
                    cmd.ExecuteNonQuery();
                }

                // B) Migrate existing table: add Description & EditedImage if missing
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info(Captures);";
                    bool hasDescription = false, hasEditedImage = false;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader.GetString(1);
                            if (name.Equals("Description", StringComparison.OrdinalIgnoreCase))
                                hasDescription = true;
                            if (name.Equals("EditedImage", StringComparison.OrdinalIgnoreCase))
                                hasEditedImage = true;
                        }
                    }

                    if (!hasDescription)
                    {
                        using var alterDesc = conn.CreateCommand();
                        alterDesc.CommandText = @"
                            ALTER TABLE Captures
                            ADD COLUMN Description TEXT NOT NULL DEFAULT '';
                        ";
                        alterDesc.ExecuteNonQuery();
                    }

                    if (!hasEditedImage)
                    {
                        using var alterEdit = conn.CreateCommand();
                        alterEdit.CommandText = @"
                            ALTER TABLE Captures
                            ADD COLUMN EditedImage BLOB NOT NULL DEFAULT x'';
                        ";
                        alterEdit.ExecuteNonQuery();
                    }
                }
            }

            return connString;
        }
    }
}



            return connString;
        }
    }
}
