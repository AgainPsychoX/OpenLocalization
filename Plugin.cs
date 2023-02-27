using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace OpenLocalization
{
    /// <summary>
    /// Localization load mode.
    /// </summary>
    public enum LoadMode
    {
        
        Disabled,
        [Description("External as fallbacks")]
        BeforeBuiltIns,
        [Description("Replace")]
        Replace,
        [Description("Built-ins as fallbacks")]
        AfterBuiltIns,
    }

    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInProcess("Pharaoh.exe")]
    public class OpenLocalization : BaseUnityPlugin
    {
        public const string pluginGUID = "com.github.AgainPsychoX.PANE.OpenLocalization";
        public const string pluginName = "OpenLocalization";
        public const string pluginVersion = "0.1.1.0";

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
            Logger.LogInfo($"Plugin {this.Info.Metadata.Name} is loaded!");
        }

        public MyConfig ConfigEntries;
        public static MyConfig GetConfig()
        {
            return _instance.ConfigEntries;
        }

        public class MyConfig
        {
            public ConfigEntry<LoadMode> LoadMode;
            public ConfigEntry<string> ExportOnlyLanguage;
        }
        protected void PrepareConfigEntries()
        {
            ConfigEntries = new MyConfig();

            ConfigEntries.LoadMode = Config.Bind("General", "Localization load mode", LoadMode.BeforeBuiltIns,
                new ConfigDescription("Decides how to load the external (unofficial) localizations related to the built-ins (official ones). You might need to restart the game for changes to apply."));
            Config.Bind("General", "_reloadButton", "",
                new ConfigDescription("Reloads external locaizations (from `Pharaoh_Data\\Localization` folder) for faster in-game testing.`)", null,
                    new ConfigurationManagerAttributes
                    {
                        HideSettingName = true,
                        HideDefaultButton = true,
                        ReadOnly = true,
                        CustomDrawer = (ConfigEntryBase entry) => {
                            if (GUILayout.Button("Reload external localization", GUILayout.ExpandWidth(true)))
                            {
                                LocalizationManagerPatches.ReloadLocalizations();
                            }
                        },
                    }
                )
            );

            var builtInLanguages = LocalizationManagerPatches.GetBuiltInLanguageSource().GetLanguages();
            ConfigEntries.ExportOnlyLanguage = Config.Bind("Exporting", "Export only language", "English",
                new ConfigDescription("", new AcceptableValueList<string>(builtInLanguages.ToArray()), new ConfigurationManagerAttributes { Order = 1999 }));
            Config.Bind("Exporting", "_exportButton", "",
                new ConfigDescription("Exports built-in localization (embedded in the game assets) into CSV files, into `Pharaoh_Data\\LocalizationExport` folder.", null,
                    new ConfigurationManagerAttributes
                    {
                        HideSettingName = true, 
                        HideDefaultButton = true, 
                        ReadOnly = true, 
                        Order = 1998,
                        CustomDrawer = (ConfigEntryBase entry) => {
                            if (GUILayout.Button("Export built-in localization", GUILayout.ExpandWidth(true)))
                            {
                                LocalizationManagerPatches.ExportBuiltInLanguageSource(ConfigEntries.ExportOnlyLanguage.Value);
                            }
                        },
                    }
                )
            );

            Config.Bind("Loaded languages", "_listLanguages", "",
                new ConfigDescription("", null,
                    new ConfigurationManagerAttributes
                    {
                        HideSettingName = true,
                        HideDefaultButton = true, 
                        ReadOnly = true,
                        CustomDrawer = (ConfigEntryBase entry) => {
                            GUILayout.Label(
                                String.Format("Built-in languages: \n{0}", 
                                    ConfigEntries.LoadMode.Value == LoadMode.Replace 
                                        ? "(replaced)" 
                                        : String.Join("\n", builtInLanguages.ConvertAll(x => "+ " + x))
                                ),
                                GUILayout.ExpandWidth(true)
                           );
                           GUILayout.Label(
                                String.Format("External languages: \n{0}",
                                    String.Join("\n", LocalizationManagerPatches.GetExternalLanguageSource().GetLanguages().ConvertAll(x => "+ " + x))
                                ),
                                GUILayout.ExpandWidth(true)
                           );
                            GUILayout.Label(
                                String.Format("All languages: \n{0}",
                                    String.Join("\n", I2.Loc.LocalizationManager.GetAllLanguages().ConvertAll(x => "+ " + x))
                                ),
                                GUILayout.ExpandWidth(true)
                           );
                        },
                    }
                )
            );
        }
    }
}
