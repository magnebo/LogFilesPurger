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
        private static readonly ILog Logger = LogManager.GetLogger(typeof (DateLogFilesPurger));
        private readonly string _dateFormat;
        private readonly string _defaultLogFileExtension;
        private readonly List<string> _logFilesBaseFolders;
        private readonly int _maxDateRollBackups;
        private readonly List<string> _logFilesExtensionsToNotDelete;

        public DateLogFilesPurger(string logFilesBaseFolder, string dateFormat, int maxDateRollBackups)
            : this(new List<string>{logFilesBaseFolder}, dateFormat, maxDateRollBackups)
        {
        }

        public DateLogFilesPurger(List<string> logFilesBaseFolders, string dateFormat, int maxDateRollBackups)
        {
            _logFilesBaseFolders = logFilesBaseFolders;
            _dateFormat = dateFormat;
            _maxDateRollBackups = maxDateRollBackups;
            var logFilesExtensionsToNotDelete = ConfigurationManager.AppSettings["LogFileExtensionsToNotDelete"];
            _logFilesExtensionsToNotDelete =
                new List<string>(logFilesExtensionsToNotDelete.Split(new char[] { ',', ';' },
                                                                     StringSplitOptions.RemoveEmptyEntries));
            _defaultLogFileExtension = ConfigurationManager.AppSettings["DefaultLogFileExtension"];
            if (string.IsNullOrWhiteSpace(_defaultLogFileExtension))
            {
                _defaultLogFileExtension = ".log";
            }
        }

        public void PurgeLogFiles()
        {
            foreach (var logFilesBaseFolder in _logFilesBaseFolders)
            {
                if (string.IsNullOrWhiteSpace(logFilesBaseFolder))
                {
                    continue;
                }
                List<string> logFilesFolders = GetAllSubFolders(logFilesBaseFolder);
                foreach (string logFilesFolder in logFilesFolders)
                {
                    PurgeLogFiles(logFilesFolder);
                }
                PurgeLogFiles(logFilesBaseFolder);
            }
        }

        private void PurgeLogFiles(string folder)
        {
            string[] files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                string fileToLower = file.ToLower();
                if (fileToLower.EndsWith(_defaultLogFileExtension))
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

        private List<string> GetAllSubFolders(string logFilesBaseFolder)
        {
            string[] subFolders = Directory.GetDirectories(logFilesBaseFolder, "*", SearchOption.AllDirectories);
            var result = new List<string>(subFolders);
            return result;
        }
    }
}