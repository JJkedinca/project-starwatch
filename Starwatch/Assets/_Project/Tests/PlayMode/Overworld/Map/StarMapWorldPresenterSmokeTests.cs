using System.Collections;
using NUnit.Framework;
using ProjectStarwatch.Core.Data;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using ProjectStarwatch.Overworld.Map;
using ProjectStarwatch.Overworld.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectStarwatch.Tests.PlayMode.Overworld.Map
{
    /// <summary>
    /// Thin smoke: world presenter places one active node per model node.
    /// </summary>
    public sealed class StarMapWorldPresenterSmokeTests
    {
        [UnityTest]
        public IEnumerator RebuildNodes_CreatesOneActiveVisualPerModelNode()
        {
            var root = new GameObject("map-smoke");
            var nodesRoot = new GameObject("nodes").transform;
            nodesRoot.SetParent(root.transform, false);
            var routesRoot = new GameObject("routes").transform;
            routesRoot.SetParent(root.transform, false);
            var fleetsRoot = new GameObject("fleets").transform;
            fleetsRoot.SetParent(root.transform, false);

            var nodePrefabGo = new GameObject("node-prefab");
            nodePrefabGo.AddComponent<MapNodeView>();
            nodePrefabGo.SetActive(false);

            var presenter = root.AddComponent<StarMapWorldPresenter>();
            var so = new SerializedObjectProxy(presenter);
            so.Set("_nodesRoot", nodesRoot);
            so.Set("_routesRoot", routesRoot);
            so.Set("_fleetsRoot", fleetsRoot);
            so.Set("_nodePrefab", nodePrefabGo.GetComponent<MapNodeView>());

            var nodes = new[]
            {
                new StarMapNodeModel(
                    new ContentId("alpha"),
                    MapNodeKind.System,
                    new Vector2(1f, 2f),
                    new MapNodePresentationState(
                        MapNodeVisibility.Explored,
                        false,
                        false,
                        false,
                        false,
                        "Alpha")),
                new StarMapNodeModel(
                    new ContentId("beta"),
                    MapNodeKind.System,
                    new Vector2(3f, 1f),
                    new MapNodePresentationState(
                        MapNodeVisibility.Explored,
                        false,
                        false,
                        false,
                        false,
                        "Beta")),
            };

            presenter.RebuildNodes(nodes);
            yield return null;

            Assert.That(presenter.PickPoints.Count, Is.EqualTo(2));

            Object.Destroy(root);
            Object.Destroy(nodePrefabGo);
        }

        /// <summary>
        /// Tiny reflection helper so PlayMode smoke can set private SerializeFields
        /// without dragging the whole scene into the test.
        /// </summary>
        private sealed class SerializedObjectProxy
        {
            private readonly object _target;

            public SerializedObjectProxy(object target)
            {
                _target = target;
            }

            public void Set(string fieldName, object value)
            {
                var field = _target.GetType().GetField(
                    fieldName,
                    System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.NonPublic);
                Assert.That(field, Is.Not.Null, fieldName);
                field.SetValue(_target, value);
            }
        }
    }
}
