using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YATT.Data;

namespace YATT.Data
{
    public class Settings
    {
        public string DefaultSaveLocation { get; set; }
        public List<SpeedProfileEntry> SpeedProfiles { get; set; } = new List<SpeedProfileEntry>();
    }
}