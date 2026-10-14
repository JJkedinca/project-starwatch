# TODOs

## Active engine

Work targets **Unity** (`Starwatch/`).

## Unity — Current

- Extract `OverworldMapQuery` (or similar presentation-model builder) from `OverworldMapPresenter` when Step 2 travel/route UI lands, so the presenter stays intent + refresh and does not keep growing label/summary/planet-row construction.
- Add a reactive overworld map refresh path (typed map/sim event or subscription) so fleet markers and inspection stay current when `TravelSystem` / `SimulationLoop` mutate state without UI clicks.
- Introduce a thin `IOverworldMapView` (or bridge facade) when presenter EditMode tests need to avoid UI Toolkit / concrete `_viewBridge.StarMap` coupling.

## Unity — High Level Plan



## Unity — Backlog

- Split UI Toolkit presentation out of `EndlessHeros.Core` into its own assembly once Overworld/UI surface grows enough that domain/application vs presentation boundaries matter.

