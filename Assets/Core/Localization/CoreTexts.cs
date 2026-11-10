using UnityEngine;

namespace MobileParadise.Core.Localization
{
    /// <summary>
    /// Texts shared by every game (settings window, back button).
    /// Registered automatically at startup, before the first scene loads.
    /// </summary>
    public static class CoreTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("settings.title", "Configurações", "Settings");
            Texts.Set("settings.language", "Idioma", "Language");
            Texts.Set("settings.font", "Tamanho da letra", "Text size");
            Texts.Set("settings.close", "Fechar", "Close");
            Texts.Set("common.continue", "Continuar", "Continue");
            Texts.Set("music.title", "Música", "Music");
            Texts.Set("music.on", "Música: ligada", "Music: on");
            Texts.Set("music.off", "Música: desligada", "Music: off");
            Texts.Set("music.auto", "Automática", "Automatic");
            Texts.Set("music.silent", "Sem música", "No music");
            Texts.Set("music.repeat_on", "Repetir: ligado", "Repeat: on");
            Texts.Set("music.repeat_off", "Repetir: desligado", "Repeat: off");
            Texts.Set("music.volume", "Volume da música", "Music volume");
            Texts.Set("music.sfx", "Volume dos efeitos", "Effects volume");
            Texts.Set("common.back_to_menu", "Voltar ao menu", "Back to menu");
        }
    }
}
