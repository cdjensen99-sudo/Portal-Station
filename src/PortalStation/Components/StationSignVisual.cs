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

        _text.color = highlighted && PortalTextHelper.TryGetHighlightColor(out Color highlightColor)
            ? highlightColor
            : _defaultColor;
    }
}
