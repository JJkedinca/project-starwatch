using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Research;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Research offers, the active project, and completed research. The stockpile button opens and closes it. One authored button per <see cref="ResearchOfferSelector.kSlotCount"/>.
    /// </summary>
    internal sealed class ResearchPanelView : IResearchView
    {
        public event Action<ContentId> StartRequested;

        public event Action<ContentId> SwitchConfirmed;

        public event Action Opened;

        public bool IsOpen => _open;

        private readonly Button[] _offerButtons = new Button[ResearchOfferSelector.kSlotCount];
        private readonly ContentId[] _offerIds = new ContentId[ResearchOfferSelector.kSlotCount];
        private readonly string[] _offerEffects = new string[ResearchOfferSelector.kSlotCount];
        private VisualElement _panel;
        private VisualElement _confirm;
        private Button _entry;
        private Label _active;
        private Label _dropped;
        private Label _completed;
        private Label _hover;
        private ContentId _pendingSwitch;
        private string _activeEffect = string.Empty;
        private string _completedEffect = string.Empty;
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
            _hover = panel.Q<Label>("research-hover");
            _confirm = panel.Q<VisualElement>("research-confirm");
            var confirmYes = panel.Q<Button>("research-confirm-yes");
            var confirmNo = panel.Q<Button>("research-confirm-no");
            if (confirmYes != null)
            {
                confirmYes.clicked += ConfirmSwitch;
            }

            if (confirmNo != null)
            {
                confirmNo.clicked += HideConfirm;
            }

            WatchHover(_active);
            WatchHover(_completed);
            panel.RegisterCallback<PointerLeaveEvent>(_ => HideHover());
            SetOpen(false);
            HideConfirm();
            Apply(ResearchPanelModel.Empty);
        }

        public void DismissSwitch()
        {
            HideConfirm();
        }

        public void PromptSwitch(ContentId researchId)
        {
            _pendingSwitch = researchId;
            if (_confirm != null)
            {
                _confirm.style.display = DisplayStyle.Flex;
            }
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
            _activeEffect = model.HasActive ? model.ActiveEffectText : string.Empty;
            Set(_dropped, model.ScienceDropped ? "Science is being dropped. Start a project to use it." : string.Empty);
            Set(_completed, CompletedText(model));
            _completedEffect = model.CompletedTooltip ?? string.Empty;
        }

        private void Toggle()
        {
            SetOpen(!_open);
            if (_open)
            {
                Opened?.Invoke();
            }
            else
            {
                HideHover();
                HideConfirm();
            }
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
            WatchHover(button);
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
                _offerEffects[i] = shown ? offer.EffectText : string.Empty;
                button.SetEnabled(shown);
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

        private void WatchHover(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            element.RegisterCallback<PointerEnterEvent>(_ => ShowHover(EffectFor(element)));
        }

        private string EffectFor(VisualElement element)
        {
            if (element == _active)
            {
                return _activeEffect;
            }

            if (element == _completed)
            {
                return _completedEffect;
            }

            for (var i = 0; i < _offerButtons.Length; i++)
            {
                if (element == _offerButtons[i])
                {
                    return _offerEffects[i];
                }
            }

            return string.Empty;
        }

        private void ShowHover(string text)
        {
            if (_hover == null)
            {
                return;
            }

            _hover.text = text ?? string.Empty;
            _hover.style.display = string.IsNullOrEmpty(_hover.text) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void HideHover()
        {
            if (_hover != null)
            {
                _hover.style.display = DisplayStyle.None;
            }
        }

        private void ConfirmSwitch()
        {
            var researchId = _pendingSwitch;
            HideConfirm();
            if (!researchId.IsEmpty)
            {
                SwitchConfirmed?.Invoke(researchId);
            }
        }

        private void HideConfirm()
        {
            _pendingSwitch = default;
            if (_confirm != null)
            {
                _confirm.style.display = DisplayStyle.None;
            }
        }
    }
}
