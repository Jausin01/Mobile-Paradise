using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// All on-screen strings of the chess game (Portuguese first, English second).
    /// Registered automatically at startup, before the first scene loads.
    /// </summary>
    public static class ChessTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.chess", "Xadrez", "Chess");
            Texts.Set("chess.choose_level", "Escolha a dificuldade", "Choose the difficulty");
            Texts.Set("chess.easy", "Fácil", "Easy");
            Texts.Set("chess.medium", "Médio", "Medium");
            Texts.Set("chess.hard", "Difícil", "Hard");
            Texts.Set("chess.your_turn", "Sua vez", "Your turn");
            Texts.Set("chess.thinking", "Pensando...", "Thinking...");
            Texts.Set("chess.check", "Xeque! Sua vez", "Check! Your turn");
            Texts.Set("chess.win", "Xeque-mate! Você ganhou!", "Checkmate! You win!");
            Texts.Set("chess.lose", "Xeque-mate. Você perdeu.", "Checkmate. You lost.");
            Texts.Set("chess.stalemate", "Empate: o rei não tem jogadas", "Draw: stalemate");
            Texts.Set("chess.draw", "Empate", "Draw");
            Texts.Set("chess.undo", "Desfazer", "Undo");
            Texts.Set("chess.new_game", "Novo jogo", "New game");
            Texts.Set("chess.back", "Voltar", "Back");
            Texts.Set("chess.pieces", "Peças", "Pieces");
            Texts.Set("chess.board", "Tabuleiro", "Board");
            Texts.Set("chess.set.wood", "Madeira", "Wood");
            Texts.Set("chess.set.marble", "Mármore", "Marble");
            Texts.Set("chess.play_as", "Jogar de", "Play as");
            Texts.Set("chess.color.white", "Brancas", "White");
            Texts.Set("chess.color.black", "Pretas", "Black");
            Texts.Set("chess.draw_repetition", "Empate: posição repetida", "Draw: repeated position");
            Texts.Set("chess.promote", "Escolha a peça", "Choose a piece");
        }
    }
}
