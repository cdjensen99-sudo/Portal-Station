using Jotunn.Managers;
using UnityEngine;

namespace PortalStation;

public static class StationPrefabBuilder
{
    private static GameObject _cachedPrefab;

    public static GameObject Build()
    {
        if (_cachedPrefab != null)
        {
            return _cachedPrefab;
        }

        GameObject station = PrefabManager.Instance.CreateClonedPrefab(ModConstants.PrefabStation, "piece_wood_wall");
        if (station == null)
        {
            station = PrefabManager.Instance.CreateClonedPrefab(ModConstants.PrefabStation, "woodwall");
        }

        if (station == null)
        {
            PortalStationPlugin.Log.LogError("Could not clone piece_wood_wall for Portal Station root.");
            return null;
        }

        StripRootPieceComponents(station);
        StationVisualHelper.SetRenderersEnabled(station, false);

        foreach (Collider collider in station.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }

        ConfigurePlacementCollider(station);
        ConfigureZNetView(station);

        if (station.GetComponent<RunicStation>() == null)
        {
            station.AddComponent<RunicStation>();
        }

        StationFrameBuilder.BuildFrame(station.transform);
        StationFrameBuilder.BuildBoard(station.transform);

        StationHeaderSign header = StationSignGridBuilder.BuildHeaderSign(station.transform);
        StationDestinationSign[] destinations = StationSignGridBuilder.BuildDestinationSigns(station.transform);
        station.GetComponent<RunicStation>().BindChildSigns(header, destinations);

        station.transform.localScale = Vector3.one * ModConstants.StationVisualScale;

        StationPrefabSanitizer.StripChildNetworking(station);
        PrefabBuildHelper.FinalizeBuildPiece(station);
        StationInteractionColliders.Disable(station);

        _cachedPrefab = station;
        return station;
    }

    private static void StripRootPieceComponents(GameObject station)
    {
        PieceTable pieceTable = station.GetComponent<PieceTable>();
        if (pieceTable != null)
        {
            Object.DestroyImmediate(pieceTable);
        }

        ItemDrop itemDrop = station.GetComponent<ItemDrop>();
        if (itemDrop != null)
        {
            Object.DestroyImmediate(itemDrop);
        }
    }

    private static void ConfigurePlacementCollider(GameObject station)
    {
        BoxCollider placement = station.GetComponent<BoxCollider>();
        if (placement == null)
        {
            placement = station.AddComponent<BoxCollider>();
        }

        placement.center = new Vector3(0f, 2.1f, 0f);
        placement.size = new Vector3(3.2f, 4.2f, 0.5f);
    }

    private static void ConfigureZNetView(GameObject station)
    {
        ZNetView nview = station.GetComponent<ZNetView>();
        if (nview == null)
        {
            nview = station.AddComponent<ZNetView>();
        }

        nview.m_persistent = true;
        nview.m_distant = false;
    }
}
