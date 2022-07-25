using HarmonyLib;
using ProcGen;
using ProcGenGame;
using System.Collections.Generic;
using System.Linq;
using TemplateClasses;

namespace SurvivalNotRequired.Patches
{
    /// <summary>
    /// Adds a gas vent and a floor lamp to the starting base.
    /// </summary>
    [HarmonyPatch(typeof(TemplateSpawning), nameof(TemplateSpawning.DetermineTemplatesForWorld))]
    public static class StartingBasePatch
    {
        /// <summary>
        /// Postfix for <see cref="TemplateSpawning.DetermineTemplatesForWorld"/>.
        /// </summary>
        public static void Postfix(WorldGenSettings settings, ref List<KeyValuePair<Vector2I, TemplateContainer>> __result)
        {
            if (string.IsNullOrEmpty(settings.world.startingBaseTemplate))
                return;

            // iterate through every template containing a headquarter
            // assume this is the starting base template
            foreach (var templateContainer in __result
                .Where(t => t.Value?.buildings != null && t.Value.buildings.Any(b => b.id == HeadquartersConfig.ID))
                .Select(t => t.Value))
            {
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
        }
    }
}