namespace MobileParadise.Games.WordSearch.Logic.Categories
{
    public static class NatureCategory
    {
        public static WordCategory Create()
        {
            return new WordCategory("nature", "Natureza", "Nature",
                new string[]
                {
                    "sol", "lua", "estrela", "nuvem", "chuva", "vento", "mar", "rio", "lago", "montanha",
                    "floresta", "árvore", "flor", "folha", "pedra", "terra", "fogo", "gelo", "neve", "praia",
                    "ilha", "campo", "grama", "céu"
                },
                new string[]
                {
                    "sun", "moon", "star", "cloud", "rain", "wind", "sea", "river", "lake", "mountain",
                    "forest", "tree", "flower", "leaf", "stone", "earth", "fire", "ice", "snow", "beach",
                    "island", "field", "grass", "sky"
                });
        }
    }
}
