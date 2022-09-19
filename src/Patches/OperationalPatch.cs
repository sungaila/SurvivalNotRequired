using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace SurvivalNotRequired.Patches
{
    /// <summary>
    /// Keep the headquarters operational even if oxygen and power generation are not.
    /// </summary>
    [HarmonyPatch(typeof(Operational), "UpdateOperational")]
    public static class OperationalPatch
    {
        private static readonly string[] _instanceNameWhitelist = new[]
        {
            "HeadquartersComplete",
            "ExobaseHeadquartersComplete"
        };

        private static readonly string[] _isOperationalBlacklist = new[]
        {
            "pipesHaveRoom",
            "output_connected",
            "output_conduit",
            "GeneratorConnected"
        };

        /// <summary>
        /// The property <see cref="Operational.IsOperational"/> found via reflection. This way we can access the private setter.
        /// </summary>
        private static readonly PropertyInfo _isOperationalPropertyInfo = typeof(Operational).GetProperty(nameof(Operational.IsOperational));

        public static bool Prefix(Operational __instance)
        {
            // ignore every Operational except for Printing Pod and Mini-Pod
            if (!_instanceNameWhitelist.Contains(__instance.name))
                return true;

            // recreate the method UpdateOperational plus a blacklist for ignored flags
            // I'd rather call the original method but for that Operational.Flags.GetEnumerator() would need patching
            // and that is too expensive I fear
            bool newOperationalValue = true;

            foreach (var flag in __instance.Flags)
            {
                if (_isOperationalBlacklist.Contains(flag.Key.Name))
                    continue;

                if (!flag.Value)
                {
                    newOperationalValue = false;
                    break;
                }
            }

            if (newOperationalValue == __instance.IsOperational)
                return false;

            _isOperationalPropertyInfo.SetValue(__instance, newOperationalValue);

            if (!__instance.IsOperational)
                __instance.SetActive(false);

            if (__instance.IsOperational)
                __instance.GetComponent<KPrefabID>().AddTag(GameTags.Operational);
            else
                __instance.GetComponent<KPrefabID>().RemoveTag(GameTags.Operational);

            __instance.Trigger((int)GameHashes.OperationalChanged, __instance.IsOperational);
            Game.Instance.Trigger((int)GameHashes.BuildingStateChanged, __instance.gameObject);

            return false;
        }
    }
}