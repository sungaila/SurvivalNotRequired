using HarmonyLib;
using Klei;
using KMod;
using SurvivalNotRequired.Patches;
using System;
using System.IO;

namespace SurvivalNotRequired
{
    /// <summary>
    /// Loading the mod settings before anything is patched.
    /// </summary>
    public class MyUserMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            try
            {
                var settingsPath = Path.Combine(Path.GetDirectoryName(typeof(MyUserMod).Assembly.Location), "settings.yaml");
                var settings = YamlIO.LoadFile<ModSettings>(
                    settingsPath,
                    (error, _) => Debug.LogWarning($"Failed to load settings.yaml for the mod SurvivalNotRequired. Using default values as fallback. Error: {error.message}"));

                if (settings.OxygenMinTemperatureInKelvin.HasValue)
                    TelepadStatesInstancePatch.OxygenMinTemperatureInKelvin = settings.OxygenMinTemperatureInKelvin.Value;

                if (settings.OxygenOutputInKgPerSecond.HasValue)
                    TelepadStatesInstancePatch.OxygenOutputInKgPerSecond = settings.OxygenOutputInKgPerSecond.Value;

                if (settings.WattageRating.HasValue)
                    TelepadStatesInstancePatch.WattageRating = settings.WattageRating.Value;

                if (settings.SelfHeatKilowattsWhenActive.HasValue)
                    TelepadStatesInstancePatch.SelfHeatKilowattsWhenActive = settings.SelfHeatKilowattsWhenActive.Value;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load settings.yaml for the mod SurvivalNotRequired. Using default values as fallback. Exception: {ex}");
            }

            base.OnLoad(harmony);
        }
    }
}