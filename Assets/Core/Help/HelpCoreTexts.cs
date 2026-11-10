using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Core.Help
{
    /// <summary>
    /// The texts of the help window itself (the games' explanations are in the Games folder).
    /// </summary>
    public static class HelpCoreTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("help.title", "Como jogar", "How to play");
            Texts.Set("help.list", "Lista de jogos", "Game list");
        }
    }
}
