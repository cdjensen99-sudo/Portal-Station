using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace PortalStation;

[BepInPlugin(ModConstants.ModGuid, ModConstants.ModName, ModConstants.ModVersion)]
[BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class PortalStationPlugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;

    private Harmony harmony;

    private void Awake()
    {
        Log = Logger;
        harmony = new Harmony(ModConstants.ModGuid);

        try
        {
            harmony.PatchAll(typeof(PortalStationPlugin).Assembly);
            Log.LogInfo($"Harmony applied {harmony.GetPatchedMethods().Count()} patch(es).");
        }
        catch (Exception ex)
        {
            Log.LogError($"Harmony PatchAll failed: {ex}");
        }

        StationRegistrar.Init();

        Log.LogInfo($"{ModConstants.ModName} {ModConstants.ModVersion} ({ModConstants.BuildLabel}) loaded.");
    }

    private void OnDestroy()
    {
        harmony?.UnpatchSelf();
    }
}
