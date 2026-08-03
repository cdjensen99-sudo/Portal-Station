using UnityEngine;

namespace PortalStation;

internal static class PortalDisplaySignSpawner
{
    private const float PortalSignLocalY = 3.1f;
    private const float PortalSignLocalZ = -0.1f;

    internal static void EnsureDisplaySign(GameObject portalObject)
    {
        if (portalObject == null)
        {
            return;
        }

        TeleportWorld portal = portalObject.GetComponent<TeleportWorld>();
        if (portal == null)
        {
            return;
        }

        if (portalObject.GetComponentInChildren<PortalNameSign>(true) != null)
        {
            return;
        }

        if (ZNetScene.instance == null)
        {
            return;
        }

        GameObject source = StationVisualHelper.TryGetPrefab("sign", "piece_sign");
        if (source == null)
        {
            return;
        }

        GameObject signObject = NetworkPrefabHelper.RunWithoutZdoCreation(() => Object.Instantiate(source));
        signObject.SetActive(false);
        signObject.name = "_portal_display_sign";
        StripSign(signObject);
        EnsureInteractionCollider(signObject);

        signObject.transform.SetParent(portalObject.transform, false);
        signObject.transform.localPosition = new Vector3(0f, PortalSignLocalY, PortalSignLocalZ);
        signObject.transform.localRotation = Quaternion.identity;
        signObject.transform.localScale = Vector3.one;

        signObject.AddComponent<StationSignVisual>();
        PortalNameSign portalSign = signObject.AddComponent<PortalNameSign>();
        signObject.SetActive(true);
        portalSign.Bind(portal);
    }

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

    private static void EnsureInteractionCollider(GameObject signObject)
    {
        BoxCollider collider = signObject.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = signObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.8f, 0.9f, 0.15f);
            collider.center = new Vector3(0f, 0f, 0.08f);
        }

        collider.enabled = true;

        int pieceLayer = LayerMask.NameToLayer("piece");
        if (pieceLayer >= 0)
        {
            signObject.layer = pieceLayer;
        }
    }
}
