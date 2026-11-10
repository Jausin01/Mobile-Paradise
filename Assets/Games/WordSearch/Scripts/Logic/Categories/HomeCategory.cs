namespace MobileParadise.Games.WordSearch.Logic.Categories
{
    public static class HomeCategory
    {
        public static WordCategory Create()
        {
            return new WordCategory("home", "Casa", "Home",
                new string[]
                {
                    "casa", "porta", "janela", "cama", "mesa", "cadeira", "sofá", "cozinha", "banheiro", "quarto",
                    "sala", "telhado", "parede", "chave", "espelho", "tapete", "lâmpada", "relógio", "armário", "fogão",
                    "geladeira", "cortina", "jardim", "escada"
                },
                new string[]
                {
                    "house", "door", "window", "bed", "table", "chair", "sofa", "kitchen", "bathroom", "bedroom",
                    "roof", "wall", "key", "mirror", "carpet", "lamp", "clock", "closet", "stove", "fridge",
                    "curtain", "garden", "stairs", "floor"
                });
        }
    }
}
