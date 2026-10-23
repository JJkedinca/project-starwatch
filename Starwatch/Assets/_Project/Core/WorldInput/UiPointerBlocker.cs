using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Core.WorldInput
{
    /// <summary>
    /// True when the pointer is over a picking UI element (not a document/layout pass-through root).
    /// </summary>
    public static class UiPointerBlocker
    {
        public static bool IsOverUi(IPanel panel, Vector2 screenPosition)
        {
            if (panel == null)
            {
                return false;
            }

            // Input System pointer positions are bottom-left origin; panel space is top-left.
            var panelPos = RuntimePanelUtils.ScreenToPanel(
                panel,
                new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            var picked = panel.Pick(panelPos);
            if (picked == null)
            {
                return false;
            }

            var visualTree = panel.visualTree;
            if (picked == visualTree)
            {
                return false;
            }

            if (picked.parent == visualTree)
            {
                // Document roots (and other top-level pass-through hosts) let world input through.
                return false;
            }

            return true;
        }

        public static bool IsOverUi(UIDocument document, Vector2 screenPosition)
        {
            if (document == null || document.rootVisualElement == null)
            {
                return false;
            }

            return IsOverUi(document.rootVisualElement.panel, screenPosition);
        }
    }
}
