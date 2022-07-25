using HarmonyLib;
using KMod;
using System.Collections.Generic;

namespace SurvivalNotRequired
{
    /// <summary>
    /// Not in use for now.
    /// </summary>
    public class MyUserMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
        }

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods)
        {
            base.OnAllModsLoaded(harmony, mods);
        }
    }
}