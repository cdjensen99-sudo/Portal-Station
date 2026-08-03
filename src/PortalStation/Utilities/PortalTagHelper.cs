using UnityEngine;

namespace PortalStation;

internal static class PortalTagHelper
{
    internal static string GetPortalTag(TeleportWorld portal)
    {
        if (portal == null)
        {
            return string.Empty;
        }

        ZNetView nview = portal.GetComponent<ZNetView>();
        ZDO zdo = nview?.GetZDO();
        if (zdo == null)
        {
            return string.Empty;
        }

        return PortalTextHelper.StripRichText(zdo.GetString(ModConstants.ZdoPortalTag, string.Empty));
    }

    internal static string GetPortalDisplayText(TeleportWorld portal)
    {
        ZDO zdo = portal?.GetComponent<ZNetView>()?.GetZDO();
        return zdo?.GetString(ModConstants.ZdoDisplayText, string.Empty) ?? string.Empty;
    }

    internal static bool TrySetPortalDisplay(TeleportWorld portal, string displayText, out string error)
    {
        error = null;
        if (portal == null)
        {
            error = "Portal not found.";
            return false;
        }

        displayText = PortalTextHelper.ClampDisplayName(displayText?.Trim() ?? string.Empty);

        ZNetView nview = portal.GetComponent<ZNetView>();
        if (nview == null || !nview.IsValid())
        {
            error = "Portal is not ready yet.";
            return false;
        }

        nview.GetZDO().Set(ModConstants.ZdoDisplayText, displayText);
        RefreshPortalSign(portal);
        return true;
    }

    internal static bool TrySetPortalTagAndDisplay(
        TeleportWorld portal,
        string portalName,
        string displayText,
        out string error)
    {
        error = null;
        if (portal == null)
        {
            error = "No linked portal found.";
            return false;
        }

        string plainName = PortalTextHelper.ClampPortalName(portalName);
        if (string.IsNullOrWhiteSpace(plainName))
        {
            error = "Portal name cannot be empty.";
            return false;
        }

        if (plainName.Length > ModConstants.PortalNameMaxLength)
        {
            error = $"Portal name cannot exceed {ModConstants.PortalNameMaxLength} characters.";
            return false;
        }

        portal.SetText(plainName);

        ZNetView nview = portal.GetComponent<ZNetView>();
        string finalDisplay = PortalTextHelper.BuildDisplayText(plainName, displayText);
        if (nview != null && nview.IsValid())
        {
            nview.GetZDO().Set(ModConstants.ZdoDisplayText, finalDisplay);
            nview.GetZDO().Set(ModConstants.ZdoStationLinked, true);
        }

        PortalNameSign displaySign = portal.GetComponentInChildren<PortalNameSign>(true);
        displaySign?.SetDisplayText(PortalTextHelper.FormatDisplayForRender(finalDisplay));

        return true;
    }

    internal static bool TryClearPortal(TeleportWorld portal, out string error)
    {
        error = null;
        if (portal == null)
        {
            error = "No linked portal found.";
            return false;
        }

        portal.SetText(string.Empty);

        ZNetView nview = portal.GetComponent<ZNetView>();
        if (nview != null && nview.IsValid())
        {
            nview.GetZDO().Set(ModConstants.ZdoDisplayText, string.Empty);
            nview.GetZDO().Set(ModConstants.ZdoStationLinked, false);
        }

        RefreshPortalSign(portal);
        return true;
    }

    internal static void RefreshPortalSign(TeleportWorld portal)
    {
        PortalNameSign displaySign = portal?.GetComponentInChildren<PortalNameSign>(true);
        displaySign?.RefreshFromPortal();
    }
}
