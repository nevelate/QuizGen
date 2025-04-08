using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGen
{
    internal static class AppSettings
    {
        private static Configuration _configuration;
        private static ExeConfigurationFileMap _fileMap;
        private static KeyValueConfigurationCollection _appSettings;

        static AppSettings()
        {
            _fileMap = new ExeConfigurationFileMap();
            _fileMap.ExeConfigFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuizGen", "app.config");
            _configuration = ConfigurationManager.OpenMappedExeConfiguration(_fileMap, ConfigurationUserLevel.None);
            _appSettings = _configuration.AppSettings.Settings;
        }

        public static string? GetSetting(string key)
        {
            return _appSettings[key]?.Value;
        }

        public static void SetSetting(string key, string value)
        {
            if (_appSettings[key] != null)
            {
                _appSettings[key].Value = value;
            }
            else
            {
                _appSettings.Add(key, value);
            }
            _configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(_configuration.AppSettings.SectionInformation.Name);
        }
    }
}
