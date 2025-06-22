using ClickableTransparentOverlay;
using System.Runtime.InteropServices;
using System.Text;
using Vortice.Mathematics;
using Vulkan.Win32;
using ImGuiNET;
using Swed32;
using System.Numerics;
using Veldrid;
using L4D2MultiMenu;
using System.Threading;

namespace L4D2Menu
{
    class Program : Overlay
    {
        #region DLL Imports
        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion

        #region Constants
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SURVIVOR_TEAM = 2;
        private const int INFECTED_TEAM = 3;
        #endregion

        #region Fields
        private Encoding encoding = Encoding.ASCII;
        private ImDrawListPtr drawList;
        private Offsets offsets = new Offsets();
        private Swed swed = new Swed("left4dead2");

        private IntPtr client;
        private IntPtr engine;
        private IntPtr testViewMatrixAddress = IntPtr.Zero;

        private Entity localPlayer = new Entity();
        private List<Entity> commonInfected = new List<Entity>();
        private List<Entity> specialInfected = new List<Entity>();
        private List<Entity> survivors = new List<Entity>();

        private Vector3 offsetVector = new Vector3(0, 0, 15);
        private Vector3 offsetVectorCommon = new Vector3(0, 0, 3);

        private Vector2 windowSize = new Vector2(1920, 1080);
        private Vector2 lineOrigin = new Vector2(1920 / 2, 1080);
        private Vector2 windowCenter = new Vector2(1920 / 2, 1080 / 2);
        private Vector2 windowLocation = new Vector2(0, 0);

        private bool enableESP = true;
        private bool enableAutoShove = false;
        private bool enableHealthText = false;

        private bool survivorLineEnable = true;
        private bool survivorBoxEnable = true;
        private bool survivorDotEnable = true;

        private bool specialInfectedLineEnable = true;
        private bool specialInfectedBoxEnable = true;
        private bool specialInfectedDotEnable = true;

        private bool commonInfectedLineEnable = true;
        private bool commonInfectedBoxEnable = true;
        private bool commonInfectedDotEnable = true;

        private Vector4 survivorColor = new Vector4(0, 0, 1, 1);
        private Vector4 specialInfectedColor = new Vector4(1, 0, 1, 1);
        private Vector4 commonInfectedColor = new Vector4(1, 0, 0, 1);

        #endregion

        #region Main Program / Initialization
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start().Wait();

            IntPtr consoleWnd = GetConsoleWindow();
            ShowWindow(consoleWnd, SW_HIDE);

            Thread mainLogicThread = new Thread(program.MainLogic)
            {
                IsBackground = true
            };
            mainLogicThread.Start();
        }
        #endregion

        #region Overlay and Rendering
        protected override void Render()
        {
            DrawMenu();
            DrawOverlay();
            ESP();
            ImGui.End();
        }

        private void DrawOverlay()
        {
            ImGui.SetNextWindowSize(windowSize);
            ImGui.SetNextWindowPos(windowLocation);
            ImGui.Begin("overlay",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoInputs |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse
            );
        }

        private void DrawMenu()
        {
            ImGui.Begin("L4D2 Menu by attackN");

            if (ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    ImGui.Checkbox("ESP", ref enableESP);
                    if (enableESP)
                    {
                        ImGui.SameLine();
                        ImGui.Checkbox("Show health", ref enableHealthText);
                    }
                    ImGui.Checkbox("AutoShove", ref enableAutoShove);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Color Settings"))
                {
                    ImGui.ColorPicker4("Survivor Color", ref survivorColor);
                    ImGui.Checkbox("Survivor Line", ref survivorLineEnable);
                    ImGui.Checkbox("Survivor Box", ref survivorBoxEnable);
                    ImGui.Checkbox("Survivor Dot", ref survivorDotEnable);

                    ImGui.ColorPicker4("Special Infected Color", ref specialInfectedColor);
                    ImGui.Checkbox("Special Infected Line", ref specialInfectedLineEnable);
                    ImGui.Checkbox("Special Infected Box", ref specialInfectedBoxEnable);
                    ImGui.Checkbox("Special Infected Dot", ref specialInfectedDotEnable);

                    ImGui.ColorPicker4("Common Infected Color", ref commonInfectedColor);
                    ImGui.Checkbox("Common Infected Line", ref commonInfectedLineEnable);
                    ImGui.Checkbox("Common Infected Box", ref commonInfectedBoxEnable);
                    ImGui.Checkbox("Common Infected Dot", ref commonInfectedDotEnable);

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
            ImGui.End();
        }
        #endregion



        #region Game Logic & Entities
        private void MainLogic()
        {
            RECT window;
            GetWindowRect(swed.GetProcess().MainWindowHandle, out window);

            windowLocation = new Vector2(window.Left, window.Top);
            windowSize = new Vector2(window.Right - window.Left, window.Bottom - window.Top);
            lineOrigin = new Vector2(windowLocation.X + windowSize.X / 2, window.Bottom);
            windowCenter = new Vector2(lineOrigin.X, window.Bottom - windowSize.Y / 2);

            client = swed.GetModuleBase("client.dll");
            engine = swed.GetModuleBase("engine.dll");

            while (true)
            {
                ReloadEntities();

                if (enableAutoShove)
                {
                    AutoShove();
                }

                Thread.Sleep(5);
            }
        }

        private void ReloadEntities()
        {
            commonInfected.Clear();
            specialInfected.Clear();
            survivors.Clear();

            localPlayer.address = swed.ReadPointer(client, offsets.localPlayer);
            UpdateEntity(localPlayer);

            UpdateEntities();

            commonInfected = commonInfected.OrderBy(x => x.magnitude).ToList();
            specialInfected = specialInfected.OrderBy(x => x.magnitude).ToList();
        }

        private void UpdateEntities()
        {
            for (int i = 0; i < 850; i++)
            {
                Entity entity = new Entity
                {
                    address = swed.ReadPointer(client, offsets.entityList + i * 0x10)
                };

                if (entity.address == IntPtr.Zero) continue;
                UpdateEntity(entity);

                if (entity.lifeState < 1) continue;

                if (entity.teamNum == SURVIVOR_TEAM && entity.health > 0)
                {
                    survivors.Add(entity);
                }
                else if (entity.teamNum == INFECTED_TEAM)
                {
                    if (entity.name.Contains("inf"))
                    {
                        commonInfected.Add(entity);
                    }
                    else
                    {
                        if (entity.health > 1)
                            specialInfected.Add(entity);
                    }
                }
            }
        }

        private void UpdateEntity(Entity entity)
        {
            entity.lifeState = swed.ReadInt(entity.address, offsets.lifeState);
            if (entity.lifeState < 1) return;

            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.viewOffset = swed.ReadVec(entity.address, offsets.viewOffset);
            entity.abs = entity.origin + entity.viewOffset;
            entity.health = swed.ReadInt(entity.address, offsets.health);
            entity.teamNum = swed.ReadInt(entity.address, offsets.teamNum);
            entity.jumpFlag = swed.ReadInt(entity.address, offsets.jumpFlag);
            entity.magnitude = CalculateMagnitude(entity.origin, localPlayer.origin);

            var currentViewMatrix = ReadMatrix();
            entity.originScreenPosition = World2Screen(currentViewMatrix, entity.origin, (int)windowSize.X, (int)windowSize.Y) + windowLocation;
            entity.absScreenPosition = World2Screen(currentViewMatrix, entity.abs, (int)windowSize.X, (int)windowSize.Y) + windowLocation;

            var entityStringPointer = swed.ReadPointer(entity.address, 0x10);
            entity.name = encoding.GetString(swed.ReadBytes(entityStringPointer, 10));
            if (entity.maxHealth == 0)
            {
                entity.maxHealth = entity.health;
                entity.infectedType = GetInfectedType(entity.maxHealth);
            }
        }
        #endregion

        #region Aimbot Logic
        private void AimAt(Vector3 angles)
        {
            // Directly writes angles (snap). We'll replace usage with 'HumanizedAimAt'
            IntPtr engineAnglesBase = swed.ReadPointer(engine, offsets.engineAngles);
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset, angles.Y);
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset + 0x4, angles.X);
        }

        // For a more human approach, we add these:

        // read your local angles from memory
        private Vector3 ReadLocalAngles()
        {
            IntPtr engineAnglesBase = swed.ReadPointer(engine, offsets.engineAngles);
            float pitch = swed.ReadFloat(engineAnglesBase, offsets.engineAnglesOffset + 0x4);
            float yaw = swed.ReadFloat(engineAnglesBase, offsets.engineAnglesOffset);
            return new Vector3(yaw, pitch, 0);
        }

        private void WriteLocalAngles(Vector3 angles)
        {
            IntPtr engineAnglesBase = swed.ReadPointer(engine, offsets.engineAngles);
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset, angles.X);
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset + 0x4, angles.Y);
        }

        // angle lerp
        private Vector3 LerpAngles(Vector3 from, Vector3 to, float t)
        {
            return new Vector3(
                from.X + t * (to.X - from.X),
                from.Y + t * (to.Y - from.Y),
                0
            );
        }
        private Vector3 AddRandomOffset(Vector3 angles, float maxOffset = 1.5f)
        {
            Random rng = new Random();
            float offsetX = (float)(rng.NextDouble() * maxOffset * 2 - maxOffset);
            float offsetY = (float)(rng.NextDouble() * maxOffset * 2 - maxOffset);

            angles.X += offsetX;
            angles.Y += offsetY;
            return angles;
        }



        #endregion

        #region Math & Matrix Operations
        private float CalculateMagnitude(Vector3 from, Vector3 destination)
        {
            return (float)Math.Sqrt(
                Math.Pow(destination.X - from.X, 2) +
                Math.Pow(destination.Y - from.Y, 2) +
                Math.Pow(destination.Z - from.Z, 2));
        }

        private Vector3 CalculateAngles(Vector3 from, Vector3 destination)
        {
            float deltaX = destination.X - from.X;
            float deltaY = destination.Y - from.Y;
            float deltaZ = destination.Z - from.Z;

            float yaw = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);
            float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float pitch = -(float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);

            return new Vector3(yaw, pitch, 0);
        }

        private Vector2 World2Screen(ViewMatrix matrix, Vector3 pos, int width, int height)
        {
            Vector2 screenCoordinates = new Vector2();

            float screenW = matrix.m41 * pos.X + matrix.m42 * pos.Y + matrix.m43 * pos.Z + matrix.m44;
            if (screenW < 0.001f) return new Vector2(-99, -99);

            float screenX = (matrix.m11 * pos.X) + (matrix.m12 * pos.Y) + (matrix.m13 * pos.Z) + matrix.m14;
            float screenY = (matrix.m21 * pos.X) + (matrix.m22 * pos.Y) + (matrix.m23 * pos.Z) + matrix.m24;
            float camX = width / 2f;
            float camY = height / 2f;

            screenCoordinates.X = camX + (camX * screenX / screenW);
            screenCoordinates.Y = camY - (camY * screenY / screenW);
            return screenCoordinates;
        }

        private ViewMatrix ReadMatrix()
        {
            var viewMatrix = new ViewMatrix();
            IntPtr matrixPointer = swed.ReadPointer(engine, offsets.viewMatrix);
            float[] matrix = swed.ReadMatrix(matrixPointer + offsets.viewMatrixOffset);

            if (testViewMatrixAddress != IntPtr.Zero)
            {
                matrix = swed.ReadMatrix(testViewMatrixAddress);
            }

            viewMatrix.m11 = matrix[0];
            viewMatrix.m12 = matrix[1];
            viewMatrix.m13 = matrix[2];
            viewMatrix.m14 = matrix[3];
            viewMatrix.m21 = matrix[4];
            viewMatrix.m22 = matrix[5];
            viewMatrix.m23 = matrix[6];
            viewMatrix.m24 = matrix[7];
            viewMatrix.m31 = matrix[8];
            viewMatrix.m32 = matrix[9];
            viewMatrix.m33 = matrix[10];
            viewMatrix.m34 = matrix[11];
            viewMatrix.m41 = matrix[12];
            viewMatrix.m42 = matrix[13];
            viewMatrix.m43 = matrix[14];
            viewMatrix.m44 = matrix[15];

            return viewMatrix;
        }
        #endregion

        #region ESP
        private void ESP()
        {
            drawList = ImGui.GetWindowDrawList();
            if (!enableESP) return;

            try
            {
                foreach (var entity in commonInfected.ToList())
                {
                    if (entity != null && IsPixelInsideScreen(entity.originScreenPosition))
                    {
                        DrawVisuals(entity, commonInfectedColor, commonInfectedLineEnable, commonInfectedBoxEnable, commonInfectedDotEnable);
                    }
                }
                foreach (var entity in specialInfected.ToList())
                {
                    if (entity != null && IsPixelInsideScreen(entity.originScreenPosition))
                    {
                        DrawVisuals(entity, specialInfectedColor, specialInfectedLineEnable, specialInfectedBoxEnable, specialInfectedDotEnable);
                    }
                }
                foreach (var entity in survivors.ToList())
                {
                    if (entity != null && IsPixelInsideScreen(entity.originScreenPosition))
                    {
                        DrawVisuals(entity, survivorColor, survivorLineEnable, survivorBoxEnable, survivorDotEnable);
                    }
                }
            }
            catch (Exception)
            {
                // Could log error here
            }
        }

        private bool IsPixelInsideScreen(Vector2 pixel)
        {
            return pixel.X > windowLocation.X && pixel.X < windowLocation.X + windowSize.X &&
                   pixel.Y > windowLocation.Y && pixel.Y < windowLocation.Y + windowSize.Y;
        }

        private void DrawHealthText(Entity entity)
        {
            if (entity.health < 10) return; // Skip if HP < 10

            string displayText = $"{entity.infectedType} [{entity.health} HP]";
            Vector2 textPos = new Vector2(entity.originScreenPosition.X, entity.originScreenPosition.Y - 15);

            drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), displayText);
        }

        private string GetInfectedType(int maxHealth)
        {
            switch (maxHealth)
            {
                case 4000: return "Tank";
                case 1000: return "Witch";
                case 600: return "Charger";
                case 325: return "Jockey";
                case 250: return "Hunter/Smoker"; // Both share 250 HP
                case 100: return "Spitter";
                case 50: return "Boomer/Common Infected";
                default: return "Unknown";
            }
        }

        private void DrawVisuals(Entity entity, Vector4 color, bool line, bool box, bool dot)
        {
            if (!IsPixelInsideScreen(entity.originScreenPosition)) return;

            if (line)
            {
                drawList.AddLine(lineOrigin, entity.originScreenPosition, ImGui.ColorConvertFloat4ToU32(color), 2);
            }
            if (box)
            {
                Vector2 width = new Vector2((entity.originScreenPosition.Y - entity.absScreenPosition.Y) / 2f, 0f);
                drawList.AddRect(entity.absScreenPosition - width, entity.originScreenPosition + width, ImGui.ColorConvertFloat4ToU32(color), 2f);
            }
            if (dot)
            {
                drawList.AddCircleFilled(entity.originScreenPosition, 5f, ImGui.ColorConvertFloat4ToU32(color));
            }
            if (enableHealthText) DrawHealthText(entity);
        }
        #endregion
        #region AutoShove – iyileştirilmiş

        #region AutoShove – patched

        private const int KEY_TOGGLE_AUTOSHOVE = 0x46;  // F
        private const float SHOVE_RANGE = 95f;    // oyun birimi
        private const float SHOVE_FOV = 35f;    // derece
        private const float SMOOTH_FACTOR = 0.30f;  // 0–1
        private static readonly Random rng = new Random();

        private DateTime nextShoveTime = DateTime.MinValue;

        private void AutoShove()
        {
            bool keyHeld = GetAsyncKeyState(KEY_TOGGLE_AUTOSHOVE) < 0;

            // Tuş bırakıldıysa shove düğmesini de bırak
            if (!keyHeld)
            {
                swed.WriteInt(client, offsets.forceShove, 4); // release
                return;
            }

            if (DateTime.Now < nextShoveTime) return;

            // lifeState > 1 canlı kabul ediliyor (senin kodunda böyle)
            Entity target = specialInfected
                .Where(e => e.health > 0 && e.lifeState > 1 && e.magnitude < SHOVE_RANGE)
                .OrderBy(e => e.magnitude)
                .FirstOrDefault();

            if (target == null) return;

            Vector3 desired = CalculateAngles(localPlayer.origin,
                                              target.origin - offsetVector);

            if (AngleDiffDeg(ReadLocalAngles(), desired) > SHOVE_FOV) return;

            // yumuşatılmış nişan
            Vector3 cur = ReadLocalAngles();
            Vector3 smooth = LerpAngles(cur, desired, SMOOTH_FACTOR);
            smooth = AddRandomOffset(smooth, 1.0f);
            WriteLocalAngles(smooth);

            swed.WriteInt(client, offsets.forceShove, 5);   // press
            nextShoveTime = DateTime.Now.AddMilliseconds(rng.Next(180, 260));
        }

        /* ---------- Yardımcı ---------- */
        private float AngleDiffDeg(Vector3 a, Vector3 b)
        {
            float dx = NormalizeDeg(a.X - b.X);
            float dy = NormalizeDeg(a.Y - b.Y);
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        private float NormalizeDeg(float deg)
        {
            while (deg > 180) deg -= 360;
            while (deg < -180) deg += 360;
            return deg;
        }

        #endregion
    }
    #endregion
}
