using HarmonyLib;
using UnityEngine;

namespace SurvivalNotRequired.Patches
{
    /// <summary>
    /// Modifies the exobaseheadquarters building definition.
    /// But only if <see cref="TelepadStatesInstancePatch.ExtendMiniPod"/> is set.
    /// </summary>
    [HarmonyPatch(typeof(ExobaseHeadquartersConfig))]
    public static class ExobaseHeadquartersConfigPatch
    {
        /// <summary>
        /// Postfix for <see cref="ExobaseHeadquartersConfig.CreateBuildingDef"/>.
        /// </summary>
        [HarmonyPatch(typeof(ExobaseHeadquartersConfig), nameof(ExobaseHeadquartersConfig.CreateBuildingDef))]
        [HarmonyPostfix]
        public static void CreateBuildingDefPostfix(ref BuildingDef __result)
        {
            if (!ModSettings.Instance.ExtendMiniPod)
                return;

            HeadquartersConfigPatch.ModifyBuildingDef(ref __result, new CellOffset(1, 0), ExobaseHeadquartersConfig.ID);
        }

        /// <summary>
        /// Postfix for <see cref="ExobaseHeadquartersConfig.ConfigureBuildingTemplate"/>.
        /// </summary>
        [HarmonyPatch(typeof(ExobaseHeadquartersConfig), nameof(ExobaseHeadquartersConfig.ConfigureBuildingTemplate))]
        [HarmonyPostfix]
        public static void ConfigureBuildingTemplatePostfix(GameObject go, Tag prefab_tag)
        {
            if (!ModSettings.Instance.ExtendMiniPod)
                return;

            HeadquartersConfigPatch.ModifyBuildingTemplate(ref go, new CellOffset(-1, 0), new CellOffset(1, 0));
        }
    }
}