using TMPro;
using UnityEngine;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// Put on any TMP text. Multiplies its design-time size by the global FontScale
    /// and updates live when the setting changes.
    /// Texts with Auto Size on scale their maximum (and minimum), so they still shrink to fit their box.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class ScaledText : MonoBehaviour
    {
        private TMP_Text _text;
        private float _baseSize;
        private float _baseMin;
        private bool _autoSize;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _autoSize = _text.enableAutoSizing;
            _baseSize = _autoSize ? _text.fontSizeMax : _text.fontSize;
            _baseMin = _text.fontSizeMin;
        }

        private void OnEnable()
        {
            GameContext.Settings.OnChanged += Apply;
            Apply();
        }

        private void OnDisable()
        {
            GameContext.Settings.OnChanged -= Apply;
        }

        private void Apply()
        {
            float scale = GameContext.Settings.FontScale;
            if (_autoSize)
            {
                _text.fontSizeMax = _baseSize * scale;
                _text.fontSizeMin = _baseMin * scale;
            }
            else
            {
                _text.fontSize = _baseSize * scale;
            }
        }
    }
}
