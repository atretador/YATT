using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using YATT.Data;
using YATT.Enums;
using YATT.Services;

namespace YATT
{
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private readonly SettingsService _settingsService;
        
        // Temporary save location bound to the TextBox.
        private string _tempDefaultSaveLocation;
        public string TempDefaultSaveLocation
        {
            get => _tempDefaultSaveLocation;
            set
            {
                if (_tempDefaultSaveLocation != value)
                {
                    _tempDefaultSaveLocation = value;
                    OnPropertyChanged();
                }
            }
        }
        public Array SpeedUnitTypes => Enum.GetValues(typeof(SpeedUnitType));

        private ObservableCollection<SpeedProfileEntry> _speedProfiles;
        public ObservableCollection<SpeedProfileEntry> SpeedProfiles
        {
            get => _speedProfiles;
            set
            {
                if (_speedProfiles != value)
                {
                    _speedProfiles = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public SettingsWindow(SettingsService settingsService)
        {
            _settingsService = settingsService;
            DataContext = this; // Set the DataContext to this window instance.

            InitializeComponent();
            LoadSettings();
        }
        
        private void LoadSettings()
        {
            var settings = _settingsService.GetSettings();
            TempDefaultSaveLocation = settings.DefaultSaveLocation;
            // Ensure we have an observable collection (empty if none).
            SpeedProfiles = new ObservableCollection<SpeedProfileEntry>(settings.SpeedProfiles ?? new List<SpeedProfileEntry>());
        }
        
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Save Location",
                AllowMultiple = false
            });
            if (folderDialog.Count > 0)
            {
                TempDefaultSaveLocation = folderDialog[0].Path.LocalPath;
            }
        }
        
        // Event handler for the "Add Profile" button.
        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Add a new profile with default values.
            SpeedProfiles.Add(new SpeedProfileEntry { ProfileName = "", Speed = 0, UnitType = SpeedUnitType.Mb });
        }
        
        // Remove button click handler for each profile entry.
        private void RemoveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is SpeedProfileEntry profile)
            {
                SpeedProfiles.Remove(profile);
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = _settingsService.GetSettings();
            settings.DefaultSaveLocation = TempDefaultSaveLocation;
            settings.SpeedProfiles = new List<SpeedProfileEntry>(SpeedProfiles);
            _settingsService.SaveSettings();
            Close();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
