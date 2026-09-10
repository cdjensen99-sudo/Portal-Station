using System.Text.RegularExpressions;
using UnityEngine;

namespace PortalStation;

internal static class PortalTextHelper
{
    private static readonly Regex HexColorTagRegex = new Regex(
        @"<#[0-9a-fA-F]{6,8}>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex NamedColorTagRegex = new Regex(
        @"<color=[^>]+>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HexValueRegex = new Regex(
        @"#?[0-9a-fA-F]{6,8}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    internal static int DisplayNameMaxLength =>
        ModConfig.DisplayNameMaxLength?.Value ?? ModConstants.DefaultDisplayNameMaxLength;

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

    internal static string ClampDisplayName(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        int maxLength = DisplayNameMaxLength;
        if (text.Length <= maxLength)
        {
            return text;
        }

        return text.Substring(0, maxLength);
    }

    internal static string BuildDisplayText(string portalName, string displayText)
    {
        if (!string.IsNullOrWhiteSpace(displayText))
        {
            return displayText.Trim();
        }

        return portalName ?? string.Empty;
    }

    internal static string FormatDisplayForRender(string displayText, bool highlighted = false)
    {
        if (string.IsNullOrWhiteSpace(displayText) || displayText == "...")
        {
            return displayText ?? string.Empty;
        }

        string trimmed = displayText.Trim();
        bool hasExplicitColor = HasExplicitColor(trimmed);

        // Explicit per-sign color wins while inactive. Active destination always uses highlight color.
        if (hasExplicitColor && !highlighted)
        {
            return trimmed;
        }

        string colorTag = highlighted
            ? GetHighlightColorTag()
            : ModConfig.DefaultPortalDescription?.Value?.Trim();

        if (string.IsNullOrEmpty(colorTag))
        {
            return hasExplicitColor ? StripRichText(trimmed) : trimmed;
        }

        string plain = hasExplicitColor ? StripRichText(trimmed) : trimmed;
        return NormalizeColorTag(colorTag) + plain;
    }

    internal static bool PortalNamesEqual(string left, string right)
    {
        return string.Equals(
            StripRichText(left),
            StripRichText(right),
            System.StringComparison.OrdinalIgnoreCase);
    }

    internal static string GetHighlightColorTag()
    {
        string raw = ModConfig.DefaultHighlightColor?.Value?.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            return ModConfig.DefaultHighlightColorValue;
        }

        return raw;
    }

    internal static bool TryGetHighlightColor(out Color color)
    {
        return TryParseColorTag(GetHighlightColorTag(), out color);
    }

    internal static bool TryGetDefaultDisplayColor(out Color color)
    {
        string raw = ModConfig.DefaultPortalDescription?.Value?.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            color = Color.white;
            return false;
        }

        return TryParseColorTag(raw, out color);
    }

    internal static bool HasExplicitColor(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return HexColorTagRegex.IsMatch(text) || NamedColorTagRegex.IsMatch(text);
    }

    private static string NormalizeColorTag(string colorTag)
    {
        if (string.IsNullOrWhiteSpace(colorTag))
        {
            return string.Empty;
        }

        string trimmed = colorTag.Trim();
        if (trimmed.StartsWith("<", System.StringComparison.Ordinal))
        {
            return trimmed;
        }

        if (trimmed.StartsWith("#", System.StringComparison.Ordinal))
        {
            return $"<{trimmed}>";
        }

        return $"<color={trimmed}>";
    }

    private static bool TryParseColorTag(string colorTag, out Color color)
    {
        color = new Color(135f / 255f, 206f / 255f, 235f / 255f);
        if (string.IsNullOrWhiteSpace(colorTag))
        {
            return false;
        }

        string trimmed = colorTag.Trim();
        Match namedTag = Regex.Match(trimmed, @"<color=([^>]+)>", RegexOptions.IgnoreCase);
        if (namedTag.Success)
        {
            return TryNamedColor(namedTag.Groups[1].Value, out color);
        }

        Match hexTag = HexColorTagRegex.Match(trimmed);
        if (hexTag.Success)
        {
            return TryParseHex(hexTag.Value.Trim('<', '>'), out color);
        }

        if (TryParseHex(trimmed, out color))
        {
            return true;
        }

        return TryNamedColor(trimmed, out color);
    }

    private static bool TryParseHex(string value, out Color color)
    {
        color = Color.white;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string hex = value.Trim();
        if (!hex.StartsWith("#", System.StringComparison.Ordinal))
        {
            hex = "#" + hex;
        }

        return ColorUtility.TryParseHtmlString(hex, out color);
    }

    private static bool TryNamedColor(string name, out Color color)
    {
        switch (name.Trim().ToLowerInvariant())
        {
            case "white": color = Color.white; return true;
            case "black": color = Color.black; return true;
            case "red": color = Color.red; return true;
            case "green": color = Color.green; return true;
            case "blue": color = Color.blue; return true;
            case "yellow": color = Color.yellow; return true;
            case "cyan": color = Color.cyan; return true;
            case "magenta": color = Color.magenta; return true;
            case "gray":
            case "grey": color = Color.gray; return true;
            case "orange": color = new Color(1f, 0.5f, 0f); return true;
            default:
                color = Color.white;
                return false;
        }
    }
}
