using ImGuiNET;
using System.Numerics;

namespace L4D2MultiMenu
{
    internal class EspRenderer
    {
        private ImDrawListPtr drawList;

        public void BeginOverlay(GameState state)
        {
            ImGui.SetNextWindowSize(state.WindowSize);
            ImGui.SetNextWindowPos(state.WindowLocation);
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

        public void RenderMenu(Settings settings)
        {
            ImGui.Begin("L4D2 Menu by attackN");

            Vector4 activeColor = new Vector4(0f, 1f, 0f, 1f);
            Vector4 inactiveColor = new Vector4(1f, 0f, 0f, 1f);
            string espStatus = settings.EnableEsp ? "ON" : "OFF";
            string autoShoveStatus = settings.EnableAutoShove ? "ON" : "OFF";

            ImGui.TextColored(settings.EnableEsp ? activeColor : inactiveColor, $"ESP: {espStatus}");
            ImGui.SameLine();
            ImGui.TextColored(settings.EnableAutoShove ? activeColor : inactiveColor, $"AutoShove: {autoShoveStatus}");

            if (ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    bool enableEsp = settings.EnableEsp;
                    if (ImGui.Checkbox("ESP", ref enableEsp))
                    {
                        settings.EnableEsp = enableEsp;
                    }
                    if (settings.EnableEsp)
                    {
                        ImGui.SameLine();
                        bool enableHealthText = settings.EnableHealthText;
                        if (ImGui.Checkbox("Show health", ref enableHealthText))
                        {
                            settings.EnableHealthText = enableHealthText;
                        }
                    }
                    bool enableAutoShove = settings.EnableAutoShove;
                    if (ImGui.Checkbox("AutoShove", ref enableAutoShove))
                    {
                        settings.EnableAutoShove = enableAutoShove;
                    }
                    if (settings.EnableAutoShove)
                    {
                        ImGui.Indent();
                        bool requireAutoShoveKey = settings.RequireAutoShoveKey;
                        if (ImGui.Checkbox("Require AutoShove key (F)", ref requireAutoShoveKey))
                        {
                            settings.RequireAutoShoveKey = requireAutoShoveKey;
                        }
                        ImGui.Unindent();
                    }
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Color Settings"))
                {
                    Vector4 survivorColor = settings.SurvivorColor;
                    if (ImGui.ColorPicker4("Survivor Color", ref survivorColor))
                    {
                        settings.SurvivorColor = survivorColor;
                    }
                    bool survivorLineEnable = settings.SurvivorLineEnable;
                    if (ImGui.Checkbox("Survivor Line", ref survivorLineEnable))
                    {
                        settings.SurvivorLineEnable = survivorLineEnable;
                    }
                    bool survivorBoxEnable = settings.SurvivorBoxEnable;
                    if (ImGui.Checkbox("Survivor Box", ref survivorBoxEnable))
                    {
                        settings.SurvivorBoxEnable = survivorBoxEnable;
                    }
                    bool survivorDotEnable = settings.SurvivorDotEnable;
                    if (ImGui.Checkbox("Survivor Dot", ref survivorDotEnable))
                    {
                        settings.SurvivorDotEnable = survivorDotEnable;
                    }

                    Vector4 specialInfectedColor = settings.SpecialInfectedColor;
                    if (ImGui.ColorPicker4("Special Infected Color", ref specialInfectedColor))
                    {
                        settings.SpecialInfectedColor = specialInfectedColor;
                    }
                    bool specialInfectedLineEnable = settings.SpecialInfectedLineEnable;
                    if (ImGui.Checkbox("Special Infected Line", ref specialInfectedLineEnable))
                    {
                        settings.SpecialInfectedLineEnable = specialInfectedLineEnable;
                    }
                    bool specialInfectedBoxEnable = settings.SpecialInfectedBoxEnable;
                    if (ImGui.Checkbox("Special Infected Box", ref specialInfectedBoxEnable))
                    {
                        settings.SpecialInfectedBoxEnable = specialInfectedBoxEnable;
                    }
                    bool specialInfectedDotEnable = settings.SpecialInfectedDotEnable;
                    if (ImGui.Checkbox("Special Infected Dot", ref specialInfectedDotEnable))
                    {
                        settings.SpecialInfectedDotEnable = specialInfectedDotEnable;
                    }

                    Vector4 commonInfectedColor = settings.CommonInfectedColor;
                    if (ImGui.ColorPicker4("Common Infected Color", ref commonInfectedColor))
                    {
                        settings.CommonInfectedColor = commonInfectedColor;
                    }
                    bool commonInfectedLineEnable = settings.CommonInfectedLineEnable;
                    if (ImGui.Checkbox("Common Infected Line", ref commonInfectedLineEnable))
                    {
                        settings.CommonInfectedLineEnable = commonInfectedLineEnable;
                    }
                    bool commonInfectedBoxEnable = settings.CommonInfectedBoxEnable;
                    if (ImGui.Checkbox("Common Infected Box", ref commonInfectedBoxEnable))
                    {
                        settings.CommonInfectedBoxEnable = commonInfectedBoxEnable;
                    }
                    bool commonInfectedDotEnable = settings.CommonInfectedDotEnable;
                    if (ImGui.Checkbox("Common Infected Dot", ref commonInfectedDotEnable))
                    {
                        settings.CommonInfectedDotEnable = commonInfectedDotEnable;
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
            ImGui.End();
        }

        public void RenderEsp(GameState state, Settings settings)
        {
            drawList = ImGui.GetWindowDrawList();
            if (!settings.EnableEsp) return;

            try
            {
                foreach (var entity in state.CommonInfected.ToList())
                {
                    if (entity != null && IsPixelInsideScreen(state, entity.originScreenPosition))
                    {
                        DrawVisuals(state, entity, settings.CommonInfectedColor, settings.CommonInfectedLineEnable, settings.CommonInfectedBoxEnable, settings.CommonInfectedDotEnable, settings.EnableHealthText);
                    }
                }
                foreach (var entity in state.SpecialInfected.ToList())
                {
                    if (entity != null && IsPixelInsideScreen(state, entity.originScreenPosition))
                    {
                        DrawVisuals(state, entity, settings.SpecialInfectedColor, settings.SpecialInfectedLineEnable, settings.SpecialInfectedBoxEnable, settings.SpecialInfectedDotEnable, settings.EnableHealthText);
                    }
                }
                foreach (var entity in state.Survivors.ToList())
                {
                    if (entity != null && IsPixelInsideScreen(state, entity.originScreenPosition))
                    {
                        DrawVisuals(state, entity, settings.SurvivorColor, settings.SurvivorLineEnable, settings.SurvivorBoxEnable, settings.SurvivorDotEnable, settings.EnableHealthText);
                    }
                }
            }
            catch (Exception)
            {
                // Could log error here
            }
        }

        private bool IsPixelInsideScreen(GameState state, Vector2 pixel)
        {
            return pixel.X > state.WindowLocation.X && pixel.X < state.WindowLocation.X + state.WindowSize.X &&
                   pixel.Y > state.WindowLocation.Y && pixel.Y < state.WindowLocation.Y + state.WindowSize.Y;
        }

        private void DrawHealthText(Entity entity)
        {
            if (entity.health < 10) return;

            string displayText = $"{entity.infectedType} [{entity.health} HP]";
            Vector2 textPos = new Vector2(entity.originScreenPosition.X, entity.originScreenPosition.Y - 15);

            drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), displayText);
        }

        private void DrawVisuals(GameState state, Entity entity, Vector4 color, bool line, bool box, bool dot, bool drawHealth)
        {
            if (!IsPixelInsideScreen(state, entity.originScreenPosition)) return;

            if (line)
            {
                drawList.AddLine(state.LineOrigin, entity.originScreenPosition, ImGui.ColorConvertFloat4ToU32(color), 2);
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
            if (drawHealth) DrawHealthText(entity);
        }
    }
}
