using TMPro;
using UnityEngine;

namespace PortalStation;

public sealed class StationSignVisual : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private Color _defaultColor = Color.white;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>(true);
        if (_text != null)
        {
            _defaultColor = _text.color;
            _text.richText = true;
            _text.text = "...";
        }
    }

    public void SetDisplayText(string text)
    {
        if (_text != null)
        {
            _text.text = text ?? string.Empty;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (_text == null)
        {
            return;
        }

        // Rich-text color tags (default/active) already drive appearance via SetDisplayText.
        // Keep vertex color as a fallback only when the rendered string has no color tag.
        if (PortalTextHelper.HasExplicitColor(_text.text))
        {
            _text.color = Color.white;
            return;
        }

        if (highlighted && PortalTextHelper.TryGetHighlightColor(out Color highlightColor))
        {
            _text.color = highlightColor;
            return;
        }

        if (!highlighted && PortalTextHelper.TryGetDefaultDisplayColor(out Color defaultDisplayColor))
        {
            _text.color = defaultDisplayColor;
            return;
        }

        _text.color = _defaultColor;
    }
}
