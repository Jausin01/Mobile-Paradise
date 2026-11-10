namespace MobileParadise.Games.WordSearch.Logic.Categories
{
    public static class AnimalsCategory
    {
        public static WordCategory Create()
        {
            return new WordCategory("animals", "Animais", "Animals",
                new string[]
                {
                    "gato", "cachorro", "cavalo", "vaca", "porco", "ovelha", "galinha", "pato", "peixe", "leão",
                    "tigre", "urso", "zebra", "girafa", "macaco", "coelho", "elefante", "cobra", "sapo", "tartaruga",
                    "golfinho", "baleia", "águia", "papagaio"
                },
                new string[]
                {
                    "cat", "dog", "horse", "cow", "pig", "sheep", "chicken", "duck", "fish", "lion",
                    "tiger", "bear", "zebra", "giraffe", "monkey", "rabbit", "elephant", "snake", "frog", "turtle",
                    "dolphin", "whale", "eagle", "parrot"
                });
        }
    }
}
