using System;
using System.Data.SQLite;
using System.IO;

class Program
{
    static void Main()
    {
        string dbPath = @"e:\wii\code\AuroraDbManager_wei\AuroraDbManager\DateExample\settings.db";
        
        if (!File.Exists(dbPath))
        {
            Console.WriteLine("Database file not found: " + dbPath);
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                connection.Open();
                
                // Get all tables
                Console.WriteLine("=== Tables in settings.db ===");
                using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine("Table: " + reader.GetString(0));
                    }
                }
                
                // Check each table for data
                string[] tables = {"SystemSettings", "UserSettings", "ScanPaths", "Profiles", "ActiveTUs", "QuickViews", "Trainers", "UserFavorites", "UserHidden", "UserRecent"};
                
                foreach (var table in tables)
                {
                    Console.WriteLine($"\n=== {table} ===");
                    try
                    {
                        using (var cmd = new SQLiteCommand($"SELECT COUNT(*) FROM {table}", connection))
                        {
                            int count = Convert.ToInt32(cmd.ExecuteScalar());
                            Console.WriteLine($"Row count: {count}");
                            
                            if (count > 0)
                            {
                                using (var cmd2 = new SQLiteCommand($"SELECT * FROM {table} LIMIT 3", connection))
                                using (var reader = cmd2.ExecuteReader())
                                {
                                    Console.WriteLine("Sample data:");
                                    while (reader.Read())
                                    {
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            Console.Write($"{reader.GetName(i)}: {reader.GetValue(i)} | ");
                                        }
                                        Console.WriteLine();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading {table}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}