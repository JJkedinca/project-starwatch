using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.Map
{
    /// <summary>
    /// Shared HUD hit test so map pan and picking ignore the same panels.
    /// </summary>
    internal static class MapHudHitTest
    {
        internal static bool IsOverBlockingPanel(UIDocument uiDocument, Vector2 screenPosition)
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                return false;
            }

            var root = uiDocument.rootVisualElement;
            var panel = root.panel;
            if (panel == null)
            {
                return false;
            }

            var panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPosition);
            var picked = panel.Pick(panelPos);
            if (picked == null)
            {
                return false;
            }

            for (var current = picked; current != null; current = current.parent)
            {
                var name = current.name;
                if (name == "fleet-roster-host"
                    || name == "inspection-panel"
                    || name == "fleet-bar-host"
                    || name == "top-bar"
                    || name == "toast-stack"
                    || name == "star-system"
                    || name == "settlement-panel")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
