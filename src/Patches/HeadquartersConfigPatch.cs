using HarmonyLib;
using UnityEngine;

namespace SurvivalNotRequired.Patches
{
    /// <summary>
    /// Modifies the headquarters building definition.
    /// Now it dispenses a baseline of oxygen and electricity.
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
            ModifyBuildingDef(ref __result, new CellOffset(-1, 0), new CellOffset(2, 0));
        }

        internal static void ModifyBuildingDef(ref BuildingDef buildingDef, CellOffset utilityOutputOffset, CellOffset powerOutputOffset)
        {
            // the headquarters should be shown in every relevant overlay
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, HeadquartersConfig.ID);
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.WireIDs, HeadquartersConfig.ID);

            // there is a gas output at the bottom left corner
            buildingDef.OutputConduitType = ConduitType.Gas;
            buildingDef.UtilityOutputOffset = utilityOutputOffset;

            // and a power output at the bottom right corner
            buildingDef.GeneratorWattageRating = TelepadStatesInstancePatch.WattageRating;
            buildingDef.GeneratorBaseCapacity = 20000f;
            buildingDef.RequiresPowerOutput = true;
            buildingDef.PowerOutputOffset = powerOutputOffset;
            buildingDef.SelfHeatKilowattsWhenActive = TelepadStatesInstancePatch.SelfHeatKilowattsWhenActive;
        }

        /// <summary>
        /// Postfix for <see cref="HeadquartersConfig.ConfigureBuildingTemplate"/>.
        /// </summary>
        [HarmonyPatch(typeof(HeadquartersConfig), nameof(HeadquartersConfig.ConfigureBuildingTemplate))]
        [HarmonyPostfix]
        public static void ConfigureBuildingTemplatePostfix(GameObject go)
        {
            ModifyBuildingTemplate(ref go);
        }

        internal static void ModifyBuildingTemplate(ref GameObject go)
        {
            Storage defaultStorage = BuildingTemplates.CreateDefaultStorage(go);
            defaultStorage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);

            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.alwaysDispense = true;
            conduitDispenser.conduitType = ConduitType.Gas;
            conduitDispenser.elementFilter = new SimHashes[1]
            {
                SimHashes.Oxygen
            };

            // this converter is used indirectly but won't be converting anything
            ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
            elementConverter.OutputMultiplier = 1f;
            elementConverter.outputElements = new ElementConverter.OutputElement[]
            {
                new ElementConverter.OutputElement(TelepadStatesInstancePatch.OxygenOutputInKgPerSecond, SimHashes.Oxygen, TelepadStatesInstancePatch.OxygenMinTemperatureInKelvin, false, true)
            };

            Generator generator = go.AddOrGet<Generator>();
            generator.powerDistributionOrder = 10;
        }
    }
}