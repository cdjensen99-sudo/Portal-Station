using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PortalStation;

public sealed class PortalStationBoard : MonoBehaviour
{
    private ZNetView _nview;
    private StationHeaderSign _headerSign;
    private readonly StationDestinationSign[] _destinationSigns = new StationDestinationSign[ModConstants.DestinationSlotCount];
    private bool _initialized;
    private bool _rpcRegistered;

    private void Awake()
    {
        _nview = GetComponent<ZNetView>();
        _headerSign = GetComponentInChildren<StationHeaderSign>(true);
        RegisterDestinationSigns();

        if (StationInstanceHelper.IsPlacedInstance(gameObject))
        {
            TryRegisterRpcs();
        }
    }

    private void Start()
    {
        if (!StationInstanceHelper.IsPlacedInstance(gameObject))
        {
            return;
        }

        TryRegisterRpcs();
        Initialize();
        StationInteractionColliders.Enable(gameObject);
        RefreshSigns();
    }

    public void BindChildSigns(StationHeaderSign header, StationDestinationSign[] destinations)
    {
        _headerSign = header;
        for (int i = 0; i < destinations.Length && i < ModConstants.DestinationSlotCount; i++)
        {
            _destinationSigns[i] = destinations[i];
        }
    }

    internal void TryRegisterRpcs()
    {
        if (!StationInstanceHelper.IsPlacedInstance(gameObject))
        {
            return;
        }

        _nview = GetComponent<ZNetView>();
        if (_rpcRegistered || _nview == null || !_nview.IsValid())
        {
            return;
        }

        _nview.Register<string, string, string>(ModConstants.RpcApplyStationConfig, RPC_ApplyStationConfig);
        _nview.Register<int>(ModConstants.RpcActivateDestination, RPC_ActivateDestination);
        _rpcRegistered = true;
    }

    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        RegisterDestinationSigns();
        _headerSign?.Bind(this);
        for (int i = 0; i < ModConstants.DestinationSlotCount; i++)
        {
            _destinationSigns[i]?.Bind(this, i);
        }
    }

    private void RegisterDestinationSigns()
    {
        for (int i = 0; i < ModConstants.DestinationSlotCount; i++)
        {
            _destinationSigns[i] = null;
        }

        foreach (StationDestinationSign sign in GetComponentsInChildren<StationDestinationSign>(true))
        {
            int index = sign.ResolveSlotIndex();
            if (index < 0 || index >= ModConstants.DestinationSlotCount)
            {
                continue;
            }

            _destinationSigns[index] = sign;
        }
    }

    public string GetStationName()
    {
        return _nview?.GetZDO()?.GetString(ModConstants.ZdoStationName, string.Empty) ?? string.Empty;
    }

    public string GetSlotPortalName(int index)
    {
        if (index < 0 || index >= ModConstants.DestinationSlotCount)
        {
            return string.Empty;
        }

        return _nview?.GetZDO()?.GetString(ModConstants.SlotPortalKey(index), string.Empty) ?? string.Empty;
    }

    public string GetSlotDisplayRaw(int index)
    {
        if (index < 0 || index >= ModConstants.DestinationSlotCount)
        {
            return string.Empty;
        }

        return _nview?.GetZDO()?.GetString(ModConstants.SlotDisplayKey(index), string.Empty) ?? string.Empty;
    }

    public string GetSlotBoardText(int index)
    {
        string display = GetSlotDisplayRaw(index);
        if (!string.IsNullOrWhiteSpace(PortalTextHelper.StripRichText(display)))
        {
            return display;
        }

        return GetSlotPortalName(index);
    }

    public string GetSlotDisplayText(int index)
    {
        return PortalTextHelper.BuildDisplayText(GetSlotPortalName(index), GetSlotDisplayRaw(index));
    }

    public TeleportWorld GetLinkedPortal()
    {
        string linked = _nview?.GetZDO()?.GetString(ModConstants.ZdoLinkedPortal, string.Empty);
        if (string.IsNullOrEmpty(linked) || !TryParseZdoId(linked, out ZDOID uid))
        {
            return null;
        }

        GameObject portalObject = ZNetScene.instance?.FindInstance(uid);
        return portalObject != null ? portalObject.GetComponent<TeleportWorld>() : null;
    }

    public string GetLinkedPortalTag()
    {
        return PortalTagHelper.GetPortalTag(GetLinkedPortal());
    }

    public void ActivateDestination(int slotIndex)
    {
        if (_nview == null || !_nview.IsValid())
        {
            return;
        }

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            ServerActivateDestination(slotIndex);
            return;
        }

        _nview.InvokeRPC(ModConstants.RpcActivateDestination, slotIndex);
    }

    public void OpenConfiguration()
    {
        StationConfigUI.Instance?.Open(this);
    }

    public bool TryApplyConfiguration(
        string stationName,
        string[] portalNames,
        string[] displayNames,
        out string message)
    {
        message = string.Empty;
        _nview = GetComponent<ZNetView>();
        TryRegisterRpcs();

        if (_nview == null || !_nview.IsValid())
        {
            message = "Station is not ready yet. Try again in a moment.";
            return false;
        }

        string portalPayload = EncodeSlots(portalNames);
        string displayPayload = EncodeSlots(displayNames);

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            return ServerApplyConfiguration(stationName, portalPayload, displayPayload, out message);
        }

        _nview.InvokeRPC(ModConstants.RpcApplyStationConfig, stationName, portalPayload, displayPayload);
        RefreshSigns();
        message = "Configuration sent to server.";
        return true;
    }

    public void RefreshSigns()
    {
        StationSignVisual headerVisual = _headerSign != null
            ? _headerSign.GetComponent<StationSignVisual>()
            : null;
        string display = string.IsNullOrWhiteSpace(GetStationName()) ? "..." : GetStationName();
        headerVisual?.SetDisplayText(PortalTextHelper.FormatDisplayForRender(display));

        for (int i = 0; i < ModConstants.DestinationSlotCount; i++)
        {
            _destinationSigns[i]?.RefreshFromStation();
        }
    }

    private void RPC_ApplyStationConfig(long sender, string stationName, string portalPayload, string displayPayload)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer())
        {
            return;
        }

        ServerApplyConfiguration(stationName, portalPayload, displayPayload, out _);
    }

    private void RPC_ActivateDestination(long sender, int slotIndex)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer())
        {
            return;
        }

        ServerActivateDestination(slotIndex);
    }

    private bool ServerApplyConfiguration(
        string stationName,
        string portalPayload,
        string displayPayload,
        out string message)
    {
        message = string.Empty;
        string[] portalNames = DecodeSlots(portalPayload);
        string[] displayNames = DecodeSlots(displayPayload);

        for (int i = 0; i < ModConstants.DestinationSlotCount; i++)
        {
            portalNames[i] = PortalTextHelper.ClampPortalName(portalNames[i]);
            displayNames[i] = PortalTextHelper.ClampDisplayName(displayNames[i]);
        }

        List<PortalStationBoard> group = StationGroupHelper.GetLinkedGroup(this, stationName);
        if (!StationGroupHelper.TryCollectPortalNames(group, this, portalNames, out string duplicate))
        {
            message = $"Duplicate portal name \"{duplicate}\" across linked stations.";
            NotifyPlayer(message);
            return false;
        }

        ZDO zdo = _nview.GetZDO();
        zdo.Set(ModConstants.ZdoStationName, stationName?.Trim() ?? string.Empty);
        for (int i = 0; i < ModConstants.DestinationSlotCount; i++)
        {
            zdo.Set(ModConstants.SlotPortalKey(i), portalNames[i] ?? string.Empty);
            zdo.Set(ModConstants.SlotDisplayKey(i), displayNames[i] ?? string.Empty);
        }

        TeleportWorld portal = StationGroupHelper.FindNearestPortal(
            transform.position,
            ModConstants.PortalLinkRadius);
        if (portal != null)
        {
            ZNetView portalView = portal.GetComponent<ZNetView>();
            if (portalView != null && portalView.IsValid())
            {
                zdo.Set(ModConstants.ZdoLinkedPortal, portalView.GetZDO().m_uid.ToString());
            }

            message = "Station configuration saved and linked to nearby portal.";
        }
        else
        {
            message = "Names saved. Build a portal within 15 m to enable teleport linking.";
        }

        RefreshSigns();
        RefreshAllLinkedStations(group);
        NotifyPlayer(message);
        return true;
    }

    private void ServerActivateDestination(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= ModConstants.DestinationSlotCount)
        {
            return;
        }

        TeleportWorld portal = ResolveLinkedPortal();
        if (portal == null)
        {
            NotifyPlayer("No linked portal found within 15 m.");
            return;
        }

        string portalName = GetSlotPortalName(slotIndex);
        if (string.IsNullOrWhiteSpace(PortalTextHelper.StripRichText(portalName)))
        {
            if (!PortalTagHelper.TryClearPortal(portal, out string clearError))
            {
                NotifyPlayer(clearError ?? "Could not clear portal name.");
                return;
            }

            RefreshSigns();
            RefreshAllLinkedStations();
            NotifyPlayer("Portal name cleared (unnamed pairing).");
            return;
        }

        string displayText = _nview.GetZDO().GetString(ModConstants.SlotDisplayKey(slotIndex), string.Empty);
        if (!PortalTagHelper.TrySetPortalTagAndDisplay(portal, portalName, displayText, out string error))
        {
            NotifyPlayer(error ?? "Could not update portal.");
            return;
        }

        RefreshSigns();
        RefreshAllLinkedStations();
        NotifyPlayer($"Portal set to \"{PortalTextHelper.StripRichText(GetSlotDisplayText(slotIndex))}\".");
    }

    private TeleportWorld ResolveLinkedPortal()
    {
        TeleportWorld portal = GetLinkedPortal();
        if (portal != null)
        {
            return portal;
        }

        portal = StationGroupHelper.FindNearestPortal(transform.position, ModConstants.PortalLinkRadius);
        if (portal == null)
        {
            return null;
        }

        ZNetView portalView = portal.GetComponent<ZNetView>();
        if (portalView != null && portalView.IsValid())
        {
            _nview.GetZDO().Set(ModConstants.ZdoLinkedPortal, portalView.GetZDO().m_uid.ToString());
        }

        return portal;
    }

    private static void RefreshAllLinkedStations(IEnumerable<PortalStationBoard> stations = null)
    {
        if (stations != null)
        {
            foreach (PortalStationBoard station in stations)
            {
                station?.RefreshSigns();
            }

            return;
        }

        foreach (PortalStationBoard station in Object.FindObjectsByType<PortalStationBoard>(FindObjectsSortMode.None))
        {
            station.RefreshSigns();
        }
    }

    private static bool TryParseZdoId(string value, out ZDOID zdoId)
    {
        zdoId = ZDOID.None;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        string[] parts = value.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!long.TryParse(parts[0], out long userId))
        {
            return false;
        }

        if (!uint.TryParse(parts[1], out uint id))
        {
            return false;
        }

        zdoId = new ZDOID(userId, id);
        return true;
    }

    private static string EncodeSlots(string[] values)
    {
        if (values == null)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < ModConstants.DestinationSlotCount; i++)
        {
            if (i > 0)
            {
                builder.Append('\n');
            }

            builder.Append(values.Length > i ? values[i] ?? string.Empty : string.Empty);
        }

        return builder.ToString();
    }

    private static string[] DecodeSlots(string payload)
    {
        string[] values = new string[ModConstants.DestinationSlotCount];
        if (string.IsNullOrEmpty(payload))
        {
            return values;
        }

        string[] split = payload.Split('\n');
        for (int i = 0; i < ModConstants.DestinationSlotCount && i < split.Length; i++)
        {
            values[i] = split[i];
        }

        return values;
    }

    private static void NotifyPlayer(string message)
    {
        Player.m_localPlayer?.Message(MessageHud.MessageType.Center, message);
    }
}
