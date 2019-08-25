﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using LogMonitor.WatchingObject;

namespace LogMonitor.Watcher
{
    public class Watcher
    {
        private static List<BaseWatchingObject> _watchingList;
        private static List<BaseWatcher> _threadList;
        private static XmlDocument xmlDocument;
        private static string _configFilePath = @"C:\Windows\Temp\config\watcher.config";


        public Watcher()
        {
            
        }

        public static void Monitoring()
        {
            PopulatingWatchingList();
            _threadList = new List<BaseWatcher>();

            Console.WriteLine("starting watchers.......");
            foreach (BaseWatchingObject watchingObject in _watchingList)
            {
                string watchType = watchingObject.GetWatchType();
                switch (watchType)
                {
                    case "file":
                        WatchingFileObject watchingFileObject = (WatchingFileObject)watchingObject;
                        string path = watchingFileObject.GetPath();
                        string fileName = watchingFileObject.GetFileName();
 
                        FileWatcher fileWatcher = new FileWatcher(path, "*." + watchingFileObject.GetFileType(), fileName, new FileInfo(path+"\\"+fileName).Length);
                        _threadList.Add(fileWatcher);
                        break;

                }
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

        public static void PopulatingWatchingList()
        {
            xmlDocument = new XmlDocument();
            xmlDocument.Load(_configFilePath);
            XmlNodeList watcherNodeList = xmlDocument.GetElementsByTagName("watcher");
            _watchingList = new List<BaseWatchingObject>();

            foreach (XmlNode watcher in watcherNodeList)
            {
                string watchType = watcher.Attributes["watchType"].Value;
                if(watchType.Equals("file"))
                {
                    WatchingFileObject watchingFileObject = new WatchingFileObject(watchType, watcher.Attributes["fileType"].Value, watcher.Attributes["name"].Value, watcher.Attributes["path"].Value);
                    _watchingList.Add(watchingFileObject);
                }
            }

        }

        public static List<BaseWatcher> GetWatcherList()
        {
            return _threadList;
        }

        public static List<BaseWatchingObject> GetWatchingList()
        {
            return _watchingList;
        }
        public static void UpdateWatchingList(BaseWatchingObject watchingObject)
        {
            _watchingList.Add(watchingObject);
        }

    }
}
