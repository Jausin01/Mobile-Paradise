namespace MobileParadise.Core.UI
{
    /// <summary>
    /// True while a window of the overlay (settings, music, help) is open on top of a game.
    /// Games that run in real time (Snake, Breakout) stop while it is true.
    /// </summary>
    public static class UiPause
    {
        public static bool IsOpen;
    }
}
