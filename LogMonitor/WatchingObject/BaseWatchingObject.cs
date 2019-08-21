using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor.WatchingObject
{
    public class BaseWatchingObject
    {
        private string _watchType;
        public BaseWatchingObject(string watchType)
        {
            _watchType = watchType;
        }


        public string GetWatchType()
        {
            return _watchType;
        }

    }
}
