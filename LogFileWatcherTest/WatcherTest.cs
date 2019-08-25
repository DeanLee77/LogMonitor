
using LogMonitor.Watcher;
using LogMonitor.WatchingObject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace LogFileWatcherTest
{
    [TestClass]
    public class WatcherTest
    {
        [TestMethod]
        public void Test_PopulatingWatchingList()
        {
            Watcher.PopulatingWatchingList();
            List<BaseWatchingObject> watchList = Watcher.GetWatchingList();
            bool memberCheck = true;

            for(int i = 0; i < watchList.Count; i++)
            {
                BaseWatchingObject bwo = watchList[i];

                if (i == 0)
                {
                    if (bwo.GetType() != typeof(WatchingFileObject)
                        || !bwo.GetWatchType().Equals("file")
                        || !((WatchingFileObject)bwo).GetFileType().Equals("log")
                        || !((WatchingFileObject)bwo).GetFileName().Equals("app.log")
                        || !((WatchingFileObject)bwo).GetPath().Equals("C:\\Windows\\Temp\\logs"))
                    {
                        memberCheck = false;
                    }
                }
                else if (i ==1)
                {
                    if (bwo.GetType() != typeof(WatchingFileObject)
                        || !bwo.GetWatchType().Equals("file")
                        || !((WatchingFileObject)bwo).GetFileType().Equals("csv")
                        || !((WatchingFileObject)bwo).GetFileName().Equals("payments.csv")
                        || !((WatchingFileObject)bwo).GetPath().Equals("C:\\Windows\\Temp\\csv"))
                    {
                        memberCheck = false;
                    }
                }
                else if (i == 2)
                {
                    if (bwo.GetType() != typeof(WatchingFileObject)
                        || !bwo.GetWatchType().Equals("file")
                        || !((WatchingFileObject)bwo).GetFileType().Equals("xml")
                        || !((WatchingFileObject)bwo).GetFileName().Equals("watcher.xml")
                        || !((WatchingFileObject)bwo).GetPath().Equals("C:\\Windows\\Temp\\config"))
                    {
                        memberCheck = false;
                    }
                }
            }

            Assert.AreEqual(true, memberCheck);


        }

        [TestMethod]
        public void Test_FileWatcher()
        {
            string fileName = @"test.txt";
            if(File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            using (StreamWriter writer = File.CreateText(fileName))
            {
                string filePath = Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory);

                FileWatcher watcher = new FileWatcher(filePath, ".txt", fileName);
                watcher.Start();

                writer.WriteLine("2019-07-15 09:26:38,468, [DEBUG], this is a first test log line.");
                watcher.Finish();

                Assert.AreEqual(watcher.GetFullPath(), filePath + "\\" + fileName);
            }
            
        }
    }
}
