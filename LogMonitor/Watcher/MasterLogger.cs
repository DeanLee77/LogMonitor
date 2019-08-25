using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor.Watcher
{
    internal class MasterLogger
    {
        internal string Time;
        internal string Location;
        internal string Level;
        internal string Output;

        public MasterLogger(string time, string location, string level, string output)
        {
            Time = time;
            Location = location;
            Level = level;
            Output = output;
        }


    }
}
