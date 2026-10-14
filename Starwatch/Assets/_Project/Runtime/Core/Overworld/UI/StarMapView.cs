using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run.Universe;
using UnityEngine;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Owns the map surface and map-node element lifecycle.
    /// </summary>
    internal sealed class StarMapView
    {
        public event Action<ContentId> NodeClicked;

        private VisualElement _mapSurface;
        private readonly Dictionary<ContentId, MapNodeView> _nodesById = new();
        private float _scale = MapLayout.kDefaultScale;
        private Vector2 _originOffset = MapLayout.kDefaultPadding;

        /// <summary>
        /// Binds to the map-surface element from Overworld UXML.
        /// </summary>
        public void Bind(VisualElement mapSurface)
        {
            _mapSurface = mapSurface;
        }

        /// <summary>
        /// Clears and rebuilds node elements from the current universe graph.
        /// </summary>
        public void Rebuild(IEnumerable<MapNode> nodes)
        {
            Clear();
            if (_mapSurface == null || nodes == null)
            {
                return;
            }

            List<MapNode> validNodes = new();
            List<Vector2> positions = new();
            foreach (var node in nodes)
            {
                if (node == null || node.Id.IsEmpty)
                {
                    continue;
                }

                validNodes.Add(node);
                positions.Add(node.MapPosition);
            }

            _originOffset = MapLayout.ComputeOriginOffset(positions, _scale, MapLayout.kDefaultPadding);

            for (var i = 0; i < validNodes.Count; i++)
            {
                var node = validNodes[i];
                var view = new MapNodeView(node.Id, node.Kind);
                view.Clicked += id => NodeClicked?.Invoke(id);
                view.SetMapPosition(node.MapPosition, _scale, _originOffset);
                _nodesById[node.Id] = view;
                _mapSurface.Add(view.Root);
            }
        }

        /// <summary>
        /// Updates styling for one node.
        /// </summary>
        public void SetNodeState(ContentId nodeId, MapNodePresentationState state)
        {
            if (!_nodesById.TryGetValue(nodeId, out var view))
            {
                return;
            }

            view.ApplyState(state);
        }

        /// <summary>
        /// Removes all node elements from the map surface.
        /// </summary>
        public void Clear()
        {
            foreach (var pair in _nodesById)
            {
                pair.Value.Root.RemoveFromHierarchy();
            }

            _nodesById.Clear();
        }

        /// <summary>
        /// Returns whether a node element exists on the map.
        /// </summary>
        public bool HasNode(ContentId nodeId)
        {
            return _nodesById.ContainsKey(nodeId);
        }

        /// <summary>
        /// Number of node elements currently on the map.
        /// </summary>
        public int NodeCount => _nodesById.Count;
    }
}
