using MobileParadise.Core.Audio;
using MobileParadise.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileParadise.Core.UI
{
    /// <summary>
    /// The music-note button and the music window: music on/off, current track, restart / pause /
    /// next / repeat, and the two volume sliders (music and effects).
    /// Lives on the SettingsOverlay prefab, next to the gear.
    /// </summary>
    public class MusicPanel : MonoBehaviour
    {
        private static readonly Color SelectedColor = new Color(0.62f, 0.82f, 0.50f);
        private static readonly Color UnselectedColor = new Color(0.93f, 0.80f, 0.50f);
        private const float BeepInterval = 0.12f;

        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _closeText;

        [SerializeField] private Button _musicToggleButton;
        [SerializeField] private TMP_Text _musicToggleText;
        [SerializeField] private TMP_Text _trackText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Image _pauseIcon;
        [SerializeField] private Sprite _playSprite;
        [SerializeField] private Sprite _pauseSprite;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _repeatButton;
        [SerializeField] private TMP_Text _repeatText;

        [SerializeField] private TMP_Text _musicVolumeText;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private TMP_Text _sfxVolumeText;
        [SerializeField] private Slider _sfxSlider;

        private AudioSource _audioSource;
        private AudioClip _beep;
        private float _lastBeepTime;

        private void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _beep = CreateBeep();

            LocalizedText.Bind(_titleText, "music.title");
            LocalizedText.Bind(_musicVolumeText, "music.volume");
            LocalizedText.Bind(_sfxVolumeText, "music.sfx");
            LocalizedText.Bind(_closeText, "settings.close");

            _openButton.onClick.AddListener(Open);
            _closeButton.onClick.AddListener(Close);
            _musicToggleButton.onClick.AddListener(() => GameContext.Settings.SetMusicOn(!GameContext.Settings.MusicOn));
            _restartButton.onClick.AddListener(AudioManager.Restart);
            _pauseButton.onClick.AddListener(AudioManager.TogglePause);
            _nextButton.onClick.AddListener(AudioManager.Next);
            _repeatButton.onClick.AddListener(() => GameContext.Settings.SetMusicRepeat(!GameContext.Settings.MusicRepeat));
            _musicSlider.onValueChanged.AddListener(volume => GameContext.Settings.SetMusicVolume(volume));
            _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

            GameContext.Settings.OnChanged += Refresh;
            AudioManager.MusicChanged += Refresh;
            Texts.LanguageChanged += Refresh;

            _panel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (GameContext.Settings != null) GameContext.Settings.OnChanged -= Refresh;
            AudioManager.MusicChanged -= Refresh;
            Texts.LanguageChanged -= Refresh;
        }

        private void Open()
        {
            _musicSlider.SetValueWithoutNotify(GameContext.Settings.MusicVolume);
            _sfxSlider.SetValueWithoutNotify(GameContext.Settings.SfxVolume);
            UiPause.IsOpen = true;
            _panel.SetActive(true);
            Refresh();   // after SetActive: Refresh does nothing while the window is hidden
        }

        private void Close()
        {
            UiPause.IsOpen = false;
            _panel.SetActive(false);
        }

        private void OnSfxVolumeChanged(float volume)
        {
            GameContext.Settings.SetSfxVolume(volume);

            // A short beep so the player hears the new effects volume.
            if (Time.unscaledTime - _lastBeepTime >= BeepInterval)
            {
                _lastBeepTime = Time.unscaledTime;
                _audioSource.volume = volume;
                _audioSource.PlayOneShot(_beep);
            }
        }

        private void Refresh()
        {
            if (!_panel.activeInHierarchy) return;

            bool musicOn = GameContext.Settings.MusicOn;
            bool repeat = GameContext.Settings.MusicRepeat;
            bool paused = AudioManager.IsPaused;

            _musicToggleText.text = Texts.Get(musicOn ? "music.on" : "music.off");
            _musicToggleButton.targetGraphic.color = musicOn ? SelectedColor : UnselectedColor;

            _trackText.text = TrackLabel(musicOn);

            _pauseIcon.sprite = paused ? _playSprite : _pauseSprite;
            _repeatButton.targetGraphic.color = repeat ? SelectedColor : UnselectedColor;
            _repeatText.text = Texts.Get(repeat ? "music.repeat_on" : "music.repeat_off");

            // With the music off there is nothing to control.
            _restartButton.interactable = musicOn;
            _pauseButton.interactable = musicOn;
            _nextButton.interactable = musicOn;
        }

        private static string TrackLabel(bool musicOn)
        {
            if (!musicOn) return Texts.Get("music.silent");

            string playing = AudioManager.GetDisplayName(AudioManager.PlayingTrack);
            bool automatic = GameContext.Settings.MusicTrack == "";
            if (automatic)
            {
                string auto = Texts.Get("music.auto");
                return playing == "" ? auto : auto + ": " + playing;
            }
            return playing;
        }

        private static AudioClip CreateBeep()
        {
            const int sampleRate = 44100;
            int samples = sampleRate / 12;
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)sampleRate;
                float fade = 1f - i / (float)samples;
                data[i] = Mathf.Sin(2f * Mathf.PI * 660f * time) * 0.25f * fade;
            }

            AudioClip clip = AudioClip.Create("Beep", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
