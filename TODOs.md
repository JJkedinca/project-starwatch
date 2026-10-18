# TODOs

## Active engine

Work targets **Unity** (`Starwatch/`).

## Unity — Current

- Enrich system/planet presentation with authored archetypes and richer inspect copy (Phase 2 planet logic still owns settlement/modifiers).
- Replace the Phase 1 map placeholders with an authored icon set. Fog is a visibility class on each map node. The fleet, planets, and star systems are flat UI Toolkit markers. The plan's separate icon assets and `FogOfWarView` were not built.

## Unity — High Level Plan



## Unity — Backlog

- Split UI Toolkit presentation out of `EndlessHeros.Core` into its own assembly once Overworld/UI surface grows enough that domain/application vs presentation boundaries matter.

## Unity — Deferred from Phase 3

Not part of the research slice. The slice only keeps completed research on the archive and in-progress research on the timeline.

- Phase 4: apply fleet, weapon, and travel effects. Phase 3 only stores those capability ids.
- Phase 5: invasion-counter technologies.
- Phase 6: play a rewind and restore completed research. Do not build the rewind flow in the research slice.
- Blueprints, separate research-lead assets, and separate technology-effect assets. Effects stay embedded on `ResearchDefinitionSO`.
- Parallel research projects, extra offer slots, and abandoning or pausing an active project.
- Mutually exclusive specializations.
- Scanning a planet can add one research to the run's available projects (`ResearchKnowledge`). The chance and which project come from the scan, not from a planet id on `ResearchDefinitionSO`. A discovery reveals the project and does not complete it. Until that exists, a new run starts with every catalog research available.
- Faction technology, relics with immediate effects, and Ascension reactivation of completed research.

