# TODOs

## Active engine

Work targets **Unity** (`Starwatch/`).

## Unity — Current

- Extract `OverworldMapQuery` (or similar presentation-model builder) from `OverworldMapPresenter` if label/summary/planet construction keeps growing. Travel intents and fleet-marker refresh now live on the presenter.
- Introduce a thin `IOverworldMapView` (or bridge facade) if presenter tests should stop building an `OverworldViewBridge` visual tree. Navigation coverage already lives in `OverworldMapPresenterTests`.
- Enrich system/planet presentation with authored archetypes and richer inspect copy (Phase 2 planet logic still owns settlement/modifiers).
- Replace the Phase 1 map placeholders with an authored icon set. Fog is a visibility class on each map node. The fleet, planets, and star systems are flat UI Toolkit markers. The plan's separate icon assets and `FogOfWarView` were not built.

## Unity — High Level Plan



## Unity — Backlog

- Split UI Toolkit presentation out of `EndlessHeros.Core` into its own assembly once Overworld/UI surface grows enough that domain/application vs presentation boundaries matter.

