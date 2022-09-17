using System;
using YamlDotNet.Serialization;

namespace SurvivalNotRequired
{
    [Serializable]
    internal class ModSettings
    {
        [YamlMember(Alias = "oxygenMinTemperatureInKelvin")]
        public float? OxygenMinTemperatureInKelvin { get; set; }

        [YamlMember(Alias = "oxygenOutputInKgPerSecond")]
        public float? OxygenOutputInKgPerSecond { get; set; }

        [YamlMember(Alias = "wattageRating")]
        public float? WattageRating { get; set; }

        [YamlMember(Alias = "selfHeatKilowattsWhenActive")]
        public float? SelfHeatKilowattsWhenActive { get; set; }

        [YamlMember(Alias = "extendMiniPod")]
        public bool? ExtendMiniPod { get; set; }

        [YamlMember(Alias = "capacityInKg")]
        public float? CapacityInKg { get; set; }
    }
}