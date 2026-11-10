namespace MobileParadise.Games.WordSearch.Logic.Categories
{
    public static class CountriesCategory
    {
        public static WordCategory Create()
        {
            return new WordCategory("countries", "Países", "Countries",
                new string[]
                {
                    "Brasil", "Portugal", "Argentina", "Chile", "Peru", "Japão", "China", "Índia", "Itália", "França",
                    "Espanha", "Alemanha", "Rússia", "Canadá", "México", "Egito", "Cuba", "Angola", "Uruguai", "Colômbia",
                    "Grécia", "Suíça", "Turquia", "Holanda"
                },
                new string[]
                {
                    "Brazil", "Portugal", "Argentina", "Chile", "Peru", "Japan", "China", "India", "Italy", "France",
                    "Spain", "Germany", "Russia", "Canada", "Mexico", "Egypt", "Cuba", "Angola", "Uruguay", "Colombia",
                    "Greece", "Sweden", "Turkey", "Norway"
                });
        }
    }
}
