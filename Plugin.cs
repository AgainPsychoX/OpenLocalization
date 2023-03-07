using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace OpenLocalization
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInProcess("Pharaoh.exe")]
    public class OpenLocalization : BaseUnityPlugin
    {
        public const string pluginGUID = "com.github.AgainPsychoX.PANE.OpenLocalization";
        public const string pluginName = "OpenLocalization";
        public const string pluginVersion = "0.2.0.0";

        private static OpenLocalization _instance;
        public static OpenLocalization GetInstance()
        {
            return _instance;
        }
        public static ManualLogSource GetLogger()
        {
            return _instance.Logger;
        }

        void Awake()
        {
            _instance = this;
            PrepareConfigEntries();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), pluginGUID);
            ReloadLocalizations();
            Logger.LogInfo($"Plugin {this.Info.Metadata.Name} is loaded!");
        }

        public MyConfig ConfigEntries;
        public static MyConfig GetConfig()
        {
            return _instance.ConfigEntries;
        }

        public class MyConfig
        {
            internal ConfigEntry<string> LocalizationsDirectoryRaw;
            public ConfigEntry<bool> SkipAssets;

            public string LocalizationsDirectory
            {
                get {
                    return Path.GetFullPath(LocalizationsDirectoryRaw.Value);
                }
                set {
                    LocalizationsDirectoryRaw.Value = PathUtils.GetPathPossiblyRelativeToGameRoot(value);
                }
            }
        }
        protected void PrepareConfigEntries()
        {
            ConfigEntries = new MyConfig();

            ConfigEntries.LocalizationsDirectoryRaw = Config.Bind("General", "LocalizationsDirectory", PathUtils.GetPathPossiblyRelativeToGameRoot(DefaultLocalizationsDirectory),
                new ConfigDescription("Directory path used for storing and loading the localizations files."));
            ConfigEntries.SkipAssets = Config.Bind("General", "SkipAssets", true,
                new ConfigDescription("Skips loading the built-in localizations from assets. If no localization files are found, the assets will be converted to the files then used."));
            ConfigEntries.SkipAssets.SettingChanged += SkipAssetsSettingChanged;

            Config.Bind("General", "_Reload", false,
                new ConfigDescription("Button to reload the localizations", null,
                    new ConfigurationManagerAttributes
                    {
                        HideSettingName = true,
                        HideDefaultButton = true,
                        ReadOnly = true,
                        CustomDrawer = (ConfigEntryBase entry) => {
                            if (GUILayout.Button("Reload the localizations", GUILayout.ExpandWidth(true)))
                            {
                                ReloadLocalizations();
                                entry.BoxedValue = false;
                            }
                        },
                    }
                )
            );
            Config.Bind("General", "_Update", false,
                new ConfigDescription("Button to update the localizations from online spreadsheets.", null,
                    new ConfigurationManagerAttributes
                    {
                        HideSettingName = true,
                        HideDefaultButton = true,
                        ReadOnly = true,
                        CustomDrawer = (ConfigEntryBase entry) => {
                            if (GUILayout.Button("Update the localizations from online", GUILayout.ExpandWidth(true)))
                            {
                                UpdateLocalizations();
                                entry.BoxedValue = false;
                            }
                        },
                    }
                )
            );

            Config.Bind("Information", "_Info", "",
                new ConfigDescription("", null,
                    new ConfigurationManagerAttributes
                    {
                        HideSettingName = true,
                        HideDefaultButton = true,
                        ReadOnly = true,
                        CustomDrawer = (ConfigEntryBase entry) =>
                        {
                            GUILayout.Label(
                                String.Format("Built-in languages: \n{0}",
                                    String.Join("\n", BuiltInLocalizationSource.Data.GetLanguages().ConvertAll(x => "+ " + x))),
                                GUILayout.ExpandWidth(true)
                            );
                            GUILayout.Label(
                                 String.Format("All languages: \n{0}",
                                     String.Join("\n", I2.Loc.LocalizationManager.GetAllLanguages().ConvertAll(x => "+ " + x))),
                                 GUILayout.ExpandWidth(true)
                            );
                            GUILayout.BeginVertical();
                            GUILayout.Label("Mod written by AgainPsychoX#4444.\nSpecial thanks to Danie!#9942 and Takia_Gecko#1037.", GUILayout.ExpandWidth(true));
                            if (GUILayout.Button("Join our Discord!", GUILayout.ExpandWidth(true)))
                            {
                                System.Diagnostics.Process.Start("https://discord.gg/n5phrj222e");
                            }
                            GUILayout.EndVertical();
                        },
                    }
                )
            );
        }

        private void SkipAssetsSettingChanged(object sender, EventArgs e)
        {
            if (ConfigEntries.SkipAssets.Value)
            {
                BuiltInLocalizationSource.Load();
            }
            else
            {
                BuiltInLocalizationSource.SourceData = LocalizationSource.GetBuiltInLanguageSourceFromAssets();
            }
        }

        static public string DefaultLocalizationsDirectory
        {
            get {
                return Path.Combine(BepInEx.Paths.GameRootPath, "Localizations");
            }
        }

        public List<LocalizationSource> LocalizationSources { get; protected set;  }

        public LocalizationSource BuiltInLocalizationSource { get; protected set; }

        protected void ReloadLocalizations()
        {
            Logger.LogInfo("Reloading...");
            LocalizationSources?.Clear();
            LocalizationSources = LocalizationSource.LoadAll(GetConfig().LocalizationsDirectory).OrderBy(s => s.Order).ToList();
            BuiltInLocalizationSource = LocalizationSources.Find(s => s.Name == LocalizationSource.builtInSourceName);
            if (ConfigEntries.SkipAssets.Value)
            {
                BuiltInLocalizationSource.SourceData = LocalizationSource.GetBuiltInLanguageSourceFromAssets();
            }
            LocalizationManagerPatches.ReloadSources();
            Logger.LogInfo("Reloaded!");
        }

        protected void UpdateLocalizations()
        {
            Logger.LogInfo("Updating...");
            foreach (var source in LocalizationSources)
            {
                source.Update();
            }
            Logger.LogInfo("Updated!");
        }
    }
}
