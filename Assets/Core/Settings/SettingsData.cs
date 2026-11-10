using System;

namespace MobileParadise.Core.Settings
{
    /// <summary>
    /// Serializable settings shared by every game.
    /// Bump Version when fields change so old saves can be migrated.
    /// Missing fields in an old save keep the defaults below.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        public int Version = 3;
        public float FontScale = 1.0f;   // 1.0 = normal, 1.5 = large, 2.0 = extra large
        public string Language = "";     // "pt", "en" or "" (not chosen yet: use the phone's language)

        public float MusicVolume = 0.5f; // 0..1
        public float SfxVolume = 1.0f;   // 0..1
        public bool MusicOn = true;
        public bool MusicRepeat = true;  // true: the track loops; false: the next track plays when it ends
        public string MusicTrack = "";   // "" = automatic (menu / game music), otherwise a file name in Resources/Audio/Music
    }
}
