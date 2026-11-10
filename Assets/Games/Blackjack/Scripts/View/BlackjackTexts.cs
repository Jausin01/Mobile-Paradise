using MobileParadise.Core.Localization;
using UnityEngine;

namespace MobileParadise.Games.Blackjack.View
{
    /// <summary>
    /// All texts of the Blackjack game, Portuguese and English. Registered once at startup.
    /// </summary>
    public static class BlackjackTexts
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            Texts.Set("game.blackjack", "Blackjack", "Blackjack");
            Texts.Set("bj.ready", "Vamos jogar?", "Ready to play?");
            Texts.Set("bj.new_game", "Novo jogo", "New game");
            Texts.Set("bj.back", "Voltar", "Back");
            Texts.Set("bj.chips", "Fichas", "Chips");
            Texts.Set("bj.bet", "Aposta", "Bet");
            Texts.Set("bj.deal", "Distribuir", "Deal");
            Texts.Set("bj.hit", "Pedir", "Hit");
            Texts.Set("bj.stand", "Parar", "Stand");
            Texts.Set("bj.double", "Dobrar", "Double");
            Texts.Set("bj.dealer", "Banca", "Dealer");
            Texts.Set("bj.you", "Você", "You");
            Texts.Set("bj.place_bet", "Faça sua aposta", "Place your bet");
            Texts.Set("bj.your_turn", "Sua vez", "Your turn");
            Texts.Set("bj.blackjack", "Blackjack!", "Blackjack!");
            Texts.Set("bj.win", "Você ganhou!", "You win!");
            Texts.Set("bj.dealer_bust", "A banca estourou!", "Dealer busts!");
            Texts.Set("bj.push", "Empate", "Push");
            Texts.Set("bj.lose", "Você perdeu", "You lose");
            Texts.Set("bj.bust", "Você estourou", "Bust");
            Texts.Set("bj.broke", "Acabaram as fichas", "Out of chips");
            Texts.Set("bj.restart", "Recomeçar", "Start over");
        }
    }
}
