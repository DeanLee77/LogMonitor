using LogMonitor.Reader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogMonitor.Watcher
{
    public class FileWatcher : BaseWatcher
    {
        private FileSystemWatcher _fileWatcher;
        private string _fullPath = "";
        private string _fileName;
        private string _filePath;
        private string _filter;
        private long _fileSize;
        private long _newFileSize;
        private string _jsonString;

        public FileWatcher(string filePath, string filter, string fileName, long fileSize)
            :base()
        {
            _fileName = fileName;
            _filePath = filePath;
            _filter = filter;
            _fileSize = fileSize;
            
        }

        public override void RunThread()
        {
            _fileWatcher = new FileSystemWatcher(_filePath, _filter);
            _fileWatcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.Size
                                     | NotifyFilters.DirectoryName;


            _fileWatcher.Changed += OnChanged;
            _fileWatcher.Created += OnChanged;
            _fileWatcher.Deleted += OnChanged;
            
            _fileWatcher.EnableRaisingEvents = true;
            this.Wait();
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            _fullPath = e.FullPath;
            _newFileSize = new FileInfo(e.FullPath).Length;

            ReadFile();

            Console.WriteLine($"File: {e.FullPath}, Change type: {e.ChangeType}, offset:{_fileSize}, sizeNow:{_fileSize}, new string:{_jsonString}  ");

        }

        private void ReadFile()
        {
            _rwls.EnterReadLock();
            try
            {
                using (FileStream fs = new FileStream(_fullPath, FileMode.Open, FileAccess.Read))
                {

                    fs.Seek(_fileSize, SeekOrigin.Begin);
                    int nextByte;

                    while ((nextByte = fs.ReadByte()) > 0)
                    {
                        _sb.Append(Convert.ToChar(nextByte));
                    }
                    fs.Close();
                }
                
                _jsonString = _sb.ToString();
                _sb.Clear();
            }
            finally
            {
                _fileSize = _newFileSize;
                _rwls.ExitReadLock();
            }
        }

        public string GetFullPath()
        {
            return _fullPath;
        }

        public string GetFileName()
        {
            return _fileName;
        }

       
        private void WritingOnFile()
        {
            string[] filePathArray = _filePath.Split('\\');
            string fileKind = filePathArray[filePathArray.Length - 1];
            string[] stringArray;
            string jsonResult;
            MasterLogger m_logger;
            switch (fileKind)
            {
                case "logs":
                    //reading log file : comma separated and format is timestamp, number, info, description
                    //JSON format needs to be Time, Location, Level, Output
                    stringArray = _jsonString.Split(',');
                    // Time => DateTime.Now, Location => _fullPath, Level => stringArray[2], Output => stringArray[3]
                    m_logger = new MasterLogger(DateTime.Now.ToString("dd/MM/yy hh:mm tt"), _fullPath, stringArray[2], stringArray[3]);
                    jsonResult = JsonConvert.SerializeObject(m_logger);

                    break;
                case "xml":
                    //reading xml file : hierachy is <actions>-<action>-<id>,<name>,<description>,<level>,<timestamp>
                    stringArray = _jsonString.Split(',');

                    break;
                case "csv":
                    // reading csv file: comma separated and format is Item No, Description, Cost, Paid, Due Date, Result, Notes
                    //JSON format needs to be Time, Location, Level, Output
                    stringArray = _jsonString.Split(',');
                    // Time => DateTime.Now, Location => _fullPath, Level => stringArray[5], Output => stringArray[]
                    m_logger = new MasterLogger(DateTime.Now.ToString("dd/MM/yy hh:mm tt"), _fullPath, stringArray[5], stringArray[1]+" "+stringArray[2]+" "+stringArray[3]+" "+stringArray[6]);
                    jsonResult = JsonConvert.SerializeObject(m_logger);

                    break;
            }

            _rwls.EnterWriteLock();
            try
            {
                
                using (StreamWriter sw = File.AppendText(_fullPath))
                {
                    //jsonResult = sw.

                    //fs.Seek(_fileSize, SeekOrigin.Begin);
                    int nextByte;

                    //while ((nextByte = fs.ReadByte()) > 0)
                    //{
                    //    _sb.Append(Convert.ToChar(nextByte));
                    //}
                    //fs.Close();
                }

                _jsonString = _sb.ToString();
                _sb.Clear();
            }
            finally
            {
                _rwls.ExitWriteLock();
            }
        }
        
    }
}
