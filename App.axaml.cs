using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
using YATT.Data;
using YATT.Services;

namespace YATT
{
    public partial class App : Application
    {        
        private IServiceProvider _serviceProvider;
        private SettingsService _settingsService;
        private Window _mainWindow;
        public ICommand ExitCommand { get; }
        public ICommand ShowCommand { get; }

        public App()
        {
            ExitCommand = new RelayCommand(TrayExit);
            ShowCommand = new RelayCommand(TrayShow);
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                _serviceProvider = services.BuildServiceProvider();

                // Create MainWindow and inject dependencies
                _mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                desktop.MainWindow = _mainWindow;

                _settingsService = _serviceProvider.GetRequiredService<SettingsService>();
                _settingsService.UpdateTray(this);

                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                desktop.Exit += OnExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<TorrentManagerService>();
            services.AddSingleton<SettingsService>();

            // Register MainWindow and other windows
            services.AddTransient<MainWindow>();
            services.AddTransient<SettingsWindow>();
            services.AddTransient<AddTorrentDialog>();
        }

        public static T GetService<T>() where T : class
        {
            return ((App)Current)._serviceProvider.GetRequiredService<T>();
        }

        private void TrayShow()
        {
            if (_mainWindow != null)
            {
                // If the window is minimized or not visible, restore it
                if (!_mainWindow.IsVisible || _mainWindow.WindowState == WindowState.Minimized)
                {
                    _mainWindow.Show();
                    _mainWindow.WindowState = WindowState.Normal;
                }
                // Bring the window to the foreground
                _mainWindow.Activate();
            }
        }

        private async void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            var torrentManager = _serviceProvider.GetRequiredService<TorrentManagerService>();
            await torrentManager.SaveAllTorrentsStateAsync();
            await ((MainWindow)_mainWindow).SaveSessionAsync();
        }

        public void TrayExit()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}