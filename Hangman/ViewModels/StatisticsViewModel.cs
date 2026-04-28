using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Hangman.Models;
using Hangman.Services;

namespace Hangman.ViewModels
{
    public class StatisticsViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private readonly ViewModelBase _previousViewModel;

        public ObservableCollection<StatDisplayItem> StatsList { get; set; } = new();

        public ICommand BackCommand => new RelayCommand(_ => _mainViewModel.NavigateTo(_previousViewModel));

        public StatisticsViewModel(MainViewModel mainViewModel, ViewModelBase previousViewModel)
        {
            _mainViewModel = mainViewModel;
            _previousViewModel = previousViewModel;
            LoadData();
        }

        private void LoadData()
        {
            var stats = DataService.LoadStats();
            StatsList.Clear();
            foreach (var s in stats)
            {
                var categories = s.GamesPlayed.Keys.Union(s.GamesWon.Keys).Distinct();
                foreach (var c in categories)
                {
                    StatsList.Add(new StatDisplayItem
                    {
                        UserName = s.UserName,
                        Category = c,
                        Played = s.GamesPlayed.ContainsKey(c) ? s.GamesPlayed[c] : 0,
                        Won = s.GamesWon.ContainsKey(c) ? s.GamesWon[c] : 0
                    });
                }
            }
        }
    }
}