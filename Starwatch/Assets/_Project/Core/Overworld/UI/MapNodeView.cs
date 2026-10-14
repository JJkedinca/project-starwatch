using System;
using EndlessHeros.Core.Data;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Overworld;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Thin UI Toolkit element for one map node. Presentation only.
    /// </summary>
    internal sealed class MapNodeView
    {
        public event Action<ContentId> Clicked;

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

            Root.clicked += () => Clicked?.Invoke(_nodeId);
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

            var label = string.IsNullOrEmpty(state.RevealedLabel) ? _nodeId.Value : state.RevealedLabel;
            Root.text = state.HasFleet ? "F\n" + label : label;
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
