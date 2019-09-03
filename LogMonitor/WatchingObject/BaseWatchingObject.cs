using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor.WatchingObject
{
    public class BaseWatchingObject
    {
        protected string _watchType;
        protected int _watchId;
        public BaseWatchingObject(string watchType, int watchId)
        {
            _watchType = watchType;
            _watchId = watchId;
        }


        public string GetWatchType()
        {
            return _watchType;
        }
        public int GetWatchId()
        {
            return _watchId;
        }

    }
}
