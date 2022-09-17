using HarmonyLib;
using System.Collections.Generic;
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
        internal static readonly Tag StorageTag = TagManager.Create("6DB61B5E-2CC7-4354-BFAE-A0577DD7D65B");
        internal static readonly Tag ConduitDispenserTag = TagManager.Create("32EAE057-3F2E-44F0-833A-D9A212CD5F21");
        internal static readonly Tag ElementConverterTag = TagManager.Create("051FC465-2947-453C-BE2E-FEAFDF51B5EA");
        internal static readonly Tag GeneratorTag = TagManager.Create("B217C2D3-AAA6-4D7C-B079-C8EC0332BCAC");

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
            Storage storage = go.AddComponentAndTag<Storage>(StorageTag);
            storage.capacityKg = TelepadStatesInstancePatch.CapacityInKg;
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);

            ConduitDispenser conduitDispenser = go.AddComponentAndTag<ConduitDispenser>(ConduitDispenserTag);
            conduitDispenser.alwaysDispense = true;
            conduitDispenser.conduitType = ConduitType.Gas;
            conduitDispenser.elementFilter = new SimHashes[1]
            {
                SimHashes.Oxygen
            };

            // this converter is used indirectly but won't be converting anything
            ElementConverter elementConverter = go.AddComponentAndTag<ElementConverter>(ElementConverterTag);
            elementConverter.OutputMultiplier = 1f;
            elementConverter.outputElements = new ElementConverter.OutputElement[]
            {
                new ElementConverter.OutputElement(TelepadStatesInstancePatch.OxygenOutputInKgPerSecond, SimHashes.Oxygen, TelepadStatesInstancePatch.OxygenMinTemperatureInKelvin, false, true)
            };

            Generator generator = go.AddComponentAndTag<Generator>(GeneratorTag);
            generator.powerDistributionOrder = 10;
        }
    }
}