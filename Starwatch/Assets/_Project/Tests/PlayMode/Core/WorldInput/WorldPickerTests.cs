using System.Collections;
using NUnit.Framework;
using ProjectStarwatch.Core.WorldInput;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectStarwatch.Tests.PlayMode.Core.WorldInput
{
    public sealed class WorldPickerTests
    {
        [UnityTest]
        public IEnumerator TryPick_ExactHit_SelectsCollider()
        {
            var setup = CreateSetup(nodePosition: Vector3.zero, fleetPosition: new Vector3(10f, 0.4f, 0f));
            yield return null;

            var screen = setup.Camera.WorldToScreenPoint(Vector3.zero);
            var query = new WorldPickQuery(StarwatchLayers.MapNode.ToMask(), tolerancePixels: 0f, maxDistance: 100f);
            var hit = WorldPicker.TryPick(setup.Camera, screen, query, out var rayHit);

            Assert.That(hit, Is.True);
            Assert.That(rayHit.collider, Is.EqualTo(setup.NodeCollider));
            setup.Destroy();
        }

        [UnityTest]
        public IEnumerator TryPick_NearMiss_UsesPixelTolerance()
        {
            var setup = CreateSetup(nodePosition: Vector3.zero, fleetPosition: new Vector3(10f, 0.4f, 0f));
            yield return null;

            var onNode = setup.Camera.WorldToScreenPoint(Vector3.zero);
            var nearMiss = new Vector2(onNode.x + 8f, onNode.y);
            var farMiss = new Vector2(onNode.x + 80f, onNode.y);

            var nearQuery = new WorldPickQuery(StarwatchLayers.MapNode.ToMask(), tolerancePixels: 24f, maxDistance: 100f);
            var farQuery = new WorldPickQuery(StarwatchLayers.MapNode.ToMask(), tolerancePixels: 4f, maxDistance: 100f);

            Assert.That(WorldPicker.TryPick(setup.Camera, nearMiss, nearQuery, out _), Is.True);
            Assert.That(WorldPicker.TryPick(setup.Camera, farMiss, farQuery, out _), Is.False);
            setup.Destroy();
        }

        [UnityTest]
        public IEnumerator TryPick_LayerMask_ExcludesOtherLayers()
        {
            var setup = CreateSetup(nodePosition: Vector3.zero, fleetPosition: Vector3.zero + Vector3.up * 0.4f);
            yield return null;

            var screen = setup.Camera.WorldToScreenPoint(Vector3.zero);
            var nodeOnly = new WorldPickQuery(StarwatchLayers.MapNode.ToMask(), 24f, 100f);
            var fleetOnly = new WorldPickQuery(StarwatchLayers.MapFleet.ToMask(), 24f, 100f);

            Assert.That(WorldPicker.TryPick(setup.Camera, screen, nodeOnly, out var nodeHit), Is.True);
            Assert.That(nodeHit.collider, Is.EqualTo(setup.NodeCollider));
            Assert.That(WorldPicker.TryPick(setup.Camera, screen, fleetOnly, out var fleetHit), Is.True);
            Assert.That(fleetHit.collider, Is.EqualTo(setup.FleetCollider));
            setup.Destroy();
        }

        [UnityTest]
        public IEnumerator TryPick_HigherCollider_WinsWhenBothInMask()
        {
            var setup = CreateSetup(nodePosition: Vector3.zero, fleetPosition: new Vector3(0f, 0.4f, 0f));
            yield return null;

            var screen = setup.Camera.WorldToScreenPoint(Vector3.zero);
            var both = new WorldPickQuery(
                StarwatchLayersExtensions.ToMask(StarwatchLayers.MapNode, StarwatchLayers.MapFleet),
                0f,
                100f);
            Assert.That(WorldPicker.TryPick(setup.Camera, screen, both, out var hit), Is.True);
            Assert.That(hit.collider, Is.EqualTo(setup.FleetCollider));
            setup.Destroy();
        }

        private static Setup CreateSetup(Vector3 nodePosition, Vector3 fleetPosition)
        {
            var root = new GameObject("world-picker-test");
            var cameraGo = new GameObject("cam");
            cameraGo.transform.SetParent(root.transform, false);
            cameraGo.transform.position = new Vector3(0f, 20f, 0f);
            cameraGo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var camera = cameraGo.AddComponent<Camera>();
            camera.fieldOfView = 60f;

            var nodeGo = new GameObject("node");
            nodeGo.layer = StarwatchLayers.MapNode.ToIndex();
            nodeGo.transform.SetParent(root.transform, false);
            nodeGo.transform.position = nodePosition;
            var nodeCollider = nodeGo.AddComponent<SphereCollider>();
            nodeCollider.isTrigger = true;
            nodeCollider.radius = 0.75f;

            var fleetGo = new GameObject("fleet");
            fleetGo.layer = StarwatchLayers.MapFleet.ToIndex();
            fleetGo.transform.SetParent(root.transform, false);
            fleetGo.transform.position = fleetPosition;
            var fleetCollider = fleetGo.AddComponent<SphereCollider>();
            fleetCollider.isTrigger = true;
            fleetCollider.radius = 0.4f;
            var body = fleetGo.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            return new Setup(root, camera, nodeCollider, fleetCollider);
        }

        private sealed class Setup
        {
            public Camera Camera { get; }

            public SphereCollider NodeCollider { get; }

            public SphereCollider FleetCollider { get; }

            private readonly GameObject _root;

            public Setup(GameObject root, Camera camera, SphereCollider node, SphereCollider fleet)
            {
                _root = root;
                Camera = camera;
                NodeCollider = node;
                FleetCollider = fleet;
            }

            public void Destroy()
            {
                Object.Destroy(_root);
            }
        }
    }
}
