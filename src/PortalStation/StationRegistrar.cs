using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace PortalStation;

internal static class StationRegistrar
{
    private static bool _registered;
    private static string _registeredBuildLabel = string.Empty;

    internal static void Init()
    {
        PrefabManager.OnVanillaPrefabsAvailable += Register;
    }

    private static void Register()
    {
        if (_registered && _registeredBuildLabel == ModConstants.BuildLabel)
        {
            return;
        }

        if (_registered)
        {
            PrefabManager.Instance.DestroyPrefab(ModConstants.PrefabStation);
            PrefabManager.Instance.DestroyPrefab(ModConstants.LegacyPrefabStation);
            _registered = false;
        }

        GameObject prefab = StationPrefabBuilder.Build();
        if (prefab == null)
        {
            PortalStationPlugin.Log.LogError("Failed to build Portal Station prefab.");
            return;
        }

        // CreateClonedPrefab already registers with PrefabManager; AddPiece hooks hammer/ZNetScene.
        PieceConfig config = new PieceConfig
        {
            Name = "$piece_portal_station",
            Description = "$piece_portal_station_desc",
            PieceTable = "Hammer",
            CraftingStation = CraftingStations.Workbench,
            Category = PieceCategories.Misc,
            Enabled = true
        };
        config.AddRequirement("Wood", ModConfig.CraftCostWood.Value);
        config.AddRequirement("Coal", ModConfig.CraftCostCoal.Value);

        Sprite icon = StationHammerIcon.GetSprite();
        if (icon != null)
        {
            config.Icon = icon;
        }

        PieceManager.Instance.AddPiece(new CustomPiece(prefab, fixReference: false, config));

        _registered = true;
        _registeredBuildLabel = ModConstants.BuildLabel;
        PrefabManager.OnVanillaPrefabsAvailable -= Register;
        PortalStationPlugin.Log.LogInfo(
            $"Portal Station registered with Jotunn ({ModConstants.BuildLabel}).");
    }
}
