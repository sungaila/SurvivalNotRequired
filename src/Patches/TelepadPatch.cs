using System.Linq;

namespace SurvivalNotRequired.Patches
{
    //[HarmonyPatch(typeof(Telepad), nameof(Telepad.Update))]
    public static class TelepadPatch
    {
        public static void Postfix(Telepad __instance)
        {
            if (__instance.TryGetComponent<Operational>(out var operational))
            {
                Debug.Log("");
                Debug.Log($"name: {operational.name}");
                Debug.Log($"isActiveAndEnabled: {operational.isActiveAndEnabled}");
                Debug.Log($"IsFunctional: {operational.IsFunctional}");
                Debug.Log($"IsOperational: {operational.IsOperational}");
                Debug.Log($"Flags: {string.Join(", ", operational.Flags.Select(f => $"{f.Key.Name}:{f.Key.FlagType}:{f.Value}"))}");
            }
        }
    }
}