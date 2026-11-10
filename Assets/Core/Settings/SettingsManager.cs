using System;
using MobileParadise.Core.Localization;
using MobileParadise.Core.Save;
using UnityEngine;

namespace MobileParadise.Core.Settings
{
    /// <summary>
    /// Holds the current settings, saves them on change, applies them
    /// (language) and notifies listeners through OnChanged.
    /// The AudioManager listens to OnChanged and applies the music and effects settings.
    /// </summary>
    public class SettingsManager
    {
        public const string SaveKey = "settings";
        public const float MinFontScale = 1.0f;
        public const float MaxFontScale = 2.0f;

        private SettingsData _data;

        public event Action OnChanged;

        public float FontScale { get { return _data.FontScale; } }
        public float MusicVolume { get { return _data.MusicVolume; } }
        public float SfxVolume { get { return _data.SfxVolume; } }
        public bool MusicOn { get { return _data.MusicOn; } }
        public bool MusicRepeat { get { return _data.MusicRepeat; } }
        public string MusicTrack { get { return _data.MusicTrack == null ? "" : _data.MusicTrack; } }

        public string Language
        {
            get { return string.IsNullOrEmpty(_data.Language) ? Texts.ResolveSystemLanguage() : _data.Language; }
        }

        public SettingsManager()
        {
            _data = SaveSystem.Load<SettingsData>(SaveKey);
            Apply();
        }

        public void SetFontScale(float scale)
        {
            if (scale < MinFontScale) scale = MinFontScale;
            if (scale > MaxFontScale) scale = MaxFontScale;
            _data.FontScale = scale;
            Commit();
        }

        public void SetLanguage(string code)
        {
            _data.Language = code;
            Commit();
        }

        public void SetMusicVolume(float volume)
        {
            _data.MusicVolume = Mathf.Clamp01(volume);
            Commit();
        }

        public void SetSfxVolume(float volume)
        {
            _data.SfxVolume = Mathf.Clamp01(volume);
            Commit();
        }

        public void SetMusicOn(bool on)
        {
            _data.MusicOn = on;
            Commit();
        }

        public void SetMusicRepeat(bool repeat)
        {
            _data.MusicRepeat = repeat;
            Commit();
        }

        public void SetMusicTrack(string track)
        {
            _data.MusicTrack = track == null ? "" : track;
            Commit();
        }

        private void Apply()
        {
            AudioListener.volume = 1f;   // volumes are per kind now (music / effects)
            Texts.SetLanguage(Language);
        }

        private void Commit()
        {
            SaveSystem.Save(SaveKey, _data);
            Apply();
            if (OnChanged != null) OnChanged();
        }
    }
}
