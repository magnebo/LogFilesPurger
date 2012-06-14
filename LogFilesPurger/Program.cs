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
 
            var logFilesBaseFolder = ConfigurationManager.AppSettings["LogFilesBaseFolder"];
            Logger.InfoFormat("log files base folder: {0}", logFilesBaseFolder);
            var dateFormat = ConfigurationManager.AppSettings["DateFormat"];
            Logger.InfoFormat("date format: {0}", dateFormat);
            var maxDateRollBackupsAsString = ConfigurationManager.AppSettings["MaxDateRollBackups"];
            int maxDateRollBackups;
            if (!int.TryParse(maxDateRollBackupsAsString, out maxDateRollBackups))
            {
                maxDateRollBackups = 30;
            }
            Logger.InfoFormat("max date roll backups: {0}", maxDateRollBackups);
            var dateLogFilePurger = new DateLogFilesPurger(logFilesBaseFolder, dateFormat, maxDateRollBackups);
            dateLogFilePurger.PurgeLogfiles();
        }
    }
}
