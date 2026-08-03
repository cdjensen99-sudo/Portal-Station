using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace PortalStation;

internal static class StationVisualHelper
{
    internal static GameObject TryGetPrefab(params string[] prefabNames)
    {
        foreach (string prefabName in prefabNames)
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (prefab != null)
            {
                return prefab;
            }
        }

        if (ZNetScene.instance != null)
        {
            foreach (string prefabName in prefabNames)
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
                if (prefab != null)
                {
                    return prefab;
                }
            }
        }

        PortalStationPlugin.Log.LogWarning(
            $"StationVisualHelper: could not resolve prefab from candidates: {string.Join(", ", prefabNames)}");
        return null;
    }

    internal static GameObject AddVisualChild(
        Transform parent,
        string prefabName,
        Vector3 localPosition,
        Quaternion localRotation,
        params string[] prefabNameFallbacks)
    {
        return AddVisualChild(parent, prefabName, localPosition, localRotation, StationPieceAnchor.BottomCenter, prefabNameFallbacks);
    }

    internal static GameObject AddVisualChild(
        Transform parent,
        string prefabName,
        Vector3 localPosition,
        Quaternion localRotation,
        StationPieceAnchor anchor,
        params string[] prefabNameFallbacks)
    {
        List<string> candidates = new List<string> { prefabName };
        if (prefabNameFallbacks != null)
        {
            candidates.AddRange(prefabNameFallbacks);
        }

        GameObject source = TryGetPrefab(candidates.ToArray());
        if (source == null || parent == null)
        {
            return null;
        }

        GameObject visual = NetworkPrefabHelper.RunWithoutZdoCreation(() => Object.Instantiate(source));
        visual.SetActive(false);
        visual.name = prefabName;
        StripForVisualChild(visual);
        visual.SetActive(true);

        StationPiecePlacement.Place(parent, visual, localPosition, localRotation, anchor);
        return visual;
    }

    internal static GameObject AddBeamChild(
        Transform parent,
        string prefabName,
        Vector3 localPosition,
        params string[] prefabNameFallbacks)
    {
        List<string> candidates = new List<string> { prefabName };
        if (prefabNameFallbacks != null)
        {
            candidates.AddRange(prefabNameFallbacks);
        }

        GameObject source = TryGetPrefab(candidates.ToArray());
        if (source == null || parent == null)
        {
            return null;
        }

        Quaternion rotation = StationPiecePlacement.GetBeamRotationAlongX(source);
        return AddVisualChild(parent, prefabName, localPosition, rotation, StationPieceAnchor.BottomCenter, prefabNameFallbacks);
    }

    internal static void StripForVisualChild(GameObject visual)
    {
        PortalDisplaySignSpawner.StripSign(visual);

        foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }
    }

    internal static void SetRenderersEnabled(GameObject root, bool enabled)
    {
        if (root == null)
        {
            return;
        }

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = enabled;
        }
    }
}
