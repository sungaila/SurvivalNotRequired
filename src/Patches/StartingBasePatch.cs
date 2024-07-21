using HarmonyLib;
using ProcGen;
using ProcGenGame;
using System.Collections.Generic;
using System.Linq;
using TemplateClasses;
using UnityEngine;
using static ProcGenGame.TemplateSpawning;

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
        public static void Postfix(WorldGenSettings settings, List<TerrainCell> terrainCells, SeededRandom myRandom, ref List<RectInt> placedPOIBounds, bool isRunningDebugGen, ref List<WorldTrait> placedStoryTraits, WorldGen.OfflineCallbackFunction successCallbackFn, ref List<TemplateSpawner> __result)
        {
            if (string.IsNullOrEmpty(settings.world.startingBaseTemplate))
                return;

            var headquarterTemplates = __result
                .Where(t => t.container.buildings != null && t.container.buildings.Any(b => b.id == HeadquartersConfig.ID))
                .Select(t => t.container)
                .ToList();

            if (headquarterTemplates.Count == 0)
            {
                Debug.LogWarning($"No template containing a headquarter found. Skip building plaments for the mod SurvivalNotRequired.");
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

                    var headquarters = templateContainer.buildings.Single(b => b.id == HeadquartersConfig.ID);

                    // a gas vent to the left connected with gas pipes
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

                    // a floor lamp to the right connected with wires
                    // unless something is taking this place (like in the Frosty Planet Pack DLC)
                    var blockingBuildings = templateContainer.buildings.Where(b => b.location_x >= headquarters.location_x + 3 && b.location_y >= headquarters.location_y).ToList();

                    if (blockingBuildings.Count > 0)
                    {
                        Debug.LogWarning($"Skipped placement of floor lamp in world generation for the mod SurvivalNotRequired. Another building ({string.Join(", ", blockingBuildings.Select(b => b.id))}) is blocking its place.");
                        return;
                    }

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
                finally
                {
                    _templatesPatched.Add(templateContainer.name);
                }
            }
        }
    }
}