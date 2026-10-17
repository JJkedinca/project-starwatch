using System;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Found-outpost, found-colony, and abandon actions for the selected planet.
    /// </summary>
    internal sealed class SettlementCommitmentView
    {
        public event Action FoundOutpostClicked;

        public event Action FoundColonyClicked;

        public event Action AbandonClicked;

        private VisualElement _root;
        private Button _outpostButton;
        private Button _colonyButton;
        private Button _abandonButton;

        /// <summary>
        /// Binds the SettlementCommitment.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _outpostButton = root.Q<Button>("found-outpost-button");
            _colonyButton = root.Q<Button>("found-colony-button");
            _abandonButton = root.Q<Button>("abandon-button");
            if (_outpostButton != null)
            {
                _outpostButton.clicked += () => FoundOutpostClicked?.Invoke();
            }

            if (_colonyButton != null)
            {
                _colonyButton.clicked += () => FoundColonyClicked?.Invoke();
            }

            if (_abandonButton != null)
            {
                _abandonButton.clicked += () => AbandonClicked?.Invoke();
            }

            Hide();
        }

        /// <summary>
        /// Hides commitment actions.
        /// </summary>
        public void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        /// <summary>
        /// Shows the costs and which commitments are available.
        /// </summary>
        public void Show(SettlementCommitmentModel model)
        {
            if (_root == null)
            {
                return;
            }

            Set(_outpostButton, model.OutpostLabel, model.CanFoundOutpost);
            Set(_colonyButton, model.ColonyLabel, model.CanFoundColony);
            if (_abandonButton != null)
            {
                _abandonButton.SetEnabled(model.CanAbandon);
            }

            _root.style.display = DisplayStyle.Flex;
        }

        private static void Set(Button button, string label, bool enabled)
        {
            if (button == null)
            {
                return;
            }

            button.text = label ?? string.Empty;
            button.SetEnabled(enabled);
        }
    }
}
