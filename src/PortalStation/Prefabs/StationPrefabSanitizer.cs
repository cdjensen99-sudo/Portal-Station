using UnityEngine;

namespace PortalStation;

internal static class StationPrefabSanitizer
{
    internal static void StripChildNetworking(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        foreach (WearNTear wear in root.GetComponentsInChildren<WearNTear>(true))
        {
            if (wear != null && wear.gameObject != root)
            {
                Object.DestroyImmediate(wear);
            }
        }

        ZNetView rootView = root.GetComponent<ZNetView>();

        foreach (ZNetView view in root.GetComponentsInChildren<ZNetView>(true))
        {
            if (view == null || view == rootView)
            {
                continue;
            }

            Object.DestroyImmediate(view);
        }

        foreach (Piece piece in root.GetComponentsInChildren<Piece>(true))
        {
            if (piece == null || piece.gameObject == root)
            {
                continue;
            }

            Object.DestroyImmediate(piece);
        }

        foreach (ItemDrop item in root.GetComponentsInChildren<ItemDrop>(true))
        {
            if (item == null || item.gameObject == root)
            {
                continue;
            }

            Object.DestroyImmediate(item);
        }
    }
}
