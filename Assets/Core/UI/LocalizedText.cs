using MobileParadise.Core.Localization;
using TMPro;
using UnityEngine;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// Keeps a TMP text in the current language and updates it when the language changes.
    /// Use LocalizedText.Bind(text, "some.key") from code or add the component in the Editor.
    /// If the key is not registered, the text is left as it is.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;

        private TMP_Text _text;

        public static void Bind(TMP_Text text, string key)
        {
            LocalizedText localized = text.GetComponent<LocalizedText>();
            if (localized == null)
            {
                localized = text.gameObject.AddComponent<LocalizedText>();
            }
            localized.SetKey(key);
        }

        public void SetKey(string key)
        {
            _key = key;
            Apply();
        }

        private void OnEnable()
        {
            Texts.LanguageChanged += Apply;
            Apply();
        }

        private void OnDisable()
        {
            Texts.LanguageChanged -= Apply;
        }

        private void Apply()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
            if (string.IsNullOrEmpty(_key) || !Texts.Has(_key)) return;

            _text.text = Texts.Get(_key);
        }
    }
}
