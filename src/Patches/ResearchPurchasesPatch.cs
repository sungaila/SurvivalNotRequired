using System.Linq;
using System.Reflection;

namespace SurvivalNotRequired.Patches
{
    /// <summary>
    /// Unlocks certain technologies from the beginning.
    /// Not in use for now.
    /// </summary>
    //[HarmonyPatch(typeof(Tech), nameof(Tech.IsComplete))]
    public static class ResearchPurchasesPatch
    {
        private static readonly string[] _techIdsToPurchase = new[]
        {
            "GasPiping"
        };

        /// <summary>
        /// The private method <see cref="BuildMenu"/>.OnResearchComplete(Tech) found via reflection.
        /// </summary>
        private static readonly MethodInfo? _onResearchCompleteMethodInfo = typeof(BuildMenu).GetMethod("OnResearchComplete", BindingFlags.NonPublic | BindingFlags.Instance, default, new[] { typeof(object) }, default);

        /// <summary>
        /// Prefix for <see cref="Tech.IsComplete"/>.
        /// </summary>
        public static void Prefix(Tech __instance)
        {
            if (!_techIdsToPurchase.Contains(__instance.Id) || Research.Instance is null)
                return;

            // is this tech on the whitelist and still not purchased? purchase it now!
            // also inform the BuildMenu about this
            if (Research.Instance.Get(__instance) is TechInstance techInstance && !techInstance.IsComplete())
            {
                techInstance.Purchased();

                if (BuildMenu.Instance != null)
                    _onResearchCompleteMethodInfo?.Invoke(BuildMenu.Instance, new[] { __instance });
            }
        }
    }
}