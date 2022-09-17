using UnityEngine;

namespace SurvivalNotRequired
{
    internal static class ComponentExtensions
    {
        internal static T AddComponentAndTag<T>(this GameObject go, Tag tag) where T : Component
        {
            var result = go.AddComponent<T>();
            result.AddTag(tag);
            return result;
        }
    }
}
