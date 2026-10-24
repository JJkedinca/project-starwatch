using ProjectStarwatch.Overworld.Map;
using NUnit.Framework;
using UnityEngine;

namespace ProjectStarwatch.Tests.EditMode.Overworld.Map
{
    /// <summary>
    /// EditMode coverage for right-panel inset camera framing.
    /// </summary>
    public sealed class SystemViewportFramingTests
    {
        [Test]
        public void MeasureRightPanelInset_NullPanel_IsZero()
        {
            Assert.That(SystemViewportFraming.MeasureRightPanelInsetNormalized(null), Is.EqualTo(0f));
        }

        [Test]
        public void CameraFocus_ZeroInset_CentersOnStar()
        {
            var camera = CreateTopDownCamera();
            try
            {
                var star = new Vector3(3f, 0f, -2f);
                var focus = SystemViewportFraming.CameraFocusForUncoveredCenter(
                    camera,
                    star,
                    cameraHeight: 20f,
                    rightPanelInsetNormalized: 0f);

                Assert.That(focus.x, Is.EqualTo(star.x).Within(0.001f));
                Assert.That(focus.y, Is.EqualTo(20f).Within(0.001f));
                Assert.That(focus.z, Is.EqualTo(star.z).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }

        [Test]
        public void CameraFocus_RightInset_ShiftsCameraTowardPanel()
        {
            var camera = CreateTopDownCamera();
            try
            {
                var star = Vector3.zero;
                var focus = SystemViewportFraming.CameraFocusForUncoveredCenter(
                    camera,
                    star,
                    cameraHeight: 20f,
                    rightPanelInsetNormalized: 0.32f);

                // Star should appear left of screen center; camera moves right (+X for this orientation).
                Assert.That(focus.x, Is.GreaterThan(star.x));
                Assert.That(focus.y, Is.EqualTo(20f).Within(0.001f));
                Assert.That(focus.z, Is.EqualTo(star.z).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }

        private static Camera CreateTopDownCamera()
        {
            var go = new GameObject("TestMapCamera");
            var camera = go.AddComponent<Camera>();
            camera.orthographic = false;
            camera.transform.position = new Vector3(0f, 20f, 0f);
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            camera.fieldOfView = 60f;
            return camera;
        }
    }
}
