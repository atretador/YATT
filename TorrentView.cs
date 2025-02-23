using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using MonoTorrent.Client;

namespace YATT
{
    public class TorrentView : INotifyPropertyChanged
    {
        private readonly TorrentManager _torrentManager;
        private double _downloadSpeed;
        private double _uploadSpeed;
        public string FileName { get; set; }
        public string Name => _torrentManager.Torrent.Name;
        public string SavePath => _torrentManager.SavePath;
        public double Progress => _torrentManager.Complete ? Math.Round(_torrentManager.Progress) : 100;  // Convert to percentage

        public double DownloadSpeed
        {
            get => _downloadSpeed;
            set
            {
                _downloadSpeed = value;
                OnPropertyChanged();
            }
        }

        public double UploadSpeed
        {
            get => _uploadSpeed;
            set
            {
                _uploadSpeed = value;
                OnPropertyChanged();
            }
        }

        public TorrentState Status => _torrentManager.State;

        public event PropertyChangedEventHandler PropertyChanged;

        public TorrentView(TorrentManager manager)
        {
            _torrentManager = manager;
            _torrentManager.TorrentStateChanged += (_, __) => OnPropertyChanged(nameof(Status));
        }

        public void UpdateProgress() => OnPropertyChanged(nameof(Progress));

        public void UpdateSpeeds()
        {
            // Update the download/upload speeds safely on the UI thread
            Dispatcher.UIThread.Post(() =>
            {
                DownloadSpeed = _torrentManager.Monitor.DownloadRate / 1024;  // Convert bytes to kilobytes
                UploadSpeed = _torrentManager.Monitor.UploadRate / 1024;      // Convert bytes to kilobytes
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}