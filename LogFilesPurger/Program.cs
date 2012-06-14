using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;

namespace LogFilesPurger
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

        static void Main(string[] args)
        {
            var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            var logFileInfo = new FileInfo(logFilePath);
            if (!logFileInfo.Exists)
            {
                throw new ApplicationException("Could not find log4net.config file.");
            }

            XmlConfigurator.ConfigureAndWatch(logFileInfo);
 
            var logFilesBaseFoldersValue = ConfigurationManager.AppSettings["LogFilesBaseFolders"];
            var logFilesBaseFolders =
                new List<string>(logFilesBaseFoldersValue.Split(new char[] {',', ';'},
                                                                StringSplitOptions.RemoveEmptyEntries));

            Logger.InfoFormat("Log files base folders: {0}", string.Join(",", logFilesBaseFolders.ToArray()));
            var dateFormat = ConfigurationManager.AppSettings["DateFormat"];
            Logger.InfoFormat("Date format: {0}", dateFormat);
            var maxDateRollBackupsAsString = ConfigurationManager.AppSettings["MaxDateRollBackups"];
            int maxDateRollBackups;
            if (!int.TryParse(maxDateRollBackupsAsString, out maxDateRollBackups))
            {
                maxDateRollBackups = 30;
            }
            Logger.InfoFormat("max date roll backups: {0}", maxDateRollBackups);
            var dateLogFilePurger = new DateLogFilesPurger(logFilesBaseFolders, dateFormat, maxDateRollBackups);
            dateLogFilePurger.PurgeLogfiles();
        }
    }
}
