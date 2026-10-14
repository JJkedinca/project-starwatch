# TODOs

## Active engine

Work targets **Unity** (`Starwatch/`).

## Unity — Current

- Extract `OverworldMapQuery` (or similar presentation-model builder) from `OverworldMapPresenter` when travel UI lands, so the presenter stays intent + refresh and does not keep growing label/summary/planet construction.
- Add fleet travel UI (destination select, start travel via `TravelService`, active-travel presentation).
- Add a reactive overworld map refresh path (typed map/sim event or subscription) so fleet markers and inspection stay current when `TravelSystem` / `SimulationLoop` mutate state without UI clicks.
- Introduce a thin `IOverworldMapView` (or bridge facade) if presenter tests should stop building an `OverworldViewBridge` visual tree. Navigation coverage already lives in `OverworldMapPresenterTests`.
- Enrich system/planet presentation with authored archetypes and richer inspect copy (Phase 2 planet logic still owns settlement/modifiers).

## Unity — High Level Plan



## Unity — Backlog

- Split UI Toolkit presentation out of `EndlessHeros.Core` into its own assembly once Overworld/UI surface grows enough that domain/application vs presentation boundaries matter.

