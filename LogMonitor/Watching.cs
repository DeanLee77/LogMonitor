using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogMonitor
{
    class LogFileWatcher
    {
        private string Path;
        private string FileName;
        private string FullPath;
        private const string MasterLogPath = @"C:\Windows\Temp\masterLog\masterLog.log";
        private AutoResetEvent ResetEvent = new AutoResetEvent(false);

        public LogFileWatcher(string Path, string FileName)
        {
            this.Path = Path;
            this.FileName = FileName;
        }

        public void Run()
        {
            using (FileSystemWatcher watcher = new FileSystemWatcher(this.Path, this.FileName))
            {

                watcher.NotifyFilter = NotifyFilters.LastAccess
                                      | NotifyFilters.LastWrite
                                      | NotifyFilters.FileName
                                      | NotifyFilters.Size
                                      | NotifyFilters.DirectoryName;


                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                watcher.Deleted += OnChanged;
                Console.WriteLine($"path: {watcher.Path}, fileName: {watcher.Filter}");
                watcher.EnableRaisingEvents = true;
                
                ResetEvent.WaitOne();
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            ResetEvent.Set();
            FullPath = e.FullPath;
            
        }

        public string GetFullPath()
        {
            return this.FullPath;
        }

        public AutoResetEvent GetThreadInWatcher()
        {
            return this.ResetEvent;
        }

        public void SetSignalThread()
        {
            this.ResetEvent.Set();
        }
    }
}
