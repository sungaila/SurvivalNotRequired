using HarmonyLib;
using ProcGen;
using ProcGenGame;
using System.Collections.Generic;
using System.Linq;
using TemplateClasses;
using UnityEngine;

namespace SurvivalNotRequired.Patches
{
    /// <summary>
    /// Adds a gas vent and a floor lamp to the starting base.
    /// </summary>
    [HarmonyPatch(typeof(TemplateSpawning), nameof(TemplateSpawning.DetermineTemplatesForWorld))]
    public static class StartingBasePatch
    {
        private static readonly List<string> _templatesPatched = [];

        /// <summary>
        /// Postfix for <see cref="TemplateSpawning.DetermineTemplatesForWorld"/>.
        /// </summary>
        public static void Postfix(WorldGenSettings settings, List<TerrainCell> terrainCells, SeededRandom myRandom, ref List<RectInt> placedPOIBounds, bool isRunningDebugGen, ref List<WorldTrait> placedStoryTraits, WorldGen.OfflineCallbackFunction successCallbackFn, ref List<TemplateSpawning.TemplateSpawner> __result)
        {
            // if nothing is enabled then skip patching the templates
            if (!ModSettings.Instance.EnableTemplateGasVent && !ModSettings.Instance.EnableTemplateFloorLamp && !ModSettings.Instance.EnableTemplateLiquidValve)
                return;

            if (string.IsNullOrEmpty(settings.world.startingBaseTemplate))
                return;

            var headquarterTemplates = __result
                .Where(t => t.container.buildings != null && t.container.buildings.Any(b => b.id == HeadquartersConfig.ID))
                .Select(t => t.container)
                .ToList();

            if (headquarterTemplates.Count == 0)
            {
                Debug.LogWarning($"No template containing a headquarter found. Skip building placements for the mod SurvivalNotRequired.");
                return;
            }

            // iterate through every template containing a headquarter
            // assume this is the starting base template
            foreach (var templateContainer in headquarterTemplates)
            {
                try
                {
                    // make sure that the templates won't be patched twice
                    if (_templatesPatched.Contains(templateContainer.name))
                        return;

                    // make sure there is a single headquarter in the template
                    if (templateContainer.buildings.FirstOrDefault(b => b.id == HeadquartersConfig.ID) is not Prefab headquarters)
                        continue;

                    // a gas vent to the left connected with gas pipes
                    if (ModSettings.Instance.EnableTemplateGasVent)
                    {
                        templateContainer.buildings.Add(new Prefab(
                            GasConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x - 1,
                            headquarters.location_y,
                            SimHashes.SedimentaryRock,
                            headquarters.temperature,
                            _connections: (int)UtilityConnections.Up));

                        templateContainer.buildings.Add(new Prefab(
                            GasConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x - 1,
                            headquarters.location_y + 1,
                            SimHashes.SedimentaryRock,
                            headquarters.temperature,
                            _connections: (int)(UtilityConnections.Up | UtilityConnections.Down)));

                        templateContainer.buildings.Add(new Prefab(
                            GasConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x - 1,
                            headquarters.location_y + 2,
                            SimHashes.SedimentaryRock,
                            headquarters.temperature,
                            _connections: (int)(UtilityConnections.Up | UtilityConnections.Down)));

                        templateContainer.buildings.Add(new Prefab(
                            GasConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x - 1,
                            headquarters.location_y + 3,
                            SimHashes.SedimentaryRock,
                            headquarters.temperature,
                            _connections: (int)(UtilityConnections.Left | UtilityConnections.Down)));

                        templateContainer.buildings.Add(new Prefab(
                            GasConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x - 2,
                            headquarters.location_y + 3,
                            SimHashes.SedimentaryRock,
                            headquarters.temperature,
                            _connections: (int)UtilityConnections.Right));

                        templateContainer.buildings.Add(new Prefab(
                            GasVentConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x - 2,
                            headquarters.location_y + 3,
                            SimHashes.IronOre,
                            headquarters.temperature));
                    }

                    // skip if something is taking this place to the right (like the Wood Pile in the Frosty Planet Pack DLC)
                    if (templateContainer.buildings.Any(b => b.location_x >= headquarters.location_x + 3 && b.location_y >= headquarters.location_y))
                        continue;

                    // a floor lamp to the right connected with wires
                    if (ModSettings.Instance.EnableTemplateFloorLamp)
                    {
                        templateContainer.buildings.Add(new Prefab(
                            WireConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 2,
                            headquarters.location_y,
                            SimHashes.IronOre,
                            headquarters.temperature,
                            _connections: (int)UtilityConnections.Down));

                        templateContainer.buildings.Add(new Prefab(
                            WireConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 2,
                            headquarters.location_y - 1,
                            SimHashes.IronOre,
                            headquarters.temperature,
                            _connections: (int)(UtilityConnections.Up | UtilityConnections.Right)));

                        templateContainer.buildings.Add(new Prefab(
                            WireConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 3,
                            headquarters.location_y - 1,
                            SimHashes.IronOre,
                            headquarters.temperature,
                            _connections: (int)(UtilityConnections.Left | UtilityConnections.Up)));

                        templateContainer.buildings.Add(new Prefab(
                            WireConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 3,
                            headquarters.location_y,
                            SimHashes.IronOre,
                            headquarters.temperature,
                            _connections: (int)UtilityConnections.Down));

                        templateContainer.buildings.Add(new Prefab(
                            FloorLampConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 3,
                            headquarters.location_y,
                            SimHashes.IronOre,
                            headquarters.temperature));
                    }

                    // a liquid valve to the right connected with pipes
                    if (ModSettings.Instance.EnableTemplateLiquidValve)
                    {
                        templateContainer.buildings.Add(new Prefab(
                            LiquidConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 2,
                            headquarters.location_y,
                            SimHashes.SandStone,
                            headquarters.temperature,
                            _connections: (int)UtilityConnections.Down));

                        templateContainer.buildings.Add(new Prefab(
                            LiquidConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 2,
                            headquarters.location_y - 1,
                            SimHashes.SandStone,
                            headquarters.temperature,
                            _connections: (int)(UtilityConnections.Up | UtilityConnections.Right)));

                        templateContainer.buildings.Add(new Prefab(
                            LiquidConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 3,
                            headquarters.location_y - 1,
                            SimHashes.SandStone,
                            headquarters.temperature,
                            _connections: (int)(UtilityConnections.Left | UtilityConnections.Right)));

                        templateContainer.buildings.Add(new Prefab(
                            LiquidConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 4,
                            headquarters.location_y - 1,
                            SimHashes.SandStone,
                            headquarters.temperature,
                            _connections: (int)(UtilityConnections.Left | UtilityConnections.Up)));

                        templateContainer.buildings.Add(new Prefab(
                            LiquidConduitConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 4,
                            headquarters.location_y,
                            SimHashes.SandStone,
                            headquarters.temperature,
                            _connections: (int)UtilityConnections.Down));

                        templateContainer.buildings.Add(new Prefab(
                            LiquidValveConfig.ID,
                            Prefab.Type.Building,
                            headquarters.location_x + 4,
                            headquarters.location_y,
                            SimHashes.IronOre,
                            headquarters.temperature));
                    }
                }
                finally
                {
                    _templatesPatched.Add(templateContainer.name);
                }
            }
        }
    }
}