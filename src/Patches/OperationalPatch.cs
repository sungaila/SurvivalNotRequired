using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static Operational;

namespace Sungaila.SurvivalNotRequired.Patches
{
    /// <summary>
    /// Keep the headquarters operational even if oxygen, water and power generation are not.
    /// </summary>
    [HarmonyPatch(typeof(Operational))]
    public static class OperationalPatch
    {
        private static readonly string[] _instanceNameWhitelist =
        [
            "HeadquartersComplete",
            "ExobaseHeadquartersComplete"
        ];

        private static readonly string[] _isOperationalBlacklist =
        [
            "pipesHaveRoom",
            "output_connected",
            "output_conduit",
            "GeneratorConnected",
            "generatorWireConnected"
        ];

        /// <summary>
        /// A stub for the original method UpdateOperational to be used in a reverse patch.
        /// </summary>
        [HarmonyPatch(typeof(Operational), "UpdateOperational")]
        [HarmonyReversePatch]
        public static void UpdateOperational(Operational __instance) => throw new NotImplementedException("This is a stub that will be overwritten by Harmony.");

        [HarmonyPatch(typeof(Operational), "UpdateOperational")]
        [HarmonyPrefix]
        public static bool UpdateOperationalPrefix(Operational __instance)
        {
            // ignore every Operational except for Printing Pod and Mini-Pod
            // the rest will execute the original method instead
            if (!_instanceNameWhitelist.Contains(__instance.name))
                return true;

            // remember original flag states
            var flagsBackup = new Dictionary<Flag, bool>();
            flagsBackup.AddRange(__instance.Flags);

            // temporarily set blacklisted flags to true
            foreach (var flag in __instance.Flags.Keys.ToArray())
            {
                if (_isOperationalBlacklist.Contains(flag.Name))
                    __instance.Flags[flag] = true;
            }

            // execute original method with modified flags
            UpdateOperational(__instance);

            // restore original flag states
            foreach (var item in flagsBackup)
            {
                __instance.Flags[item.Key] = item.Value;
            }

            // make sure that the original method is not called again
            return false;
        }
    }
}