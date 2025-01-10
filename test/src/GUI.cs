using System;
using System.Numerics;
using Gowtu;
using ImGuiNET;

namespace GowtuApp
{
    public delegate void MouseEnterEvent(long id);
    public delegate void MouseLeaveEvent(long id);
    public delegate void MouseClickEvent(long id);

    public static class GUI
    {
        public static event MouseEnterEvent MouseEnter;
        public static event MouseLeaveEvent MouseLeave;
        public static event MouseClickEvent MouseClick;

        private static long idCounter = 0;
        private static long  hoveredId = -1;
        private static double hoverTimer = 0.0f;

        private static long CreateId()
        {
            long id = idCounter;
            idCounter++;
            return id;
        }

        private static long GetId()
        {
            return idCounter - 1;
        }

        private static bool IsHovered()
        {
            return GetId() == hoveredId;
        }

        private static void CheckHoveredState()
        {
            long id = idCounter - 1;

            if(ImGui.IsItemHovered())
            {
                if(hoveredId != id)
                {
                    hoveredId = id;
                    MouseEnter?.Invoke(id);
                }

                hoverTimer += Time.DeltaTime;
            }
            else
            {
                if(hoveredId == id)
                {
                    hoveredId = -1;
                    MouseLeave?.Invoke(id);

                    hoverTimer = 0;
                }
            }
        }

        private static bool ApplyHoveredStyleColor(ImGuiCol styleCol)
        {
            if(!IsHovered())
                return false;

            float t = (float)((Math.Sin(hoverTimer * 4) + 1.0) * 0.5f);

            Color color1 = new Color(102, 102, 102, 255);
            Color color2 = new Color(176, 176, 176, 255);

            Color color = Color.Lerp(color1, color2, t);

            ImGui.PushStyleColor(styleCol, new Vector4(color.r, color.g, color.b, color.a));
            return true;
        }

        private static void ResetHoveredState()
        {
            hoverTimer = 0.0;
            hoveredId = -1;
        }

        public static void EndFrame()
        {
            idCounter = 0;
        }

        public static bool BeginWindow(string name, Vector2 size)
        {
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);

            ImGuiWindowFlags flags = 0;
            flags |= ImGuiWindowFlags.NoTitleBar;
            flags |= ImGuiWindowFlags.NoResize;
            flags |= ImGuiWindowFlags.NoMove;
            flags |= ImGuiWindowFlags.NoScrollbar;	
            flags |= ImGuiWindowFlags.NoBackground;
            flags |= ImGuiWindowFlags.NoScrollWithMouse;
            flags |= ImGuiWindowFlags.NoDecoration;
            flags |= ImGuiWindowFlags.NoBackground;
            flags |= ImGuiWindowFlags.NoDocking;
            flags |= ImGuiWindowFlags.NoCollapse;
            flags |= ImGuiWindowFlags.NoSavedSettings;
            flags |= ImGuiWindowFlags.NoFocusOnAppearing;            

            return ImGui.Begin(name, flags);
        }

        public static void EndWindow()
        {
            ImGui.End();
        }

        public static void CenterWindow()
        {
            // Get the size of the window
            var windowSize = ImGui.GetWindowSize();
            var viewportSize = ImGui.GetIO().DisplaySize;

            // Calculate the position to center the window
            var windowPos = new System.Numerics.Vector2((viewportSize.X - windowSize.X) * 0.5f, (viewportSize.Y - windowSize.Y) * 0.5f);

            // Set the window position
            ImGui.SetWindowPos(windowPos);
        }

        public static void SameLine()
        {
            ImGui.SameLine();
        }

        public static bool Button(string text, Vector2 size, Vector2 offset = default(Vector2), bool enabled = true)
        {
            long id = CreateId();

            var windowSize = ImGui.GetWindowSize();
            var cursorPos = ImGui.GetCursorPos();
            cursorPos.X = (windowSize.X - size.X) * 0.5f;
            cursorPos += offset;
            ImGui.SetCursorPos(cursorPos);

            bool result = false;

            if(enabled)
            {
                bool popColors = ApplyHoveredStyleColor(ImGuiCol.ButtonHovered);

                result = ImGui.Button(text, size);

                if(popColors)
                    ImGui.PopStyleColor(1);

                CheckHoveredState();
            }
            else
            {
                var color = ImGui.GetStyle().Colors[(int)ImGuiCol.Button];
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, color);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color);
                ImGui.Button(text, size);
                ImGui.PopStyleColor(2);
            }

            if(result)
            {
                ResetHoveredState();
                MouseClick?.Invoke(id);
            }

            return result;
        }

        public static void Text(string text)
        {
            CreateId();

            var windowSize = ImGui.GetWindowSize();
            var textSize = ImGui.CalcTextSize(text);
            var cursorPos = ImGui.GetCursorPos();
            cursorPos.X = (windowSize.X - textSize.X) * 0.5f;
            ImGui.SetCursorPos(cursorPos);
            ImGui.Text(text);
        }

        public static bool Combo(string label, ref int current_item, string[] items, int items_count, System.Numerics.Vector2 size)
        {
            long id = CreateId();

            var windowSize = ImGui.GetWindowSize();
            var contentSize = ImGui.GetContentRegionAvail();

            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new System.Numerics.Vector2(-10000, -10000));

            int current = 0;
            ImGui.SetNextItemWidth(size.X);
            ImGui.Combo("##dummy_combo", ref current, items, items_count);

            // Define the size of the combo box (you can also use ImGui::CalcTextSize to get the size of the items)
            var comboSize = ImGui.GetItemRectSize();

            // Calculate the starting position to center the combo box
            float startX = (contentSize.X - comboSize.X) * 0.5f;

            ImGui.SetCursorPos(cursorPos);

            // Set the cursor position for the combo box
            ImGui.SetCursorPosX(startX);

            bool popColors = ApplyHoveredStyleColor(ImGuiCol.FrameBgHovered);

            bool result = ImGui.Combo(label, ref current_item, items, items_count);

            if(popColors)
                ImGui.PopStyleColor(1);

            CheckHoveredState();

            if(result)
            {
                ResetHoveredState();
                MouseClick?.Invoke(id);
            }

            return result;
        }

        public static bool SliderFloat(string label, System.Numerics.Vector2 size, ref float value, float min, float max, int decimalPlaces = 3)
        {
            long id = CreateId();

            if(decimalPlaces <= 0)
                decimalPlaces = 1;

            var windowSize = ImGui.GetWindowSize();
            var cursorPos = ImGui.GetCursorPos();
            cursorPos.X = (windowSize.X - size.X) * 0.5f;
            ImGui.SetCursorPos(cursorPos);
            ImGui.SetNextItemWidth(size.X);

            bool popColors = ApplyHoveredStyleColor(ImGuiCol.FrameBgHovered);

            bool result = ImGui.SliderFloat(label, ref value, min, max, "%." + decimalPlaces + "f", ImGuiSliderFlags.NoInput);

            if(popColors)
                ImGui.PopStyleColor(1);

            CheckHoveredState();

            if(result)
            {
                ResetHoveredState();
                //MouseClick?.Invoke(id);
            }

            return result;
        }
    }
}