using HarmonyLib;
using I2.Loc;
using System;

namespace OpenLocalization
{
    static class LocalizationManagerPatches
    {   
        [HarmonyPatch(typeof(LocalizationManager), "RegisterSourceInResources")]
        static class RegisterSourceInResourcesPatch
        {
            /// <summary>
            /// Function to be executed before the actual patched function.
            /// </summary>
            /// <param name="__state">
            /// Index of next `LocalizationSource` to be added for the `LocalizationManager` 
            /// after the original function run, which will load original assets.
            /// If set to -1, there is no sources to add after the original function run.
            /// </param>
            /// <returns>Returns `true` is original function should run, `false` to prevent it. </returns>
            [HarmonyPrefix]
            static bool Prefix(out int __state)
            {
                __state = -1;
                bool skipAssets = OpenLocalization.GetConfig().SkipAssets.Value;
                if (skipAssets)
                {
                    foreach (var thing in OpenLocalization.GetInstance().LocalizationSources)
                    {
                        AddSource(thing.Data);
                    }
                    return false; // prevents calling original function
                }
                else
                {
                    var list = OpenLocalization.GetInstance().LocalizationSources;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Name == LocalizationSource.builtInSourceName)
                        {
                            __state = i + 1;
                            break;
                        }
                        AddSource(list[i].Data);
                    }
                    return true;
                }
            }

            [HarmonyPostfix]
            static void Postfix(int __state)
            {
                if (__state < 0) return;

                var list = OpenLocalization.GetInstance().LocalizationSources;
                for (int i = __state; i < list.Count; i++)
                {
                    AddSource(list[i].Data);
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

        public static void ReloadSources()
        {
            LocalizationManager.Sources.Clear();
            LocalizationManager.UpdateSources();
        }
    }
}
