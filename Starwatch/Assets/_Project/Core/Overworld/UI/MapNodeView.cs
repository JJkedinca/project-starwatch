using System;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Thin UI Toolkit element for one map node. Presentation only.
    /// Fleet arrows live on <see cref="FleetMarkerView"/>, not on the node.
    /// </summary>
    internal sealed class MapNodeView
    {
        public event Action<ContentId> Clicked;

        public event Action<ContentId> RightClicked;

        public event Action<ContentId> PointerEntered;

        public event Action<ContentId> PointerLeft;

        internal Button Root { get; }

        private readonly ContentId _nodeId;

        public MapNodeView(ContentId nodeId, MapNodeKind kind)
        {
            _nodeId = nodeId;
            Root = new Button();
            Root.name = "node-" + (nodeId.IsEmpty ? "unknown" : nodeId.Value);
            Root.AddToClassList("map-node");
            if (kind == MapNodeKind.System)
            {
                Root.AddToClassList("map-node--system");
            }

            Root.style.width = MapLayout.kNodeSize;
            Root.style.height = MapLayout.kNodeSize;
            if (kind != MapNodeKind.System)
            {
                var radius = MapLayout.kNodeSize * 0.5f;
                Root.style.borderTopLeftRadius = radius;
                Root.style.borderTopRightRadius = radius;
                Root.style.borderBottomLeftRadius = radius;
                Root.style.borderBottomRightRadius = radius;
            }

            Root.style.overflow = Overflow.Visible;

            Root.clicked += () => Clicked?.Invoke(_nodeId);
            // A Button only raises ClickEvent for the left mouse button.
            Root.RegisterCallback<PointerUpEvent>(OnPointerUp);
            Root.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            Root.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button != (int)MouseButton.RightMouse)
            {
                return;
            }

            evt.StopPropagation();
            RightClicked?.Invoke(_nodeId);
        }

        private void OnPointerEnter(PointerEnterEvent evt)
        {
            PointerEntered?.Invoke(_nodeId);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            PointerLeft?.Invoke(_nodeId);
        }

        /// <summary>
        /// Applies visibility / selection styling without leaking hidden names.
        /// </summary>
        public void ApplyState(MapNodePresentationState state)
        {
            Root.EnableInClassList("map-node--hidden", state.Visibility == MapNodeVisibility.Hidden);
            Root.EnableInClassList("map-node--explored", state.Visibility == MapNodeVisibility.Explored);
            Root.EnableInClassList("map-node--present", state.Visibility == MapNodeVisibility.Present);
            Root.EnableInClassList("map-node--explorable", state.IsExplorable);
            Root.EnableInClassList("map-node--selected", state.IsSelected);

            if (state.Visibility == MapNodeVisibility.Hidden)
            {
                Root.text = state.IsExplorable ? "?" : string.Empty;
                return;
            }

            Root.text = string.IsNullOrEmpty(state.RevealedLabel) ? _nodeId.Value : state.RevealedLabel;
        }

        /// <summary>
        /// Places the node using map-space coordinates.
        /// </summary>
        public void SetMapPosition(UnityEngine.Vector2 mapPosition, float scale, UnityEngine.Vector2 originOffset)
        {
            var local = MapLayout.ToLocalPosition(mapPosition, scale, originOffset);
            Root.style.left = local.x;
            Root.style.top = local.y;
        }
    }
}
