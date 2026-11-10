using UnityEngine;

namespace MobileParadise.Games.Chess.View
{
    /// <summary>
    /// A pair of square colors. Add a line to All to add a new board style.
    /// </summary>
    public class BoardTheme
    {
        public readonly Color Light;
        public readonly Color Dark;

        public BoardTheme(Color light, Color dark)
        {
            Light = light;
            Dark = dark;
        }

        public static readonly BoardTheme[] All =
        {
            new BoardTheme(new Color(0.94f, 0.85f, 0.71f), new Color(0.71f, 0.53f, 0.39f)),   // classic brown
            new BoardTheme(new Color(0.93f, 0.93f, 0.82f), new Color(0.46f, 0.59f, 0.34f)),   // green
            new BoardTheme(new Color(0.87f, 0.89f, 0.91f), new Color(0.42f, 0.55f, 0.69f)),   // blue
            new BoardTheme(new Color(0.91f, 0.86f, 0.93f), new Color(0.55f, 0.42f, 0.65f))    // purple
        };
    }
}
