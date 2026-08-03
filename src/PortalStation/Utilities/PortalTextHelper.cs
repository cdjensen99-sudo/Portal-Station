namespace PortalStation;

internal static class PortalTextHelper
{
    internal static string StripRichText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return text.RemoveRichTextTags().Trim();
    }

    internal static string ClampPortalName(string text)
    {
        string plain = StripRichText(text);
        if (plain.Length <= ModConstants.PortalNameMaxLength)
        {
            return plain;
        }

        return plain.Substring(0, ModConstants.PortalNameMaxLength);
    }

    internal static string BuildDisplayText(string portalName, string displayText)
    {
        if (!string.IsNullOrWhiteSpace(displayText))
        {
            return displayText.Trim();
        }

        return portalName ?? string.Empty;
    }

    internal static bool PortalNamesEqual(string left, string right)
    {
        return string.Equals(
            StripRichText(left),
            StripRichText(right),
            System.StringComparison.OrdinalIgnoreCase);
    }
}
