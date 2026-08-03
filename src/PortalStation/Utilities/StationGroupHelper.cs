using System.Collections.Generic;
using UnityEngine;

namespace PortalStation;

internal static class StationGroupHelper
{
    internal static TeleportWorld FindNearestPortal(Vector3 position, float radius)
    {
        TeleportWorld best = null;
        float bestDistance = radius;

        foreach (TeleportWorld portal in Object.FindObjectsByType<TeleportWorld>(FindObjectsSortMode.None))
        {
            if (portal == null)
            {
                continue;
            }

            float distance = Vector3.Distance(position, portal.transform.position);
            if (distance <= bestDistance)
            {
                bestDistance = distance;
                best = portal;
            }
        }

        return best;
    }

    internal static List<PortalStationBoard> GetLinkedGroup(
        PortalStationBoard station,
        string pendingStationName = null)
    {
        List<PortalStationBoard> group = new List<PortalStationBoard>();
        if (station == null)
        {
            return group;
        }

        TeleportWorld portal = station.GetLinkedPortal();
        if (portal == null)
        {
            portal = FindNearestPortal(station.transform.position, ModConstants.PortalLinkRadius);
        }

        if (portal == null)
        {
            group.Add(station);
            return group;
        }

        string stationName = !string.IsNullOrWhiteSpace(pendingStationName)
            ? pendingStationName
            : station.GetStationName();
        float radius = ModConstants.StationGroupRadius;
        Vector3 portalPosition = portal.transform.position;

        foreach (PortalStationBoard other in Object.FindObjectsByType<PortalStationBoard>(FindObjectsSortMode.None))
        {
            if (other == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(stationName)
                || !PortalTextHelper.PortalNamesEqual(other.GetStationName(), stationName))
            {
                continue;
            }

            TeleportWorld otherPortal = other.GetLinkedPortal();
            if (otherPortal == null)
            {
                otherPortal = FindNearestPortal(other.transform.position, ModConstants.PortalLinkRadius);
            }

            if (otherPortal == null)
            {
                continue;
            }

            if (Vector3.Distance(otherPortal.transform.position, portalPosition) > radius)
            {
                continue;
            }

            group.Add(other);
        }

        if (group.Count == 0)
        {
            group.Add(station);
        }

        return group;
    }

    internal static bool TryCollectPortalNames(
        IEnumerable<PortalStationBoard> stations,
        PortalStationBoard editingStation,
        string[] editingPortalNames,
        out string duplicateName)
    {
        duplicateName = null;
        HashSet<string> seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        foreach (PortalStationBoard station in stations)
        {
            for (int i = 0; i < ModConstants.DestinationSlotCount; i++)
            {
                string portalName;
                if (station == editingStation && editingPortalNames != null)
                {
                    portalName = editingPortalNames[i];
                }
                else
                {
                    portalName = station.GetSlotPortalName(i);
                }

                portalName = PortalTextHelper.StripRichText(portalName);
                if (string.IsNullOrWhiteSpace(portalName))
                {
                    continue;
                }

                if (!seen.Add(portalName))
                {
                    duplicateName = portalName;
                    return false;
                }
            }
        }

        return true;
    }
}
