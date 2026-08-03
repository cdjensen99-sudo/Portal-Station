using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PortalStation;

internal static class StationHammerIcon
{
    private const string ResourceName = "PortalStation.Assets.Station_Icon.png";
    private const string PluginIconFileName = "Station_Icon.png";
    private static Sprite _cachedSprite;

    internal static void Apply(Piece piece)
    {
        if (piece == null)
        {
            return;
        }

        Sprite sprite = GetSprite();
        if (sprite != null)
        {
            piece.m_icon = sprite;
        }
    }

    internal static Sprite GetSprite()
    {
        if (_cachedSprite != null)
        {
            return _cachedSprite;
        }

        byte[] data = LoadIconBytes();
        if (data == null || data.Length == 0)
        {
            return null;
        }

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!TryDecodePng(texture, data))
        {
            UnityEngine.Object.Destroy(texture);
            PortalStationPlugin.Log.LogWarning("Failed to decode station hammer icon texture.");
            return null;
        }

        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float pixelsPerUnit = ResolvePixelsPerUnit();
        _cachedSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);
        return _cachedSprite;
    }

    private static byte[] LoadIconBytes()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream != null)
        {
            byte[] embedded = new byte[stream.Length];
            stream.Read(embedded, 0, embedded.Length);
            return embedded;
        }

        string pluginDirectory = Path.GetDirectoryName(assembly.Location);
        if (string.IsNullOrEmpty(pluginDirectory))
        {
            PortalStationPlugin.Log.LogWarning($"Station hammer icon resource not found: {ResourceName}");
            return null;
        }

        string iconPath = Path.Combine(pluginDirectory, PluginIconFileName);
        if (!File.Exists(iconPath))
        {
            PortalStationPlugin.Log.LogWarning(
                $"Station hammer icon missing from embedded resource and plugin folder: {iconPath}");
            return null;
        }

        return File.ReadAllBytes(iconPath);
    }

    private static bool TryDecodePng(Texture2D texture, byte[] data)
    {
        MethodInfo instanceMethod = typeof(Texture2D).GetMethod(
            "LoadImage",
            BindingFlags.Instance | BindingFlags.Public,
            null,
            new[] { typeof(byte[]) },
            null);
        if (instanceMethod != null)
        {
            return (bool)instanceMethod.Invoke(texture, new object[] { data });
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.GetName().Name.StartsWith("UnityEngine.ImageConversion", StringComparison.Ordinal))
            {
                continue;
            }

            Type conversionType = assembly.GetType("UnityEngine.ImageConversion");
            MethodInfo extensionMethod = conversionType?.GetMethod(
                "LoadImage",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(Texture2D), typeof(byte[]) },
                null);
            if (extensionMethod != null)
            {
                return (bool)extensionMethod.Invoke(null, new object[] { texture, data });
            }
        }

        return false;
    }

    private static float ResolvePixelsPerUnit()
    {
        GameObject portalPrefab = StationVisualHelper.TryGetPrefab("portal", "portal_wood", "piece_portal");
        Piece portalPiece = portalPrefab?.GetComponent<Piece>();
        return portalPiece?.m_icon != null ? portalPiece.m_icon.pixelsPerUnit : 100f;
    }
}
