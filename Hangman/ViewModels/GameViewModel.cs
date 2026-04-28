using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class LetterItem : ViewModelBase
    {
        public char Char { get; set; }
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(); }
        }
    }

    public class GameViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        public User CurrentUser { get; }
        private DispatcherTimer _timer;

        public ObservableCollection<LetterItem> Keyboard { get; set; } = new();
        public ObservableCollection<char> GuessedLetters { get; set; } = new();

        private int _level;
        public int Level { get => _level; set { _level = value; OnPropertyChanged(); } }

        private int _mistakes;
        public int Mistakes
        {
            get => _mistakes;
            set
            {
                _mistakes = value;
                OnPropertyChanged();
                UpdateHangmanImage();
            }
        }

        private int _timeLeft;
        public int TimeLeft { get => _timeLeft; set { _timeLeft = value; OnPropertyChanged(); } }

        private string _currentCategory = "All Categories";
        public string CurrentCategory { get => _currentCategory; set { _currentCategory = value; OnPropertyChanged(); } }

        public string WordToGuess { get; set; } = string.Empty;

        public string CurrentWordDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(WordToGuess)) return "";
                var display = WordToGuess.Select(c => GuessedLetters.Contains(c) ? c : '_');
                return string.Join(" ", display);
            }
        }

        private string _hangmanImagePath = string.Empty;
        public string HangmanImagePath { get => _hangmanImagePath; set { _hangmanImagePath = value; OnPropertyChanged(); } }

        private bool _isAboutVisible;
        public bool IsAboutVisible { get => _isAboutVisible; set { _isAboutVisible = value; OnPropertyChanged(); } }

        private bool _isSaveLoadOverlayVisible;
        public bool IsSaveLoadOverlayVisible { get => _isSaveLoadOverlayVisible; set { _isSaveLoadOverlayVisible = value; OnPropertyChanged(); } }

        public string SaveLoadTitle => "Save Game";
        public string ConfirmButtonText => "Save";

        public ObservableCollection<string> AvailableSaves { get; set; } = new();

        private string _selectedSave = string.Empty;
        public string SelectedSave
        {
            get => _selectedSave;
            set
            {
                _selectedSave = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value)) NewSaveName = value;
            }
        }

        private string _newSaveName = string.Empty;
        public string NewSaveName { get => _newSaveName; set { _newSaveName = value; OnPropertyChanged(); } }

        public ICommand GuessCommand => new RelayCommand(GuessLetter);
        public ICommand NewGameCommand => new RelayCommand(_ => StartNewGame());
        public ICommand SaveGameCommand => new RelayCommand(_ => PrepareSaveMenu());
        public ICommand ConfirmSaveLoadCommand => new RelayCommand(_ => ExecuteSave());
        public ICommand CloseSaveLoadOverlayCommand => new RelayCommand(_ => CloseSaveOverlay());
        public ICommand StatisticsCommand => new RelayCommand(_ => GoToStatistics());
        public ICommand CancelCommand => new RelayCommand(_ => GoToLogin());
        public ICommand SetCategoryCommand => new RelayCommand(cat => ChangeCategory(cat?.ToString() ?? "All Categories"));
        public ICommand AboutCommand => new RelayCommand(_ => IsAboutVisible = true);
        public ICommand CloseAboutCommand => new RelayCommand(_ => IsAboutVisible = false);

        public GameViewModel(MainViewModel mainViewModel, User user, GameState? loadedState = null)
        {
            _mainViewModel = mainViewModel;
            CurrentUser = user;

            for (char c = 'A'; c <= 'Z'; c++) Keyboard.Add(new LetterItem { Char = c });

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            if (loadedState != null) LoadFromState(loadedState);
            else StartNewGame();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TimeLeft--;
            if (TimeLeft <= 0)
            {
                _timer.Stop();
                LoseGame();
            }
        }

        private void UpdateHangmanImage()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            HangmanImagePath = Path.Combine(basePath, "Images", $"Hangman_{Mistakes}.png");
        }

        private void StartNewGame()
        {
            Level = 0;
            StartNextWord();
        }

        private void StartNextWord()
        {
            _timer.Stop();
            TimeLeft = 30;
            Mistakes = 0;
            GuessedLetters.Clear();

            var words = CurrentCategory == "All Categories"
                ? DataService.Words.Values.SelectMany(x => x).ToList()
                : DataService.Words[CurrentCategory];

            WordToGuess = words[new Random().Next(words.Count)].ToUpper();

            foreach (var key in Keyboard) key.IsEnabled = true;

            OnPropertyChanged(nameof(CurrentWordDisplay));
            _timer.Start();
        }

        private void LoadFromState(GameState state)
        {
            Level = state.Level;
            Mistakes = state.Mistakes;
            TimeLeft = state.TimeLeft;
            CurrentCategory = state.Category;
            WordToGuess = state.WordToGuess;
            GuessedLetters = new ObservableCollection<char>(state.GuessedLetters);

            foreach (var k in Keyboard) k.IsEnabled = !GuessedLetters.Contains(k.Char);
            OnPropertyChanged(nameof(CurrentWordDisplay));
            UpdateHangmanImage();
            _timer.Start();
        }

        private void GuessLetter(object? parameter)
        {
            if (parameter == null) return;
            char c = parameter.ToString()![0];

            var key = Keyboard.FirstOrDefault(k => k.Char == c);
            if (key == null || !key.IsEnabled) return;

            key.IsEnabled = false;

            if (WordToGuess.Contains(c))
            {
                GuessedLetters.Add(c);
                OnPropertyChanged(nameof(CurrentWordDisplay));

                if (!CurrentWordDisplay.Contains('_'))
                {
                    _timer.Stop();
                    Level++;
                    if (Level >= 3) WinGame();
                    else
                    {
                        MessageBox.Show("Level won! Next word incoming.");
                        StartNextWord();
                    }
                }
            }
            else
            {
                Mistakes++;
                if (Mistakes >= 6)
                {
                    _timer.Stop();
                    LoseGame();
                }
            }
        }

        private void WinGame()
        {
            MessageBox.Show("Congratulations! You won the game!");
            DataService.UpdateStats(CurrentUser.Name, CurrentCategory, true);
            StartNewGame();
        }

        private void LoseGame()
        {
            MessageBox.Show($"Game Over! The word was: {WordToGuess}");
            DataService.UpdateStats(CurrentUser.Name, CurrentCategory, false);
            StartNewGame();
        }

        private void ChangeCategory(string category)
        {
            _timer.Stop();
            CurrentCategory = category;
            StartNewGame();
        }

        private void GoToStatistics()
        {
            AutoSave();
            _timer.Stop();
            _mainViewModel.NavigateTo(new StatisticsViewModel(_mainViewModel, this));
        }

        private void GoToLogin()
        {
            AutoSave();
            _timer.Stop();
            _mainViewModel.NavigateTo(new LoginViewModel(_mainViewModel));
        }

        private void AutoSave()
        {
            var state = CreateState();
            DataService.SaveGameState(state, $"{CurrentUser.Name}_autosave");
        }

        private GameState CreateState()
        {
            return new GameState
            {
                UserName = CurrentUser.Name,
                Level = Level,
                Mistakes = Mistakes,
                TimeLeft = TimeLeft,
                Category = CurrentCategory,
                WordToGuess = WordToGuess,
                GuessedLetters = GuessedLetters.ToList()
            };
        }

        private void PrepareSaveMenu()
        {
            _timer.Stop();
            AvailableSaves.Clear();

            string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (Directory.Exists(saveDir))
            {
                var files = Directory.GetFiles(saveDir, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var state = DataService.LoadGameState(Path.GetFileName(file));
                        if (state != null && state.UserName == CurrentUser.Name)
                        {
                            AvailableSaves.Add(Path.GetFileNameWithoutExtension(file));
                        }
                    }
                    catch {}
                }
            }

            string baseName = $"{CurrentUser.Name}_save";
            string finalName = baseName;
            int i = 1;
            while (AvailableSaves.Contains(finalName))
            {
                finalName = $"{baseName}({i})";
                i++;
            }
            NewSaveName = finalName;
            IsSaveLoadOverlayVisible = true;
        }

        private void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(NewSaveName))
            {
                MessageBox.Show("Please enter a name for your save!");
                return;
            }
            DataService.SaveGameState(CreateState(), NewSaveName);
            MessageBox.Show("Game saved successfully!");
            IsSaveLoadOverlayVisible = false;
            _timer.Start();
        }

        private void CloseSaveOverlay()
        {
            IsSaveLoadOverlayVisible = false;
            _timer.Start();
        }
    }
}