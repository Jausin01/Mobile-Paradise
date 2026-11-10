using System;
using MobileParadise.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Menu
{
    /// <summary>
    /// Component on the button prefab: shows one GameEntry and reports clicks.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class GameButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _icon;

        public void Setup(GameEntry entry, Action<GameEntry> onClick)
        {
            _label.text = entry.DisplayName;
            if (!string.IsNullOrEmpty(entry.TitleKey))
            {
                LocalizedText.Bind(_label, entry.TitleKey);
            }

            if (_icon != null)
            {
                _icon.sprite = entry.Icon;
                _icon.gameObject.SetActive(entry.Icon != null);
            }

            GetComponent<Button>().onClick.AddListener(() => onClick(entry));
        }
    }
}
