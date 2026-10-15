using EndlessHeros.Core.Identifiers;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Side-panel detail for the selected planet, including scan action.
    /// </summary>
    internal sealed class PlanetInspectionView
    {
        public event System.Action<ContentId> ScanClicked;

        private VisualElement _root;
        private Label _name;
        private Label _detail;
        private Button _scanButton;
        private ContentId _planetId;

        /// <summary>
        /// Binds the PlanetInspection.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _name = root.Q<Label>("planet-detail-name");
            _detail = root.Q<Label>("planet-detail-summary");
            _scanButton = root.Q<Button>("scan-button");
            if (_scanButton != null)
            {
                _scanButton.clicked += () =>
                {
                    if (!_planetId.IsEmpty)
                    {
                        ScanClicked?.Invoke(_planetId);
                    }
                };
            }

            Clear();
        }

        /// <summary>
        /// Hides planet detail.
        /// </summary>
        public void Clear()
        {
            _planetId = default;
            if (_name != null)
            {
                _name.text = string.Empty;
            }

            if (_detail != null)
            {
                _detail.text = string.Empty;
            }

            if (_scanButton != null)
            {
                _scanButton.SetEnabled(false);
            }

            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Shows detail for the selected planet.
        /// </summary>
        public void ShowPlanet(PlanetDetailModel model)
        {
            if (_root == null || model.PlanetId.IsEmpty)
            {
                Clear();
                return;
            }

            _planetId = model.PlanetId;
            if (_name != null)
            {
                _name.text = model.DisplayName;
            }

            if (_detail != null)
            {
                _detail.text = model.Detail;
            }

            if (_scanButton != null)
            {
                _scanButton.SetEnabled(model.CanScan);
            }

            _root.style.display = DisplayStyle.Flex;
        }
    }

    /// <summary>
    /// Presentation model for the selected planet in the inspection panel.
    /// </summary>
    internal readonly struct PlanetDetailModel
    {
        public ContentId PlanetId { get; }

        public string DisplayName { get; }

        public string Detail { get; }

        public bool CanScan { get; }

        public PlanetDetailModel(ContentId planetId, string displayName, string detail, bool canScan)
        {
            PlanetId = planetId;
            DisplayName = displayName ?? string.Empty;
            Detail = detail ?? string.Empty;
            CanScan = canScan;
        }
    }
}
