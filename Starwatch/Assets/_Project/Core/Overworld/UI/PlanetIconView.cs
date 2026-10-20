using System;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// One planet in the star-system scene: lit glyph, selection reticle, and name plate.
    /// Positioned by <see cref="StarSystemView"/> on its orbit.
    /// </summary>
    internal sealed class PlanetIconView
    {
        public event Action<ContentId> Clicked;

        internal VisualElement Root { get; }

        internal ContentId PlanetId => _planetId;

        internal float Size { get; }

        /// <summary>
        /// Scene-local Y of the planet center. Larger values sit in front.
        /// </summary>
        internal float Depth { get; private set; }

        /// <summary>
        /// How far the shadow disc is pushed off the glyph, as a fraction of its size.
        /// </summary>
        private const float kShadeOffset = 0.38f;

        private const float kReticleOutset = 10f;

        private readonly ContentId _planetId;
        private readonly Button _glyph;
        private readonly VisualElement _shade;
        private readonly Label _name;
        private readonly Label _tag;

        public PlanetIconView(ContentId planetId)
        {
            _planetId = planetId;
            Size = SystemOrbitLayout.PlanetSize(planetId);

            Root = new VisualElement();
            Root.name = "planet-" + (planetId.IsEmpty ? "unknown" : planetId.Value);
            Root.AddToClassList("planet-icon");
            Root.AddToClassList("planet-icon--hue-" + SystemOrbitLayout.HueVariant(planetId));
            Root.style.width = Size;
            Root.style.height = Size;
            Root.pickingMode = PickingMode.Ignore;

            var reticle = new VisualElement { pickingMode = PickingMode.Ignore };
            reticle.AddToClassList("planet-icon__reticle");
            var reticleSize = Size + (kReticleOutset * 2f);
            reticle.style.left = -kReticleOutset;
            reticle.style.top = -kReticleOutset;
            reticle.style.width = reticleSize;
            reticle.style.height = reticleSize;
            SetRadius(reticle, reticleSize * 0.5f);
            Root.Add(reticle);

            _glyph = new Button();
            _glyph.AddToClassList("planet-icon__glyph");
            SetRadius(_glyph, Size * 0.5f);
            _glyph.clicked += () => Clicked?.Invoke(_planetId);
            Root.Add(_glyph);

            // The button stays transparent so theme hover tints never cover the planet color.
            var body = new VisualElement { pickingMode = PickingMode.Ignore };
            body.AddToClassList("planet-icon__body");
            SetRadius(body, Size * 0.5f);
            _glyph.Add(body);

            _shade = new VisualElement { pickingMode = PickingMode.Ignore };
            _shade.AddToClassList("planet-icon__shade");
            SetRadius(_shade, Size * 0.5f);
            body.Add(_shade);

            var unknownMark = new Label("?") { pickingMode = PickingMode.Ignore };
            unknownMark.AddToClassList("planet-icon__unknown");
            body.Add(unknownMark);

            var plate = new VisualElement { pickingMode = PickingMode.Ignore };
            plate.AddToClassList("planet-icon__plate");
            Root.Add(plate);

            _name = new Label { pickingMode = PickingMode.Ignore };
            _name.AddToClassList("planet-icon__label");
            plate.Add(_name);

            _tag = new Label { pickingMode = PickingMode.Ignore };
            _tag.AddToClassList("planet-icon__tag");
            plate.Add(_tag);
        }

        /// <summary>
        /// Centers the glyph on <paramref name="center"/> and turns its shadow away from the star.
        /// </summary>
        public void PlaceAt(Vector2 center, Vector2 awayFromStar)
        {
            var half = Size * 0.5f;
            Root.style.left = center.x - half;
            Root.style.top = center.y - half;
            Depth = center.y;

            var offset = awayFromStar * (Size * kShadeOffset);
            _shade.style.translate = new Translate(offset.x, offset.y);
        }

        /// <summary>
        /// Applies scan / selection styling and name plate text.
        /// </summary>
        public void ApplyState(PlanetPresentationState state)
        {
            var scanned = state.Visibility == PlanetVisibility.Scanned;
            Root.EnableInClassList("planet-icon--scanned", scanned);
            Root.EnableInClassList("planet-icon--unscanned", !scanned);
            Root.EnableInClassList("planet-icon--selected", state.IsSelected);
            _glyph.EnableInClassList("planet-icon__glyph--scanned", scanned);
            _glyph.EnableInClassList("planet-icon__glyph--selected", state.IsSelected);

            _name.text = string.IsNullOrEmpty(state.DisplayName)
                ? _planetId.Value
                : state.DisplayName;
            _tag.text = scanned ? "SURVEYED" : "UNSCANNED";
        }

        private static void SetRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }
    }
}
