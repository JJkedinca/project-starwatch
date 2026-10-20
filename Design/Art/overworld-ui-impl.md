# Overworld UI/UX Implementation

## Purpose

This document turns the overworld UI redesign into concrete work. It covers three areas:

- **Star system view:** the orbital scene shown when a system is open.
- **Fleets:** the roster, the selected-fleet bar, fleet details, and the parts of the star map that show fleet movement.
- **Economy:** the resource bar and how stock and production rates are shown.

The star map restyle (node glyphs, backdrop, pan and zoom) is only covered where fleets depend on it.

[Additional Screens](#additional-screens) holds lighter sketches for the settlement panel, research, status messages, and the main menu. Those are design direction, not yet broken into full tasks. Rewind, time controls, and the battle HUD are out of scope.

Related docs:

- [planets.md](Design/gameplay/planets.md)
- [star-systems.md](Design/gameplay/star-systems.md)
- [economy-and-building.md](Design/gameplay/economy-and-building.md)
- [fleet-combat.md](Design/gameplay/fleet-combat.md)
- [initial-generation-impl.md](initial-generation-impl.md) for planet archetypes and star types

## Goals

- The overworld reads like a game HUD, not a form. Information lives where the player is looking.
- The player always knows two things at once: **which fleet will act** and **what it is looking at**.
- Stock and rate are both visible for every stockpiled resource. Problems (deficit, stalled economy) are impossible to miss.
- No new per-frame ticking. Everything refreshes from domain events. Motion uses USS transitions.
- Presenters keep their current intents. Most work is in views, USS, and query/read models.

## Interaction Rules

These rules are fixed for this plan:

- **Left-click** a fleet selects it. Left-click a node selects it, or enters it when it can be entered (current behavior).
- **Right-click** a node sends the selected fleet there. There is no "Plot course" or "Travel" button.
- Fleet selection and node selection are independent and both persist, as `OverworldSelection` already does. The fleet is the actor; the node is the target.
- With no fleet selected, right-click posts a hint asking the player to select a fleet (current behavior).

## Screen Layout

The overworld is reorganized into fixed regions so fleet, target, and economy information never compete for the same space.

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ TOP BAR: resources (stock + rate) │ research segment │ status toasts appear below │
├───────────────┬──────────────────────────────────────────────┬───────────────────┤
│ FLEET ROSTER  │                                              │ RIGHT COLUMN      │
│ all fleets    │        STAR MAP  or  STAR SYSTEM SCENE        │ map: target card  │
│ + status      │                                              │ system: planet /  │
│               │                                              │   settlement      │
│               ├──────────────────────────────────────────────┤ or fleet details  │
│               │ SELECTED FLEET BAR (only when a fleet is      │                   │
│               │ selected; same on map and in a system)        │                   │
└───────────────┴──────────────────────────────────────────────┴───────────────────┘
```

| Region | Shows | Visible |
| --- | --- | --- |
| Top bar | Industry, Energy, Strategic stock and net rate. Science rate and active research | Always during a run |
| Fleet roster | Every player fleet with a one-word status | Always during a run |
| Selected fleet bar | One-line summary of the selected fleet | Only while a fleet is selected |
| Right column | Target card (map), planet and settlement panels (system), or fleet details | Always; content depends on mode and focus |
| Center | Star map or star system scene | Always |

The research panel strip and the header title / status label merge into the top bar. The stockpile bar moves from the bottom to the top so the bottom edge belongs to the fleet bar.

## Shared Foundation

### Theme stylesheet

Move shared tokens and pieces out of `Overworld.uss` into `UI/Shared/Theme.uss`. `Overworld.uss` and `MainMenu.uss` both import it.

- Color variables: `--bg-deep`, `--panel-bg`, `--panel-border`, `--text-primary`, `--text-muted`, `--accent-gold` (selection), `--nav-cyan` (navigation, routes), `--good-green`, `--warn-amber`, `--bad-red`, `--transit-orange`.
- Reusable classes: `.hud-panel` (frame + top accent), `.hud-caption` (small letter-spaced caps), `.hud-chip` (status chip), `.hud-corner` (corner brackets), `.hud-rate--positive/negative/zero`.
- Replace hard-coded colors in the star system rules with the variables.

### Visual language

| Meaning | Treatment |
| --- | --- |
| Selected | Gold reticle / gold left border |
| Navigation, routes, info captions | Cyan |
| Unknown or unscanned | Dark overlay with `?`, muted tag |
| Good / positive | Green |
| Warning / running out | Amber |
| Bad / stalled / negative | Red |
| In transit | Orange |

### UI Toolkit constraints

- No gradients or shadows in USS. Glows are stacked translucent circles until a soft-circle texture asset exists.
- Lines, orbits, and routes are drawn with `generateVisualContent` + `Painter2D`, not rotated elements.
- Default-theme `Button` hover/active backgrounds override colored buttons. Colored clickable shapes use a transparent button with a colored child.
- Movement and fades use USS `transition-*` started from events, never `Update`.

## Star System View

### Implemented

The first pass is in place:

- `SystemOrbitLayout`: pure orbit math. Covered by `SystemOrbitLayoutTests`.
- `StarSystemView`: orbital scene, tilted orbits drawn with `Painter2D`, depth sorting around the star, summary line, fleet dock.
- `PlanetIconView`: lit planet body with a night-side shadow facing away from the star, hover/selection reticle, name plate with `SCANNED`/`UNSCANNED` tag.
- `StarSystemView.uxml` and the star system section of `Overworld.uss`.

The presenter, query, and model are unchanged.

### Follow-ups

1. **Planet archetype colors.** Replace the id-hash hue with an archetype class (`planet-icon--gas-giant`, `--mining`, ...) once planets carry an archetype from initial generation. Add `Archetype` to `PlanetPresentationState` through `StarSystemQuery`.
2. **Planet size class.** Replace the id-hash size with a size derived from footprint or archetype.
3. **Star type visuals.** Add a star class (`system-star--red-dwarf`, `--blue-giant`, `--binary`, `--pulsar`) on the star element. `StarSystemModel` gains the star type.
4. **Colony and outpost markers.** A small badge on the planet glyph (outpost / colony / shipyard). Needs `SettlementKind` on `PlanetPresentationState`.
5. **Fleet dock.** Once the roster exists, the dock becomes redundant. Remove it, or keep it as a filtered "fleets in this system" view that highlights roster rows. Decide when the roster ships.
6. **Glow texture.** Replace the stacked star circles with a shared soft-circle texture asset.

## Fleets

### Fleet roster

A permanent list on the left during a run. It replaces the fleet list in `StarSystemInspection.uxml`.

Each row shows:

- Fleet name and ship count.
- Status: `IDLE` at a location, or `IN TRANSIT` with the destination and time left.
- Selected row: gold left border.

Behavior:

- Click selects the fleet (`OverworldSelection.SelectFleet`).
- Double-click selects the fleet and centers the map on it (after pan/zoom exists).
- Rebuilt on fleet events: travel, created, merged, split, ship completed, rewind. These are already the events `StarMapPresenter` and `StarSystemPresenter` subscribe to.

Implementation:

- `FleetRosterView` (thin) bound to a `FleetRoster.uxml` template.
- `FleetRosterPresenter` owns the subscriptions. It is a cleaner owner than either the map or system presenter, since the roster is shown in both modes.
- `FleetChoiceModel` gains `DestinationId` and `ArrivalSeconds` (or a status enum plus those fields). `FleetListQuery.All` fills them. Tests extend `FleetListQueryTests`.

### Selected fleet bar

A single row along the bottom of the center region. It is shown only while a fleet is selected, in both the map and system modes.

Contents:

- Name, status (`Idle at Home` / `In transit → Kepler Reach · 6.2s`), ship count, speed.
- Ship stacks as compact chips (`Line ×4`, `Screen ×2`).
- **DETAILS** button that opens fleet details in the right column.
- A close (×) button that clears the fleet selection.

Implementation:

- `FleetBarView` + `FleetBar.uxml`.
- Data comes from `FleetPanelQuery`. The bar and the details panel share one `FleetPanelModel`.
- `FleetPanelPresenter` applies the model to both views. It already refreshes on the right events.

### Fleet details

The current `FleetPanel` (stacks, split, merge) becomes the details view. It opens in the right column and temporarily covers the target card or planet panel.

- Opened by **DETAILS** on the fleet bar.
- Closed by its close button, by clicking a node or planet, or by deselecting the fleet.
- Restyled with the theme: stacks as rows with counts and speed factor; split as a count stepper + button; merge candidates as rows with a merge button.

This needs one piece of UI-only state for what the right column shows:

```csharp
internal enum RightColumnFocus
{
    Target = 0,       // target card on the map, planet/settlement in a system
    FleetDetails = 1,
}
```

- Lives on `OverworldSelection` (or a small `RightColumnState` next to it) with a `Changed` event.
- Rules: `OpenFleetDetails()` sets `FleetDetails`. Selecting a node or planet, closing details, or clearing the fleet sets `Target`.
- Covered by tests next to `OverworldSelectionTests`.

### Target card (star map)

The current `StarSystemInspection` card is restyled and loses its fleet list and Travel button.

Contents:

- `TARGET` caption, gold-accented system name, status chips (`EXPLORED`, `UNEXPLORED`, `FLEET PRESENT`, `N BODIES`).
- Info rows: bodies and how many are scanned, fleets present.
- **Relation row** to the selected fleet:
  - `From Alpha · 12.4s` when reachable.
  - `Alpha is here` when the fleet is at the target.
  - `Alpha is in transit` when it cannot be retargeted, if travel rules say so.
  - `Unreachable` when there is no known route.
  - `Select a fleet to travel` when no fleet is selected.
- Hint line: `Right-click to send Alpha` when travel is possible.

Implementation:

- `StarMapCardModel` gains the relation (enum + fleet name + seconds) and chips data. `StarMapQuery.DescribeCard` fills it. Tests extend `StarMapQueryTests`.
- `StarSystemInspectionView` applies the new fields. The Travel button is removed from the UXML and the view.

### Fleet movement on the map

These are the star map pieces fleets need. Nodes and backdrop are not part of this plan.

1. **Route layer.** Replace per-route `RouteView` elements with one `Painter2D` layer, like the system orbits. `MapRouteSegment` gains the two node ids and a state:

   | State | Drawn as |
   | --- | --- |
   | Known | Dim cyan, thin |
   | Planned (selected fleet → hovered node) | Gold dashed |
   | In transit | Solid orange |

2. **Hover preview.** With a fleet selected, hovering a node draws the planned route and shows a tooltip with the target name and ETA. Hover is view-only state. It does not change `OverworldSelection`.
   - Needs `StarMapQuery.TravelPreview(run, fleetId, nodeId)` → path + seconds + reachable. Tests in `StarMapQueryTests`.
   - Reuses `TravelService` path and time rules so the preview matches the actual trip.
3. **Fleet markers.** Replace the 8px pip with arrow markers beside the node and a count when several fleets share a node. The selected fleet's marker is gold.
4. **In-transit motion.** On `FleetTravelEvent` (departed), place the marker at the origin and start a USS `translate` transition to the destination with `transition-duration` = arrival seconds. On arrival, snap to the node. No ticking.
   - Known limit: pausing or changing simulation speed desyncs the transition. Re-issue the transition from the remaining time when time controls exist.

## Economy

### Top resource bar

```text
⚙ INDUSTRY 1,240 +32   ⚡ ENERGY 410 −4 ▼   ◆ STRATEGIC 12 +1   │   ⚗ +18 ▸ Basic Travel ▓▓▓░ 62%
```

- Industry, Energy, and Strategic each show the stock as a large number and the **net rate** (production − upkeep) as a smaller colored number.
- Science has no stock. It shows only as a rate feeding the active research project, with a progress bar. The research entry button moves into this segment.
- Build-defining resources (Uranium, Exotic Crystals, Precursor Alloy) appear only once the player has any stock or production of them.

### Rate unit

Default: **per settlement cycle** (10 seconds, `EconomyTickSystem.kIntervalSeconds`). The bar visibly changes once per cycle, so `+32` matches the jump the player sees. The tooltip also shows the per-second value.

This is a default, not final. See Open Questions.

### States

| State | Condition | Display |
| --- | --- | --- |
| Gaining | Net > 0 | Green `+N` |
| Flat | Net = 0 | Muted `±0` |
| Draining | Net < 0, stock lasts > 60 s | Red `−N ▼` |
| Running out | Net < 0, stock lasts ≤ 60 s | Amber `Empty in 0:45` replaces the rate |
| Stalled | Stock cannot cover one second of total upkeep | Red `STALLED` chip on the short resource |

**Stalled** is the critical one. `EconomyService.SettleSecond` pays upkeep all-or-nothing, so one shortfall stops *every* colony's output. The tooltip says so explicitly.

### Tooltip breakdown

Hovering a resource shows:

- Gross production, upkeep, net (per cycle and per second).
- Per-colony lines: `+40 Home Prime`, `−8 Kepler Reach upkeep`. Fleet upkeep lines later.
- When stalled: which resource is short and by how much.

### Feedback

- **Gain floater.** When a cycle settles, a small `+32` rises from the number and fades. It is started from `ResourceChangedEvent` with a USS transition.
- **Cost preview (later).** Hovering a build or ship-queue button shows `1,240 → 1,090` in amber, or red if unaffordable. Views raise a hover intent with the cost. The stockpile presenter applies a preview overlay. Clearing hover removes it.

### Implementation

1. **Shared production quote.** Extract the per-colony loop in `EconomyService.SettleSecond` into an `EmpireProductionQuote` helper (sum of output and upkeep across colonies, plus per-colony lines). `EconomyService` and `StockpileQuery` both use it, so the bar and the economy cannot disagree. Tests: `EmpireProductionQuoteTests`.
2. **Stockpile read model.** `StockpileQuery.Describe` returns a `StockpileModel` instead of a raw `ResourceInventory`:

   ```csharp
   internal readonly struct StockpileEntry
   {
       public ResourceKind Kind { get; }
       public int Amount { get; }
       public float GrossPerSecond { get; }
       public float UpkeepPerSecond { get; }
       public float NetPerSecond { get; }
       public float SecondsUntilEmpty { get; }   // +inf when not draining
       public StockpileState State { get; }       // Gaining, Flat, Draining, RunningOut, Stalled
   }
   ```

   `StockpileModel` holds the entries, the science rate, the active research progress, and the cycle length. Per-colony lines are built only when the tooltip opens.
   - Tests extend `StockpileQueryTests` (create if missing) and `StockpilePresenterTests`.
3. **Refresh triggers.** `StockpilePresenter` already listens to `ResourceChangedEvent`, `SettlementFoundedEvent`, `SettlementAbandonedEvent`, and `BuildingChangedEvent`. Add `ResearchCompletedEvent` (tech effects change rates), `ResearchStartedEvent` (research segment), `ShipQueuedEvent` (stock spent), and `RewindCompletedEvent`.
4. **View.** `StockpileBarView` + `StockpileBar.uxml` rebuilt as the top bar. Each resource is a small template instance (`ResourceReadout.uxml`). The tooltip is one shared element positioned on hover.

## Additional Screens

These are sketches with a short plan each. They use the same theme, layout regions, and testing rules as the sections above.

### Settlement Panel

**Today:** when a planet is selected in a star system, the right column stacks `PlanetInspection` (name, summary, Scan), then a scroll view with Base and Production `ResourceSummary` blocks, a development label, a colony status label, Outpost / Colony / Abandon buttons, the building list, and the shipyard section. Everything is labels and default buttons in one long scroll.

**Direction:** a planet dossier with a fixed header and tabs, and settlement treated as a real commitment.

```text
┌──────────────────────────────────┐
│ (●) PLANET          [SCANNED]    │  ← reuses the planet glyph
│ ▌ Home Prime                     │
│ Temperate world · Footprint 6/10 │
│ [Rich deposit] [Geothermal]      │  ← modifier chips (after scan)
├──────────────────────────────────┤
│ OVERVIEW │ BUILDINGS │ SHIPYARD  │
├──────────────────────────────────┤
│ OUTPUT / CYCLE                   │
│ ⚙ +24   ⚡ +10   ⚗ +6   ◆ +2     │  ← net, colored like the top bar
│ Upkeep  ⚡ −4                     │
│                                  │
│ STATUS  Colony · Level 2         │
│                                  │
│ ┌──────────────────────────────┐ │
│ │ FOUND COLONY                 │ │  ← only when unsettled
│ │ Cost ⚙ 120  ◆ 10             │ │
│ │ 3rd settlement · cost +50%   │ │
│ │ ⚠ Unstable readings          │ │
│ │          [ COMMIT ]          │ │
│ └──────────────────────────────┘ │
└──────────────────────────────────┘
```

- **Header (always visible):** planet glyph, name, scan chip, archetype and footprint used / total, modifier chips. Before a scan it shows the Scan action in place of the chips.
- **Overview tab:** net output per cycle and upkeep, colored like the top bar. Base vs production becomes a hover detail on each number instead of two side-by-side blocks.
- **Settlement decision card:** replaces the Outpost / Colony buttons when the planet is unsettled. Shows cost, the escalation from the current settlement count, and known warning signs. Outpost and Colony are two cards or a two-option toggle on one card. **Commit** is the only primary button on the panel.
- **Abandon:** moved to a small overflow action in the header with a confirmation, since it throws away sunk cost.
- **Buildings tab:** the footprint drawn as slots (each building takes two). Filled slots show the building, its level, and an upgrade button. Empty slots open a picker with the catalog rows, cost, and output. Hovering a build or upgrade drives the cost preview in the top bar.
- **Shipyard tab:** only shown when the colony can build ships. Current shipyard rows, restyled as class cards with cost, build time, and queue progress.

**Model changes:**

- `SettlementPanelModel` splits its display strings (`Development`, `Status`) into typed fields: settlement kind, level, footprint used / total, and modifier list.
- `SettlementCommitmentModel` gains costs, escalation step, and warning signs, and drops its preformatted labels.
- `BuildingRowModel` gains cost, output, level, and footprint, and drops `Label` / `PlaceLabel` / `UpgradeLabel`.
- A UI-only selected-tab state lives in the view. It resets to Overview when the selected planet changes.
- Tests extend `SettlementPresenterTests` and the settlement query tests.

### Research

**Today:** a strip under the header with three offer buttons, plain "Active" and "Completed" text lines, a hover label, and an inline switch confirmation. The research button in the stockpile bar toggles it.

**Direction:** a drawer of three draft cards, opened from the research segment of the top bar. Active progress stays in the top bar.

```text
┌───────────────────────────────────────────────────────────────────────┐
│ RESEARCH                             ⚗ +18 / cycle         [ CLOSE ]  │
│ ACTIVE  ▌ Basic Travel  ▓▓▓▓▓▓░░░░ 62% · done in 0:40                 │
├───────────────────────┬───────────────────────┬───────────────────────┤
│ ┌───────────────────┐ │ ┌───────────────────┐ │ ┌───────────────────┐ │
│ │ ◇ PROPULSION      │ │ │ ◇ INDUSTRY        │ │ │ ◇ SENSORS         │ │
│ │ Improved Drives   │ │ │ Heavy Foundry     │ │ │ Deep Scan         │ │
│ │                   │ │ │                   │ │ │                   │ │
│ │ Fleet speed +20%  │ │ │ Unlocks building  │ │ │ Scan range +1     │ │
│ │                   │ │ │                   │ │ │                   │ │
│ │ ⚗ 300 · ~2:10     │ │ │ ⚗ 450 · ~3:20     │ │ │ ⚗ 250 · ~1:50     │ │
│ │    [ RESEARCH ]   │ │ │    [ RESEARCH ]   │ │ │    [ RESEARCH ]   │ │
│ └───────────────────┘ │ └───────────────────┘ │ └───────────────────┘ │
├───────────────────────┴───────────────────────┴───────────────────────┤
│ COMPLETED  Basic Survey · Colony Ships · …                  (hover)   │
└───────────────────────────────────────────────────────────────────────┘
```

- **Cards:** category, name, effect text (already on `ResearchOfferModel.EffectText`), cost, and estimated time at the current science rate. Unavailable offers are dimmed with the reason.
- **Switching:** picking a card while a project is active turns that card into an inline confirmation ("Progress on Basic Travel will be lost") with Switch / Cancel. This replaces the separate confirm row.
- **Completed:** a compact chip row. Hover shows each tech's effect. It can become a full tech log later.
- **Top bar segment:** active name, progress bar, and time left. When science drops (`ScienceDropped`), the segment turns amber.
- **Drawer:** slides down from the top bar over the map. It does not push the layout. Escape or clicking the map closes it.

**Model changes:**

- `ResearchOfferModel` gains cost and estimated seconds. `ResearchQuery` computes the estimate from the current science rate.
- `ResearchPanelModel` gains an estimated time left for the active project.
- Tests extend `ResearchPresenterTests` and the research query tests.

### Status Messages

**Today:** one `status-label` in the header holds whatever was posted last: hints, results, and service errors all share it, and it never clears.

**Direction:** split by purpose.

| Kind | Example | Where | Lifetime |
| --- | --- | --- | --- |
| Error | "Not enough Industry." | Toast under the top bar, red accent | ~4 s, or until dismissed |
| Result | "Scanned Home Prime." | Toast, cyan accent | ~3 s |
| Event | "Fleet Alpha arrived at Kepler Reach." | Toast, click to select or focus | ~5 s |
| Hint | "Right-click to send Alpha." | Contextual: target card, empty panels | While the context applies |

```text
                  ┌────────────────────────────────────┐
                  │ ▌ Not enough Industry.        [×]  │  ← newest on top, max 3
                  │ ▌ Scanned Home Prime.              │
                  └────────────────────────────────────┘
```

- `OverworldStatus.Post` gains a kind (error / result / event). Hints stop going through it and move into the models of the panels they describe. For example, the target card's relation row carries "Select a fleet to travel".
- A `ToastStack` view holds up to three toasts. Expiry uses a UniTask delay per toast, not `Update`.
- Event toasts are posted by a small presenter that subscribes to domain events (fleet arrived, ship completed, research completed, settlement founded).
- Tests: `OverworldStatus` kind and queue rules, and event-to-toast mapping in the presenter.

### Main Menu

**Today:** a title label, a free-text scenario field, a seed field, Start Run, and Quit.

**Direction:** a title screen with a separate run setup panel.

```text
┌────────────────────────────────────────────────────────────────┐
│                                                                │
│                     S T A R W A T C H                          │
│                                                                │
│                     [ CONTINUE ]      ← when a save exists     │
│                     [ NEW RUN  ]                               │
│                     [ SETTINGS ]                               │
│                     [ QUIT     ]                               │
│                                                                │
│  v0.1                                                          │
└────────────────────────────────────────────────────────────────┘

NEW RUN
┌──────────────────────────────────────┐
│ SCENARIO   [ Procedural galaxy  ▾ ]  │
│ SEED       [ 48213        ] [ ⟳ ]    │  ← blank = random; ⟳ rolls one
│                                      │
│          [ BACK ]   [ LAUNCH ]       │
└──────────────────────────────────────┘
```

- Same theme as the overworld: deep background, letter-spaced title, HUD-framed buttons. A slow starfield backdrop is an asset, not code.
- The scenario becomes a dropdown of registered scenarios instead of a typed id. A debug toggle can keep free text for testing.
- The seed field stays optional. The dice button fills a random seed so the player can note it before launching.
- **Continue** and **Settings** appear only when saves and settings exist. Until then they are hidden, not disabled.
- Needs a list of scenario ids and display names from wherever scenarios are registered. Tests cover the presenter: launch with selected scenario and seed, and random seed when blank.

## Data and Model Changes

| Type | Change | Filled by | Tests |
| --- | --- | --- | --- |
| `PlanetPresentationState` | Archetype, size class, settlement kind | `StarSystemQuery` | `StarSystemQueryTests` |
| `StarSystemModel` | Star type | `StarSystemQuery` | `StarSystemQueryTests` |
| `FleetChoiceModel` | Destination, arrival seconds | `FleetListQuery` | `FleetListQueryTests` |
| `StarMapCardModel` | Fleet relation, chips | `StarMapQuery.DescribeCard` | `StarMapQueryTests` |
| `MapRouteSegment` | Node ids, route state | `StarMapQuery` | `StarMapQueryTests` |
| New `TravelPreview` | Path, seconds, reachable | `StarMapQuery.TravelPreview` | `StarMapQueryTests` |
| New `RightColumnFocus` | Target / fleet details | `OverworldSelection` | `OverworldSelectionTests` |
| New `EmpireProductionQuote` | Empire output, upkeep, per-colony lines | Settlement | `EmpireProductionQuoteTests` |
| `StockpileQuery` result | `StockpileModel` with entries and states | `StockpileQuery` | `StockpileQueryTests`, `StockpilePresenterTests` |

Per the project rules, tests target queries, models, selection state, and presenter wiring against fake views. They do not assert on element names, USS classes, or display strings.

## Phases

Each phase leaves the game playable.

### Phase 1: Theme and layout shell

- `Theme.uss` with variables and shared classes. Star system rules switched to variables.
- `Overworld.uxml` reorganized into top bar, left roster host, center, fleet bar host, right column.
- The research strip and header merge into the top bar. The stockpile bar moves to the top unchanged.

**Done when:** every existing panel still works in its new region, and no presenter changed.

### Phase 2: Economy bar

- `EmpireProductionQuote`, `StockpileModel`, the new triggers, the rebuilt top bar.
- Net rates, the four warning states, the tooltip breakdown, the gain floater.

**Done when:** a colony with a deficit shows red, a near-empty resource shows a countdown, and an unpaid upkeep shows `STALLED`. The bar matches what `EconomyService` actually grants.

### Phase 3: Fleet roster and fleet bar

- `FleetRosterView` / `FleetRosterPresenter`, the `FleetChoiceModel` additions.
- `FleetBarView` sharing `FleetPanelModel`.
- `RightColumnFocus` and fleet details in the right column.
- Fleet list and Travel button removed from the target card.

**Done when:** the player can select any fleet from the roster in either mode, see its summary at the bottom, open and close details, and split/merge as before.

### Phase 4: Target card and hover preview

- Restyled target card with the relation row and right-click hint.
- `TravelPreview` query, `Painter2D` route layer with route states, hover tooltip.

**Done when:** with a fleet selected, hovering any node shows the course and ETA, and right-click sends the fleet along the previewed route.

### Phase 5: Fleet markers and transit motion

- Arrow markers with counts. Transit transition driven by `FleetTravelEvent`.

**Done when:** a departing fleet visibly moves along its route and arrives as the arrival event fires.

### Phase 6: Star system follow-ups

- Archetype colors, size class, star type, settlement markers, the decision on the fleet dock.
- These depend on initial generation data landing. Do them in any order once it does.

### Later: additional screens

These are not yet broken into tasks. Suggested order:

1. **Status toasts.** Small and unblocks clearer feedback for every other phase. Can land right after Phase 1.
2. **Settlement panel.** The largest remaining panel. Best after Phase 2, so its output numbers and cost preview match the top bar.
3. **Research drawer.** After Phase 1, since its entry point is the top bar.
4. **Main menu.** Independent. Do it whenever the theme exists.

## Open Questions

- **Rate unit:** per cycle (default), per second, or per minute?
- **Fleet details placement:** cover the right column (default) or slide up from the fleet bar over the map?
- **Fleet dock in the system view:** remove once the roster exists, or keep it as a filtered view?
- **Retargeting:** can a fleet in transit be given a new destination? This decides the relation row text and whether right-click works on a moving fleet.
- **Fleet naming:** fleets currently show an id-based label. Do they need player-facing names (auto-generated, renameable) before the roster ships?
- **Outpost vs colony:** two separate decision cards, or one card with a toggle?
- **Research drawer:** slide over the map (default) or replace the right column while open?
- **Event toasts:** which domain events deserve a toast, and should any of them pause the game?
- **Scenario picker:** is a scenario list still needed once the procedural galaxy replaces the vertical slice, or does New Run only need a seed?
