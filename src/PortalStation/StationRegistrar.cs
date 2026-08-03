using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace PortalStation;

internal static class StationRegistrar
{
    private static bool _registered;

    internal static void Init()
    {
        PrefabManager.OnVanillaPrefabsAvailable += Register;
    }

    private static void Register()
    {
        if (_registered)
        {
            return;
        }

        GameObject prefab = StationPrefabBuilder.Build();
        if (prefab == null)
        {
            PortalStationPlugin.Log.LogError("Failed to build Portal Station prefab.");
            return;
        }

        PrefabManager.Instance.AddPrefab(prefab);

        PieceConfig config = new PieceConfig
        {
            Name = "$piece_runic_station",
            Description = "$piece_runic_station_desc",
            PieceTable = "Hammer",
            CraftingStation = CraftingStations.Workbench,
            Category = PieceCategories.Misc,
            Enabled = true
        };
        config.AddRequirement("Wood", 58);
        config.AddRequirement("Coal", 19);

        Sprite icon = StationHammerIcon.GetSprite();
        if (icon != null)
        {
            config.Icon = icon;
        }

        PieceManager.Instance.AddPiece(new CustomPiece(prefab, fixReference: false, config));

        _registered = true;
        PrefabManager.OnVanillaPrefabsAvailable -= Register;
        PortalStationPlugin.Log.LogInfo("Portal Station registered with Jotunn (prefab + hammer piece).");
    }
}
