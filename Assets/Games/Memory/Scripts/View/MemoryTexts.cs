using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Memory.View
{
    /// <summary>
    /// All texts of the Memory game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class MemoryTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.memory", "Memória", "Memory");
            Texts.Set("memory.choose_level", "Escolha o nível", "Choose the level");
            Texts.Set("memory.easy", "Fácil", "Easy");
            Texts.Set("memory.medium", "Médio", "Medium");
            Texts.Set("memory.hard", "Difícil", "Hard");
            Texts.Set("memory.theme", "Figuras", "Pictures");
            Texts.Set("memory.theme.fruits", "Frutas", "Fruits");
            Texts.Set("memory.theme.animals", "Animais", "Animals");
            Texts.Set("memory.moves", "Jogadas", "Moves");
            Texts.Set("memory.new_game", "Novo jogo", "New game");
            Texts.Set("memory.back", "Voltar", "Back");
            Texts.Set("memory.win", "Parabéns!", "Well done!");
            Texts.Set("memory.play_again", "Jogar de novo", "Play again");
        }
    }
}
