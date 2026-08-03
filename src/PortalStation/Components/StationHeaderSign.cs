using UnityEngine;

namespace PortalStation;

public sealed class StationHeaderSign : MonoBehaviour, Interactable, Hoverable
{
    private PortalStationBoard _station;

    internal void Bind(PortalStationBoard station)
    {
        _station = station;
    }

    public string GetHoverName()
    {
        return "$piece_portal_station_header";
    }

    public string GetHoverText()
    {
        string stationName = _station != null ? _station.GetStationName() : string.Empty;
        if (string.IsNullOrWhiteSpace(stationName))
        {
            stationName = "...";
        }

        return $"\"{stationName}\"\n" +
               Localization.instance.Localize("$piece_portal_station_header\n[<color=yellow><b>$KEY_Use</b></color>] $piece_portal_station_configure");
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (hold || _station == null)
        {
            return false;
        }

        if (!PrivateArea.CheckAccess(transform.position))
        {
            return false;
        }

        _station.OpenConfiguration();
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return false;
    }
}
