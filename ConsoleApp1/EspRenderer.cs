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

            if (ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem("General"))
                {
                    var enableEsp = settings.EnableEsp;
                    ImGui.Checkbox("ESP", ref enableEsp);
                    settings.EnableEsp = enableEsp;
                    var enableHealthText = settings.EnableHealthText;
                    if (enableEsp)
                    {
                        ImGui.SameLine();
                        ImGui.Checkbox("Show health", ref enableHealthText);
                    }
                    settings.EnableHealthText = enableHealthText;
                    var enableAutoShove = settings.EnableAutoShove;
                    ImGui.Checkbox("AutoShove", ref enableAutoShove);
                    settings.EnableAutoShove = enableAutoShove;
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Color Settings"))
                {
                    var survivorColor = settings.SurvivorColor;
                    ImGui.ColorPicker4("Survivor Color", ref survivorColor);
                    settings.SurvivorColor = survivorColor;
                    var survivorLineEnable = settings.SurvivorLineEnable;
                    ImGui.Checkbox("Survivor Line", ref survivorLineEnable);
                    settings.SurvivorLineEnable = survivorLineEnable;
                    var survivorBoxEnable = settings.SurvivorBoxEnable;
                    ImGui.Checkbox("Survivor Box", ref survivorBoxEnable);
                    settings.SurvivorBoxEnable = survivorBoxEnable;
                    var survivorDotEnable = settings.SurvivorDotEnable;
                    ImGui.Checkbox("Survivor Dot", ref survivorDotEnable);
                    settings.SurvivorDotEnable = survivorDotEnable;

                    var specialInfectedColor = settings.SpecialInfectedColor;
                    ImGui.ColorPicker4("Special Infected Color", ref specialInfectedColor);
                    settings.SpecialInfectedColor = specialInfectedColor;
                    var specialInfectedLineEnable = settings.SpecialInfectedLineEnable;
                    ImGui.Checkbox("Special Infected Line", ref specialInfectedLineEnable);
                    settings.SpecialInfectedLineEnable = specialInfectedLineEnable;
                    var specialInfectedBoxEnable = settings.SpecialInfectedBoxEnable;
                    ImGui.Checkbox("Special Infected Box", ref specialInfectedBoxEnable);
                    settings.SpecialInfectedBoxEnable = specialInfectedBoxEnable;
                    var specialInfectedDotEnable = settings.SpecialInfectedDotEnable;
                    ImGui.Checkbox("Special Infected Dot", ref specialInfectedDotEnable);
                    settings.SpecialInfectedDotEnable = specialInfectedDotEnable;

                    var commonInfectedColor = settings.CommonInfectedColor;
                    ImGui.ColorPicker4("Common Infected Color", ref commonInfectedColor);
                    settings.CommonInfectedColor = commonInfectedColor;
                    var commonInfectedLineEnable = settings.CommonInfectedLineEnable;
                    ImGui.Checkbox("Common Infected Line", ref commonInfectedLineEnable);
                    settings.CommonInfectedLineEnable = commonInfectedLineEnable;
                    var commonInfectedBoxEnable = settings.CommonInfectedBoxEnable;
                    ImGui.Checkbox("Common Infected Box", ref commonInfectedBoxEnable);
                    settings.CommonInfectedBoxEnable = commonInfectedBoxEnable;
                    var commonInfectedDotEnable = settings.CommonInfectedDotEnable;
                    ImGui.Checkbox("Common Infected Dot", ref commonInfectedDotEnable);
                    settings.CommonInfectedDotEnable = commonInfectedDotEnable;

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
