using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Sudoku.View
{
    /// <summary>
    /// All texts of the Sudoku game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class SudokuTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.sudoku", "Sudoku", "Sudoku");
            Texts.Set("sudoku.choose_level", "Escolha o nível", "Choose the level");
            Texts.Set("sudoku.easy", "Fácil", "Easy");
            Texts.Set("sudoku.medium", "Médio", "Medium");
            Texts.Set("sudoku.hard", "Difícil", "Hard");
            Texts.Set("sudoku.erase", "Apagar", "Erase");
            Texts.Set("sudoku.hint", "Dica", "Hint");
            Texts.Set("sudoku.new_game", "Novo jogo", "New game");
            Texts.Set("sudoku.back", "Voltar", "Back");
            Texts.Set("sudoku.win", "Parabéns!", "Well done!");
            Texts.Set("sudoku.play_again", "Jogar de novo", "Play again");
        }
    }
}
