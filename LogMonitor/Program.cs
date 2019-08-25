using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogMonitor.Watcher;
using LogMonitor.WatchingObject;

namespace LogMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Watcher.Watcher.Monitoring();
        }
    }
}
