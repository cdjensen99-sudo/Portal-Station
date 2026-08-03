using UnityEngine;

namespace PortalStation;

public sealed class StationDestinationSign : MonoBehaviour, Hoverable
{
    private const string SlotNamePrefix = "_station_slot_";

    private PortalStationBoard _station;
    [SerializeField] private int _slotIndex = -1;
    private StationSignVisual _visual;

    internal void Bind(PortalStationBoard station, int slotIndex)
    {
        _station = station;
        _slotIndex = slotIndex;
        _visual = GetComponent<StationSignVisual>();
    }

    public int SlotIndex => ResolveSlotIndex();

    internal void SetSlotIndex(int slotIndex)
    {
        _slotIndex = slotIndex;
    }

    internal int ResolveSlotIndex()
    {
        if (_slotIndex >= 0 && _slotIndex < ModConstants.DestinationSlotCount)
        {
            return _slotIndex;
        }

        if (name.StartsWith(SlotNamePrefix)
            && int.TryParse(name.Substring(SlotNamePrefix.Length), out int parsed)
            && parsed >= 0
            && parsed < ModConstants.DestinationSlotCount)
        {
            _slotIndex = parsed;
            return _slotIndex;
        }

        return _slotIndex;
    }

    internal void RefreshFromStation()
    {
        if (_station == null || _visual == null)
        {
            return;
        }

        int slotIndex = ResolveSlotIndex();
        string display = _station.GetSlotBoardText(slotIndex);
        if (string.IsNullOrWhiteSpace(PortalTextHelper.StripRichText(display)))
        {
            display = "...";
        }

        _visual.SetDisplayText(PortalTextHelper.FormatDisplayForRender(display));

        string activeTag = _station.GetLinkedPortalTag();
        string portalName = _station.GetSlotPortalName(slotIndex);
        bool active = string.IsNullOrWhiteSpace(PortalTextHelper.StripRichText(portalName))
            ? string.IsNullOrWhiteSpace(activeTag)
            : PortalTextHelper.PortalNamesEqual(portalName, activeTag);
        _visual.SetHighlighted(active);
    }

    public string GetHoverName()
    {
        return "$piece_portal_station_destination";
    }

    public string GetHoverText()
    {
        int slotIndex = ResolveSlotIndex();
        string portalName = _station != null ? _station.GetSlotPortalName(slotIndex) : string.Empty;
        if (string.IsNullOrWhiteSpace(PortalTextHelper.StripRichText(portalName)))
        {
            return Localization.instance.Localize(
                "$piece_portal_station_destination_empty\n[<color=yellow><b>$KEY_Attack</b></color>] $piece_portal_station_clear");
        }

        string display = PortalTextHelper.StripRichText(_station.GetSlotBoardText(slotIndex));
        return $"\"{display}\"\n" +
               Localization.instance.Localize(
                   "$piece_portal_station_destination\n[<color=yellow><b>$KEY_Attack</b></color>] $piece_portal_station_activate");
    }

    internal void TryActivate()
    {
        if (_station == null)
        {
            return;
        }

        if (!PrivateArea.CheckAccess(transform.position))
        {
            return;
        }

        _station.ActivateDestination(ResolveSlotIndex());
    }
}
