using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using YATT.Services;
using YATT.Data;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using MonoTorrent.Client;
using MonoTorrent;

namespace YATT
{
    public partial class MainWindow : Window
    {
        private readonly TorrentManagerService _torrentManager;
        
        private DispatcherTimer _updateTimer;
        
        public ObservableCollection<TorrentView> Torrents { get; } = new();
        // Collections for each tab

        public MainWindow(TorrentManagerService torrentManagerService)
        {
            InitializeComponent();
            _torrentManager = torrentManagerService;
            DataContext = this; 
            
            // Initialize timer to update progress every second
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Update every 1 second
            };
            _updateTimer.Tick += (sender, args) => UpdateTorrentProgress();
            _updateTimer.Start();

            LoadSessionAsync();

            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Prevent the window from closing
            this.Hide(); // Hide the window
        }

        private async void AddTorrentButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a Torrent File",
                Filters = new List<FileDialogFilter>
                { 
                    new FileDialogFilter { Name = "Torrent Files", Extensions = new List<string> { "torrent" } }
                }
            };

            var result = await dialog.ShowAsync(this);
            if (result != null && result.Length > 0)
            {
                string torrentFilePath = result[0];

                // Open a dialog to choose a save directory
                var addTorrentDialog = new AddTorrentDialog();
                var saveDirectory = await addTorrentDialog.ShowDialog<string>(this);

                if (!string.IsNullOrWhiteSpace(saveDirectory))
                {
                    // Start the torrent using the service
                    var torrentManager = await _torrentManager.StartTorrentAsync(torrentFilePath, saveDirectory);

                    // Create a new TorrentView instance
                    var torrentView = new TorrentView(torrentManager);
                    torrentView.FileName = torrentFilePath;

                    // Add to the correct collection (Torrents)
                    Torrents.Add(torrentView);
                }
            }
        }

        private async void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TorrentView torrent)
            {
                await _torrentManager.ResumeTorrentAsync(torrent.Name);
            }
        }

        private async void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TorrentView torrent)
            {
                await _torrentManager.PauseTorrentAsync(torrent.Name);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TorrentView torrent)
            {
                if (Torrents.Contains(torrent))
                    Torrents.Remove(torrent);

                await SaveSessionAsync();
                await _torrentManager.DeleteTorrentAsync(torrent.Name, true);
            }
        }

        private async Task UpdateTorrentProgress()
        {
            //-- insert your bitcoin miner here --\\

            foreach (var torrent in Torrents)
            {
                torrent.UpdateProgress();  // Update progress for each torrent
                torrent.UpdateSpeeds();    // Update download/upload speed for each torrent
            }
            await _torrentManager.SaveAllTorrentsStateAsync();
            await SaveSessionAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Get SettingsWindow from DI container (it will resolve SettingsViewModel automatically)
            var settingsWindow = App.GetService<SettingsWindow>();
            settingsWindow.ShowDialog(this);
        }

        // session
        public async Task SaveSessionAsync()
        {
            // Save fast resume data for all active torrents.
            await _torrentManager.SaveAllTorrentsStateAsync();

            // Build the session model from the active torrents.
            var session = new SessionData();
            foreach (var kvp in Torrents) // Expose activeTorrents as a property if needed.
            {
                session.Torrents.Add(new TorrentSessionItem 
                { 
                    TorrentFile = kvp.FileName, 
                    SavePath = kvp.SavePath 
                });
            }
            
            // Serialize to JSON and write to disk.
            string json = System.Text.Json.JsonSerializer.Serialize(session);
            File.WriteAllText("session.json", json);
        }

        private async Task LoadSessionAsync()
        {
            if (File.Exists("session.json"))
            {
                string json = File.ReadAllText("session.json");
                var session = System.Text.Json.JsonSerializer.Deserialize<SessionData>(json);
                foreach (var item in session.Torrents)
                {
                    var torrentManager = await _torrentManager.StartTorrentAsync(item.TorrentFile, item.SavePath);
                    var torrentView = new TorrentView(torrentManager);
                    torrentView.FileName = item.TorrentFile;
                    Torrents.Add(torrentView);
                }
            }
        }

    }
}