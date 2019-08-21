using System;
using System.Collections.Generic;
using System.Xml;
using LogMonitor.WatchingObject;

namespace LogMonitor.Watcher
{
    public class Watcher
    {
        private static List<BaseWatchingObject> _watchingList = new List<BaseWatchingObject>();
        private static XmlDocument xmlDocument;
        private static string _configFilePath = @"C:\Windows\Temp\config\watcher.xml";


        public Watcher()
        {
            
        }

        public static void PopulatingWatchingList()
        {
            xmlDocument = new XmlDocument();
            xmlDocument.Load(_configFilePath);
            XmlNodeList watcherNodeList = xmlDocument.GetElementsByTagName("watcher");
            foreach(XmlNode watcher in watcherNodeList)
            {
                string watchType = watcher.Attributes["watchType"].Value;
                if(watchType.Equals("file"))
                {
                    WatchingFileObject watchingFileObject = new WatchingFileObject(watchType, watcher.Attributes["fileType"].Value, watcher.Attributes["name"].Value, watcher.Attributes["path"].Value);
                    _watchingList.Add(watchingFileObject);
                }
            }

        }

        public static List<BaseWatchingObject> GetWatchingList()
        {
            return _watchingList;
        }

    }
}
