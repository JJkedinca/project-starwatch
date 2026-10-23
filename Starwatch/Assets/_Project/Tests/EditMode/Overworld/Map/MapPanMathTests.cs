using ProjectStarwatch.Overworld.Map;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    public sealed class MapPanMathTests
    {
        [Test]
        public void ScreenDeltaToWorldDelta_MovesOppositeToPointer()
        {
            var camera = new GameObject("pan-cam").AddComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = 60f;
            camera.transform.position = new Vector3(0f, 20f, 0f);
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            var delta = MapPanMath.ScreenDeltaToWorldDelta(
                camera,
                new Vector2(100f, 0f),
                cameraHeight: 20f);

            Assert.That(delta.x, Is.LessThan(0f));
            Assert.That(Mathf.Abs(delta.z), Is.LessThan(0.001f));

            Object.DestroyImmediate(camera.gameObject);
        }

        [Test]
        public void ScreenDeltaToWorldDelta_UsesCameraPixelRectNotFullScreen()
        {
            var camera = new GameObject("pan-cam-rect").AddComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = 60f;
            camera.transform.position = new Vector3(0f, 20f, 0f);
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            camera.rect = new Rect(0f, 0f, 0.5f, 0.5f);
            var halfViewport = MapPanMath.ScreenDeltaToWorldDelta(
                camera,
                new Vector2(100f, 0f),
                cameraHeight: 20f);
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            var fullViewport = MapPanMath.ScreenDeltaToWorldDelta(
                camera,
                new Vector2(100f, 0f),
                cameraHeight: 20f);

            // Same pixel delta on a larger pixelRect covers less of the FOV, so world delta shrinks.
            Assert.That(Mathf.Abs(fullViewport.x), Is.LessThan(Mathf.Abs(halfViewport.x)));

            Object.DestroyImmediate(camera.gameObject);
        }
    }
}
