using System;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Thin UI Toolkit element for one map node. Presentation only.
    /// </summary>
    internal sealed class MapNodeView
    {
        public event Action<ContentId> Clicked;

        internal Button Root { get; }

        private readonly ContentId _nodeId;
        private readonly VisualElement _fleetOrbit;
        private readonly VisualElement _fleetMarker;

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

            var orbitSize = MapLayout.kNodeSize + (MapLayout.kFleetOrbitOutset * 2f);
            _fleetOrbit = new VisualElement { name = "fleet-orbit" };
            _fleetOrbit.AddToClassList("map-node__orbit");
            _fleetOrbit.pickingMode = PickingMode.Ignore;
            _fleetOrbit.style.left = -MapLayout.kFleetOrbitOutset;
            _fleetOrbit.style.top = -MapLayout.kFleetOrbitOutset;
            _fleetOrbit.style.width = orbitSize;
            _fleetOrbit.style.height = orbitSize;
            var orbitRadius = orbitSize * 0.5f;
            _fleetOrbit.style.borderTopLeftRadius = orbitRadius;
            _fleetOrbit.style.borderTopRightRadius = orbitRadius;
            _fleetOrbit.style.borderBottomLeftRadius = orbitRadius;
            _fleetOrbit.style.borderBottomRightRadius = orbitRadius;

            _fleetMarker = new VisualElement { name = "fleet-marker" };
            _fleetMarker.AddToClassList("map-node__fleet");
            _fleetMarker.pickingMode = PickingMode.Ignore;
            _fleetMarker.style.left = (orbitSize - MapLayout.kFleetPipSize) * 0.5f;
            _fleetMarker.style.top = -MapLayout.kFleetPipSize * 0.5f;
            _fleetOrbit.Add(_fleetMarker);
            Root.Add(_fleetOrbit);

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

            var showFleet = state.HasFleet && state.Visibility != MapNodeVisibility.Hidden;
            var traveling = showFleet && state.IsFleetTraveling;
            _fleetOrbit.EnableInClassList("map-node__orbit--visible", showFleet);
            _fleetOrbit.EnableInClassList("map-node__orbit--traveling", traveling);
            _fleetMarker.EnableInClassList("map-node__fleet--visible", showFleet);
            _fleetMarker.EnableInClassList("map-node__fleet--traveling", traveling);

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
