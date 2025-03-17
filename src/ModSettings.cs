using System;
using YamlDotNet.Serialization;

namespace SurvivalNotRequired
{
    [Serializable]
    internal class ModSettings
    {
        [YamlIgnore]
        public static ModSettings Instance { get; internal set; } = new();

        [YamlMember(Alias = "oxygenMinTemperatureInKelvin")]
        public float OxygenMinTemperatureInKelvin { get; set; } = 293.15f; // that's 20°C

        [YamlMember(Alias = "waterMinTemperatureInKelvin")]
        public float WaterMinTemperatureInKelvin { get; set; } = 293.15f; // that's 20°C

        [YamlMember(Alias = "oxygenOutputInKgPerSecond")]
        public float OxygenOutputInKgPerSecond { get; set; } = 0.5f;

        [YamlMember(Alias = "waterOutputInKgPerSecond")]
        public float WaterOutputInKgPerSecond { get; set; } = 0.1f;

        [YamlMember(Alias = "wattageRating")]
        public float WattageRating { get; set; } = 400f;

        [YamlMember(Alias = "selfHeatKilowattsWhenActive")]
        public float SelfHeatKilowattsWhenActive { get; set; } = 1f; // that's 1,000 DTU/s

        [YamlMember(Alias = "extendMiniPod")]
        public bool ExtendMiniPod { get; set; } = true;

        [YamlMember(Alias = "capacityGasInKg")]
        public float CapacityGasInKg { get; set; } = 10f;

        [YamlMember(Alias = "capacityLiquidInKg")]
        public float CapacityLiquidInKg { get; set; } = 10f;

        [YamlMember(Alias = "enablePower")]
        public bool EnablePower { get; set; } = true;

        [YamlMember(Alias = "enableGas")]
        public bool EnableGas { get; set; } = true;

        [YamlMember(Alias = "enableLiquid")]
        public bool EnableLiquid { get; set; } = true;

        [YamlMember(Alias = "enableTemplateGasVent")]
        public bool EnableTemplateGasVent { get; set; } = true;

        [YamlMember(Alias = "enableTemplateFloorLamp")]
        public bool EnableTemplateFloorLamp { get; set; } = true;

        [YamlMember(Alias = "enableTemplateLiquidValve")]
        public bool EnableTemplateLiquidValve { get; set; } = true;
    }
}