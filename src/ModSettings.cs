using System;
using YamlDotNet.Serialization;

namespace SurvivalNotRequired
{
    [Serializable]
    public class ModSettings
    {
        [YamlMember(Alias = "oxygenMinTemperatureInKelvin")]
        public float? OxygenMinTemperatureInKelvin { get; set; }

        [YamlMember(Alias = "oxygenOutputInKgPerSecond")]
        public float? OxygenOutputInKgPerSecond { get; set; }

        [YamlMember(Alias = "wattageRating")]
        public float? WattageRating { get; set; }

        [YamlMember(Alias = "selfHeatKilowattsWhenActive")]
        public float? SelfHeatKilowattsWhenActive { get; set; }
    }
}