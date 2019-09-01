using System;
using System.IO;
using System.Text;
using System.Threading;
using LogMonitor.Watcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace LogFileWatcherTest
{
    [TestClass]
    public class FileWatcherTest
    {
        private string _path = @"C:\Windows\Temp\logs";
        private string _filter = @"*.log";
        private string _fileName = @"app.log";
        private string _newLine = @"2019-07-15 09:26:38,468, [DEBUG], this is a first test log line !!!!!!!.";

        public FileWatcher ArrangeFileWatcher()
        {
            FileWatcher fw = new FileWatcher(_path, _filter, _fileName, new FileInfo(_path + "\\" + _fileName).Length);
            fw.Start();

            return fw;
        }

        public void WriteNewLineToFile()
        {
            using (StreamWriter sw = File.AppendText(_path + "\\" + _fileName))
            {
                sw.Write(_newLine);
                sw.Write("\n");
                sw.Flush();
                sw.Close();
            }
        }

        public MasterLogger CreateMasterLogger()
        {
            string[] stringArray = _newLine.Split(',');
            return new MasterLogger(DateTime.Now.ToString("dd/MM/yy hh:mm tt"), _path + "\\" + _fileName, stringArray[2], stringArray[3]);    
        }

        [TestMethod]
        public void FileWatcherTestMethod()
        {
            FileWatcher watcher = new FileWatcher(_path, _filter, _fileName, new FileInfo(_path + "\\" + _fileName).Length);
            watcher.Start();
            Thread.Sleep(4000);
            WriteNewLineToFile();
            Thread.Sleep(4000);
            MasterLogger m_logger = CreateMasterLogger();
            string consolidatedLogFilePath = @"C:\Windows\Temp\consolidated log\consolidatedLog.log";

            FileStream fs = new FileStream(consolidatedLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sReader = new StreamReader(fs, Encoding.UTF8);
            string line;
            string lastLine = "";
            while ((line = sReader.ReadLine()) != null)
            {
                lastLine = line;
            }

            string expectedJsonString = JsonConvert.SerializeObject(m_logger);
            watcher.Finish();
            Assert.AreEqual(lastLine, expectedJsonString);

        }
    }
}
