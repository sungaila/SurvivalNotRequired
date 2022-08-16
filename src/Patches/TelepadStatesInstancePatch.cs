using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace SurvivalNotRequired.Patches
{
    /// <summary>
    /// Dispense oxygen and provide some power. 
    /// </summary>
    [HarmonyPatch(typeof(Telepad.States), nameof(Telepad.States.InitializeStates))]
    public static class TelepadStatesInstancePatch
    {
        public static float OxygenMinTemperatureInKelvin { get; internal set; } = 293.15f; // that's 20°C
        public static float OxygenOutputInKgPerSecond { get; internal set; } = 0.5f;
        public static float WattageRating { get; internal set; } = 400f;
        public static float SelfHeatKilowattsWhenActive { get; internal set; } = 1f; // that's 1,000 DTU/s
        public static bool ExtendMiniPod { get; internal set; } = false;

        public static void Postfix(Telepad.States __instance)
        {
            static void doAdditionalStuffHandler(Telepad.StatesInstance smi, float dt)
            {
                smi.master.TryGetComponent<Operational>(out var operational);

                // generate power
                if (smi.master.TryGetComponent<Generator>(out var generator))
                {
                    generator.EnergySim200ms(dt);
                    KSelectable component = smi.master.GetComponent<KSelectable>();
                    generator.GenerateJoules(generator.WattageRating * dt);
                    component.SetStatusItem(Db.Get().StatusItemCategories.Power, Db.Get().BuildingStatusItems.Wattage, generator);
                }

                // dispense oxygen
                if (smi.master.TryGetComponent<Storage>(out var storage) &&
                    smi.master.TryGetComponent<ElementConverter>(out var elementConverter) &&
                    operational?.IsOperational == true)
                {
                    var outputElement = elementConverter.outputElements.Single(o => o.elementHash == SimHashes.Oxygen);
                    float outputMass = outputElement.massGenerationRate * elementConverter.OutputMultiplier * dt;
                    Game.Instance.accumulators.Accumulate(outputElement.accumulator, outputMass);

                    var component = smi.master.GetComponent<PrimaryElement>();
                    float temperature = outputElement.useEntityTemperature
                        ? component.Temperature
                        : Mathf.Max(outputElement.minOutputTemperature, component.Temperature);

                    storage.AddGasChunk(SimHashes.Oxygen, outputMass, temperature, default, default, true);

                    ReportManager.Instance.ReportValue(ReportManager.ReportType.OxygenCreated, outputMass, elementConverter.gameObject.GetProperName());
                }
            }

            // do additional stuff whenever the telepad is intact
            __instance.opening.Update(doAdditionalStuffHandler, UpdateRate.SIM_200ms);
            __instance.idle.Update(doAdditionalStuffHandler, UpdateRate.SIM_200ms);
            __instance.open.Update(doAdditionalStuffHandler, UpdateRate.SIM_200ms);
            __instance.unoperational.Update(doAdditionalStuffHandler, UpdateRate.SIM_200ms);
        }
    }
}