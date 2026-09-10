using BepInEx.Configuration;

namespace PortalStation;

public static class ModConfig
{
    public const string DefaultHighlightColorValue = "<#87CEEB>";

    public static ConfigEntry<int> DisplayNameMaxLength;
    public static ConfigEntry<int> CraftCostWood;
    public static ConfigEntry<int> CraftCostCoal;
    public static ConfigEntry<string> DefaultPortalDescription;
    public static ConfigEntry<string> DefaultHighlightColor;

    public static void Bind(ConfigFile config)
    {
        DisplayNameMaxLength = config.Bind(
            ModConstants.ConfigSection,
            "DisplayNameMaxLength",
            ModConstants.DefaultDisplayNameMaxLength,
            new ConfigDescription(
                "DISPLAY TEXT ONLY — does not change the portal tag/name used for pairing (still max 10 plain characters via E on the portal). " +
                "This limit applies only to sign display descriptions (station slots and E on the portal sign). " +
                "Very high values will drastically shrink text size on signs.",
                new AcceptableValueRange<int>(1, 200)));

        CraftCostWood = config.Bind(
            ModConstants.ConfigSection,
            "CraftCostWood",
            ModConstants.DefaultCraftCostWood,
            new ConfigDescription(
                "Wood required to build a Portal Station.",
                new AcceptableValueRange<int>(0, 999)));

        CraftCostCoal = config.Bind(
            ModConstants.ConfigSection,
            "CraftCostCoal",
            ModConstants.DefaultCraftCostCoal,
            new ConfigDescription(
                "Coal required to build a Portal Station.",
                new AcceptableValueRange<int>(0, 999)));

        DefaultPortalDescription = config.Bind(
            ModConstants.ConfigSection,
            "DefaultPortalDescription",
            string.Empty,
            new ConfigDescription(
                "Default color for inactive sign display names when no color code is set (e.g. <#FFD700>). " +
                "Leave empty for none. Override per sign by adding a color code before the display name."));

        DefaultHighlightColor = config.Bind(
            ModConstants.ConfigSection,
            "DefaultHighlightColor",
            DefaultHighlightColorValue,
            new ConfigDescription(
                "Color for the currently active destination on a station board " +
                "(the slot matching the linked portal's tag). Example: <#87CEEB> or <color=cyan>."));
    }
}
