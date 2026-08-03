using UnityEngine;

namespace PortalStation;

/// <summary>
/// Places 18 destination signs (3 columns × 6 rows) on the board face.
/// </summary>
internal static class StationSignGridBuilder
{
    private const float SignHeight = 0.5f;
    private const float BoardBottomY = 1f;
    private const float SignFaceEpsilon = ModConstants.StationSignFaceEpsilon;
    private const float HeaderSignForwardOffset = ModConstants.StationHeaderSignForwardOffset;
    private const float SignForwardOffset = ModConstants.StationDestinationSignForwardOffset;

    private static readonly float[] ColumnCenters = { -1f, 0f, 1f };

    internal static StationHeaderSign BuildHeaderSign(Transform root)
    {
        StationSignOrientation.ResetCache();
        float faceZ = ResolveBoardFaceZ(root) + HeaderSignForwardOffset;
        Vector3 position = new Vector3(0f, 4.3f, faceZ);
        GameObject signObject = CreateSignObject(root, "_station_header", position);
        StationHeaderSign header = signObject.AddComponent<StationHeaderSign>();
        signObject.AddComponent<StationSignVisual>();
        return header;
    }

    internal static StationDestinationSign[] BuildDestinationSigns(Transform root)
    {
        StationDestinationSign[] destinations = new StationDestinationSign[ModConstants.DestinationSlotCount];
        for (int row = 0; row < ModConstants.GridRows; row++)
        {
            float rowBottomY = BoardBottomY + row * SignHeight;
            for (int col = 0; col < ModConstants.GridColumns; col++)
            {
                int index = row * ModConstants.GridColumns + col;
                float wallFrontZ = ResolveSignWallFrontZ(root, col, row);
                float signZ = wallFrontZ + SignFaceEpsilon + SignForwardOffset;
                Vector3 position = new Vector3(ColumnCenters[col], rowBottomY, signZ);
                GameObject signObject = CreateSignObject(root, $"_station_slot_{index:D2}", position);
                signObject.AddComponent<StationSignVisual>();
                StationDestinationSign destination = signObject.AddComponent<StationDestinationSign>();
                destination.SetSlotIndex(index);
                destinations[index] = destination;
            }
        }

        return destinations;
    }

    private static float ResolveSignWallFrontZ(Transform root, int column, int row)
    {
        if (column <= 1)
        {
            return row < 4
                ? ResolveWallPieceFrontZ(root, "woodwall")
                : ResolveWallPieceFrontZ(root, "wood_wall_half");
        }

        return ResolveWallPieceFrontZ(root, "wood_wall_quarter");
    }

    private static float ResolveWallPieceFrontZ(Transform root, string wallPieceName)
    {
        float maxFrontZ = float.MinValue;
        bool found = false;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name != wallPieceName)
            {
                continue;
            }

            foreach (Renderer renderer in child.GetComponentsInChildren<Renderer>(true))
            {
                Vector3 localFront = root.InverseTransformPoint(
                    new Vector3(renderer.bounds.center.x, renderer.bounds.center.y, renderer.bounds.max.z));
                maxFrontZ = Mathf.Max(maxFrontZ, localFront.z);
                found = true;
            }
        }

        return found ? maxFrontZ : 0f;
    }

    internal static float ResolveBoardFaceZ(Transform root)
    {
        float maxFrontZ = 0f;
        bool foundWall = false;

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!IsBoardWallRenderer(renderer))
            {
                continue;
            }

            foundWall = true;
            Vector3 localFront = root.InverseTransformPoint(
                new Vector3(renderer.bounds.center.x, renderer.bounds.center.y, renderer.bounds.max.z));
            maxFrontZ = Mathf.Max(maxFrontZ, localFront.z);
        }

        if (!foundWall)
        {
            maxFrontZ = MeasureWallPlankThickness();
        }

        return maxFrontZ + SignFaceEpsilon;
    }

    private static bool IsBoardWallRenderer(Renderer renderer)
    {
        if (renderer == null)
        {
            return false;
        }

        Transform current = renderer.transform;
        while (current != null)
        {
            switch (current.name)
            {
                case "woodwall":
                case "wood_wall_half":
                case "wood_wall_quarter":
                    return true;
            }

            if (current.parent == null || current.parent.name == ModConstants.PrefabStation)
            {
                break;
            }

            current = current.parent;
        }

        return false;
    }

    private static float MeasureWallPlankThickness()
    {
        GameObject wallPrefab = StationVisualHelper.TryGetPrefab("woodwall", "piece_wood_wall");
        if (wallPrefab == null)
        {
            return 0.26f;
        }

        GameObject temp = NetworkPrefabHelper.RunWithoutZdoCreation(() => Object.Instantiate(wallPrefab));
        temp.SetActive(false);
        StationVisualHelper.StripForVisualChild(temp);
        temp.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        temp.SetActive(true);

        Renderer[] renderers = temp.GetComponentsInChildren<Renderer>(true);
        float thickness = 0.26f;
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            thickness = bounds.size.z;
        }

        Object.DestroyImmediate(temp);
        return thickness;
    }

    private static GameObject CreateSignObject(Transform root, string name, Vector3 targetAnchor)
    {
        GameObject source = StationVisualHelper.TryGetPrefab("sign", "piece_sign");
        if (source == null)
        {
            GameObject fallback = new GameObject(name);
            fallback.transform.SetParent(root, false);
            fallback.transform.localPosition = targetAnchor;
            return fallback;
        }

        GameObject signObject = NetworkPrefabHelper.RunWithoutZdoCreation(() => Object.Instantiate(source));
        signObject.SetActive(false);
        signObject.name = name;
        PortalDisplaySignSpawner.StripSign(signObject);
        signObject.SetActive(true);

        PlaceSignFacingOut(root, signObject, targetAnchor);

        if (signObject.GetComponent<Collider>() == null)
        {
            BoxCollider collider = signObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.8f, 0.9f, 0.15f);
            collider.center = new Vector3(0f, 0f, 0.08f);
            collider.enabled = false;
        }

        return signObject;
    }

    private static void PlaceSignFacingOut(Transform root, GameObject signObject, Vector3 targetAnchor)
    {
        Quaternion rotation = StationSignOrientation.GetBoardMountRotation(root) * Quaternion.Euler(0f, 180f, 0f);
        StationPieceAnchor anchor = StationSignOrientation.GetPlacementAnchor();
        StationPiecePlacement.Place(root, signObject, targetAnchor, rotation, anchor);
    }
}
