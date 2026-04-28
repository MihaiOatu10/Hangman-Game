using System.Windows;
using Hangman.Views;
using Hangman.ViewModels;

namespace Hangman
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow window = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            window.Show();
        }
    }
}