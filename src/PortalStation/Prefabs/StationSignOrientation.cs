using TMPro;
using UnityEngine;

namespace PortalStation;

internal static class StationSignOrientation
{
    private static Quaternion? _cachedLocalRotation;
    private static StationPieceAnchor _cachedAnchor = StationPieceAnchor.BottomBackCenter;

    internal static void ResetCache()
    {
        _cachedLocalRotation = null;
    }

    internal static Quaternion GetBoardMountRotation(Transform boardRoot)
    {
        if (_cachedLocalRotation.HasValue)
        {
            return _cachedLocalRotation.Value;
        }

        GameObject source = StationVisualHelper.TryGetPrefab("sign", "piece_sign");
        if (source == null || boardRoot == null)
        {
            _cachedLocalRotation = Quaternion.identity;
            return _cachedLocalRotation.Value;
        }

        Vector3 desiredForward = boardRoot.TransformDirection(Vector3.forward);
        Quaternion bestRotation = Quaternion.identity;
        StationPieceAnchor bestAnchor = StationPieceAnchor.BottomBackCenter;
        float bestScore = float.MinValue;

        foreach (Quaternion candidate in BuildRotationCandidates())
        {
            if (!TryScoreRotation(source, boardRoot, candidate, desiredForward, out float score, out StationPieceAnchor anchor))
            {
                continue;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestRotation = candidate;
                bestAnchor = anchor;
            }
        }

        _cachedLocalRotation = bestRotation;
        _cachedAnchor = bestAnchor;
        return bestRotation;
    }

    internal static StationPieceAnchor GetPlacementAnchor()
    {
        return _cachedAnchor;
    }

    private static bool TryScoreRotation(
        GameObject source,
        Transform boardRoot,
        Quaternion localRotation,
        Vector3 desiredForward,
        out float score,
        out StationPieceAnchor anchor)
    {
        score = float.MinValue;
        anchor = StationPieceAnchor.BottomBackCenter;

        GameObject temp = NetworkPrefabHelper.RunWithoutZdoCreation(() => Object.Instantiate(source));
        temp.SetActive(false);
        PortalDisplaySignSpawner.StripSign(temp);
        temp.transform.SetParent(boardRoot, false);
        temp.transform.localRotation = localRotation;
        temp.transform.localPosition = Vector3.zero;
        temp.SetActive(true);

        TextMeshProUGUI text = temp.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text == null)
        {
            Object.DestroyImmediate(temp);
            return false;
        }

        float forwardScore = Vector3.Dot(text.transform.forward, desiredForward);
        if (forwardScore < 0.85f)
        {
            Object.DestroyImmediate(temp);
            return false;
        }

        anchor = StationPieceAnchor.BottomBackCenter;
        float uprightScore = Vector3.Dot(text.transform.up, boardRoot.up);
        score = forwardScore * 2f + uprightScore;

        Object.DestroyImmediate(temp);
        return true;
    }

    private static Quaternion[] BuildRotationCandidates()
    {
        Quaternion[] results = new Quaternion[24];
        int index = 0;
        for (int yaw = 0; yaw < 360; yaw += 90)
        {
            for (int pitch = -90; pitch <= 90; pitch += 90)
            {
                results[index++] = Quaternion.Euler(pitch, yaw, 0f);
            }
        }

        return results;
    }
}
