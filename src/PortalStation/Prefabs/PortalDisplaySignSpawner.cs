using UnityEngine;

namespace PortalStation;

internal static class PortalDisplaySignSpawner
{
    internal static void StripSign(GameObject signObject)
    {
        if (signObject == null)
        {
            return;
        }

        Sign sign = signObject.GetComponent<Sign>();
        if (sign != null)
        {
            Object.DestroyImmediate(sign);
        }

        foreach (ZNetView view in signObject.GetComponentsInChildren<ZNetView>(true))
        {
            Object.DestroyImmediate(view);
        }

        Piece piece = signObject.GetComponent<Piece>();
        if (piece != null)
        {
            Object.DestroyImmediate(piece);
        }

        WearNTear wear = signObject.GetComponent<WearNTear>();
        if (wear != null)
        {
            Object.DestroyImmediate(wear);
        }

        ItemDrop itemDrop = signObject.GetComponent<ItemDrop>();
        if (itemDrop != null)
        {
            Object.DestroyImmediate(itemDrop);
        }
    }
}
