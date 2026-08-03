using UnityEngine;

namespace PortalStation;

internal enum StationPieceAnchor
{
    BottomCenter,
    BottomLeft,
    BottomFrontCenter,
    BottomBackCenter,
}

internal static class StationPiecePlacement
{
    internal static Quaternion GetBeamRotationAlongX(GameObject prefab)
    {
        if (prefab == null)
        {
            return Quaternion.identity;
        }

        float widthAtIdentity = MeasureHorizontalWidth(prefab, Quaternion.identity);
        float widthAtRotateY = MeasureHorizontalWidth(prefab, Quaternion.Euler(0f, 90f, 0f));
        return widthAtRotateY >= widthAtIdentity
            ? Quaternion.Euler(0f, 90f, 0f)
            : Quaternion.identity;
    }

    internal static void Place(
        Transform parent,
        GameObject piece,
        Vector3 targetLocalAnchor,
        Quaternion localRotation,
        StationPieceAnchor anchor)
    {
        if (parent == null || piece == null)
        {
            return;
        }

        piece.transform.SetParent(parent, false);
        piece.transform.localRotation = localRotation;
        piece.transform.localPosition = Vector3.zero;

        Vector3 anchorOffset = GetAnchorOffsetInParentSpace(piece.transform, anchor);
        piece.transform.localPosition = targetLocalAnchor - anchorOffset;
    }

    private static float MeasureHorizontalWidth(GameObject prefab, Quaternion rotation)
    {
        GameObject temp = NetworkPrefabHelper.RunWithoutZdoCreation(() => Object.Instantiate(prefab));
        temp.SetActive(false);
        StripMeasurementObject(temp);
        temp.transform.rotation = rotation;
        temp.transform.position = Vector3.zero;
        temp.SetActive(true);

        Bounds bounds = GetWorldBounds(temp);
        Object.DestroyImmediate(temp);
        return bounds.size.x;
    }

    private static void StripMeasurementObject(GameObject visual)
    {
        PortalDisplaySignSpawner.StripSign(visual);
        foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static Vector3 GetAnchorOffsetInParentSpace(Transform piece, StationPieceAnchor anchor)
    {
        Bounds bounds = GetWorldBounds(piece.gameObject);
        Vector3 worldAnchor = anchor switch
        {
            StationPieceAnchor.BottomLeft => new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
            StationPieceAnchor.BottomFrontCenter => new Vector3(bounds.center.x, bounds.min.y, bounds.max.z),
            StationPieceAnchor.BottomBackCenter => new Vector3(bounds.center.x, bounds.min.y, bounds.min.z),
            _ => new Vector3(bounds.center.x, bounds.min.y, bounds.center.z),
        };

        Transform parent = piece.parent;
        if (parent == null)
        {
            return worldAnchor - piece.position;
        }

        return parent.InverseTransformPoint(worldAnchor);
    }

    private static Bounds GetWorldBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
}
