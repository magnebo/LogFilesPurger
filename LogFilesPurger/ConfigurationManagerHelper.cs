using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace LogFilesPurger
{
    public static class ConfigurationManagerHelper
    {
        private static readonly char[] ListSeparators = {',', ';'};

        public static string GetAppSettingValue(string appSettingKey, bool canBeEmpty = false)
        {
            string value = ConfigurationManager.AppSettings[appSettingKey];
            if (!canBeEmpty && string.IsNullOrEmpty(value))
            {
                string message = string.Format("Can not find value for appSetting key '{0}'.", appSettingKey);
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }

        public static bool GetBooleanAppSettingValue(string appSettingKey)
        {
            string stringValue = GetAppSettingValue(appSettingKey);

            return bool.Parse(stringValue);
        }

        public static int GetIntegerAppSettingValue(string appSettingKey, bool canBeEmpty = false, int defaultIfMissing = 0)
        {
            string stringValue = GetAppSettingValue(appSettingKey, canBeEmpty);

            if (string.IsNullOrEmpty(stringValue))
            {
                return defaultIfMissing;
            }

            return int.Parse(stringValue);
        }

        public static int GetIntegerAppSettingValue(string appSettingKey, int centralNumber)
        {
            string appSettingkeyWithCentralNumber = string.Format("{0}|{1}", centralNumber, appSettingKey);
            string value = GetAppSettingValue(appSettingkeyWithCentralNumber, true);

            if (string.IsNullOrEmpty(value))
            {
                value = GetAppSettingValue(appSettingKey, false);
            }

            return int.Parse(value);
        }

        public static DateTime? GetDateTimeAppSettingValue(string appSettingKey, string format = "dd.MM.yyyy", bool canBeEmpty = false)
        {
            string stringValue = GetAppSettingValue(appSettingKey, canBeEmpty);

            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }
            return DateTime.ParseExact(stringValue, format, CultureInfo.InvariantCulture);
        }

        public static TimeSpan GetTimeSpanAppSettingValue(string appSettingKey, string format = "h\\:mm")
        {
            string stringValue = GetAppSettingValue(appSettingKey);

            return TimeSpan.ParseExact(stringValue, format, CultureInfo.InvariantCulture, TimeSpanStyles.None);
        }

        public static string[] GetListOfStringsAppSettingValue(string appSettingKey, bool canBeEmpty = false)
        {
            string appSettingValue = GetAppSettingValue(appSettingKey, canBeEmpty);
            if (string.IsNullOrEmpty(appSettingValue))
            {
                return new List<string>().ToArray();
            }

            return appSettingValue.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }
    }
}