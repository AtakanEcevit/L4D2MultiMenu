using ClickableTransparentOverlay;
using ImGuiNET;
using L4D2MultiMenu;
using Swed32;
using System.Text;

namespace L4D2Menu
{
    class Program : Overlay
    {
        private const int SW_HIDE = 0;

        private readonly Settings settings = new Settings();
        private readonly GameState state = new GameState();
        private readonly Offsets offsets = new Offsets();
        private readonly Swed swed = new Swed("left4dead2");

        private readonly EntityScanner entityScanner;
        private readonly AutoShoveService autoShoveService;
        private readonly WindowTracker windowTracker;
        private readonly EspRenderer espRenderer = new EspRenderer();

        public Program()
        {
            entityScanner = new EntityScanner(swed, offsets, Encoding.ASCII);
            autoShoveService = new AutoShoveService(swed, offsets);
            windowTracker = new WindowTracker(swed);
        }

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start().Wait();

            IntPtr consoleWnd = NativeMethods.GetConsoleWindow();
            NativeMethods.ShowWindow(consoleWnd, SW_HIDE);

            Thread mainLogicThread = new Thread(program.MainLogic)
            {
                IsBackground = true
            };
            mainLogicThread.Start();
        }

        protected override void Render()
        {
            espRenderer.RenderMenu(settings);
            espRenderer.BeginOverlay(state);
            espRenderer.RenderEsp(state, settings);
            ImGui.End();
        }

        private void MainLogic()
        {
            const int windowUpdateIntervalMs = 100;
            DateTime lastWindowUpdate = DateTime.MinValue;

            entityScanner.Initialize();
            autoShoveService.Initialize();

            while (true)
            {
                if ((DateTime.UtcNow - lastWindowUpdate).TotalMilliseconds >= windowUpdateIntervalMs)
                {
                    windowTracker.Update(state);
                    lastWindowUpdate = DateTime.UtcNow;
                }

                entityScanner.ReloadEntities(state);

                if (settings.EnableAutoShove)
                {
                    autoShoveService.TryAutoShove(state);
                }

                Thread.Sleep(5);
            }
        }
    }
}
