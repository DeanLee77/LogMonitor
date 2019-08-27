using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogMonitor.Reader
{
    public class FileReader : BaseReader
    {
        private string _filePath;
        private long _offset;
        private string _jsonString;
        private FileStream _fs;

        public FileReader(string filePath, long offset)
            :base()
        {
            _filePath = filePath;
            _offset = offset;
        }


        public override void RunThread()
        {
            _fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
            _fs.Lock(0, _fs.Length);
            _fs.Seek(_offset, SeekOrigin.Begin);
            Read();
            this.Wait();
        }

        public void Read()
        {
            int nextByte;
            StringBuilder sb = new StringBuilder();

            while((nextByte = _fs.ReadByte()) > 0)
            {
                sb.Append(Convert.ToChar(nextByte));
            }
            try
            {
                _fs.Unlock(_offset, _fs.Length);
                Console.WriteLine("the file is being unlocked");
            }
            catch(IOException e)
            {
                Console.WriteLine("the file is not being locked by the current process");
            }
            string[] filePathArray = _filePath.Split('\\');
            string fileKind = filePathArray[filePathArray.Length - 2];
            string[] stringArray;
            switch (fileKind)
            {
                case "logs":
                    //reading log file : comma separated and format is timestamp, number, info, description
                    stringArray = sb.ToString().Split(',');

                    
                    _jsonString = sb.ToString(); //this line is just temporary, and needs to be removed
                    break;
                case "xml":
                    //reading xml file : hierachy is <actions>-<action>-<id>,<name>,<description>,<level>,<timestamp>


                    _jsonString = sb.ToString(); //this line is just temporary, and needs to be removed
                    break;
                case "csv":
                    // reading csv file: comma separated and format is Item No, Description, Cost, Paid, Due Date, Result, Notes
                    stringArray = sb.ToString().Split(',');

                    _jsonString = sb.ToString(); //this line is just temporary, and needs to be removed
                    break;
            }
        }

        public string GetJsonString()
        {
            return _jsonString;
        }
        public void HandlingXML()
        {
            XmlDocument xml = new XmlDocument();

        }
        public void HandlingNonXML()
        {

        }

        
    }
}
