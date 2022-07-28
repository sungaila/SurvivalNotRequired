using HarmonyLib;
using Klei.CustomSettings;

namespace SurvivalNotRequired.Patches
{
    /// <summary>
    /// Sets calories burning setting to Tummyless (<see cref="STRINGS.UI.FRONTEND.CUSTOMGAMESETTINGSSCREEN.SETTINGS.CALORIE_BURN.LEVELS.DISABLED"/>).
    /// Not in use for now.
    /// </summary>
    [HarmonyPatch(typeof(CustomGameSettings))]
    public static class CustomGameSettingsPatch
    {
        /// <summary>
        /// Prefix for <see cref="CustomGameSettings.GetCurrentQualitySetting(SettingConfig)"/>.
        /// </summary>
        [HarmonyPatch(typeof(CustomGameSettings), nameof(CustomGameSettings.GetCurrentQualitySetting), new[] { typeof(SettingConfig) })]
        public static bool Prefix(SettingConfig setting, ref SettingLevel __result)
        {
            return Prefix(setting.id, ref __result);
        }

        /// <summary>
        /// Prefix for <see cref="CustomGameSettings.GetCurrentQualitySetting(string)"/>.
        /// </summary>
        /// <param name="setting_id"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(CustomGameSettings), nameof(CustomGameSettings.GetCurrentQualitySetting), new[] { typeof(string) })]
        public static bool Prefix(string setting_id, ref SettingLevel __result)
        {
            if (setting_id != nameof(CustomGameSettingConfigs.CalorieBurn))
                return true;

            __result = CustomGameSettingConfigs.CalorieBurn.GetLevel("Disabled");
            return false;
        }
    }
}