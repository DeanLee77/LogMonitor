using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor.Watcher
{
    class FileWatcher : BaseWatcher
    {
        private FileSystemWatcher _fileWatcher;
        private string _fullPath = "";
        public FileWatcher(string filePath, string fileName)
        {
            _fileWatcher = new FileSystemWatcher(filePath, fileName);
            _fileWatcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.Size
                                     | NotifyFilters.DirectoryName;


            _fileWatcher.Changed += OnChanged;
            _fileWatcher.Created += OnChanged;
            _fileWatcher.Deleted += OnChanged;
        }

        public override void RunThread()
        {
            _fileWatcher.EnableRaisingEvents = true;
            this.Wait();
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            _fullPath = e.FullPath;
        }
        private string GetFullPath()
        {
            return _fullPath;
        }
        
    }
}
