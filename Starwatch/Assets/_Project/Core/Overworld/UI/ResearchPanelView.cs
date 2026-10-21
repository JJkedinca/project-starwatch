using System;
using System.Collections.Generic;
using ProjectStarwatch.Core.Identifiers;
using ProjectStarwatch.Research;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Research offers in the right column. The stockpile entry toggles it over the target / settlement.
    /// </summary>
    internal sealed class ResearchPanelView : IResearchView
    {
        public event Action<ContentId> StartRequested;

        public event Action<ContentId> SwitchConfirmed;

        public event Action ToggleRequested;

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
                _entry.clicked += () => ToggleRequested?.Invoke();
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
            var close = panel.Q<Button>("research-close");
            if (close != null)
            {
                close.clicked += () => ToggleRequested?.Invoke();
            }

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

        public void SetOpen(bool open)
        {
            _open = open;
            if (_panel != null)
            {
                _panel.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!open)
            {
                HideHover();
                HideConfirm();
            }
        }

        public void Apply(ResearchPanelModel model)
        {
            model ??= ResearchPanelModel.Empty;
            if (_entry != null)
            {
                _entry.text = model.HasActive ? "Researching" : "No Active Research";
            }

            ApplyOffers(model.Offers);
            Set(_active, ActiveText(model));
            _activeEffect = model.HasActive ? model.ActiveEffectText : string.Empty;
            Set(_dropped, model.ScienceDropped ? "Science is being dropped. Start a project to use it." : string.Empty);
            Set(_completed, CompletedText(model));
            _completedEffect = model.CompletedTooltip ?? string.Empty;
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

                if (shown)
                {
                    var eta = offer.EstimatedSeconds > 0f
                        ? " · ~" + offer.EstimatedSeconds.ToString("0.#") + "s"
                        : string.Empty;
                    button.text = offer.Domain + "\n" + offer.Name + "\n⚗ " + offer.Cost.ToString("0.#") + eta;
                    button.tooltip = string.IsNullOrEmpty(offer.UnavailableReason)
                        ? offer.EffectText
                        : offer.UnavailableReason;
                }
                else
                {
                    button.text = string.Empty;
                    button.tooltip = string.Empty;
                }

                _offerEffects[i] = shown ? offer.EffectText : string.Empty;
                // Keep shown cards clickable so an active project can still be switched.
                button.SetEnabled(shown);
                var trulyUnavailable = shown && !offer.CanStart && !string.IsNullOrEmpty(offer.UnavailableReason);
                button.EnableInClassList("research-offer--dim", trulyUnavailable);
                button.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static string ActiveText(ResearchPanelModel model)
        {
            if (!model.HasActive)
            {
                return "No active project.";
            }

            var pct = (model.ProgressFraction * 100f).ToString("0");
            var eta = model.EstimatedSecondsLeft > 0f
                ? " · done in " + model.EstimatedSecondsLeft.ToString("0.#") + "s"
                : string.Empty;
            return $"{model.ActiveName}  {pct}%{eta}  ({model.Progress:0.#}/{model.Cost:0.#} · {model.SciencePerSecond:0.#}/s)";
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
