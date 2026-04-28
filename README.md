# Hangman Game (WPF) 🎮

A desktop-based implementation of the classic **Hangman** game, built using **C#** and **Windows Presentation Foundation (WPF)**. This application features a graphical user interface with user management, statistical tracking, and game-saving capabilities.

## ✨ Features
* **Graphical User Interface**: A clean desktop experience built with XAML, featuring a dedicated login screen, game board, and statistics dashboard.
* **User Management**: Support for creating multiple user profiles with custom names and avatars.
* **Game Categories**: Players can choose between several categories, including Cars, Movies, States, Mountains, and Rivers.
* **Save & Load System**: Manual and automatic saving of game progress, allowing users to resume their session at any time.
* **Detailed Statistics**: Tracks games played and games won per user across different categories.
* **Visual Progress**: The hangman drawing updates dynamically through seven distinct visual stages (0-6) as incorrect guesses are made.

## 🛠️ Technologies Used
* **Language**: C#
* **Framework**: .NET 8.0-Windows
* **UI Framework**: WPF (Windows Presentation Foundation)
* **Pattern**: MVVM (Model-View-ViewModel)
* **Data Storage**: JSON-based persistence for users, stats, and game states.

## 🚀 Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Visual Studio 2022 (with "Desktop development with .NET" workload)

### Installation & Run
1.  **Clone the repository**:
    ```bash
    git clone [https://github.com/MihaiOatu10/Hangman-Game.git](https://github.com/MihaiOatu10/Hangman-Game.git)
    ```
2.  **Open the project**: Open `Hangman Game.sln` in Visual Studio.
3.  **Run the application**: Press `F5` or click **Start** in Visual Studio.

## 🕹️ How to Play
1.  **Login**: Select an existing user or create a new one with a custom image.
2.  **Start Game**: Click "New Game" to begin a session.
3.  **Guessing**: Use the on-screen buttons or your physical keyboard (A-Z) to guess letters.
4.  **Win/Loss**: 
    * **Win**: Successfully complete the word to progress.
    * **Loss**: Making six mistakes or running out of the 30-second time limit per word results in a loss.

## 📁 Project Structure
* **Views**: XAML files defining the UI for Login, Game, and Statistics screens.
* **ViewModels**: Implements game logic and data binding.
* **Models**: Defines data structures for `User`, `GameState`, and `UserStats`.
* **Services**: `DataService.cs` handles word lists and JSON file operations.
* **Images**: Visual assets for the hangman stages.
