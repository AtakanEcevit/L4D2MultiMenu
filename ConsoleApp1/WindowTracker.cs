using Swed32;
using System.Numerics;

namespace L4D2MultiMenu
{
    internal class WindowTracker
    {
        private readonly Swed swed;

        public WindowTracker(Swed swed)
        {
            this.swed = swed;
        }

        public void Update(GameState state)
        {
            RECT window;
            NativeMethods.GetWindowRect(swed.GetProcess().MainWindowHandle, out window);

            state.WindowLocation = new Vector2(window.Left, window.Top);
            state.WindowSize = new Vector2(window.Right - window.Left, window.Bottom - window.Top);
            state.LineOrigin = new Vector2(state.WindowLocation.X + state.WindowSize.X / 2, window.Bottom);
            state.WindowCenter = new Vector2(state.LineOrigin.X, window.Bottom - state.WindowSize.Y / 2);
        }
    }
}
