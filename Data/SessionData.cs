using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YATT;

namespace YATT.Data
{
    [Serializable]
    public class SessionData
    {
        [JsonInclude]
        public List<TorrentSessionItem> Torrents { get; set; } = new List<TorrentSessionItem>();
    }
}