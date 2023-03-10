using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.ViewModel
{
    internal class AppInfo
    {
        public string AppName { get; set; }
        public List<AppVersion> Versions { get; set; }
    }

    public class AppVersion
    {
        public Version Version { get; set; }
        public string Link { get; set; }
    }
}
