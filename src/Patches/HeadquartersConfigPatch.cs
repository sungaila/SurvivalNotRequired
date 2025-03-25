using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Sungaila.SurvivalNotRequired.Patches
{
    /// <summary>
    /// Modifies the headquarters building definition.
    /// Now it dispenses a baseline of oxygen, water and electricity.
    /// </summary>
    [HarmonyPatch(typeof(HeadquartersConfig))]
    public static class HeadquartersConfigPatch
    {
        /// <summary>
        /// Postfix for <see cref="HeadquartersConfig.CreateBuildingDef"/>.
        /// </summary>
        [HarmonyPatch(typeof(HeadquartersConfig), nameof(HeadquartersConfig.CreateBuildingDef))]
        [HarmonyPostfix]
        public static void CreateBuildingDefPostfix(ref BuildingDef __result)
        {
            ModifyBuildingDef(ref __result, new CellOffset(2, 0), HeadquartersConfig.ID);
        }

        internal static void ModifyBuildingDef(ref BuildingDef buildingDef, CellOffset powerOutputOffset, string configId)
        {
            // a power output at the bottom right corner
            if (ModSettings.Instance.EnablePower)
            {
                buildingDef.GeneratorWattageRating = ModSettings.Instance.WattageRating;
                buildingDef.GeneratorBaseCapacity = 20000f;
                buildingDef.RequiresPowerOutput = true;
                buildingDef.PowerOutputOffset = powerOutputOffset;
                buildingDef.SelfHeatKilowattsWhenActive = ModSettings.Instance.SelfHeatKilowattsWhenActive;

                GeneratedBuildings.RegisterWithOverlay(OverlayScreen.WireIDs, configId);
            }

            // a gas output at the bottom left corner
            if (ModSettings.Instance.EnableGas)
            {
                GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, configId);
            }

            // a gas output at the bottom right corner
            if (ModSettings.Instance.EnableLiquid)
            {
                GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, configId);
            }
        }

        /// <summary>
        /// Postfix for <see cref="HeadquartersConfig.ConfigureBuildingTemplate"/>.
        /// </summary>
        [HarmonyPatch(typeof(HeadquartersConfig), nameof(HeadquartersConfig.ConfigureBuildingTemplate))]
        [HarmonyPostfix]
        public static void ConfigureBuildingTemplatePostfix(GameObject go, Tag prefab_tag)
        {
            ModifyBuildingTemplate(ref go, new CellOffset(-1, 0), new CellOffset(2, 0));
        }

        internal static void ModifyBuildingTemplate(ref GameObject go, CellOffset gasOutputOffset, CellOffset liquidOutputOffset)
        {
            if (ModSettings.Instance.EnablePower)
            {
                Generator generator = go.AddComponent<Generator>();
                generator.powerDistributionOrder = 10;
            }

            if (!ModSettings.Instance.EnableGas && !ModSettings.Instance.EnableLiquid)
                return;

            ElementConverter elementConverter = go.AddComponent<ElementConverter>();
            elementConverter.OutputMultiplier = 1f;

            var outputElements = new List<ElementConverter.OutputElement>();

            if (ModSettings.Instance.EnableGas)
            {
                Storage storageGas = go.AddComponent<Storage>();
                storageGas.capacityKg = ModSettings.Instance.CapacityGasInKg;
                storageGas.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
                storageGas.storageFilters = [GameTags.Oxygen];

                ConduitDispenser conduitDispenserGas = go.AddComponent<ConduitDispenser>();
                conduitDispenserGas.alwaysDispense = true;
                conduitDispenserGas.conduitType = ConduitType.Gas;
                conduitDispenserGas.storage = storageGas;
                conduitDispenserGas.elementFilter =
                [
                    SimHashes.Oxygen
                ];
                conduitDispenserGas.useSecondaryOutput = true;

                ConduitSecondaryOutput conduitSecondaryOutput = conduitDispenserGas.gameObject.AddComponent<ConduitSecondaryOutput>();
                conduitSecondaryOutput.portInfo = new ConduitPortInfo(ConduitType.Gas, gasOutputOffset);

                outputElements.Add(new ElementConverter.OutputElement(ModSettings.Instance.OxygenOutputInKgPerSecond, SimHashes.Oxygen, minOutputTemperature: ModSettings.Instance.OxygenMinTemperatureInKelvin, storeOutput: true));
            }

            if (ModSettings.Instance.EnableLiquid)
            {
                Storage storageLiquid = go.AddComponent<Storage>();
                storageLiquid.capacityKg = ModSettings.Instance.CapacityLiquidInKg;
                storageLiquid.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
                storageLiquid.storageFilters = [GameTags.Water];

                ConduitDispenser conduitDispenserLiquid = go.AddComponent<ConduitDispenser>();
                conduitDispenserLiquid.alwaysDispense = true;
                conduitDispenserLiquid.conduitType = ConduitType.Liquid;
                conduitDispenserLiquid.storage = storageLiquid;
                conduitDispenserLiquid.elementFilter =
                [
                    SimHashes.Water
                ];
                conduitDispenserLiquid.useSecondaryOutput = true;

                ConduitSecondaryOutput conduitSecondaryOutput = conduitDispenserLiquid.gameObject.AddComponent<ConduitSecondaryOutput>();
                conduitSecondaryOutput.portInfo = new ConduitPortInfo(ConduitType.Liquid, liquidOutputOffset);

                outputElements.Add(new ElementConverter.OutputElement(ModSettings.Instance.WaterOutputInKgPerSecond, SimHashes.Water, minOutputTemperature: ModSettings.Instance.WaterMinTemperatureInKelvin, storeOutput: true));
            }

            elementConverter.outputElements = [.. outputElements];
        }
    }
}