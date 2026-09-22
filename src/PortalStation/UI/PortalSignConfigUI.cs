using UnityEngine;

namespace PortalStation;

/// <summary>
/// IMGUI for editing a field portal's pairing tag and rich-text display name (E on the portal sign).
/// </summary>
public sealed class PortalSignConfigUI : MonoBehaviour
{
    public static PortalSignConfigUI Instance { get; private set; }

    public static bool IsOpen => Instance != null && Instance._visible;

    private PortalNameSign _target;
    private bool _visible;
    private string _portalName = string.Empty;
    private string _displayName = string.Empty;
    private string _statusMessage = string.Empty;
    private Rect _windowRect = new Rect(120f, 120f, 420f, 260f);
    private static Texture2D _solidTexture;
    private GUIStyle _panelStyle;
    private GUIStyle _labelStyle;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (!_visible)
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Open(PortalNameSign sign)
    {
        if (sign == null)
        {
            return;
        }

        _target = sign;
        _portalName = sign.GetPortalTagForEdit();
        _displayName = sign.GetDisplayTextForEdit();
        _statusMessage = string.Empty;
        _visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Close()
    {
        _visible = false;
        _target = null;
        _statusMessage = string.Empty;
    }

    private void OnGUI()
    {
        if (!_visible || _target == null)
        {
            return;
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            Close();
            Event.current.Use();
            return;
        }

        EnsureStyles();

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _solidTexture);
        GUI.color = previousColor;

        _windowRect = GUI.ModalWindow(
            913428,
            _windowRect,
            DrawWindow,
            Localization.instance.Localize("$piece_portal_station_portal_sign_title"),
            _panelStyle);
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();
        GUILayout.Label(Localization.instance.Localize("$piece_portal_station_portal_sign_help"), _labelStyle);
        GUILayout.Space(8f);

        GUILayout.Label(Localization.instance.Localize("$piece_portal_station_portal_tag_label"), _labelStyle);
        _portalName = PortalTextHelper.ClampPortalName(
            GUILayout.TextField(_portalName, GUILayout.Width(360f)));

        GUILayout.Space(6f);
        GUILayout.Label(Localization.instance.Localize("$piece_portal_station_display_label"), _labelStyle);
        _displayName = PortalTextHelper.ClampDisplayName(
            GUILayout.TextField(_displayName, GUILayout.Width(360f)));

        if (!string.IsNullOrEmpty(_statusMessage))
        {
            GUILayout.Space(6f);
            GUILayout.Label(_statusMessage, _labelStyle);
        }

        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(Localization.instance.Localize("$piece_portal_station_cancel"), GUILayout.Width(140f)))
        {
            Close();
        }

        if (GUILayout.Button(Localization.instance.Localize("$piece_portal_station_commit"), GUILayout.Width(140f)))
        {
            Commit();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
    }

    private void Commit()
    {
        if (_target == null)
        {
            return;
        }

        if (!_target.TryApplyConfiguration(_portalName, _displayName, out string message))
        {
            _statusMessage = message;
            return;
        }

        _statusMessage = message;
        Close();
    }

    private void EnsureStyles()
    {
        if (_solidTexture == null)
        {
            _solidTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _solidTexture.SetPixel(0, 0, Color.white);
            _solidTexture.Apply();
        }

        if (_panelStyle != null)
        {
            return;
        }

        _panelStyle = new GUIStyle(GUI.skin.window)
        {
            normal = { background = MakeTexture(new Color(0.1f, 0.12f, 0.16f, 0.97f)) },
            onNormal = { background = MakeTexture(new Color(0.1f, 0.12f, 0.16f, 0.97f)) },
            fontSize = 16,
            padding = new RectOffset(16, 16, 24, 16)
        };

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            wordWrap = true,
            normal = { textColor = new Color(0.95f, 0.95f, 0.9f, 1f) }
        };
    }

    private static Texture2D MakeTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
