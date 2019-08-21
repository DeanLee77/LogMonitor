using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            string LogPath = @"C:\Windows\Temp\logs";
            string FileName = "*.log";
            string CsvFilePath = @"C:\Windows\Temp\csv";
            string CsvFileFilter = "*.csv";
            string XmlFilePath = @"C:\Windows\Temp\xml";
            string XmlFileFilter = "*.xml";
            string ConsolidatedLogPath = @"C:\Windows\Temp";
            string ConsolidatedLogFile = "consolidatedLog.log";

            LogFileWatcher LogFileWatcher = new LogFileWatcher(LogPath, FileName);

            Console.WriteLine("Listening");
            Console.WriteLine("Press enter key");

            ThreadStart NewThread = new ThreadStart(LogFileWatcher.Run);
            Thread thread1 = new Thread(NewThread);
            thread1.Start();
            
            LogFileWatcher.Run();
            Console.WriteLine($"Full Path : {LogFileWatcher.GetFullPath()}");
          
            



    }
    }
}
