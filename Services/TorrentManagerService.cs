using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YATT.Services
{
    public class TorrentManagerService
    {
        private ClientEngine engine;
        public Dictionary<string, TorrentManager> activeTorrents;
        private string fastResumeDirectory = "fast_resume";
        private int currentDownloadSpeedLimit = 0; // 0 means unlimited

        public TorrentManagerService()
        {
            activeTorrents = new Dictionary<string, TorrentManager>();
            Directory.CreateDirectory(fastResumeDirectory); // Ensure fast resume directory exists
            InitializeEngine(0);
        }

        private async Task InitializeEngine(int maxDownloadSpeed)
        {
            var settings = new EngineSettingsBuilder
            {
                MaximumDownloadRate = maxDownloadSpeed * 1024 // Convert KB/s to bytes/s
            }.ToSettings();
            engine = new ClientEngine(settings);

            foreach (var torrent in activeTorrents)
            {
                await torrent.Value.Engine.UpdateSettingsAsync(settings);
            }
        }

        public async Task SetSpeed(int kbSpeedLimit)
        {
            if (kbSpeedLimit == currentDownloadSpeedLimit)
                return;
            
            currentDownloadSpeedLimit = kbSpeedLimit;
            InitializeEngine(kbSpeedLimit);
        }

        public async Task<TorrentManager> StartTorrentAsync(string torrentFile, string savePath, bool startOnAdd)
        {
            var torrent = Torrent.Load(torrentFile);

            if (activeTorrents.ContainsKey(torrent.Name))
            {
                return activeTorrents[torrent.Name];
            }

            var fastResumePath = GetFastResumeFilePath(torrent.Name);
            TorrentSettings settings = new TorrentSettings();
            TorrentManager manager;

            if (File.Exists(fastResumePath))
            {
                manager = await engine.AddAsync(torrent, savePath, settings);
                using (var stream = File.OpenRead(fastResumePath))
                {
                    if (FastResume.TryLoad(stream, out var output))
                    {
                        await manager.LoadFastResumeAsync(output);
                    }
                }
            }
            else
            {
                manager = await engine.AddAsync(torrent, savePath, settings);
            }

            activeTorrents[torrent.Name] = manager;

            await manager.HashCheckAsync(startOnAdd);
            return manager;
        }

        public async Task ResumeTorrentAsync(string torrentName)
        {
            if (activeTorrents.ContainsKey(torrentName))
            {
                var manager = activeTorrents[torrentName];
                if (manager.State != TorrentState.Downloading)
                {
                    await manager.StartAsync();
                }
            }
        }

        public async Task PauseTorrentAsync(string torrentName)
        {
            if (activeTorrents.ContainsKey(torrentName))
            {
                var manager = activeTorrents[torrentName];
                if (manager.State == TorrentState.Downloading || manager.State == TorrentState.Seeding)
                {
                    await manager.PauseAsync();
                }
            }
        }

        public async Task DeleteTorrentAsync(string torrentName, bool deleteFiles = false)
        {
            if (activeTorrents.ContainsKey(torrentName))
            {
                var manager = activeTorrents[torrentName];
                if(manager.State == TorrentState.Stopping)
                    return;
                    
                await manager.StopAsync();
                await engine.RemoveAsync(manager);
                
                File.Delete(GetFastResumeFilePath(torrentName));
                activeTorrents.Remove(torrentName);
            }
        }

        public float GetProgress(string torrentName)
        {
            return (float)(activeTorrents.ContainsKey(torrentName) ? activeTorrents[torrentName].Progress : 0f);
        }

        public async Task SaveAllTorrentsStateAsync()
        {
            foreach (var kvp in activeTorrents)
            {
                var manager = kvp.Value;
                var fastResume = await manager.SaveFastResumeAsync();
                File.WriteAllBytes(GetFastResumeFilePath(kvp.Key), fastResume.Encode());
            }
        }

        private string GetFastResumeFilePath(string torrentFile)
        {
            return Path.Combine(fastResumeDirectory, Path.GetFileNameWithoutExtension(torrentFile) + ".resume");
        }
    }
}