using UnityEngine;

namespace PortalStation;

/// <summary>
/// Display sign above a portal. E on the sign edits display text only (rich text, up to 50 chars).
/// Portal tag is edited separately via vanilla E on the TeleportWorld (max 10 chars).
/// </summary>
public sealed class PortalNameSign : MonoBehaviour, Interactable, Hoverable, TextReceiver
{
    private TeleportWorld _portal;
    private StationSignVisual _visual;
    private bool _rpcRegistered;

    internal void Bind(TeleportWorld portal)
    {
        _portal = portal;
        _visual = GetComponent<StationSignVisual>();
        RegisterRpc();
        RefreshFromPortal();
    }

    public string GetHoverName()
    {
        return "Sign";
    }

    public string GetHoverText()
    {
        string text = PortalTextHelper.StripRichText(GetStoredDisplayText());
        if (string.IsNullOrWhiteSpace(text))
        {
            text = "...";
        }

        return $"\"{text}\"\nSign\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use";
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

        TextInput.instance.RequestText(this, "$piece_sign_input", ModConstants.DisplayNameMaxLength);
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        return false;
    }

    public string GetText()
    {
        return GetStoredDisplayText();
    }

    public void SetText(string text)
    {
        if (_portal == null)
        {
            return;
        }

        text = text?.Trim() ?? string.Empty;
        if (text.Length > ModConstants.DisplayNameMaxLength)
        {
            Player.m_localPlayer?.Message(
                MessageHud.MessageType.Center,
                $"Sign text cannot exceed {ModConstants.DisplayNameMaxLength} characters.");
            return;
        }

        ZNetView nview = _portal.GetComponent<ZNetView>();
        if (nview != null && nview.IsValid() && ZNet.instance != null && ZNet.instance.IsServer())
        {
            ApplyDisplayText(text);
            return;
        }

        if (nview != null && nview.IsValid())
        {
            nview.InvokeRPC(ModConstants.RpcPortalSignText, text);
        }
    }

    internal void RefreshFromPortal()
    {
        if (_portal == null || _visual == null)
        {
            return;
        }

        string display = GetStoredDisplayText();
        if (string.IsNullOrWhiteSpace(PortalTextHelper.StripRichText(display)))
        {
            display = "...";
        }

        _visual.SetDisplayText(display);
    }

    internal void SetDisplayText(string displayText)
    {
        _visual?.SetDisplayText(displayText);
    }

    private string GetStoredDisplayText()
    {
        return PortalTagHelper.GetPortalDisplayText(_portal);
    }

    private void RegisterRpc()
    {
        if (_rpcRegistered || _portal == null)
        {
            return;
        }

        ZNetView nview = _portal.GetComponent<ZNetView>();
        if (nview == null)
        {
            return;
        }

        nview.Register<string>(ModConstants.RpcPortalSignText, RPC_ApplyPortalSignText);
        _rpcRegistered = true;
    }

    private void ApplyDisplayText(string displayText)
    {
        if (!PortalTagHelper.TrySetPortalDisplay(_portal, displayText, out string error))
        {
            Player.m_localPlayer?.Message(MessageHud.MessageType.Center, error ?? "Could not update sign.");
            return;
        }

        foreach (PortalStationBoard station in Object.FindObjectsByType<PortalStationBoard>(FindObjectsSortMode.None))
        {
            station.RefreshSigns();
        }
    }

    private void RPC_ApplyPortalSignText(long sender, string text)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer())
        {
            return;
        }

        if (text != null && text.Length > ModConstants.DisplayNameMaxLength)
        {
            return;
        }

        ApplyDisplayText(text?.Trim() ?? string.Empty);
    }
}
