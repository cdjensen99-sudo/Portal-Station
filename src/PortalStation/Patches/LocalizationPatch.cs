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

        translations["piece_runic_station"] = "Portal Station";
        translations["piece_runic_station_desc"] =
            "A sign board that quickly retargets a nearby portal. Configure station and destination names, then click a sign to switch the linked portal.";
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
