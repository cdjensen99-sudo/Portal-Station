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

        int pieceLayer = LayerMask.NameToLayer("piece");

        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
        {
            if (collider.gameObject == root)
            {
                continue;
            }

            collider.enabled = enabled;

            if (enabled && pieceLayer >= 0 && IsInteractionTarget(collider.gameObject))
            {
                collider.gameObject.layer = pieceLayer;
            }
        }
    }

    private static bool IsInteractionTarget(GameObject target)
    {
        return target.GetComponent<StationHeaderSign>() != null
            || target.GetComponent<StationDestinationSign>() != null;
    }
}
