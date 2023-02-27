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
        class RegisterSourceInResourcesPatch
        {
            [HarmonyPrefix]
            static void Prefix()
            {
                ImportLanguageSource(Path.Combine(Application.dataPath, "Localization"));
            }

            static void AddSource(LanguageSourceData source)
            {
                AccessTools.Method(typeof(LocalizationManager), "AddSource").Invoke(null, new object[] { source });
            }

            static void ImportLanguageSource(string directoryPath)
            {
                LanguageSourceData source = new LanguageSourceData();

                foreach (string path in Directory.GetFiles(directoryPath))
                {
                    string category = Path.GetFileNameWithoutExtension(path);
                    string content = File.ReadAllText(path);
                    OpenLocalization.GetLogger().LogInfo($"Importing translations for category '{category}' from file '{path}'...");
                    source.Import_CSV(category, content, eSpreadsheetUpdateMode.Merge);
                }

                AddSource(source);
                OpenLocalization.GetLogger().LogInfo(String.Format("Languages: ", String.Join(", ", source.GetLanguagesCode())));
            }

            static void ExportLanguageSource(LanguageSourceData source, string outputDirectoryPath)
            {
                OpenLocalization.GetLogger().LogInfo("Exporting to directory: " + outputDirectoryPath);
                Directory.CreateDirectory(outputDirectoryPath);
                foreach (string category in source.GetCategories(true, null))
                {
                    string outputPath = outputDirectoryPath + "/" + category + ".csv";
                    OpenLocalization.GetLogger().LogInfo("Exporting category: " + category + " to path: " + outputPath);
                    File.WriteAllText(outputPath, source.Export_CSV(category, ',', true));
                }
            }

            static LanguageSourceData GetBuiltInLanguageSource()
            {
                return LocalizationManager.Sources.Find(x => x.FindAsset("I2Languages") != null);
            }

            static void ExportBuiltInLanguageSource()
            {
                ExportLanguageSource(GetBuiltInLanguageSource(), Path.Combine(Application.dataPath, "LocalizationExport"));
            }
        }
    }
}
