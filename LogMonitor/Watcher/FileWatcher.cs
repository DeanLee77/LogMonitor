using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

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
        private string _configFilePath = @"C:\Windows\Temp\config\watcher.config";

        public FileWatcher(int watchId, string filePath, string filter, string fileName, long fileSize)
            :base(watchId)
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
            _newFileSize = new FileInfo(_fullPath).Length;

            ReadFile();
            if (!_fullPath.Equals(_configFilePath))
            {
                if (_jsonString.Trim().Length != 0 && _fileSize != _newFileSize) // this variable: this logic need due to the reason specified in 'https://devblogs.microsoft.com/oldnewthing/?p=1053'
                {
                    //Writing into file
                    WriteOnFile();
                    _fileSize = _newFileSize;
                    //Console.WriteLine($"File: {e.FullPath}, Change type: {e.ChangeType}");
                }
            }
            else if(_fullPath.Equals(_configFilePath))
            {
                OnConfigFileUpdated(e);
            }
        }
        private void ReadFile()
        {
            if(_filter.Equals("*.xml") || _filter.Equals("*.config"))
            {
                ReadXmlFile();
            }
            else
            {
                ReadNonXmlFile();
            }
        }

        private void ReadNonXmlFile()
        {
            using (FileStream fs = new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int count = (int)(_newFileSize - _fileSize);
                int offset = (int)_fileSize;
                byte[] byteArray = new byte[count];

                fs.Seek(offset, SeekOrigin.Begin);
                int returnByteNum =  fs.Read(byteArray, 0, count);
                
                if(returnByteNum == count)
                {
                    _sb.Append(Encoding.UTF8.GetString(byteArray));
                }
                else
                {
                    _sb.Append("Exception: Unable to read a file");
                }

                fs.Close();
            }
                
            _jsonString = _sb.ToString();
            _sb.Clear();
            
        }

        private void ReadXmlFile()
        {
            XmlDocument xmlDocu = new XmlDocument();
            xmlDocu.Load(_fullPath);
            String targetNode = _fileName.Split('.')[0];
            XmlNodeList nodeList = xmlDocu.DocumentElement.GetElementsByTagName(targetNode);
            if (_fileName.Equals("watcher.config"))
            {
                _jsonString = nodeList.Item(nodeList.Count - 1).OuterXml;
            }
            else
            {
                StringWriter sw = new StringWriter();
                XmlTextWriter txw = new XmlTextWriter(sw);
                nodeList.Item(nodeList.Count - 1).WriteContentTo(txw);
                //nodeList.Item(nodeList.Count - 1).WriteTo(txw);
                _jsonString = "<" + targetNode + ">" + sw.ToString() + "</" + targetNode + ">";
            }               
        }
        public string GetJsonString()
        {
            return _jsonString;
        }
        public string GetFullPath()
        {
            return _fullPath;
        }

        public string GetFileName()
        {
            return _fileName;
        }
       
        private void WriteOnFile()
        {
            string[] filePathArray = _filePath.Split('\\');
            string fileKind = filePathArray[filePathArray.Length - 1];
            string[] stringArray;
            string jsonResult ="";
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
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(_jsonString);

                    // Time => DateTime.Now, Location => _fullPath, Level => xmlDoc.level, Output => xml.name +" "+ xml.description
                    m_logger = new MasterLogger(DateTime.Now.ToString("dd/MM/yy hh:mm tt"), _fullPath, xmlDoc.SelectSingleNode("/action/level").InnerText, xmlDoc.SelectSingleNode("/action/name").InnerText + "--" + xmlDoc.SelectSingleNode("/action/description").InnerText);
                    jsonResult = JsonConvert.SerializeObject(m_logger);
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

            Object locker = new Object();
            lock(locker)
            {
                using (StreamWriter sw = File.AppendText(_consolidatedLogFilePath))
                {
                    sw.Write(jsonResult);
                    sw.Write("\n");
                    sw.Flush();
                    sw.Close();
                    Console.WriteLine(jsonResult);
                }
            }
        }

        protected virtual void OnConfigFileUpdated(EventArgs e)
        {
            //when this method is being reached ReadFile() is alrady been called so that '_jsonString' has already an approprivate value

            if (_jsonString.Trim().Length != 0) // this variable: this logic need due to the reason specified in 'https://devblogs.microsoft.com/oldnewthing/?p=1053'
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(_jsonString);
                ConfigFileUpdated?.Invoke(xmlDoc, e);
            }           
        }

        public event EventHandler ConfigFileUpdated;
    }
}
