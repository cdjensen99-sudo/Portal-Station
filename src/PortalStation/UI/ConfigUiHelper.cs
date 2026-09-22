namespace PortalStation;

internal static class ConfigUiHelper
{
    internal static bool IsOpen => StationConfigUI.IsOpen || PortalSignConfigUI.IsOpen;
}
