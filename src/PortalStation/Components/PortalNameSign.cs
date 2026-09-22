using UnityEngine;

namespace PortalStation;

/// <summary>
/// Sign above a portal. E opens a config UI for portal tag (pairing) and display text (rich text).
/// </summary>
public sealed class PortalNameSign : MonoBehaviour, Interactable, Hoverable
{
    private TeleportWorld _portal;
    private StationSignVisual _visual;

    internal void Bind(TeleportWorld portal)
    {
        _portal = portal;
        _visual = GetComponent<StationSignVisual>();
        RefreshFromPortal();
    }

    public string GetHoverName()
    {
        return "$piece_portal_station_sign";
    }

    public float GetHoverOffset()
    {
        return 0f;
    }

    public string GetHoverText()
    {
        string tag = PortalTextHelper.StripRichText(GetPortalTagForEdit());
        string display = PortalTextHelper.StripRichText(GetDisplayTextForEdit());
        if (string.IsNullOrWhiteSpace(display))
        {
            display = "...";
        }

        if (string.IsNullOrWhiteSpace(tag))
        {
            tag = "...";
        }

        return Localization.instance.Localize(
            $"\"{display}\"\n$piece_portal_station_sign\n" +
            $"$piece_portal_station_portal_tag_label: {tag}\n" +
            $"[<color=yellow><b>$KEY_Use</b></color>] $piece_portal_station_configure_portal");
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (hold || _portal == null)
        {
            return false;
        }

        if (!PrivateArea.CheckAccess(transform.position))
        {
            return false;
        }

        PortalSignConfigUI.Instance?.Open(this);
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return false;
    }

    internal string GetPortalTagForEdit()
    {
        return PortalTagHelper.GetPortalTag(_portal);
    }

    internal string GetDisplayTextForEdit()
    {
        return PortalTagHelper.GetPortalDisplayText(_portal);
    }

    internal bool TryApplyConfiguration(string portalName, string displayName, out string message)
    {
        message = string.Empty;

        if (_portal == null)
        {
            message = "Portal is not ready yet. Try again in a moment.";
            return false;
        }

        portalName = PortalTextHelper.ClampPortalName(portalName);
        displayName = PortalTextHelper.ClampDisplayName(displayName?.Trim() ?? string.Empty);

        if (!PortalTagHelper.TrySetPortalTagAndDisplay(_portal, portalName, displayName, out string error))
        {
            message = error ?? "Could not update portal sign.";
            Player.m_localPlayer?.Message(MessageHud.MessageType.Center, message);
            return false;
        }

        foreach (PortalStationBoard station in Object.FindObjectsByType<PortalStationBoard>(FindObjectsSortMode.None))
        {
            station.RefreshSigns();
        }

        message = "Portal sign saved.";
        PortalStationPlugin.Log.LogInfo(
            $"Portal sign applied (owner={_portal.GetComponent<ZNetView>()?.IsOwner() == true}).");
        return true;
    }

    internal void RefreshFromPortal()
    {
        if (_portal == null || _visual == null)
        {
            return;
        }

        string display = GetDisplayTextForEdit();
        if (string.IsNullOrWhiteSpace(PortalTextHelper.StripRichText(display)))
        {
            string tag = GetPortalTagForEdit();
            display = string.IsNullOrWhiteSpace(tag) ? "..." : tag;
        }

        _visual.SetDisplayText(PortalTextHelper.FormatDisplayForRender(display));
    }

    internal void SetDisplayText(string displayText)
    {
        _visual?.SetDisplayText(displayText);
    }
}
