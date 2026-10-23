# TODOs

## Active engine

Work targets **Unity** (`Starwatch/`).

## Unity — Current

- Replace Phase 1 map placeholders with authored icon/art. The galaxy map is 3D world-space (`StarMapWorldPresenter`); fog stays a visibility class on each node. Star-system / planet panels are still UI Toolkit. Deferred map polish (zoom, fleet pick, final art) lives in `Design/scratch/star-map-3d-impl.md`.

## Unity — Overworld UI (from overworld-ui-impl)

Phases 1–5 of [Design/Art/overworld-ui-impl.md](Design/Art/overworld-ui-impl.md) are done (theme/layout, economy bar, fleet roster/bar/details, target card + hover preview, fleet markers + transit). Remaining work from that plan:

### Phase 6 — Star system follow-ups

Depends on initial-generation archetype / star-type data for full visuals; footprint and settlement presence already exist.

- Planet archetype colors (replace id-hash hue with classes such as `planet-icon--gas-giant`).
- Planet size class from footprint / archetype (replace id-hash size).
- Star type visuals on the star element (`system-star--red-dwarf`, etc.); `StarSystemModel` gains star type.
- Colony / outpost / shipyard badges on planet glyphs (`SettlementKind` on `PlanetPresentationState`).
- Fleet dock decision: remove it now that the roster exists, or keep a filtered “fleets in this system” view that highlights roster rows.
- Soft-circle glow texture for the star (stacked translucent circles until then).

### Later — Additional screens

Suggested order from the impl doc (1–4 largely shipped; leftovers below):

1. **Status toasts** — Done (kinded stack, event presenter, hint cleanup).
2. **Settlement panel restyle** — Dossier/tabs/commitment shipped; **footprint slot grid + building picker** still open.
3. **Research drawer** — Overlay + cost/ETA on offers shipped; richer card chrome / Escape-to-close polish still open.
4. **Main menu** — Title + New Run (dropdown, dice, Launch) shipped; Continue/Settings/starfield wait on saves/assets.

### Deferred polish (same doc, not a phase)

- Stockpile cost preview on build / ship-queue hover (`1,240 → 1,090`).
- Fleet upkeep lines in the resource tooltip (per-colony lines exist; fleet lines wait).
- Unresolved open questions that still affect UI — see Open Questions in `overworld-ui-impl.md` (rate unit, fleet details placement, retargeting in transit, fleet naming, outpost vs colony card, research drawer vs right column, which events toast or pause).

## Unity — High Level Plan

Phase 3 research is in place. Next:

1. Phase 4: fleets and travel. Apply the fleet and travel capability ids from completed research. Shipyards, ship construction, fleet movement, and travel effects belong here.
2. Polish the loop that already exists: map, settlement, economy, research, fleets, and travel. Do this before invasion.
3. Phase 5: invasion and crisis.
4. Phase 6: rewind and rebuild.

Different weapon capabilities wait until after that polish pass. Phase 4 does not need weapon variety, battle simulation, or invasion.

## Unity — Backlog

- Split UI Toolkit presentation out of `ProjectStarwatch.Core` into its own assembly once Overworld/UI surface grows enough that domain/application vs presentation boundaries matter.
- Determine what the best way to route UI input is

## Unity — Deferred from Phase 3

Not part of the research slice. The slice only keeps completed research on the archive and in-progress research on the timeline.

The economy settles every simulation second. Research takes that second's Science immediately. Stockpile resource-changed notifications still batch on a 10-second window (`EconomyTickSystem.kIntervalSeconds`). The active project line shows the last settled second as Science per second. Confirming a switch drops the current project's progress and starts the other offer. Pausing a project, and clearing it without starting another, stay deferred.

- Phase 4 applies `fleet-foundry` and `screen-doctrine` when a ship is queued. Basic travel is a travel-speed multiplier, not a capability id.
- Different weapon capabilities. Not part of the fleet and travel slice. Revisit after the polish pass that follows Phase 4.
- Phase 5: invasion-counter technologies. Invasion itself waits until after the post-Phase-4 polish pass.
- Phase 6: play a rewind and restore completed research. Do not build the rewind flow in the research slice.
- Blueprints, separate research-lead assets, and separate technology-effect assets. Effects stay embedded on `ResearchDefinitionSO`.
- Parallel research projects, extra offer slots, and pausing an active project. Switching to another offer is already in.
- Capability hover text still shows the raw id, such as "Unlocks travel-i". Give a capability a display name before that text is treated as final.
- Mutually exclusive specializations.
- Scanning a planet can add one research to the run's available projects (`ResearchKnowledge`). The chance and which project come from the scan, not from a planet id on `ResearchDefinitionSO`. A discovery reveals the project and does not complete it. Until that exists, a new run starts with every catalog research available.
- Faction technology, relics with immediate effects, and Ascension reactivation of completed research.

## Unity — Deferred from Phase 4

Not part of the fleet and travel slice.

- Battle: engagements, battle simulation, weapons, shields, armor, hull, retreat, debris, defense stations, and combat intel.
- Ship upkeep through the economy. Revisit in the polish pass.
- Multi-hop movement orders along known routes.
- Per-ship damage and repair state.
- Opposing faction fleets.

