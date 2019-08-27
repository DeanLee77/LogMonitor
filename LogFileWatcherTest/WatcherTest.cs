
using LogMonitor.Watcher;
using LogMonitor.WatchingObject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

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
            string fileName = @"app.log";
            string filePath = @"C:\Windows\Temp\logs";

            FileWatcher watcher = new FileWatcher(filePath, "*.log", fileName, new FileInfo(filePath + "\\" + fileName).Length);
            watcher.Start();

            using (StreamWriter sw = File.AppendText(filePath+"\\"+fileName))
            {
                sw.Write("2019-07-15 09:26:38,468, [DEBUG], this is a first test log line !!!!!!!.");
                sw.Write("\n");
                sw.Flush();
                sw.Close();
            }

            watcher.Finish();

            Assert.AreEqual(watcher.GetFullPath(), filePath + "\\" + fileName);
        }

        [TestMethod]
        public void Test_Watcher_ReadFile()
        {
            string fileName = @"app.log";
            string filePath = @"C:\Windows\Temp\logs";

            FileWatcher watcher = new FileWatcher(filePath, "*.log", fileName, new FileInfo(filePath + "\\" + fileName).Length);
            watcher.Start();

            using (StreamWriter sw = File.AppendText(filePath + "\\" + fileName))
            {
                sw.Write("2019-07-15 09:26:38,468, [DEBUG], this is a first test log line !!!!!!!.");
                sw.Write("\n");
                sw.Flush();
                sw.Close();
            }
            Thread.Sleep(4000); //main thread needs to wait for the watcher thread to detect changes.
            watcher.Finish();

            string toBeCompared = "2019-07-15 09:26:38,468, [DEBUG], this is a first test log line !!!!!!!.\n";
            Assert.AreEqual(toBeCompared, watcher.GetJsonString());
        }

        public void Test_FileWatcher_Aux(FileWatcher watcher, string fileName, string filePath)
        {
            watcher.Start();

            using (StreamWriter sw = File.AppendText(filePath + "\\" + fileName))
            {
                sw.Write("2019-07-15 09:26:38,468, [DEBUG], this is a first test log line !!!!!!!.");
                sw.Write("\n");
                sw.Flush();
                sw.Close();
            }

            watcher.Finish();
        }
    }
}
