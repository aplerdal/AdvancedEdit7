using System;
using Hexa.NET.ImGui;

namespace AdvancedEdit.UI.Framework.Helpers;

public partial class ImGuiHelper
    {
        public class PropertyInfo
        {
            public bool CanDrag = false;

            public float Speed = 1.0f;
        }

        /// <summary>
        /// Draws a text label as bold.
        /// </summary>
        public static void BoldText(string text)
        {
            ImGuiHelper.BeginBoldText();
            ImGui.Text(text);
            ImGuiHelper.EndBoldText();
        }

        /// <summary>
        /// Draws a text label as bold with value text next to it.
        /// </summary>
        public static void BoldTextLabel(string key, string label)
        {
            ImGuiHelper.BeginBoldText();
            ImGui.Text($"{key}:");
            ImGuiHelper.EndBoldText();

            ImGui.SameLine();
            ImGui.TextColored(ImGui.GetStyle().Colors[(int)ImGuiCol.Text], label);
        }

        /// <summary>
        /// Makes any font used UI element as bold.
        /// </summary>
        public static void BeginBoldText() {
            //ImGui.PushFont(ImGuiController.DefaultFontBold);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes the BeginBoldText()
        /// </summary>
        public static void EndBoldText() {
            ImGui.PopFont();
        }

        /// <summary>
        /// Creates a hyperlink visual with an underline.
        /// </summary>
        public static void HyperLinkText(string text)
        {
            throw new NotImplementedException();
            /*var color = ThemeHandler.HyperLinkText;
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.Text(text);

            var lineEnd = ImGui.GetItemRectMax();
            var lineStart = lineEnd;
            lineStart.X = ImGui.GetItemRectMin().X;
            ImGui.GetWindowDrawList().AddLine(lineStart, lineEnd, ImGui.ColorConvertFloat4ToU32(color));

            ImGui.PopStyleColor();*/
        }

        /// <summary>
        /// Creates a tooltip for the hovered item drawn before this is called.
        /// </summary>
        public static void Tooltip(string tooltip, string shortcut = "")
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(tooltip);
                if (!string.IsNullOrEmpty(shortcut))
                    ImGui.Text($"Shortcut: {shortcut}");

                ImGui.EndTooltip();
            }
        }

        /// <summary>
        /// Increases the cursor position on the X direction.
        /// </summary>
        public static void IncrementCursorPosX(float amount) {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + amount);
        }

        /// <summary>
        /// Increases the cursor position on the Y direction.
        /// </summary>
        public static void IncrementCursorPosY(float amount) {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + amount);
        }

        public static void DrawCenteredText(string text)
        {
            float windowWidth = ImGui.GetWindowSize().X;
            float textWidth = ImGui.CalcTextSize(text).X;

            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);
            ImGui.Text(text);
        }
    }