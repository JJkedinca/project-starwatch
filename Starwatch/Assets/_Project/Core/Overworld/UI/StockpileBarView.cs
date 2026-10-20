using System;
using System.Collections.Generic;
using System.Globalization;
using ProjectStarwatch.Core.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectStarwatch.Overworld.UI
{
    /// <summary>
    /// Top bar for empire stock, net rates, and the research entry.
    /// </summary>
    internal sealed class StockpileBarView : IStockpileView
    {
        private static readonly ResourceKind[] kKinds =
        {
            ResourceKind.Industry,
            ResourceKind.Energy,
            ResourceKind.Strategic,
        };

        private readonly Dictionary<ResourceKind, Readout> _readouts = new();
        private readonly Dictionary<ResourceKind, int> _previousAmounts = new();
        private VisualElement _root;
        private VisualElement _tooltip;
        private Label _scienceRate;
        private VisualElement _researchFill;
        private Button _researchEntry;
        private StockpileModel _model = StockpileModel.Empty;
        private ResourceKind? _hoveredKind;
        private bool _amountsPrimed;

        /// <summary>
        /// Binds the StockpileBar.uxml root.
        /// </summary>
        public void Bind(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            _root = root;
            _tooltip = root.Q<VisualElement>("stockpile-tooltip");
            _scienceRate = root.Q<Label>("stockpile-science-rate");
            _researchFill = root.Q<VisualElement>("stockpile-research-progress-fill");
            _researchEntry = root.Q<Button>("stockpile-research");
            BindReadout(root.Q<VisualElement>("stockpile-industry-host"), ResourceKind.Industry, "INDUSTRY");
            BindReadout(root.Q<VisualElement>("stockpile-energy-host"), ResourceKind.Energy, "ENERGY");
            BindReadout(root.Q<VisualElement>("stockpile-strategic-host"), ResourceKind.Strategic, "STRATEGIC");
        }

        /// <summary>
        /// Shows stock, cycle rates, warning chips, and the research segment.
        /// </summary>
        public void Apply(StockpileModel model)
        {
            _model = model ?? StockpileModel.Empty;
            var animateGains = _amountsPrimed;
            for (var i = 0; i < kKinds.Length; i++)
            {
                var kind = kKinds[i];
                if (!_readouts.TryGetValue(kind, out var readout))
                {
                    continue;
                }

                var entry = _model.Get(kind);
                var previous = _previousAmounts.TryGetValue(kind, out var prior) ? prior : entry.Amount;
                ApplyReadout(readout, entry, _model.CycleSeconds);
                if (animateGains && entry.Amount > previous)
                {
                    SpawnFloater(readout.Root, entry.Amount - previous);
                }

                _previousAmounts[kind] = entry.Amount;
            }

            _amountsPrimed = true;
            ApplyResearch(_model);
            if (_hoveredKind.HasValue)
            {
                ShowTooltip(_hoveredKind.Value);
            }
        }

        private void BindReadout(VisualElement host, ResourceKind kind, string caption)
        {
            if (host == null)
            {
                return;
            }

            var item = host.Q<VisualElement>("stockpile-item") ?? host;
            var readout = new Readout(
                item,
                item.Q<Label>("stockpile-caption"),
                item.Q<Label>("stockpile-amount"),
                item.Q<Label>("stockpile-rate"),
                item.Q<Label>("stockpile-chip"));
            if (readout.Caption != null)
            {
                readout.Caption.text = caption;
            }

            item.RegisterCallback<PointerEnterEvent>(_ =>
            {
                _hoveredKind = kind;
                ShowTooltip(kind);
            });
            item.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                if (_hoveredKind == kind)
                {
                    _hoveredKind = null;
                    HideTooltip();
                }
            });
            _readouts[kind] = readout;
        }

        private void ApplyReadout(Readout readout, StockpileEntry entry, int cycleSeconds)
        {
            if (readout.Amount != null)
            {
                readout.Amount.text = entry.Amount.ToString("N0", CultureInfo.InvariantCulture);
            }

            if (readout.Rate != null)
            {
                readout.Rate.ClearClassList();
                readout.Rate.AddToClassList("stockpile-rate");
                switch (entry.State)
                {
                    case StockpileState.RunningOut:
                        readout.Rate.text = FormatEmptyIn(entry.SecondsUntilEmpty);
                        readout.Rate.AddToClassList("hud-rate--warn");
                        break;
                    case StockpileState.Stalled:
                        readout.Rate.text = FormatCycleRate(entry.NetPerSecond, cycleSeconds);
                        readout.Rate.AddToClassList("hud-rate--negative");
                        break;
                    case StockpileState.Gaining:
                        readout.Rate.text = FormatCycleRate(entry.NetPerSecond, cycleSeconds);
                        readout.Rate.AddToClassList("hud-rate--positive");
                        break;
                    case StockpileState.Draining:
                        readout.Rate.text = FormatCycleRate(entry.NetPerSecond, cycleSeconds) + " ▼";
                        readout.Rate.AddToClassList("hud-rate--negative");
                        break;
                    default:
                        readout.Rate.text = "±0";
                        readout.Rate.AddToClassList("hud-rate--zero");
                        break;
                }
            }

            if (readout.Chip != null)
            {
                var stalled = entry.State == StockpileState.Stalled;
                readout.Chip.style.display = stalled ? DisplayStyle.Flex : DisplayStyle.None;
                readout.Chip.text = stalled ? "STALLED" : string.Empty;
            }
        }

        private void ApplyResearch(StockpileModel model)
        {
            var research = model.Research;
            if (_scienceRate != null)
            {
                var perCycle = research.SciencePerSecond * model.CycleSeconds;
                _scienceRate.text = perCycle > 0f
                    ? "+" + FormatNumber(perCycle)
                    : string.Empty;
            }

            if (_researchFill != null)
            {
                _researchFill.style.width = Length.Percent(research.ProgressFraction * 100f);
            }

            if (_researchEntry != null)
            {
                _researchEntry.text = research.HasActive && !string.IsNullOrEmpty(research.ActiveName)
                    ? research.ActiveName
                    : "No Active Research";
            }
        }

        private void ShowTooltip(ResourceKind kind)
        {
            if (_tooltip == null)
            {
                return;
            }

            _tooltip.Clear();
            var entry = _model.Get(kind);
            var cycle = _model.CycleSeconds;
            AddTooltipLine(
                "Gross +" + FormatNumber(entry.GrossPerSecond * cycle)
                + " / cycle · " + FormatNumber(entry.GrossPerSecond) + "/s",
                muted: false);
            AddTooltipLine(
                "Upkeep −" + FormatNumber(entry.UpkeepPerSecond * cycle)
                + " / cycle · " + FormatNumber(entry.UpkeepPerSecond) + "/s",
                muted: true);
            AddTooltipLine(
                "Net " + FormatSigned(entry.NetPerSecond * cycle)
                + " / cycle · " + FormatSigned(entry.NetPerSecond) + "/s",
                muted: false);

            for (var i = 0; i < _model.ColonyLines.Count; i++)
            {
                var line = _model.ColonyLines[i];
                var outAmount = line.Output.Get(kind);
                var upAmount = line.Upkeep.Get(kind);
                if (outAmount > 0f)
                {
                    AddTooltipLine(
                        "+" + FormatNumber(outAmount * cycle) + " " + line.PlanetName,
                        muted: true);
                }

                if (upAmount > 0f)
                {
                    AddTooltipLine(
                        "−" + FormatNumber(upAmount * cycle) + " " + line.PlanetName + " upkeep",
                        muted: true);
                }
            }

            if (_model.IsEmpireStalled && entry.State == StockpileState.Stalled)
            {
                AddTooltipLine("Unpaid upkeep stops every colony's output.", muted: false);
            }

            if (_readouts.TryGetValue(kind, out var readout) && _root != null)
            {
                var local = readout.Root.ChangeCoordinatesTo(_root, Vector2.zero);
                _tooltip.style.left = local.x;
            }

            _tooltip.style.display = DisplayStyle.Flex;
        }

        private void AddTooltipLine(string text, bool muted)
        {
            var line = new Label(text);
            line.AddToClassList(muted ? "stockpile-tooltip__muted" : "stockpile-tooltip__line");
            _tooltip.Add(line);
        }

        private void HideTooltip()
        {
            if (_tooltip == null)
            {
                return;
            }

            _tooltip.style.display = DisplayStyle.None;
            _tooltip.Clear();
        }

        private static void SpawnFloater(VisualElement host, int gain)
        {
            if (host == null || gain <= 0)
            {
                return;
            }

            var floater = new Label("+" + gain.ToString(CultureInfo.InvariantCulture));
            floater.AddToClassList("stockpile-floater");
            host.Add(floater);
            host.schedule.Execute(() => floater.AddToClassList("stockpile-floater--fade")).StartingIn(16);
            host.schedule.Execute(() => floater.RemoveFromHierarchy()).StartingIn(820);
        }

        private static string FormatCycleRate(float perSecond, int cycleSeconds)
        {
            var perCycle = perSecond * cycleSeconds;
            if (Math.Abs(perCycle) < 0.05f)
            {
                return "±0";
            }

            return FormatSigned(perCycle);
        }

        private static string FormatEmptyIn(float seconds)
        {
            if (seconds <= 0f || float.IsInfinity(seconds) || float.IsNaN(seconds))
            {
                return "Empty";
            }

            var total = (int)Math.Ceiling(seconds);
            var minutes = total / 60;
            var secs = total % 60;
            return "Empty in " + minutes.ToString(CultureInfo.InvariantCulture)
                + ":" + secs.ToString("00", CultureInfo.InvariantCulture);
        }

        private static string FormatSigned(float value)
        {
            if (value > 0f)
            {
                return "+" + FormatNumber(value);
            }

            if (value < 0f)
            {
                return "−" + FormatNumber(-value);
            }

            return "±0";
        }

        private static string FormatNumber(float value)
        {
            if (Math.Abs(value - MathF.Round(value)) < 0.05f)
            {
                return MathF.Round(value).ToString("N0", CultureInfo.InvariantCulture);
            }

            return value.ToString("0.#", CultureInfo.InvariantCulture);
        }

        private readonly struct Readout
        {
            public VisualElement Root { get; }

            public Label Caption { get; }

            public Label Amount { get; }

            public Label Rate { get; }

            public Label Chip { get; }

            public Readout(VisualElement root, Label caption, Label amount, Label rate, Label chip)
            {
                Root = root;
                Caption = caption;
                Amount = amount;
                Rate = rate;
                Chip = chip;
            }
        }
    }
}
