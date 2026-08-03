using UnityEngine;

namespace PortalStation;

internal static class StationInstanceHelper
{
    internal static bool IsPlacedInstance(GameObject root)
    {
        if (root == null)
        {
            return false;
        }

        if (root.scene.name == "DontDestroyOnLoad")
        {
            return false;
        }

        int ghostLayer = LayerMask.NameToLayer("ghost");
        return ghostLayer < 0 || root.layer != ghostLayer;
    }
}
