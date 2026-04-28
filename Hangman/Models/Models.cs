using System;
using System.Collections.Generic;

namespace Hangman.Models
{
    public class User
    {
        public string Name { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }

    public class GameState
    {
        public string UserName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Mistakes { get; set; }
        public int TimeLeft { get; set; }
        public string Category { get; set; } = string.Empty;
        public string WordToGuess { get; set; } = string.Empty;
        public List<char> GuessedLetters { get; set; } = new List<char>();
    }

    public class UserStats
    {
        public string UserName { get; set; } = string.Empty;
        public Dictionary<string, int> GamesPlayed { get; set; } = new();
        public Dictionary<string, int> GamesWon { get; set; } = new();
    }

    public class StatDisplayItem
    {
        public string UserName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Played { get; set; }
        public int Won { get; set; }
    }
}