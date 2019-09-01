using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogMonitor.Watcher
{
    public class MasterLogger
    {
        public string Time;
        public string Location;
        public string Level;
        public string Output;

        public MasterLogger(string time, string location, string level, string output)
        {
            Time = time.Trim();
            Location = location.Trim();
            Level = level != null? Regex.Replace(level.Trim(), @"[\[\]]", ""): "";
            Output = output.Trim();
        }


    }
}
