using BepInEx;
using HarmonyLib;
using I2.Loc;
using System;
using System.IO;
using UnityEngine;

namespace OpenLocalization
{
    class LocalizationManagerPatches
    {   
        [HarmonyPatch(typeof(LocalizationManager), "RegisterSourceInResources")]
        public class RegisterSourceInResourcesPatch
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                var loadMode = OpenLocalization.GetConfig().LoadMode.Value;
                switch (loadMode)
                {
                    case LoadMode.BeforeBuiltIns:
                    case LoadMode.Replace:
                        AddSource(GetExternalLanguageSource());
                        break;
                }
                if (loadMode == LoadMode.Replace)
                {
                    return false; // prevents calling original function
                }
                return true;
            }

            [HarmonyPostfix]
            static void Postfix()
            {
                var loadMode = OpenLocalization.GetConfig().LoadMode.Value;
                if (loadMode == LoadMode.AfterBuiltIns)
                {
                    AddSource(GetExternalLanguageSource());
                }
            }
        }

        public static void AddSource(LanguageSourceData source)
        {
            AccessTools.Method(typeof(LocalizationManager), "AddSource").Invoke(null, new object[] { source });
        }

        public static void RemoveSource(LanguageSourceData source)
        {
            AccessTools.Method(typeof(LocalizationManager), "RemoveSource").Invoke(null, new object[] { source });
        }

        public static LanguageSourceData ImportLanguageSource(string directoryPath)
        {
            OpenLocalization.GetLogger().LogInfo($"Importing from directory '{directoryPath}'");
            LanguageSourceData source = new LanguageSourceData();
            foreach (string path in Directory.GetFiles(directoryPath))
            {
                string category = Path.GetFileNameWithoutExtension(path);
                string content = File.ReadAllText(path);
                OpenLocalization.GetLogger().LogDebug($"Importing translations for category '{category}' from file '{path}'");
                source.Import_CSV(category, content, eSpreadsheetUpdateMode.Merge);
            }
            OpenLocalization.GetLogger().LogInfo(String.Format("Languages: {0}", String.Join(", ", source.GetLanguages())));
            return source;
        }

        public static void ExportLanguageSource(LanguageSourceData source, string outputDirectoryPath)
        {
            OpenLocalization.GetLogger().LogInfo($"Exporting to directory '{outputDirectoryPath}'");
            Directory.CreateDirectory(outputDirectoryPath);
            foreach (string category in source.GetCategories(true, null))
            {
                string outputPath = outputDirectoryPath + "/" + category + ".csv";
                OpenLocalization.GetLogger().LogDebug($"Exporting category '{category}' to file '{outputPath}'");
                File.WriteAllText(outputPath, source.Export_CSV(category, ',', true));
            }
        }

        static LanguageSourceData _externalSource = null;

        public static LanguageSourceData GetExternalLanguageSource()
        {
            if (_externalSource == null)
            {
                _externalSource = ImportLanguageSource(Path.Combine(Application.dataPath, "Localization"));
            }
            return _externalSource;
        }

        public static LanguageSourceData GetBuiltInLanguageSource()
        {
            var found = LocalizationManager.Sources.Find(x => x.IsGlobalSource());
            if (found != null) return found;

            LanguageSourceAsset asset = ResourceManager.pInstance.GetAsset<LanguageSourceAsset>(LocalizationManager.GlobalSources[0]);
            return asset.SourceData;
        }

        public static void ExportBuiltInLanguageSource(string onlyLanguage = null)
        {
            var source = GetBuiltInLanguageSource();
            if (!string.IsNullOrWhiteSpace(onlyLanguage)) {
                var languages = source.GetLanguages();
                if (!languages.Contains(onlyLanguage))
                {
                    OpenLocalization.GetLogger().LogWarning(String.Format("Invalid language selected for exporting. Avaliable languages: ", String.Join(", ", languages)));
                }
                foreach (string language in languages)
                {
                    if (language == onlyLanguage) continue;
                    source.RemoveLanguage(language);
                }
            }
            ExportLanguageSource(source, Path.Combine(Application.dataPath, "LocalizationExport"));
        }

        public static void ReloadLocalizations()
        {
            OpenLocalization.GetLogger().LogInfo("Reloading...");
            LocalizationManager.Sources.Clear();
            _externalSource = null;
            LocalizationManager.UpdateSources();
        }
    }
}
