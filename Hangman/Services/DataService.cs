using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Hangman.Models;

namespace Hangman.Services
{
    public static class DataService
    {
        private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string DataPath = Path.Combine(BaseDir, "Data");
        private static readonly string UsersFile = Path.Combine(DataPath, "users.json");
        private static readonly string StatsFile = Path.Combine(DataPath, "stats.json");

        public static readonly Dictionary<string, List<string>> Words = new()
        {
            { "Cars", new List<string> { "AUDI", "BMW", "MERCEDES", "TOYOTA", "HONDA" } },
            { "Movies", new List<string> { "INCEPTION", "TITANIC", "AVATAR", "GLADIATOR" } },
            { "States", new List<string> { "TEXAS", "CALIFORNIA", "FLORIDA", "ALASKA" } },
            { "Mountains", new List<string> { "EVEREST", "CARPATHIANS", "ALPS", "ANDES" } },
            { "Rivers", new List<string> { "AMAZON", "NILE", "DANUBE", "MISSISSIPPI" } }
        };

        static DataService()
        {
            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
        }

        public static List<User> LoadUsers()
        {
            if (!File.Exists(UsersFile)) return new List<User>();
            try { return JsonSerializer.Deserialize<List<User>>(File.ReadAllText(UsersFile)) ?? new List<User>(); }
            catch { return new List<User>(); }
        }

        public static void SaveUsers(List<User> users)
        {
            File.WriteAllText(UsersFile, JsonSerializer.Serialize(users));
        }

        public static List<UserStats> LoadStats()
        {
            if (!File.Exists(StatsFile)) return new List<UserStats>();
            try { return JsonSerializer.Deserialize<List<UserStats>>(File.ReadAllText(StatsFile)) ?? new List<UserStats>(); }
            catch { return new List<UserStats>(); }
        }

        public static void SaveStats(List<UserStats> stats)
        {
            File.WriteAllText(StatsFile, JsonSerializer.Serialize(stats));
        }

        public static void UpdateStats(string userName, string category, bool won)
        {
            var stats = LoadStats();
            var userStat = stats.FirstOrDefault(s => s.UserName == userName);
            if (userStat == null)
            {
                userStat = new UserStats { UserName = userName };
                stats.Add(userStat);
            }

            string cat = (category == "All Categories") ? "Mixed" : category;

            if (!userStat.GamesPlayed.ContainsKey(cat)) userStat.GamesPlayed[cat] = 0;
            userStat.GamesPlayed[cat]++;

            if (won)
            {
                if (!userStat.GamesWon.ContainsKey(cat)) userStat.GamesWon[cat] = 0;
                userStat.GamesWon[cat]++;
            }

            SaveStats(stats);
        }

        public static void SaveGameState(GameState state, string fileName)
        {
            if (!fileName.EndsWith(".json")) fileName += ".json";
            string filePath = Path.Combine(DataPath, fileName);
            File.WriteAllText(filePath, JsonSerializer.Serialize(state));
        }

        public static GameState? LoadGameState(string fileName)
        {
            if (!fileName.EndsWith(".json")) fileName += ".json";
            string filePath = Path.Combine(DataPath, fileName);
            if (!File.Exists(filePath)) return null;

            try { return JsonSerializer.Deserialize<GameState>(File.ReadAllText(filePath)); }
            catch { return null; }
        }

        public static List<string> GetSaveFilesForUser(string userName)
        {
            if (!Directory.Exists(DataPath)) return new List<string>();
            return Directory.GetFiles(DataPath, $"*{userName}*.json")
                            .Select(Path.GetFileNameWithoutExtension)
                            .Cast<string>()
                            .ToList();
        }
    }
}