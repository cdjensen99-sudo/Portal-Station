using UnityEngine;

namespace PortalStation;

internal static class StationDestinationInputHelper
{
    internal static bool IsHoveringDestinationSign(Player player)
    {
        if (player == null || player != Player.m_localPlayer)
        {
            return false;
        }

        GameObject hover = player.GetHoverObject();
        if (hover == null)
        {
            return false;
        }

        return hover.GetComponent<StationDestinationSign>() != null
            || hover.GetComponentInParent<StationDestinationSign>() != null;
    }
}
