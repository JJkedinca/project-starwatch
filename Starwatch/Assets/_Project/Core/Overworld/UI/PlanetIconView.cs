using System;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Overworld;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Flat planet icon button for the star-system planet strip.
    /// </summary>
    internal sealed class PlanetIconView
    {
        public event Action<ContentId> Clicked;

        internal VisualElement Root { get; }

        private readonly ContentId _planetId;
        private readonly Button _icon;
        private readonly Label _label;

        public PlanetIconView(ContentId planetId)
        {
            _planetId = planetId;

            Root = new VisualElement();
            Root.name = "planet-" + (planetId.IsEmpty ? "unknown" : planetId.Value);
            Root.AddToClassList("planet-icon");

            _icon = new Button();
            _icon.AddToClassList("planet-icon__glyph");
            _icon.clicked += () => Clicked?.Invoke(_planetId);
            Root.Add(_icon);

            _label = new Label();
            _label.AddToClassList("planet-icon__label");
            Root.Add(_label);
        }

        /// <summary>
        /// Applies scan / selection styling and label text.
        /// </summary>
        public void ApplyState(PlanetPresentationState state)
        {
            _icon.EnableInClassList("planet-icon__glyph--scanned", state.Visibility == PlanetVisibility.Scanned);
            _icon.EnableInClassList("planet-icon__glyph--selected", state.IsSelected);
            Root.EnableInClassList("planet-icon--selected", state.IsSelected);

            _label.text = string.IsNullOrEmpty(state.DisplayName)
                ? _planetId.Value
                : state.DisplayName;
        }
    }
}
