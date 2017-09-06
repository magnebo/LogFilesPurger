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
        private readonly List<string> _dateFormats;
        private readonly string _defaultLogFileExtension;
        private readonly List<string> _logFilesBaseFolders;
        private readonly List<string> _logFilesExtensionsToNotDelete;
        private readonly int _maxDateRollBackups;

        public DateLogFilesPurger(string logFilesBaseFolder, List<string> dateFormats, int maxDateRollBackups)
            : this(new List<string> {logFilesBaseFolder}, dateFormats, maxDateRollBackups)
        {
        }

        public DateLogFilesPurger(List<string> logFilesBaseFolders, List<string> dateFormats, int maxDateRollBackups)
        {
            _logFilesBaseFolders = logFilesBaseFolders;
            _dateFormats = dateFormats;
            _maxDateRollBackups = maxDateRollBackups;
            string logFilesExtensionsToNotDelete = ConfigurationManager.AppSettings["LogFileExtensionsToNotDelete"];
            _logFilesExtensionsToNotDelete =
                new List<string>(logFilesExtensionsToNotDelete.Split(new[] {',', ';'},
                    StringSplitOptions.RemoveEmptyEntries));
            _defaultLogFileExtension = ConfigurationManager.AppSettings["DefaultLogFileExtension"];
            if (string.IsNullOrWhiteSpace(_defaultLogFileExtension))
            {
                _defaultLogFileExtension = ".log";
            }
        }

        public void PurgeLogFiles()
        {
            foreach (string logFilesBaseFolder in _logFilesBaseFolders)
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

                var deleteFile = true;
                for (var i = 1; i <= _maxDateRollBackups; i++)
                {
                    DateTime date = DateTime.Now.AddDays(-i);
                    foreach (string dateFormat in _dateFormats)
                    {
                        string filenameDate = date.ToString(dateFormat, CultureInfo.InvariantCulture);
                        if (fileToLower.Contains(filenameDate))
                        {
                            deleteFile = false;
                            break;
                        }
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