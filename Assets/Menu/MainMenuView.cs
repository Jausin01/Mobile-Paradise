using MobileParadise.Core.Scenes;
using UnityEngine;

namespace MobileParadise.Menu
{
    /// <summary>
    /// Builds one big button per game in the catalog and opens its scene on tap.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private GameCatalog _catalog;
        [SerializeField] private GameButton _buttonPrefab;
        [SerializeField] private Transform _buttonsParent;

        private void Start()
        {
            foreach (GameEntry entry in _catalog.Games)
            {
                GameButton button = Instantiate(_buttonPrefab, _buttonsParent);
                button.Setup(entry, OnGameSelected);
            }
        }

        private void OnGameSelected(GameEntry entry)
        {
            SceneLoader.Load(entry.SceneName);
        }
    }
}
