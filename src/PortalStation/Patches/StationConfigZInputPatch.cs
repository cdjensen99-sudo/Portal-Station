using HarmonyLib;
using UnityEngine;

namespace PortalStation;

[HarmonyPatch]
internal static class StationConfigZInputPatch
{
    private static bool Block => StationConfigUI.IsOpen;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZInput), nameof(ZInput.GetButtonDown), typeof(string))]
    private static bool BlockGetButtonDown(string name, ref bool __result)
    {
        if (!Block)
        {
            return true;
        }

        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZInput), nameof(ZInput.GetButton), typeof(string))]
    private static bool BlockGetButton(string name, ref bool __result)
    {
        if (!Block)
        {
            return true;
        }

        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZInput), nameof(ZInput.GetButtonUp), typeof(string))]
    private static bool BlockGetButtonUp(string name, ref bool __result)
    {
        if (!Block)
        {
            return true;
        }

        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZInput), nameof(ZInput.GetKeyDown), typeof(KeyCode), typeof(bool))]
    private static bool BlockGetKeyDown(KeyCode key, bool logWarning, ref bool __result)
    {
        if (!Block)
        {
            return true;
        }

        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZInput), nameof(ZInput.GetKey), typeof(KeyCode), typeof(bool))]
    private static bool BlockGetKey(KeyCode key, bool logWarning, ref bool __result)
    {
        if (!Block)
        {
            return true;
        }

        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ZInput), nameof(ZInput.GetKeyUp), typeof(KeyCode), typeof(bool))]
    private static bool BlockGetKeyUp(KeyCode key, bool logWarning, ref bool __result)
    {
        if (!Block)
        {
            return true;
        }

        __result = false;
        return false;
    }
}
