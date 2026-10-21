using System;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// One commitment card: Outpost / Colony choice and Commit.
    /// </summary>
    internal sealed class SettlementCommitmentView
    {
        public event Action FoundOutpostClicked;

        public event Action FoundColonyClicked;

        private VisualElement _root;
        private Button _outpostKind;
        private Button _colonyKind;
        private Button _commit;
        private Label _cost;
        private Label _escalation;
        private Label _warning;
        private SettlementCommitmentModel _model;
        private bool _preferColony;

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
            _outpostKind = root.Q<Button>("kind-outpost-button");
            _colonyKind = root.Q<Button>("kind-colony-button");
            _commit = root.Q<Button>("commit-button");
            _cost = root.Q<Label>("commitment-cost");
            _escalation = root.Q<Label>("commitment-escalation");
            _warning = root.Q<Label>("commitment-warning");
            if (_outpostKind != null)
            {
                _outpostKind.clicked += () =>
                {
                    _preferColony = false;
                    RefreshSelection();
                };
            }

            if (_colonyKind != null)
            {
                _colonyKind.clicked += () =>
                {
                    _preferColony = true;
                    RefreshSelection();
                };
            }

            if (_commit != null)
            {
                _commit.clicked += OnCommit;
            }

            Hide();
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }

        public void Show(SettlementCommitmentModel model)
        {
            if (_root == null)
            {
                return;
            }

            _model = model;
            if (!model.ShowCard)
            {
                Hide();
                return;
            }

            if (_preferColony && !model.CanFoundColony && model.CanFoundOutpost)
            {
                _preferColony = false;
            }
            else if (!_preferColony && !model.CanFoundOutpost && model.CanFoundColony)
            {
                _preferColony = true;
            }

            if (_escalation != null)
            {
                _escalation.text = model.EscalationNote;
                _escalation.style.display = string.IsNullOrEmpty(model.EscalationNote)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }

            if (_warning != null)
            {
                _warning.text = model.Warning;
                _warning.style.display = string.IsNullOrEmpty(model.Warning)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }

            RefreshSelection();
            _root.style.display = DisplayStyle.Flex;
        }

        private void OnCommit()
        {
            if (_preferColony)
            {
                FoundColonyClicked?.Invoke();
                return;
            }

            FoundOutpostClicked?.Invoke();
        }

        private void RefreshSelection()
        {
            var colony = _preferColony;
            if (_outpostKind != null)
            {
                _outpostKind.SetEnabled(_model.CanFoundOutpost || !_model.CanFoundColony);
                _outpostKind.EnableInClassList("settlement-commitment__kind--selected", !colony);
            }

            if (_colonyKind != null)
            {
                _colonyKind.SetEnabled(_model.CanFoundColony || !_model.CanFoundOutpost);
                _colonyKind.EnableInClassList("settlement-commitment__kind--selected", colony);
            }

            if (_cost != null)
            {
                _cost.text = colony ? _model.ColonyCostLabel : _model.OutpostCostLabel;
            }

            if (_commit != null)
            {
                var can = colony ? _model.CanFoundColony : _model.CanFoundOutpost;
                _commit.SetEnabled(can);
            }
        }
    }
}
