using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using LogMonitor.WatchingObject;

namespace LogMonitor.Watcher
{
    public class WatcherCore
    {
        private List<BaseWatchingObject> _watchingList;
        private List<BaseWatcher> _threadList;
        private XmlDocument xmlDocument;
        private string _configFilePath;

        public WatcherCore(string configFilePath)
        {
            _configFilePath = configFilePath;
        }

        public void Monitoring()
        {
            PopulatingWatchingList();
            _threadList = new List<BaseWatcher>();

            Console.WriteLine("starting watchers.......");
            foreach (BaseWatchingObject watchingObject in _watchingList)
            {
                string watchType = watchingObject.GetWatchType();
                BaseWatcher baseWatcher = MonitoringAux(watchType, watchingObject);

                _threadList.Add(baseWatcher);
            }

            foreach (BaseWatcher watcher in _threadList)
            {
                watcher.Start();
            }
            Console.WriteLine("Press \'q\' to quite the program");
            while (Console.Read() != 'q') ;
            foreach (BaseWatcher watcher in _threadList)
            {
                watcher.Finish();
            }

        }

        private BaseWatcher MonitoringAux(string watchType, BaseWatchingObject watchingObject)
        {
            BaseWatcher baseWatcher = null;
            switch (watchType)
            {
                case "file":
                    WatchingFileObject watchingFileObject = (WatchingFileObject)watchingObject;
                    string path = watchingFileObject.GetPath();
                    string fileName = watchingFileObject.GetFileName();
                    baseWatcher = new FileWatcher(watchingFileObject.GetWatchId(), path, "*." + watchingFileObject.GetFileType(), fileName, new FileInfo(path + "\\" + fileName).Length);

                    if (fileName.Equals("watcher.config"))
                    {
                        ((FileWatcher)baseWatcher).ConfigFileUpdated += MainConfigFileUpdated;
                    }

                    break;
                case "database":
                    WatchingDatabaseObject watchingDataObject = (WatchingDatabaseObject)watchingObject;
                    baseWatcher = new DatabaseWatcher(watchingDataObject.GetWatchId(), watchingDataObject.GetDataSource(), watchingDataObject.GetDatabaseName(), watchingDataObject.GetUserId());
                    break;
            }

            return baseWatcher;
        }

        public void PopulatingWatchingList()
        {
            xmlDocument = new XmlDocument();
            xmlDocument.Load(_configFilePath);
            XmlNodeList watcherNodeList = xmlDocument.GetElementsByTagName("watcher");
            _watchingList = new List<BaseWatchingObject>();

            foreach (XmlNode watcher in watcherNodeList)
            {
                _watchingList.Add(PopulatingWatchingListAux(watcher));
            }

        }

        private BaseWatchingObject PopulatingWatchingListAux(XmlNode watcher)
        {
            watcher = watcher.NodeType.ToString().Equals("Document") ? ((XmlDocument)watcher).GetElementsByTagName("watcher").Item(0) : watcher;
            string watchType = watcher.Attributes["watchType"].Value;
            int watchId = -1;
            Int32.TryParse(watcher.Attributes["watchId"].Value, out watchId);
            BaseWatchingObject watchingObject = null;
            if (watchType.Equals("file"))
            {
                watchingObject = new WatchingFileObject(watchType, watchId, watcher.Attributes["fileType"].Value, watcher.Attributes["name"].Value, watcher.Attributes["path"].Value);
            }
            else if (watchType.Equals("database"))
            {
                watchingObject = new WatchingDatabaseObject(watchType, watchId, watcher.Attributes["dataSource"].Value, watcher.Attributes["databaseName"].Value, watcher.Attributes["userId"].Value);
            }
            return watchingObject;
        }

        public List<BaseWatcher> GetWatcherList()
        {
            return _threadList;
        }

        public void MainConfigFileUpdated(object sender, EventArgs e)
        {
            BaseWatchingObject watchingObejct = PopulatingWatchingListAux((XmlNode)sender);
            BaseWatcher baseWatcher = MonitoringAux(watchingObejct.GetWatchType(), watchingObejct);
            baseWatcher.Start();

            Object locker = new Object();
            lock (locker)
            {
                _threadList.Add(baseWatcher);
            }

            Console.WriteLine($"e type: {((FileSystemEventArgs)e).ChangeType}, Initiating one more watcher({watchingObejct.GetWatchType()})....");
        }

    }
}
