using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using MonoTorrent;
using YATT.Data;
using YATT.Enums;

namespace YATT.Services
{
    public class SettingsService
    {
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsFilePath;

        private Settings _settings;

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);
            LoadSettings();
        }

        public void UpdateTray(App app)
        {
            NativeMenuItem profilesTray = (NativeMenuItem)TrayIcon.GetIcons(app).First().Menu.Items.FirstOrDefault();
            profilesTray.Menu.Items.Clear();

            foreach (var item in _settings.SpeedProfiles)
            {
                var menuItem = new NativeMenuItem
                {
                    Header = item.ProfileName,
                    ToggleType = NativeMenuItemToggleType.Radio,
                    IsChecked = item.Active,
                };
                menuItem.Click += (sender, args) => ActivateSpeedProfile(item.ProfileName);
                profilesTray.Menu.Items.Add(menuItem);
            }
        }

        // Get the current settings
        public Settings GetSettings()
        {
            return _settings;
        }

        // Update settings and save them to file
        public void UpdateSettings(Settings newSettings)
        {
            _settings = newSettings;
            SaveSettings();
        }

        public async Task ActivateSpeedProfile(string? header)
        {
            if(header == null)
            {
                header = _settings.SpeedProfiles.FirstOrDefault(x => x.Active).ProfileName;
            }

            var profile = _settings.SpeedProfiles.FirstOrDefault(x => x.ProfileName == header);
            // disable other profiles
            _settings.SpeedProfiles.ToList().ForEach(p => p.Active = false);
            // enable the selected one
            profile.Active = true;
        
            SaveSettings();
            var speedInKb = 0;
            switch (profile.UnitType)
            {
                case SpeedUnitType.Kb:
                    speedInKb = profile.Speed;
                    break;
                case SpeedUnitType.Mb:
                    speedInKb = profile.Speed * 1024;
                    break;
                case SpeedUnitType.Gb:
                    speedInKb = profile.Speed * 1024 * 1024;
                    break;
            }
            TorrentManagerService torrentManager = App.GetService<TorrentManagerService>();
            await torrentManager.SetSpeed(speedInKb);
        }

        // Load settings from the file, or create new default if not found
        private async Task LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
                else
                {
                    _settings = new Settings();
                    SpeedProfileEntry unlimited = new SpeedProfileEntry { Active = true, ProfileName = "Unlimited", Speed = 0, UnitType = SpeedUnitType.Kb };
                    _settings.SpeedProfiles.Add(unlimited);
                }
                await ActivateSpeedProfile(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                _settings = new Settings(); // Default settings if loading fails
            }
        }

        // Save the current settings to a file
        public void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
                UpdateTray((App)App.Current);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
}