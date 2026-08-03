using UnityEngine;

namespace PortalStation;

/// <summary>
/// Portal Station frame + board built from the player's manual construction sequence.
/// Origin: ground-level center of the 3 m wide structure (midpoint between the two legs).
/// X = left/right, Y = up, Z = forward (board face).
/// </summary>
internal static class StationFrameBuilder
{
    private const float XLeft = -1.5f;
    private const float XRight = 1.5f;
    private const float ZPlane = 0f;

    private const float YGround = 0f;
    private const float YBottomBeam = 1f;
    private const float YSidePole2m = 1f;
    private const float YSidePole1m = 3f;
    private const float YTopBeam = 4f;

    private const float XQuarterCenter = 1f;

    internal static void BuildFrame(Transform root)
    {
        if (root == null)
        {
            return;
        }

        BuildLegs(root);
        BuildBottomBeams(root);
        BuildSidePosts(root);
        BuildTopBeams(root);
    }

    internal static void BuildBoard(Transform root)
    {
        if (root == null)
        {
            return;
        }

        AddWall(root, "woodwall", new Vector3(-0.5f, YBottomBeam, ZPlane), "piece_wood_wall");
        AddWall(root, "wood_wall_half", new Vector3(-0.5f, YSidePole1m, ZPlane));
        AddQuarter(root, new Vector3(XQuarterCenter, YBottomBeam, ZPlane));
        AddQuarter(root, new Vector3(XQuarterCenter, YBottomBeam + 1f, ZPlane));
        AddQuarter(root, new Vector3(XQuarterCenter, YSidePole1m, ZPlane));
    }

    private static void BuildLegs(Transform root)
    {
        AddPole(root, "wood_pole", new Vector3(XLeft, YGround, ZPlane));
        AddPole(root, "wood_pole", new Vector3(XRight, YGround, ZPlane));
    }

    private static void BuildBottomBeams(Transform root)
    {
        AddBeam(root, "wood_beam", new Vector3(-0.5f, YBottomBeam, ZPlane));
        AddBeam(root, "wood_beam_1", new Vector3(1f, YBottomBeam, ZPlane));
    }

    private static void BuildSidePosts(Transform root)
    {
        AddPole(root, "wood_pole2", new Vector3(XLeft, YSidePole2m, ZPlane));
        AddPole(root, "wood_pole2", new Vector3(XRight, YSidePole2m, ZPlane));
        AddPole(root, "wood_pole", new Vector3(XLeft, YSidePole1m, ZPlane));
        AddPole(root, "wood_pole", new Vector3(XRight, YSidePole1m, ZPlane));
    }

    private static void BuildTopBeams(Transform root)
    {
        AddBeam(root, "wood_beam_1", new Vector3(-1f, YTopBeam, ZPlane));
        AddBeam(root, "wood_beam", new Vector3(0.5f, YTopBeam, ZPlane));
    }

    private static void AddPole(Transform root, string prefabName, Vector3 localPosition)
    {
        StationVisualHelper.AddVisualChild(root, prefabName, localPosition, Quaternion.identity);
    }

    private static void AddBeam(Transform root, string prefabName, Vector3 localPosition)
    {
        StationVisualHelper.AddBeamChild(root, prefabName, localPosition);
    }

    private static void AddWall(Transform root, string prefabName, Vector3 localPosition, params string[] fallbackNames)
    {
        StationVisualHelper.AddVisualChild(
            root,
            prefabName,
            localPosition,
            Quaternion.identity,
            StationPieceAnchor.BottomFrontCenter,
            fallbackNames);
    }

    private static void AddQuarter(Transform root, Vector3 localPosition)
    {
        StationVisualHelper.AddVisualChild(
            root,
            "wood_wall_quarter",
            localPosition,
            Quaternion.identity,
            StationPieceAnchor.BottomFrontCenter);
    }
}
