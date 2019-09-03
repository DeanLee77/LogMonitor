using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor.WatchingObject
{
    public class WatchingDatabaseObject : BaseWatchingObject
    {
        private string _dataSource;
        private string _databaseName;
        private string _userId;

        public WatchingDatabaseObject(string watchType, int watchId, string dataSource, string databaseName, string userId) : base(watchType, watchId)
        {
            _databaseName = databaseName;
            _dataSource = dataSource;
            _userId = userId;
        }

        public string GetDataSource()
        {
            return _dataSource;
        }
        public string GetDatabaseName()
        {
            return _databaseName;
        }
        public string GetUserId()
        {
            return _userId;
        }
    }
}
