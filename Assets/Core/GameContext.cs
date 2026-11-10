using MobileParadise.Core.Settings;
using UnityEngine;

namespace MobileParadise.Core
{
    /// <summary>
    /// The only global access point: games ask here for shared services.
    /// Settings are created at startup (so volume and language are applied before
    /// the first scene) and lazily otherwise.
    /// </summary>
    public static class GameContext
    {
        private static SettingsManager _settings;

        public static SettingsManager Settings
        {
            get
            {
                if (_settings == null) _settings = new SettingsManager();
                return _settings;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            object touch = Settings;
        }
    }
}
