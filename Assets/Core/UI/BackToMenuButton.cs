using MobileParadise.Core.Scenes;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// Put on a Button in any game scene: returns to the main menu.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BackToMenuButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(SceneLoader.LoadMenu);
        }
    }
}
