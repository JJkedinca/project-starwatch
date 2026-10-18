using System;
using System.Collections.Generic;
using EndlessHeros.Core.Identifiers;
using EndlessHeros.Research;
using UnityEngine.UIElements;

namespace EndlessHeros.Overworld.UI
{
    /// <summary>
    /// Research offers, the active project, and completed research. The stockpile button opens and closes it. One authored button per <see cref="ResearchOfferSelector.kSlotCount"/>.
    /// </summary>
    internal sealed class ResearchPanelView : IResearchView
    {
        public event Action<ContentId> StartRequested;

        private readonly Button[] _offerButtons = new Button[ResearchOfferSelector.kSlotCount];
        private readonly ContentId[] _offerIds = new ContentId[ResearchOfferSelector.kSlotCount];
        private VisualElement _panel;
        private Button _entry;
        private Label _active;
        private Label _dropped;
        private Label _completed;
        private bool _open;

        public void Bind(VisualElement panel, Button entry)
        {
            if (panel == null)
            {
                return;
            }

            _panel = panel;
            _entry = entry;
            if (_entry != null)
            {
                _entry.clicked += Toggle;
            }

            for (var i = 0; i < ResearchOfferSelector.kSlotCount; i++)
            {
                BindOffer(panel.Q<Button>($"research-offer-{i}"), i);
            }

            _active = panel.Q<Label>("research-active");
            _dropped = panel.Q<Label>("research-dropped");
            _completed = panel.Q<Label>("research-completed");
            SetOpen(false);
            Apply(ResearchPanelModel.Empty);
        }

        public void Apply(ResearchPanelModel model)
        {
            model ??= ResearchPanelModel.Empty;
            if (_entry != null)
            {
                _entry.text = model.HasActive ? "Researching" : "no Active Research";
            }

            ApplyOffers(model.Offers);
            Set(_active, ActiveText(model));
            Set(_dropped, model.ScienceDropped ? "Science is being dropped. Start a project to use it." : string.Empty);
            Set(_completed, CompletedText(model));
        }

        private void Toggle()
        {
            SetOpen(!_open);
        }

        private void SetOpen(bool open)
        {
            _open = open;
            if (_panel != null)
            {
                _panel.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void BindOffer(Button button, int slot)
        {
            _offerButtons[slot] = button;
            if (button == null)
            {
                return;
            }

            button.clicked += () => OnOfferClicked(slot);
        }

        private void OnOfferClicked(int slot)
        {
            var researchId = _offerIds[slot];
            if (!researchId.IsEmpty)
            {
                StartRequested?.Invoke(researchId);
            }
        }

        private void ApplyOffers(IReadOnlyList<ResearchOfferModel> offers)
        {
            for (var i = 0; i < ResearchOfferSelector.kSlotCount; i++)
            {
                var button = _offerButtons[i];
                var offer = offers != null && i < offers.Count ? offers[i] : null;
                var shown = offer != null && !offer.Id.IsEmpty;
                _offerIds[i] = shown ? offer.Id : default;
                if (button == null)
                {
                    continue;
                }

                button.text = shown ? offer.Name : string.Empty;
                button.SetEnabled(shown && offer.CanStart);
                button.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static string ActiveText(ResearchPanelModel model)
        {
            if (!model.HasActive)
            {
                return "No active project.";
            }

            return $"{model.ActiveName} {model.Progress:0.#}/{model.Cost:0.#} at {model.SciencePerSecond:0.#} Science/s";
        }

        private static string CompletedText(ResearchPanelModel model)
        {
            if (model.CompletedNames == null || model.CompletedNames.Count == 0)
            {
                return string.Empty;
            }

            var text = model.CompletedNames[0] ?? string.Empty;
            for (var i = 1; i < model.CompletedNames.Count; i++)
            {
                text += "\n" + (model.CompletedNames[i] ?? string.Empty);
            }

            return text;
        }

        private static void Set(Label label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }
    }
}
