using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace OpenLocalization
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInProcess("Pharaoh.exe")]
    public class OpenLocalization : BaseUnityPlugin
    {
        public const string pluginGUID= "com.github.AgainPsychoX.PANE.OpenLocalization";
        public const string pluginName = "OpenLocalization";
        public const string pluginVersion = "0.1.0.0";

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
            Logger.LogInfo($"Plugin {this.Info.Metadata.Name} is loaded!");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), pluginGUID);
        }
    }
}
