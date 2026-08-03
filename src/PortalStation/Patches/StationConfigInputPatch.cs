using HarmonyLib;
using UnityEngine;

namespace PortalStation;

[HarmonyPatch(typeof(Player), "TakeInput")]
internal static class PlayerTakeInputStationConfigPatch
{
    private static void Postfix(ref bool __result)
    {
        if (StationConfigUI.IsOpen)
        {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(PlayerController), "TakeInput", typeof(bool))]
internal static class PlayerControllerTakeInputStationConfigPatch
{
    private static void Postfix(ref bool __result)
    {
        if (StationConfigUI.IsOpen)
        {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.SetControls))]
internal static class PlayerSetControlsStationConfigPatch
{
    private static bool Prefix(
        ref Vector3 movedir,
        ref bool attack,
        ref bool attackHold,
        ref bool secondaryAttack,
        ref bool secondaryAttackHold,
        ref bool block,
        ref bool blockHold,
        ref bool jump,
        ref bool crouch,
        ref bool run,
        ref bool autoRun,
        ref bool dodge)
    {
        if (!StationConfigUI.IsOpen)
        {
            return true;
        }

        movedir = Vector3.zero;
        attack = false;
        attackHold = false;
        secondaryAttack = false;
        secondaryAttackHold = false;
        block = false;
        blockHold = false;
        jump = false;
        crouch = false;
        run = false;
        autoRun = false;
        dodge = false;
        return true;
    }
}

[HarmonyPatch(typeof(Player), "PlayerAttackInput")]
internal static class PlayerAttackInputStationConfigPatch
{
    private static bool Prefix()
    {
        return !StationConfigUI.IsOpen;
    }
}

[HarmonyPatch(typeof(Minimap), "Update")]
internal static class MinimapStationConfigPatch
{
    private static bool Prefix()
    {
        return !StationConfigUI.IsOpen;
    }
}

[HarmonyPatch(typeof(GameCamera), "UpdateMouseCapture")]
internal static class GameCameraMouseCaptureStationConfigPatch
{
    private static bool Prefix()
    {
        if (!StationConfigUI.IsOpen)
        {
            return true;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        return false;
    }
}

[HarmonyPatch(typeof(GameCamera), "UpdateCamera")]
internal static class GameCameraUpdateStationConfigPatch
{
    private static bool Prefix()
    {
        return !StationConfigUI.IsOpen;
    }
}
