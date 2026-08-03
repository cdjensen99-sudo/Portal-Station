using UnityEngine;

namespace PortalStation;

internal static class StationInteractionColliders
{
    internal static void Disable(GameObject root)
    {
        SetChildColliderState(root, false);
    }

    internal static void Enable(GameObject root)
    {
        SetChildColliderState(root, true);
    }

    private static void SetChildColliderState(GameObject root, bool enabled)
    {
        if (root == null)
        {
            return;
        }

        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
        {
            if (collider.gameObject == root)
            {
                continue;
            }

            collider.enabled = enabled;
        }
    }
}
