using HarmonyLib;
using System.Linq;
using UnityEngine;
using static StateMachine;

namespace Sungaila.SurvivalNotRequired.Patches
{
    /// <summary>
    /// Dispense oxygen, water and provide some power. 
    /// </summary>
    [HarmonyPatch(typeof(Telepad.States))]
    public static class TelepadStatesInstancePatch
    {
        [HarmonyPatch(typeof(Telepad.States), nameof(Telepad.States.InitializeStates))]
        [HarmonyPostfix]
        public static void InitializeStatesPostfix(Telepad.States __instance, BaseState default_state)
        {
            // do additional stuff whenever the telepad is intact
            __instance.opening.Update(DoAdditionalStuffHandler, UpdateRate.SIM_200ms);
            __instance.idle.Update(DoAdditionalStuffHandler, UpdateRate.SIM_200ms);
            __instance.open.Update(DoAdditionalStuffHandler, UpdateRate.SIM_200ms);
            __instance.unoperational.Update(DoAdditionalStuffHandler, UpdateRate.SIM_200ms);
        }

        private static void DoAdditionalStuffHandler(Telepad.StatesInstance smi, float dt)
        {
            smi.master.TryGetComponent<Operational>(out var operational);

            // do not generate or dispense if the telepad is not operational
            if (operational?.IsOperational != true)
                return;

            // get element converter and storage
            if (!smi.master.TryGetComponent<ElementConverter>(out var elementConverter))
                return;

            // dispense oxygen
            if (ModSettings.Instance.EnableGas && smi.master.GetComponents<Storage>().FirstOrDefault(c => c.storageFilters.SingleOrDefault() == GameTags.Oxygen) is Storage storageGas)
            {
                var outputElement = elementConverter.outputElements.First(o => o.elementHash == SimHashes.Oxygen);
                float outputMass = outputElement.massGenerationRate * elementConverter.OutputMultiplier * dt;
                outputMass = Mathf.Max(Mathf.Min(outputMass, storageGas.RemainingCapacity()), 0);
                Game.Instance.accumulators.Accumulate(outputElement.accumulator, outputMass);

                var component = smi.master.GetComponent<PrimaryElement>();
                float temperature = outputElement.useEntityTemperature
                    ? component.Temperature
                    : Mathf.Max(outputElement.minOutputTemperature, component.Temperature);

                storageGas.AddGasChunk(SimHashes.Oxygen, outputMass, temperature, default, default, false);

                ReportManager.Instance.ReportValue(ReportManager.ReportType.OxygenCreated, outputMass, elementConverter.gameObject.GetProperName());
            }

            // dispense water
            if (ModSettings.Instance.EnableLiquid && smi.master.GetComponents<Storage>().FirstOrDefault(c => c.storageFilters.SingleOrDefault() == GameTags.Water) is Storage storageLiquid)
            {
                var outputElement = elementConverter.outputElements.First(o => o.elementHash == SimHashes.Water);
                float outputMass = outputElement.massGenerationRate * elementConverter.OutputMultiplier * dt;
                outputMass = Mathf.Max(Mathf.Min(outputMass, storageLiquid.RemainingCapacity()), 0);
                Game.Instance.accumulators.Accumulate(outputElement.accumulator, outputMass);

                var component = smi.master.GetComponent<PrimaryElement>();
                float temperature = outputElement.useEntityTemperature
                    ? component.Temperature
                    : Mathf.Max(outputElement.minOutputTemperature, component.Temperature);

                storageLiquid.AddLiquid(SimHashes.Water, outputMass, temperature, default, default, false);
            }
        }
    }
}