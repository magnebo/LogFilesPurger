using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using log4net;
using log4net.Config;

namespace LogFilesPurger
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (Program));

        private static void Main(string[] args)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            var logFileInfo = new FileInfo(logFilePath);
            if (!logFileInfo.Exists)
            {
                throw new ApplicationException("Could not find log4net.config file.");
            }

            XmlConfigurator.ConfigureAndWatch(logFileInfo);

            string logFilesBaseFoldersValue = ConfigurationManager.AppSettings["LogFilesBaseFolders"];
            var logFilesBaseFolders =
                new List<string>(logFilesBaseFoldersValue.Split(new[] {',', ';'},
                    StringSplitOptions.RemoveEmptyEntries));

            Logger.InfoFormat("Log files base folders: {0}", string.Join(",", logFilesBaseFolders.ToArray()));
            List<string> dateFormats = ConfigurationManagerHelper.GetListOfStringsAppSettingValue("DateFormats").ToList();
            Logger.InfoFormat("Date format: {0}", string.Join(",", dateFormats));
            string maxDateRollBackupsAsString = ConfigurationManager.AppSettings["MaxDateRollBackups"];
            int maxDateRollBackups;
            if (!int.TryParse(maxDateRollBackupsAsString, out maxDateRollBackups))
            {
                maxDateRollBackups = 30;
            }
            Logger.InfoFormat("Max date roll backups: {0}", maxDateRollBackups);
            var dateLogFilePurger = new DateLogFilesPurger(logFilesBaseFolders, dateFormats, maxDateRollBackups);
            dateLogFilePurger.PurgeLogFiles();
        }
    }
}