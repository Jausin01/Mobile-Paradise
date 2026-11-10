using UnityEngine;

namespace MobileParadise.Core.Help.Games
{
    public static class BlackjackHelp
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            HelpRegistry.Register("Blackjack_Game", "game.blackjack",
                "Chegue o mais perto possível de 21 pontos, sem passar de 21.\n\n" +
                "As cartas de 2 a 10 valem o próprio número. Valete, Dama e Rei valem 10. O Ás vale 11 ou 1, do jeito que for melhor para você.\n\n" +
                "Escolha sua aposta com as fichas e toque em Distribuir. Depois:\n" +
                "Pedir: recebe mais uma carta.\n" +
                "Parar: fica com as cartas que tem.\n" +
                "Dobrar: dobra a aposta, recebe só mais uma carta e para.\n\n" +
                "A banca compra cartas até chegar a 17 ou mais. Quem chegar mais perto de 21 ganha. Se você passar de 21, perde na hora. Blackjack (21 com as duas primeiras cartas) paga 3 para 2. No empate você recebe a aposta de volta.\n\n" +
                "Se as fichas acabarem, você pode recomeçar.",
                "Get as close to 21 points as you can without going over.\n\n" +
                "Cards 2 to 10 are worth their number. Jack, Queen and King are worth 10. The Ace is worth 11 or 1, whichever is better for you.\n\n" +
                "Choose your bet with the chips and tap Deal. Then:\n" +
                "Hit: take one more card.\n" +
                "Stand: keep the cards you have.\n" +
                "Double: double your bet, take just one more card and stand.\n\n" +
                "The dealer draws until reaching 17 or more. Whoever is closer to 21 wins. If you go over 21, you lose right away. Blackjack (21 with the first two cards) pays 3 to 2. On a tie you get your bet back.\n\n" +
                "If you run out of chips, you can start over.");
        }
    }
}
