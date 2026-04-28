using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Hangman.Models;
using Hangman.Services;
using System.Text.Json;

namespace Hangman.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        public ObservableCollection<User> Users { get; set; }

        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                IsLoadOverlayVisible = false;
            }
        }

        private bool _isLoadOverlayVisible = false;
        public bool IsLoadOverlayVisible
        {
            get => _isLoadOverlayVisible;
            set { _isLoadOverlayVisible = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableSaves { get; set; } = new();

        private string _selectedSave = string.Empty;
        public string SelectedSave
        {
            get => _selectedSave;
            set { _selectedSave = value; OnPropertyChanged(); }
        }

        private bool _isCreating = false;
        public bool IsCreatingNewUser
        {
            get => _isCreating;
            set { _isCreating = value; OnPropertyChanged(); }
        }

        private string _newUserName = "";
        public string NewUserName
        {
            get => _newUserName;
            set { _newUserName = value; OnPropertyChanged(); }
        }

        private string _newUserImagePath = "";
        public string NewUserImagePath
        {
            get => _newUserImagePath;
            set { _newUserImagePath = value; OnPropertyChanged(); }
        }

        public ICommand ShowNewUserCommand => new RelayCommand(_ => IsCreatingNewUser = true);
        public ICommand HideNewUserCommand => new RelayCommand(_ => IsCreatingNewUser = false);

        public ICommand PickImageCommand => new RelayCommand(_ => {
            var ofd = new OpenFileDialog { Filter = "Images|*.jpg;*.png;*.gif" };
            if (ofd.ShowDialog() == true) { NewUserImagePath = ofd.FileName; }
        });

        public ICommand SaveNewUserCommand => new RelayCommand(_ => {
            if (string.IsNullOrWhiteSpace(NewUserName)) return;

            if (Users.Any(u => u.Name.Equals(NewUserName.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("User already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newUser = new User { Name = NewUserName.Trim(), ImagePath = NewUserImagePath };
            Users.Add(newUser);
            DataService.SaveUsers(Users.ToList());
            IsCreatingNewUser = false;
            NewUserName = "";
            NewUserImagePath = "";
        });

        public ICommand DeleteUserCommand => new RelayCommand(_ => {
            if (SelectedUser == null) return;
            Users.Remove(SelectedUser);
            DataService.SaveUsers(Users.ToList());
            SelectedUser = null;
        }, _ => SelectedUser != null);

        public ICommand PlayCommand => new RelayCommand(_ => {
            if (SelectedUser != null)
                _mainViewModel.NavigateTo(new GameViewModel(_mainViewModel, SelectedUser));
        }, _ => SelectedUser != null);

        public ICommand ShowLoadOverlayCommand => new RelayCommand(_ => {
            if (SelectedUser == null)
            {
                MessageBox.Show("Please select a user from the list first!");
                return;
            }
            LoadAvailableSaves();
            IsLoadOverlayVisible = true;
        }, _ => SelectedUser != null);

        public ICommand HideLoadOverlayCommand => new RelayCommand(_ => IsLoadOverlayVisible = false);

        public ICommand ConfirmLoadCommand => new RelayCommand(_ => ExecuteLoad());

        public ICommand CancelCommand => new RelayCommand(_ => Application.Current.Shutdown());

        private void LoadAvailableSaves()
        {
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
                        if (state != null && state.UserName == SelectedUser!.Name)
                        {
                            AvailableSaves.Add(Path.GetFileNameWithoutExtension(file));
                        }
                    }
                    catch { }
                }
            }
        }

        private void ExecuteLoad()
        {
            if (string.IsNullOrEmpty(SelectedSave))
            {
                MessageBox.Show("Please select a save from the list!");
                return;
            }

            string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            string filePath = Path.Combine(saveDir, SelectedSave + ".json");

            if (File.Exists(filePath))
            {
                try
                {
                    var state = DataService.LoadGameState(SelectedSave + ".json");
                    if (state != null && state.UserName == SelectedUser!.Name)
                    {
                        IsLoadOverlayVisible = false;
                        _mainViewModel.NavigateTo(new GameViewModel(_mainViewModel, SelectedUser, state));
                    }
                    else
                    {
                        MessageBox.Show("This save does not belong to the selected user!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("Error loading the save file.");
                }
            }
        }

        public LoginViewModel(MainViewModel main)
        {
            _mainViewModel = main;
            Users = new ObservableCollection<User>(DataService.LoadUsers());
        }
    }
}