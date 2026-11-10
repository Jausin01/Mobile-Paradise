using System.Collections.Generic;
using UnityEngine;

namespace MobileParadise.Menu
{
    /// <summary>
    /// Data asset with the list of games. Adding a game = adding one entry here.
    /// Create via: right click > Create > Mobile Paradise > Game Catalog.
    /// </summary>
    [CreateAssetMenu(fileName = "GameCatalog", menuName = "Mobile Paradise/Game Catalog")]
    public class GameCatalog : ScriptableObject
    {
        public List<GameEntry> Games = new List<GameEntry>();
    }
}
