using UnityEngine;

namespace PortalStation;

internal static class PrefabBuildHelper
{
    internal static void FinalizeBuildPiece(GameObject prefab)
    {
        int pieceLayer = LayerMask.NameToLayer("piece");
        if (pieceLayer >= 0)
        {
            prefab.layer = pieceLayer;
        }
    }
}
