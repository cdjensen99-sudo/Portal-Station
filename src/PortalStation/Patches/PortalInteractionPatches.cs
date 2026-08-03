using HarmonyLib;
using UnityEngine;

namespace PortalStation;

[HarmonyPatch(typeof(WearNTear), "OnPlaced")]
internal static class PortalPlacementPatch
{
    private static void Postfix(WearNTear __instance)
    {
        if (__instance == null)
        {
            return;
        }

        PortalDisplaySignSpawner.EnsureDisplaySign(__instance.gameObject);
    }
}

[HarmonyPatch(typeof(TeleportWorld), "Awake")]
internal static class PortalAwakeSignPatch
{
    private static void Postfix(TeleportWorld __instance)
    {
        if (__instance == null)
        {
            return;
        }

        if (__instance.GetComponent<PortalSignInitRunner>() != null)
        {
            return;
        }

        __instance.gameObject.AddComponent<PortalSignInitRunner>();
    }
}

[HarmonyPatch(typeof(TeleportWorld), "SetText")]
internal static class PortalSetTextPatch
{
    private static void Postfix(TeleportWorld __instance)
    {
        if (__instance == null)
        {
            return;
        }

        // Portal tag changed via vanilla E — refresh sign but do not copy tag into display text.
        PortalTagHelper.RefreshPortalSign(__instance);
    }
}

[HarmonyPatch(typeof(Player), "Update")]
internal static class DestinationActivatePatch
{
    private static void Postfix(Player __instance)
    {
        if (__instance != Player.m_localPlayer)
        {
            return;
        }

        if (StationConfigUI.IsOpen)
        {
            return;
        }

        if (!ZInput.GetButtonDown("Attack"))
        {
            return;
        }

        if (__instance.InAttack() || __instance.InMinorAction())
        {
            return;
        }

        if (!StationDestinationInputHelper.IsHoveringDestinationSign(__instance))
        {
            return;
        }

        GameObject hover = __instance.GetHoverObject();
        StationDestinationSign destination = hover.GetComponent<StationDestinationSign>()
            ?? hover.GetComponentInParent<StationDestinationSign>();
        destination?.TryActivate();
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetControls))]
internal static class PlayerSetControlsDestinationPatch
{
    private static void Prefix(
        ref bool attack,
        ref bool attackHold,
        ref bool secondaryAttack,
        ref bool secondaryAttackHold)
    {
        if (!StationDestinationInputHelper.IsHoveringDestinationSign(Player.m_localPlayer))
        {
            return;
        }

        attack = false;
        attackHold = false;
        secondaryAttack = false;
        secondaryAttackHold = false;
    }
}

[HarmonyPatch(typeof(Player), "PlayerAttackInput")]
internal static class PlayerAttackInputDestinationPatch
{
    private static bool Prefix()
    {
        return !StationDestinationInputHelper.IsHoveringDestinationSign(Player.m_localPlayer);
    }
}
