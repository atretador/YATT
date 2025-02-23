using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonoTorrent.Client;

namespace YATT.Data
{
    [Serializable]
    public class TorrentSessionItem
    {
        public string TorrentFile { get; set; }
        public string SavePath { get; set; }
        public TorrentState State { get; set; }
    }
}