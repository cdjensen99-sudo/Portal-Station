using UnityEngine;

namespace PortalStation;

public sealed class StationConfigUI : MonoBehaviour
{
    public static StationConfigUI Instance { get; private set; }

    public static bool IsOpen => Instance != null && Instance._visible;

    private PortalStationBoard _target;
    private bool _visible;
    private string _stationName = string.Empty;
    private readonly string[] _portalNames = new string[ModConstants.DestinationSlotCount];
    private readonly string[] _displayNames = new string[ModConstants.DestinationSlotCount];
    private Vector2 _scroll;
    private string _statusMessage = string.Empty;
    private Rect _windowRect = new Rect(80f, 40f, 760f, 660f);
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

    public void Open(PortalStationBoard station)
    {
        if (station == null)
        {
            return;
        }

        _target = station;
        _stationName = station.GetStationName();
        for (int i = 0; i < ModConstants.DestinationSlotCount; i++)
        {
            _portalNames[i] = station.GetSlotPortalName(i);
            _displayNames[i] = station.GetSlotDisplayRaw(i);
        }

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
            913427,
            _windowRect,
            DrawWindow,
            Localization.instance.Localize("$piece_portal_station_configure_title"),
            _panelStyle);
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();
        GUILayout.Label(Localization.instance.Localize("$piece_portal_station_name_label"), _labelStyle);
        _stationName = GUILayout.TextField(_stationName, GUILayout.Width(680f));

        GUILayout.Space(8f);
        GUILayout.Label(Localization.instance.Localize("$piece_portal_station_slots_help"), _labelStyle);
        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(470f));

        for (int row = 0; row < ModConstants.GridRows; row++)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < ModConstants.GridColumns; col++)
            {
                int index = row * ModConstants.GridColumns + col;
                GUILayout.BeginVertical(GUILayout.Width(230f));
                GUILayout.Label($"#{index + 1}", _labelStyle);
                GUILayout.Label("Portal tag", _labelStyle);
                string portal = GUILayout.TextField(_portalNames[index], GUILayout.Width(210f));
                _portalNames[index] = PortalTextHelper.ClampPortalName(portal);
                GUILayout.Label("Display text", _labelStyle);
                _displayNames[index] = GUILayout.TextField(
                    _displayNames[index],
                    GUILayout.Width(210f));
                if (_displayNames[index].Length > ModConstants.DisplayNameMaxLength)
                {
                    _displayNames[index] = _displayNames[index].Substring(0, ModConstants.DisplayNameMaxLength);
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }

        GUILayout.EndScrollView();

        if (!string.IsNullOrEmpty(_statusMessage))
        {
            GUILayout.Label(_statusMessage, _labelStyle);
        }

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

        if (!_target.TryApplyConfiguration(_stationName, _portalNames, _displayNames, out string message))
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
