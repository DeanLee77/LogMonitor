
using LogMonitor.Watcher;
using LogMonitor.WatchingObject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            WatchingFileObject wfo2 = new WatchingFileObject("file", "csv", "payments.csv", "C:\\Windows\\Temp\\csv");
            WatchingFileObject wfo3 = new WatchingFileObject("file", "xml", "watcher.xml", "C:\\Windows\\Temp\\xml");
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
    }
}
