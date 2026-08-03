using System.Collections.Generic;
using HarmonyLib;

namespace PortalStation;

internal static class ModLocalization
{
    private static bool registered;
    private static readonly AccessTools.FieldRef<Localization, Dictionary<string, string>> TranslationsField =
        AccessTools.FieldRefAccess<Localization, Dictionary<string, string>>("m_translations");

    internal static void Register()
    {
        if (registered || Localization.instance == null)
        {
            return;
        }

        if (!(TranslationsField(Localization.instance) is Dictionary<string, string> translations))
        {
            return;
        }

        registered = true;

        translations["piece_portal_station"] = "Portal Station";
        translations["piece_portal_station_desc"] =
            "A sign board that quickly retargets a nearby portal. Configure station and destination names, then click a sign to switch the linked portal.";
        translations["piece_portal_station_header"] = "Station Nameplate";
        translations["piece_portal_station_configure"] = "Configure station";
        translations["piece_portal_station_configure_title"] = "Portal Station Configuration";
        translations["piece_portal_station_name_label"] = "Station name";
        translations["piece_portal_station_slots_help"] =
            "Portal name (max 10, used for pairing) and optional display text (color codes allowed). Linked stations with the same name within 10 m share one destination list.";
        translations["piece_portal_station_cancel"] = "Cancel";
        translations["piece_portal_station_commit"] = "Commit";
        translations["piece_portal_station_destination"] = "Destination Sign";
        translations["piece_portal_station_destination_empty"] = "Empty destination slot";
        translations["piece_portal_station_activate"] = "Activate destination";
        translations["piece_portal_station_clear"] = "Clear portal name";
    }
}

[HarmonyPatch(typeof(FejdStartup), "SetupGui")]
internal static class FejdStartupSetupGuiPatch
{
    private static void Postfix()
    {
        ModLocalization.Register();
    }
}
