namespace PortalStation;

public static class ModConstants
{
    public const string ModName = "Portal Station";
    public const string ModVersion = "0.3.2";
    public const string ModGuid = "com.portalstation";
    public const string BuildLabel = "v0.3.2";

    public const string ConfigSection = "PortalStation";

    public const string PrefabStation = "portal_station";
    public const string LegacyPrefabStation = "runic_portal_station";

    public const float StationVisualScale = 0.75f;

    public const float StationHeaderSignForwardOffset = 0.1f;
    public const float StationDestinationSignForwardOffset = 0.37f;
    public const float StationSignFaceEpsilon = 0.02f;

    public const int DestinationSlotCount = 18;
    public const int GridColumns = 3;
    public const int GridRows = 6;

    public const int PortalNameMaxLength = 10;
    public const int DefaultDisplayNameMaxLength = 50;
    public const int DefaultCraftCostWood = 58;
    public const int DefaultCraftCostCoal = 19;

    public const float PortalLinkRadius = 15f;
    public const float StationGroupRadius = 10f;

    public const string ZdoStationName = "PortalStation_StationName";
    public const string ZdoLinkedPortal = "PortalStation_LinkedPortal";
    public const string ZdoDisplayText = "PortalStation_DisplayText";
    public const string ZdoPortalTag = "tag";
    public const string ZdoStationLinked = "PortalStation_StationLinked";

    public const string RpcApplyStationConfig = "portal_station.apply_config";
    public const string RpcActivateDestination = "portal_station.activate_destination";
    public const string RpcPortalSignText = "portal_station.portal_sign";

    public static string SlotPortalKey(int index) => $"PortalStation_Slot{index}_Portal";
    public static string SlotDisplayKey(int index) => $"PortalStation_Slot{index}_Display";
}
