using MobileParadise.Core.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// Put on a Button: plays the "ui_click" sound when it is pressed.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(PlayClick);
        }

        private static void PlayClick()
        {
            AudioManager.PlaySfx("ui_click");
        }
    }
}
