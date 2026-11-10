using System;
using System.Collections.Generic;
using MobileParadise.Core.Scenes;
using MobileParadise.Core.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MobileParadise.Core.Audio
{
    /// <summary>
    /// Plays sound effects and background music. Sounds are plain files dropped in
    /// Assets/Core/Resources/Audio/Sfx and Assets/Core/Resources/Audio/Music; a file that does not
    /// exist is simply skipped, so the game works with or without sounds.
    ///
    /// Music: the main menu is silent. In "automatic" mode every game scene starts with "music_menu"
    /// (its own menu) and the game calls PlayMusic("music_game") when the match starts. The player can
    /// instead pick one track (Next button) that plays everywhere, turn the music off, pause it,
    /// restart it and choose if it repeats (loops) or moves on to the next track when it ends.
    /// Music and effects have their own volumes (see SettingsManager).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private const string SfxFolder = "Audio/Sfx/";
        private const string MusicFolder = "Audio/Music/";
        private const float ResumeGrace = 0.5f;   // seconds after the app returns before "track ended" is trusted

        private static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            { "music_menu", "Phantom Protocol" },
            { "music_game", "Be My Rose" },
            { "check_it_out", "Check It Out" },
            { "chilled_pressure", "Chilled Pressure" },
            { "new_chapter", "New Chapter" },
            { "living_in_love", "Living In Love" }
        };

        private static AudioManager _instance;

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private AudioClip[] _tracks = new AudioClip[0];
        private AudioSource _sfx;
        private AudioSource _music;
        private string _context = "";   // what the current scene asks for
        private string _playing = "";   // file name of the track that is loaded ("" = nothing)
        private bool _paused;
        private bool _appPaused;
        private float _resumeTime;

        /// <summary>Raised when the track, or its paused state, changes.</summary>
        public static event Action MusicChanged;

        public static string PlayingTrack
        {
            get { return _instance == null ? "" : _instance._playing; }
        }

        public static bool IsPaused
        {
            get { return _instance != null && _instance._paused; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            GameObject go = new GameObject("AudioManager");
            _instance = go.AddComponent<AudioManager>();
            DontDestroyOnLoad(go);
        }

        // ---- Effects -------------------------------------------------------------

        public static void PlaySfx(string name)
        {
            PlaySfx(name, 1f);
        }

        /// <summary>
        /// Plays a sound effect; volume (0..1) is relative, on top of the player's effects volume.
        /// </summary>
        public static void PlaySfx(string name, float volume)
        {
            if (_instance == null) return;

            AudioClip clip = _instance.GetClip(SfxFolder + name);
            if (clip != null)
            {
                _instance._sfx.PlayOneShot(clip, volume);
            }
        }

        // ---- Music (called by games) ---------------------------------------------

        public static void PlayMusic(string name)
        {
            if (_instance == null) return;

            _instance._context = name;
            _instance.Refresh();
        }

        // ---- Music (called by the music window) ----------------------------------

        public static void TogglePause()
        {
            if (_instance != null) _instance.DoTogglePause();
        }

        public static void Restart()
        {
            if (_instance != null) _instance.DoRestart();
        }

        public static void Next()
        {
            if (_instance != null) _instance.DoNext();
        }

        /// <summary>
        /// Name shown to the player: the known title, or the file name made readable.
        /// </summary>
        public static string GetDisplayName(string track)
        {
            if (string.IsNullOrEmpty(track)) return "";

            string name;
            if (DisplayNames.TryGetValue(track, out name)) return name;

            name = track.StartsWith("music_") ? track.Substring(6) : track;
            return name.Replace('_', ' ');
        }

        // ---- Unity ---------------------------------------------------------------

        private void Awake()
        {
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;

            _music = gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;

            _tracks = Resources.LoadAll<AudioClip>(MusicFolder.TrimEnd('/'));
            Array.Sort(_tracks, (a, b) => string.CompareOrdinal(a.name, b.name));

            _context = MusicForScene(SceneManager.GetActiveScene().name);
            GameContext.Settings.OnChanged += Refresh;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Refresh();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (GameContext.Settings != null) GameContext.Settings.OnChanged -= Refresh;
        }

        private void OnApplicationPause(bool paused)
        {
            _appPaused = paused;
            if (!paused) _resumeTime = Time.unscaledTime;
        }

        private void Update()
        {
            // Only reached when repeat is off: the track ended, go to the next one.
            if (_playing == "" || _paused || _appPaused || _music.loop || _music.isPlaying) return;
            if (Time.unscaledTime - _resumeTime < ResumeGrace) return;

            AdvanceAfterEnd();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _context = MusicForScene(scene.name);
            Refresh();
        }

        private static string MusicForScene(string sceneName)
        {
            return sceneName == SceneLoader.MenuScene ? "" : "music_menu";
        }

        // ---- Music logic ---------------------------------------------------------

        /// <summary>
        /// Applies volumes and loop, and starts / stops / changes the track if the wanted one differs.
        /// </summary>
        private void Refresh()
        {
            SettingsManager settings = GameContext.Settings;
            _sfx.volume = settings.SfxVolume;
            _music.volume = settings.MusicVolume;
            _music.loop = settings.MusicRepeat;
            if (!settings.MusicOn) _paused = false;

            Play(WantedTrack(settings));
        }

        private string WantedTrack(SettingsManager settings)
        {
            if (!settings.MusicOn || _context == "") return "";

            string chosen = settings.MusicTrack;
            if (chosen != "" && GetClip(MusicFolder + chosen) != null) return chosen;

            return _context;   // automatic
        }

        private void Play(string name)
        {
            if (name == _playing) return;

            if (name == "")
            {
                _playing = "";
                _music.Stop();
                RaiseChanged();
                return;
            }

            AudioClip clip = GetClip(MusicFolder + name);
            if (clip == null) return;   // no file yet: keep what is playing

            _playing = name;
            _music.clip = clip;
            _music.Play();
            if (_paused) _music.Pause();
            RaiseChanged();
        }

        private void DoTogglePause()
        {
            if (_playing == "") return;

            _paused = !_paused;
            if (_paused) _music.Pause(); else _music.UnPause();
            RaiseChanged();
        }

        private void DoRestart()
        {
            if (_playing == "") return;

            _paused = false;
            _music.Stop();
            _music.Play();
            RaiseChanged();
        }

        /// <summary>
        /// Cycle: automatic, then every track in name order, then automatic again.
        /// </summary>
        private void DoNext()
        {
            SettingsManager settings = GameContext.Settings;
            int index = IndexOfTrack(settings.MusicTrack) + 1;   // -1 (automatic) becomes 0
            string next = index >= _tracks.Length ? "" : _tracks[index].name;

            string before = _playing;
            _paused = false;
            settings.SetMusicTrack(next);   // Refresh() runs from the change event

            if (_playing == before) DoRestart();   // same track wanted again: start it over so the press is felt
        }

        private void AdvanceAfterEnd()
        {
            if (_tracks.Length == 0) return;

            int index = (IndexOfTrack(_playing) + 1) % _tracks.Length;
            string next = _tracks[index].name;

            SettingsManager settings = GameContext.Settings;
            if (settings.MusicTrack != "")
            {
                settings.SetMusicTrack(next);
                if (_playing == next) DoRestart();   // only one track
            }
            else
            {
                if (_playing == next) DoRestart(); else Play(next);   // automatic mode: just move on
            }
        }

        private int IndexOfTrack(string name)
        {
            for (int i = 0; i < _tracks.Length; i++)
            {
                if (_tracks[i].name == name) return i;
            }
            return -1;
        }

        private void RaiseChanged()
        {
            if (MusicChanged != null) MusicChanged();
        }

        private AudioClip GetClip(string path)
        {
            AudioClip clip;
            if (!_clips.TryGetValue(path, out clip))
            {
                clip = Resources.Load<AudioClip>(path);   // null if the file is missing
                _clips[path] = clip;
            }
            return clip;
        }
    }
}
