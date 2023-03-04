using HarmonyLib;
using I2.Loc;
using System;

namespace OpenLocalization
{
    [HarmonyPatch(typeof(LanguageSourceData), "GetSourcePlayerPrefName")]
    class LanguageSourceDataPatches
    {
        [HarmonyPrefix]
        static bool Prefix(ref LanguageSourceData __instance, ref string __result)
        {
            if (__instance.owner is LocalizationSource source)
            {
                __result = $"OpenLocalization_{source.Name}";
                return false; // as we early return the new value
            }
            return true;
        }
    }
}
