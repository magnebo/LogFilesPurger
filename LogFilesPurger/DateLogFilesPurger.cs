using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using log4net;

namespace LogFilesPurger
{
    public class DateLogFilesPurger
    {
        private const string DefaultLogFileExtension = ".log";
        private static readonly ILog Logger = LogManager.GetLogger(typeof (DateLogFilesPurger));
        private readonly string _dateFormat;
        private readonly string _logFilesBaseFolder;
        private readonly int _maxDateRollBackups;
        private readonly List<string> _logFilesExtensionsToNotDelete;

        public DateLogFilesPurger(string logFilesBaseFolder, string dateFormat, int maxDateRollBackups)
        {
            _logFilesBaseFolder = logFilesBaseFolder;
            _dateFormat = dateFormat;
            _maxDateRollBackups = maxDateRollBackups;
            var logFilesExtensionsToNotDelete = ConfigurationManager.AppSettings["LofFilesExtensionsToNotDelete"];
            _logFilesExtensionsToNotDelete =
                new List<string>(logFilesExtensionsToNotDelete.Split(new char[] {',', ';'},
                                                                     StringSplitOptions.RemoveEmptyEntries));
        }

        public void PurgeLogfiles()
        {
            List<string> logfilesFolders = GetAllSubFolders();
            foreach (string logfilesFolder in logfilesFolders)
            {
                PurgeLogfiles(logfilesFolder);
            }
        }

        private void PurgeLogfiles(string folder)
        {
            string[] files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                string fileToLower = file.ToLower();
                if (fileToLower.EndsWith(DefaultLogFileExtension))
                {
                    continue;
                }

                if (_logFilesExtensionsToNotDelete.Any(x => fileToLower.EndsWith(x.ToLower())))
                {
                    continue;
                }

                bool deleteFile = true;
                for (int i = 1; i <= _maxDateRollBackups; i++)
                {
                    DateTime date = DateTime.Now.AddDays(-i);
                    string filenameDate = date.ToString(_dateFormat, CultureInfo.InvariantCulture);
                    if (fileToLower.Contains(filenameDate))
                    {
                        deleteFile = false;
                        break;
                    }
                }

                if (deleteFile)
                {
                    Logger.InfoFormat("Deleting log file: {0}", file);
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Failed to delete file: {0}", file);
                        Logger.ErrorFormat(message, ex);
                    }
                }
            }
        }

        private List<string> GetAllSubFolders()
        {
            string[] subFolders = Directory.GetDirectories(_logFilesBaseFolder, "*", SearchOption.AllDirectories);
            var result = new List<string>(subFolders);
            return result;
        }
    }
}