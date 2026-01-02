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
                    ImGui.Checkbox("ESP", ref settings.EnableEsp);
                    if (settings.EnableEsp)
                    {
                        ImGui.SameLine();
                        ImGui.Checkbox("Show health", ref settings.EnableHealthText);
                    }
                    ImGui.Checkbox("AutoShove", ref settings.EnableAutoShove);
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Color Settings"))
                {
                    ImGui.ColorPicker4("Survivor Color", ref settings.SurvivorColor);
                    ImGui.Checkbox("Survivor Line", ref settings.SurvivorLineEnable);
                    ImGui.Checkbox("Survivor Box", ref settings.SurvivorBoxEnable);
                    ImGui.Checkbox("Survivor Dot", ref settings.SurvivorDotEnable);

                    ImGui.ColorPicker4("Special Infected Color", ref settings.SpecialInfectedColor);
                    ImGui.Checkbox("Special Infected Line", ref settings.SpecialInfectedLineEnable);
                    ImGui.Checkbox("Special Infected Box", ref settings.SpecialInfectedBoxEnable);
                    ImGui.Checkbox("Special Infected Dot", ref settings.SpecialInfectedDotEnable);

                    ImGui.ColorPicker4("Common Infected Color", ref settings.CommonInfectedColor);
                    ImGui.Checkbox("Common Infected Line", ref settings.CommonInfectedLineEnable);
                    ImGui.Checkbox("Common Infected Box", ref settings.CommonInfectedBoxEnable);
                    ImGui.Checkbox("Common Infected Dot", ref settings.CommonInfectedDotEnable);

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
