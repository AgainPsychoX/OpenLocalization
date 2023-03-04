using System;
using System.Linq;
using System.IO;
using BepInEx.Configuration;
using I2.Loc;
using System.Collections.Generic;
using static I2.Loc.LanguageSourceData;
using BepInEx.Logging;

namespace OpenLocalization
{
    public class LocalizationSource : ILanguageSource
    {
        protected static ManualLogSource Logger {
            get { return OpenLocalization.GetLogger(); }
        }

        public const string builtInSourceName = "BuiltIn";
        public const string configFileName = "config.cfg";

        public string DirectoryPath { get; }
        public string NameFromDirectoryPath
        {
            get => GetNameFromDirectoryPath(DirectoryPath);
        }
        public string Name {
            get { return ConfigEntries.Name.Value; }
        }
        public bool IsEnabled
        {
            get { return ConfigEntries.Enabled.Value; }
            set { ConfigEntries.Enabled.Value = value; }
        }
        public int Order
        {
            get { return ConfigEntries.Order.Value; }
            set { ConfigEntries.Order.Value = value; }
        }
        public string Website { get; }
        public LanguageSourceData Data { get; protected set;  }
        protected ConfigFile Config { get; }

        protected MyConfig ConfigEntries { get; }

        // Implement ILanguageSource to act as source.owner
        public LanguageSourceData SourceData {
            get => Data;
            set { Data = value; }
        }

        protected class MyConfig
        {
            public ConfigEntry<string> Name;
            public ConfigEntry<bool> Enabled;
            public ConfigEntry<int> Order;
            public ConfigEntry<string> Website;
            public ConfigEntry<string> SpreadsheetName;
            public ConfigEntry<string> SpreadsheetKey;
            public ConfigEntry<string> WebServiceURL;
            public ConfigEntry<eGoogleUpdateFrequency> UpdateFrequency;
        }

        public LocalizationSource(LanguageSourceData data, string directory, bool saveConfigImmediately = true)
        {
            DirectoryPath = directory;
            Data = data;
            if (saveConfigImmediately) Directory.CreateDirectory(DirectoryPath);
            Config = new ConfigFile(Path.Combine(directory, configFileName), saveConfigImmediately);
            Config.SaveOnConfigSet = saveConfigImmediately;

            ConfigEntries = new MyConfig();

            ConfigEntries.Name = Config.Bind("General", "Name", NameFromDirectoryPath);
            ConfigEntries.Enabled = Config.Bind("General", "Enabled", true);
            ConfigEntries.Order = Config.Bind("General", "Order", Name == builtInSourceName ? 1 : 2,
                new ConfigDescription("Lower means loading eariler, which equals higher priority (because of the way the localization in the game works)."));
            ConfigEntries.Website = Config.Bind<string>("General", "Website", null);

            ConfigEntries.SpreadsheetName = Config.Bind("Updates", "SpreadsheetName", data.Google_SpreadsheetName ?? "");
            ConfigEntries.SpreadsheetKey  = Config.Bind("Updates", "SpreadsheetKey",  data.Google_SpreadsheetKey ?? "");
            ConfigEntries.WebServiceURL   = Config.Bind("Updates", "WebServiceURL",   data.Google_WebServiceURL ?? "");
            ConfigEntries.UpdateFrequency = Config.Bind("Updates", "Frequency",       data.GoogleUpdateFrequency);

            // TODO: listen to Name changes and update directory name (move)? would require disposing wouldn't it...
        }

        public static LocalizationSource Load(string directory)
        {
            var instance = new LocalizationSource(new LanguageSourceData(), directory);
            instance.Load();
            return instance;
        }

        public void Load()
        {
            Logger.LogInfo($"Loading localization source '{Name}'");
            Config.Reload();
            Data.ClearAllData();
            foreach (string path in Directory.GetFiles(DirectoryPath))
            {
                string category = Path.GetFileNameWithoutExtension(path);
                string content = File.ReadAllText(path);
                Logger.LogDebug($"Importing translations for category '{category}' from file '{path}'");
                Data.Import_CSV(category, content, eSpreadsheetUpdateMode.Merge);
            }
            Data.mIsGlobalSource = true;
            Data.owner = this;
            Logger.LogDebug(String.Format("Languages: {0}", String.Join(", ", Data.GetLanguages())));
        }

        public void Save()
        {
            Logger.LogInfo($"Saving localization source '{Name}'");
            Directory.CreateDirectory(DirectoryPath);
            Config.Save();
            foreach (string category in Data.GetCategories(true, null))
            {
                string outputPath = DirectoryPath + "/" + category + ".csv";
                Logger.LogDebug($"Exporting category '{category}' to file '{outputPath}'");
                File.WriteAllText(outputPath, Data.Export_CSV(category, ',', true));
            }
        }

        public void Update()
        {
            Logger.LogInfo($"Updating localization source '{Name}'");
            Data.Import_Google(true, false);
        }

        /// <summary>
        /// Loads all `LocalizationSource`s from the given directory.
        /// Makes sure the built-in source is here.
        /// </summary>
        /// <returns>Enumerable collection of all loaded localization sources.</returns>
        public static IEnumerable<LocalizationSource> LoadAll(string directory)
        {
            bool foundBuiltIn = false;

            if (Directory.Exists(directory)) 
            {
                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    if (!File.Exists(Path.Combine(subDirectory, configFileName)))
                        continue;

                    if (GetNameFromDirectoryPath(subDirectory) == builtInSourceName)
                        foundBuiltIn = true;

                    yield return Load(subDirectory);
                }
            }

            if (!foundBuiltIn)
            {
                var subDirectory = Path.Combine(directory, builtInSourceName);
                var instance = new LocalizationSource(GetBuiltInLanguageSourceFromAssets(), subDirectory, false);
                instance.Save();
                yield return instance;
            }
        }

        public static LanguageSourceData GetBuiltInLanguageSourceFromAssets()
        {
            var found = LocalizationManager.Sources.Find(x => x.IsGlobalSource());
            if (found != null) return found;

            LanguageSourceAsset asset = ResourceManager.pInstance.GetAsset<LanguageSourceAsset>(LocalizationManager.GlobalSources[0]);
            return asset.SourceData;
        }

        private static string GetNameFromDirectoryPath(string path)
        {
            var parts = path.Split(new char[] {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            }, options: StringSplitOptions.RemoveEmptyEntries);
            return parts.Last();
        }
    }
}
