using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor.WatchingObject
{
    public class WatchingFileObject : BaseWatchingObject
    {
        protected string _fileType;
        protected string _fileName;
        protected string _path;

        public WatchingFileObject(string watchType, string fileType, string fileName, string path) 
            : base(watchType)
        {
            _fileType = fileType;
            _fileName = fileName;
            _path = path;
        }

        public string GetFileType()
        {
            return _fileType;
        }
        public string GetFileName()
        {
            return _fileName;
        }
        public string GetPath()
        {
            return _path;
        }
    }
}
