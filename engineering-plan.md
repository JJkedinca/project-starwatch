# Starwatch Engineering Plan

## Purpose

This document translates [integration-plan.md](integration-plan.md) into the
runtime code systems for the first playable campaign loop:

> Explore → settle → develop → research → fight → rewind → rebuild with
> knowledge.

Implementation is **horizontal**, not a single upfront vertical stub of every
system. Build a thin shared spine, then ship one playable feature end-to-end
before adding the next. Do not pre-create empty shells for wormholes, shipyards,
invasions, or rewind “for completeness.”

Balance values, final presentation, procedural content, multiplayer, and
post-run progression remain outside the first implementation pass.

## Current Repository Status

The active Unity project is `Starwatch/`. Gameplay content lives under
`Starwatch/Assets/_Project/`. The namespace root remains `ProjectStarwatch`.

The application shell is largely in place: Bootstrap → Main Menu → Start Run →
Overworld, with project-scoped DI, a paused-until-run simulation clock, and a
guarded run-start path. Phase 0 finishes the shared simulation spine on top of
that shell. Feature phases begin with the map loop.

## Engineering Principles

- Build horizontally: finish one thin playable slice before starting the next.
- Grow contracts when a feature needs them; do not pre-stub later systems.
- Keep simulation state in plain C# types wherever possible.
- Keep gameplay rules out of MonoBehaviour views.
- Use ScriptableObjects for authored content and definitions.
- Route player actions through services or commands, not direct UI mutation.
- Publish domain events when state changes so views remain decoupled.
- Use one simulation clock and managed ticks instead of scattered `Update`
  loops.
- Make scenario generation and combat deterministic from a run seed.
- Keep scenario state, timeline state, and run archive state separate.
- Prefer a small number of authoritative systems over many passive components.
- Add EditMode tests for every non-trivial service and resolver.
- Use PlayMode tests only for scene, Unity lifecycle, travel presentation, or
  other frame-dependent behavior.

## Horizontal Implementation Rule

Each phase after the shared spine must deliver a complete thin slice:

1. Only the data, services, content, and views needed for that feature.
2. A playable or headless-provable outcome for that feature alone.
3. Explicit deferrals for anything that belongs to a later phase.

Later phases may extend earlier state types. They should not require earlier
phases to have invented placeholder systems for them.

## Runtime Architecture

The lists below are the **target catalog** for the connected campaign loop.
They are not a Phase 0 shopping list. Add a type when the phase that needs it
begins.

### Layer 1: Content definitions

ScriptableObjects describe authored content:

- `ScenarioDefinition`
- `MapNodeDefinition`
- `StarSystemDefinition`
- `PlanetDefinition`
- `PlanetModifiers`
- `BuildingDefinitionSO`
- `ResourceDefinition`
- `ProductionRecipeDefinition`
- `ShipyardDefinition`
- `StartingEmpireDefinition`
- `ResearchDefinition`
- `ShipClassDefinition`
- `WeaponDefinition`
- `DefenseDefinition`
- `FactionDefinition`
- `FactionFleetDefinition`
- `ModifierDefinition`
- `PresentationAssetSet`
- `RunOutcomeRuleDefinition`
- `SabotageDefinition`
- `InvasionDefinition`

Definitions contain configuration and references. They do not contain mutable
run state.

### Layer 2: Runtime state

Plain C# state represents the current simulation:

- `RunState`
- `ScenarioState`
- `TimelineState`
- `RunArchive`
- `SystemState`
- `PlanetState`
- `ColonyState`
- `FleetState`
- `ResearchState`
- `ProductionQueueState`
- `ShipyardState`
- `FactionState`
- `FactionRelationshipState`
- `ActiveEngagementState`
- `CrisisState`

State should use stable content IDs rather than direct scene-object references.
This makes reset, testing, and serialization possible.

### Layer 3: Services and systems

Services validate commands and mutate authoritative state:

- `ScenarioService`
- `ExplorationService`
- `TravelService`
- `SettlementService`
- `EconomyService`
- `ResearchService`
- `FleetService`
- `ProductionService`
- `ShipConstructionService`
- `CombatService`
- `EngagementService`
- `FactionService`
- `InvasionService`
- `TimelineOutcomeEvaluator`
- `RewindService`
- `ArchiveService`
- `VisibilityService`
- `EffectRegistry`

Systems perform time-driven work:

- `SimulationClock`
- `TravelSystem`
- `EconomyTickSystem`
- `ResearchTickSystem`
- `ProductionSystem`
- `InvasionTickSystem`
- `BattleSimulation`

### Layer 4: Presentation

MonoBehaviours and UI Toolkit presenters display state and send intents:

- `StarMapView`
- `SystemView`
- `PlanetView`
- `FleetView`
- `BattleView`
- `ResearchView`
- `EconomyView`
- `CrisisView`
- `ArchiveView`

Views must not calculate resource output, resolve combat, or decide whether an
action is valid.

### Presentation and art strategy

The first implementation is intentionally 2D-first. This is a presentation
choice, not a simulation constraint:

- Strategic map nodes, planets, fleets, factions, resources, research, and
  discoveries use authored 2D icons or sprites.
- Early battles use authored world-space 2D or 2.5D token prefabs with clear
  silhouettes, health bars, range indicators, and combat effects.
- Later 3D ship, planet, and station prefabs replace those presentation assets
  without changing runtime state, combat, economy, or research systems.

Use a shared `PresentationAssetSet` or equivalent asset references on content
definitions. A definition may provide:

- `IconSprite` for map and UI presentation.
- `TokenPrefab` for the first battle presentation.
- Optional `WorldPrefab` or `BattlePrefab` for the later 3D presentation.

Views should request a presentation asset by stable content ID through a
factory or catalog. They should not know whether the returned asset is a
sprite, token, or 3D prefab. This keeps the 2D prototype replaceable.

Use authored prefabs and shared materials. Do not create gameplay visuals with
`GameObject.CreatePrimitive`, `new Material`, or runtime shader lookup. Apply
per-instance tinting through `MaterialPropertyBlock` when 3D assets are added.

### Layer 5: Composition

VContainer LifetimeScopes compose the systems:

- Bootstrap scope for constructing the project container.
- Project scope for the simulation clock, event bus, catalogs, scene flow, and
-  run manager, active engagements, and effect queries.
- Main menu scope for the menu view and run-start intents.
- Overworld scope for map, travel, exploration, economy, research, and
  invasion.
- Battle scope for battle simulation and battle presentation only.

The project scope must survive scene changes. Scene scopes are created and
disposed with their scenes and receive the project scope as their parent
container. Gameplay state belongs to `RunManager` and runtime services, not to
scene objects, so loading a new scene does not silently reset the run.

## Application Shell and Scene Flow

The game needs a working application shell before the first gameplay system can
be exercised.

### Required scenes

Create and register these scenes in Build Settings:

1. `Assets/_Project/Scenes/Bootstrap.unity` — build index 0.
2. `Assets/_Project/Scenes/MainMenu.unity`.
3. `Assets/_Project/Scenes/Overworld.unity`.
4. `Assets/_Project/Scenes/Battle.unity`.

`Bootstrap.unity` is the only scene that should be entered directly. The
current template `SampleScene.unity` can remain temporarily for reference but
must not be the production entry point.

### Bootstrap flow

Implement:

- `ProjectLifetimeScope`
- `UpdateManager`
- `GameEntryPoint`
- `SceneFlow`
- `SceneId`
- `ISceneLoader`

The flow is:

1. Unity loads `Bootstrap.unity`.
2. The Bootstrap scene contains `ProjectLifetimeScope` and `UpdateManager`.
   The scope marks its GameObject `DontDestroyOnLoad` and is assigned
   `ObjectDatabaseSO`.
3. `ProjectLifetimeScope` builds the project container.
4. `GameEntryPoint` loads `MainMenu.unity` after the container is ready.
5. `SceneFlow` loads scene-specific scopes as children of the project scope.
6. The main menu starts a run through `RunManager`.
7. `SceneFlow` loads `Overworld.unity` with the active run available.
8. A battle request loads `Battle.unity` additively; the overworld scene and
   scope remain active.
9. Completing the battle unloads the battle presentation and returns focus to
   the overworld without recreating the run.

`SceneFlow` owns scene transitions. Views and feature services must not call
`SceneManager.LoadScene` directly.

### Project lifetime registrations

`ProjectLifetimeScope` always registers the shell/spine:

- `SimulationClock`
- `UpdateManager`
- MessagePipe or the project event bus
- `ObjectDatabaseSO` (and feature catalogs as those phases land)
- `RunManager`
- `SceneFlow`
- `GameEntryPoint`
- `IRunStateStore` implementation

Register these on the project scope when their horizontal phase begins, not
sooner:

- `ArchiveService`, `VisibilityService` → Phase 1+
- `PresentationCatalog` → when first views need it
- `EffectRegistry` → Phase 3
- `BattleService`, `EngagementService` → Phase 4
- `TimelineOutcomeEvaluator` → Phase 5 / 6

It also owns project-wide configuration such as the active scenario catalog
and application logging.

### Scene lifetime registrations

`MainMenuLifetimeScope` registers:

- `MainMenuPresenter`.
- `MainMenuView`.
- `RunStartService` or `StartRunCommandHandler`.

`OverworldLifetimeScope` registers:

- Scenario, map, travel, exploration, economy, research, and invasion
  services.
- Overworld views and presenters.

`BattleLifetimeScope` registers:

- `BattleSimulation`.
- Combat policies and resolvers.
- Battle views and presenters.

`BattleService` and active engagement state remain project-scoped so overworld
travel can add reinforcements while the battle presentation is loaded.
`BattleLifetimeScope` owns only battle-specific simulation/presentation objects.
Scene scopes should not own `RunManager`, the archive, or the simulation clock.

### Main menu and starting a run

The first main menu only needs:

- Game title.
- `Start Run` button.
- Optional development-only scenario and seed fields.
- Quit button where supported.

Implement:

- `MainMenuView` — UI Toolkit document and visual controls.
- `MainMenuPresenter` — translates view events into commands.
- `StartRunCommand`.
- `RunStartService`.
- `RunStartConfiguration`.

`RunStartService` selects the authored scenario, creates a deterministic seed,
asks `RunManager` to create the run, and requests `SceneFlow` to load the
overworld. The main menu must not construct `ScenarioState` or `TimelineState`
itself.

### Application-shell tests

- Bootstrap creates exactly one project lifetime object.
- Project services survive a main-menu to overworld transition.
- Starting a run creates scenario, timeline, archive, and seed state.
- Starting a run loads the overworld scene.
- A battle transition preserves the active run.
- Returning from battle does not create a second run or second project scope.
- Scene-specific scopes are disposed when their scenes unload.

## Shared Foundation Systems

These systems are prerequisites for all gameplay phases.

### Simulation time

Required types:

- `GameTime`
- `SimulationClock`
- `SimulationSpeed`
- `ITickable`
- `UpdateManager`

Responsibilities:

- Advance simulation time while unpaused.
- Support pause and a small set of speed values.
- Dispatch managed ticks to registered systems.
- Allow tests to advance time without waiting for real seconds.

The clock should not directly know about planets, fleets, or invasions. It only
publishes time advancement.

### Identifiers and deterministic randomness

Required types:

- `ContentId`
- `EntityId`
- `RunSeed`
- `ISimulationRandom`
- `DeterministicRandom`

Scenario generation, research offers, challenge selection, and combat rolls
must be reproducible from explicit seeds. Randomness should be injected into
services rather than obtained from `UnityEngine.Random` inside game rules.

### Events and commands

Use MessagePipe or a small equivalent event abstraction for domain events such
as:

- `NodeExplored`
- `PlanetScanned`
- `MapKnowledgeChanged`
- `SettlementFounded`
- `ChallengeRevealed`
- `ResourceChanged`
- `ResearchCompleted`
- `FleetArrived`
- `BattleStarted`
- `BattleEnded`
- `CrisisStarted`
- `CrisisResolved`
- `TimelineEnded`
- `RewindCompleted`
- `ShipConstructionCompleted`
- `FactionContacted`
- `FleetJoinedEngagement`
- `CombatIntelRecorded`
- `TimelineOutcomeChanged`

UI intents should be explicit commands or service calls, for example:

- `ExploreNode`
- `ScanPlanet`
- `SettlePlanet`
- `StartResearch`
- `BuildInfrastructure`
- `MoveFleet`
- `MergeFleet`
- `RetreatFleet`
- `RewindTimeline`

### Command results

Normal player-command rejection is expected gameplay feedback, not an
exception. Shared command types should return:

- `CommandResult`
- `CommandResult<T>`
- `CommandErrorCode`
- `CommandError`

Examples include insufficient resources, an invalid route, an occupied
shipyard, an unmet research prerequisite, or a planet that cannot be settled.
Presenters map error codes to player-facing explanations.

### Effects and modifiers

Research, relics, species traits, and discoveries must register effects through
one data-driven interface rather than creating feature-specific research-ID
checks.

Implement:

- `EffectDefinition`
- `EffectRegistry`
- `IEffectQuery`
- `CapabilityId`
- `ModifierContext`

`EffectRegistry` is rebuilt from completed run research, current timeline
assets, and active discoveries. Read-only consumers such as production, travel,
and damage resolution query capabilities and numeric modifiers through
`IEffectQuery`.

### Content lookup

Required types:

- `ObjectDatabaseSO`
- `ScenarioCatalog`
- `ResearchCatalog`
- `ShipCatalog`
- `BuildingCatalog`
- `ProductionCatalog`
- `FactionCatalog`
- `PresentationCatalog`

The object database converts stable IDs into ScriptableObject definitions. Runtime
systems should not search the AssetDatabase or scene hierarchy.

### Knowledge and visibility ownership

Persistent knowledge belongs to `RunArchive`, not to map views or mutable
timeline visibility:

- `MapKnowledge` stores explored nodes, known routes, and scanned details.
- `CombatIntel` stores observed enemy classes, weapons, defenses, and
  weaknesses.
- `DiscoveryKnowledge` stores leads, blueprints, and known triggers.

`VisibilityService` combines persistent archive knowledge with temporary sensor
coverage from the current timeline. Fog of war is only a presentation of that
query. `ArchiveService.RecordObservation` must be called when exploration,
scanning, or combat reveals information, before a timeline can be reset.

## Phase 0: Shared Simulation Spine

Phase 0 is only the shared spine needed before the first feature slice. The
application shell already exists; this phase finishes the contracts every later
feature will extend. It does **not** stub economy, combat, invasion, rewind,
presentation catalogs, or shipyards.

### Already in place from the shell

- Lifetime scopes, `SceneFlow`,
  `GameEntryPoint`
- `RunManager`, `RunState`, stub scenario/timeline/archive types
- `SimulationClock`, `UpdateManager`, run-start pause/reset boundary
- `RunStartService` with overlapping-start guard
- `IRunStateStore` / `InMemoryRunStateStore`
- `EventJournal` (string stub), empty `ObjectDatabaseSO`

### Implement in Phase 0

- `ContentId`, `EntityId`
- `ISimulationRandom`, `DeterministicRandom`
- `CommandResult`, `CommandResult<T>`, `CommandErrorCode`, `CommandError`
- `EventRecord` and a journal that stores records, not only strings
- Minimal `StartingEmpireDefinition` data (home system id, starter fleet
  placeholder; colony/resources can stay empty until Phase 2)
- Real shapes for `ScenarioState`, `TimelineState`, and `RunArchive` that own
  separate collections
- `TimelineFactory` that materializes a fresh timeline from starting-empire
  data without aliasing scenario or archive lists
- `RunOutcome` kind/reason type only (no outcome evaluator yet)

`RunManager` continues to own the active run and expose read-only access to
scenario, timeline, archive, and rewind index. Feature services still own
mutations.

### Deferred out of Phase 0

Add these only when their feature phase begins:

- Map graph, fog, travel, visibility → Phase 1
- Colonies, resources, buildings, production queues, shipyards → Phase 2 / 4
- Research offers, effect registry → Phase 3
- Fleets, engagements, battle simulation → Phase 4
- Invasion schedule, crises, outcome evaluator rules → Phase 5 / 6
- Rewind service and rebuild flow → Phase 6
- Presentation catalog and 2D/3D fallback rules → when first views need assets

### Tests

- A fixture starting empire creates a run with separate scenario, timeline, and
  archive instances.
- Mutating timeline-only data does not change scenario or archive.
- A new timeline does not share mutable list/dictionary references with the
  previous timeline or scenario state.
- The simulation clock remains paused until run start, resets for a new run,
  and advances deterministically when resumed.
- Invalid commands can return stable error codes without partial mutation
  (cover with a tiny sample command helper if needed).
- Deterministic random reproduces the same sequence from the same seed.

### Deliverable

A headless EditMode test can create a run from a fixture starting empire,
advance the clock, mutate timeline-only state, and prove the three persistence
layers stay isolated. Map content and PlayMode feature smoke tests belong to
later phases.

## Phase 1: Map, Exploration, and Travel

First horizontal feature slice. Prove authored geography, fog of war, scanning,
and travel before adding settlement or factions as systems.

### In scope

- Authored scenario graph and starting empire placement
- Exploration, scanning, route travel
- Archive map knowledge + `VisibilityService`
- Minimal map presentation (2D icons / fog)

### Deferred to later phases

- Settlement, resources, buildings → Phase 2
- Planet challenges → after the economy loop is playable
- Research and discoveries as tech unlocks → Phase 3
- Faction contact, sorties, and diplomacy → later map/politics slice or Phase 5
  setup (scenario may still mark a faction-owned node as inert content)
- Invasion wormhole runtime, scouts, crises → Phase 5 (scenario may place a
  labeled wormhole node with no invasion director yet)
- Battle presentation → Phase 4

### Application entry

The shell already reaches Overworld. Phase 1 wires `RunStartService` /
`ScenarioLoader` so Start Run loads the authored map into the existing run.
Do not add a second run-construction path inside the overworld.

### Content assets

Create one authored `ScenarioDefinition` containing:

- Four or five important systems.
- One or two non-system nodes.
- Connected routes and one or two chokepoints.
- Planet definitions and local modifiers for inspection/scanning.
- A `StartingEmpireDefinition` with a home system and small starter fleet.
- Optional inert markers for a future wormhole or faction capital (content
  only; no runtime invasion/faction services yet).

No procedural generation.

### Initial presentation assets

Create a small authored 2D asset set for:

- System and non-system node icons.
- Planet icons.
- Player fleet marker.
- Fog / unknown node presentation.
- Consistent map UI style.

Use `PresentationAssetSet` (or equivalent) only as far as the map needs.
No battle tokens, research icons, or invasion markers required yet.

### Runtime systems

Implement:

- `ScenarioLoader`
- `MapGraph`
- `NodeState`
- `SystemState`
- `PlanetState`
- `ExplorationService`
- `SensorService`
- `TravelService`
- `TravelSystem`
- `RouteService`
- `VisibilityService`
- `ArchiveService` map-knowledge recording
- `MapKnowledge` on `RunArchive`

Responsibilities:

- Load the authored scenario into runtime state.
- Validate route connectivity.
- Reveal nodes and broad system composition.
- Reveal planet details after scanning.
- Start and complete fleet travel on simulation time.
- Record map discoveries in the archive immediately.

### Presentation

Implement:

- `StarMapView`
- `MapNodeView`
- `RouteView`
- `SystemInspectionView`
- `PlanetInspectionView`
- `FogOfWarView`

The map view observes state and events. It does not own the graph or reveal
hidden content on its own.

### Tests

- `Start Run` loads the authored scenario into one run and shows the overworld
  map.
- The route graph is connected.
- Exploration reveals only the intended information.
- Scanning reveals planet details and records archive map knowledge.
- Travel takes simulation time and arrives at the correct node.
- Invalid travel returns a command error. A fleet can travel a known route into an unexplored node; arrival explores that node. Planet scans stay a separate manual action.
- Starting placement comes from `StartingEmpireDefinition`, not loader
  constants.

### Deliverable

The player can start from a defined home system, inspect the map, travel a
known route (arrival explores the destination), scan planets, and move a
starter fleet toward a meaningful expansion choice.

## Phase 2: Settlement and Economy

Horizontal slice: commit to planets and run a small economy. Do not invent
shipyards or fleet production here; those arrive with the fleet slice.

### In scope

- Outposts / settlement / colonies
- Resources, buildings, upkeep
- Economy tick + resource ledger

### Deferred

- Frontier Pressure and colony stability (`FrontierPressureService`,
  `ColonyStabilityService`) → after the base settlement loop is in. Add them as
  further restrictions on cost, upkeep, and output.
- Planet challenges (`PlanetChallengeDefinition`, `PlanetChallengeService`,
  `PlanetChallengeView`) → after the economy loop is playable. `ChallengeRevealed`
  stays reserved on `GameEventType`.
- Shipyards, production queues, ship construction → Phase 4
- Research infrastructure as tech progression → Phase 3 (buildings may produce
  Science now without a research UI)
- Rewind clearing of colonies → Phase 6 (keep colony/resource state
  timeline-owned so rewind can clear it later)

### Content assets

Create:

- `ResourceDefinition`
- `PlanetModifiers`
- `BuildingDefinitionSO`

Small resource set: Industry, Energy, Science, one strategic material.

### Runtime systems

Implement:

- `SettlementService`
- `OutpostService`
- `ColonyService`
- `BuildingService`
- `EconomyService`
- `EconomyTickSystem`
- `ProductionService` (resource output only)
- `ResourceLedger`

Responsibilities:

- Start an outpost or settlement with escalating commitment costs.
- Create and remove `ColonyState`.
- Validate building capacity and level upgrades.
- Apply archetype multipliers without hard-locking off-specialization
  buildings.
- Produce and consume resources on simulation ticks.
- Apply building upkeep from the ledger.

Economy calculations should be pure functions where possible. Views never
calculate income.

### Presentation

Implement:

- `PlanetDevelopmentView`
- `BuildingListView`
- `ResourceSummaryView`
- `SettlementCommitmentView`
- `ColonyStatusView`

### Tests

- Settlement consumes the correct cost and creates a colony.
- Settlement cost increases according to the configured curve.
- Building capacity and level requirements are enforced.
- Archetype multipliers affect output without blocking off-specialization
  buildings.
- Production and upkeep update correctly over a simulated interval.
- Colony and resource state is timeline-owned (ready for later rewind).

### Deliverable

The player must choose between at least two useful planets because settling
both immediately spends a rising cost and then pays upkeep. Frontier Pressure
and stability can tighten that choice after this loop works.

## Phase 3: Research and Discovery

Horizontal slice: choose research, unlock capabilities, store completed tech in
the archive. Full rewind flow still waits for Phase 6.

### In scope

- Research pool, one active project, prerequisites
- Research rate from existing infrastructure
- Discovery-gated path + blueprint recording
- `EffectRegistry` / `IEffectQuery` for data-driven unlocks
- Archive storage for completed research and leads

### Deferred

- Exercising rewind restore of completed research → Phase 6
- Fleet-specific combat effects beyond unlock IDs → Phase 4
- Invasion-counter technologies → Phase 5

### Content assets

Create:

- `ResearchDefinition`
- `ResearchPrerequisite`
- `ResearchLeadDefinition`
- `BlueprintDefinition`
- `TechnologyEffectDefinition`
- `ResearchDomainDefinition`

First pool: core construction/energy/travel/fleet techs, one economic
specialization, one fleet specialization, one discovery-gated path.

### Runtime systems

Implement:

- `ResearchService`
- `ResearchPoolService`
- `ResearchPrerequisiteService`
- `ResearchRateService`
- `ResearchTickSystem`
- `DiscoveryService`
- `BlueprintService`
- `TechnologyUnlockService`
- `EffectRegistry`

Responsibilities:

- Build three eligible offers from prerequisites and discoveries.
- Start one active project; advance from global research rate.
- Refresh offers after selection or completion.
- Record blueprints and completed research in the archive.
- Register capabilities through `EffectRegistry`.
- Keep labs, production, and resources in timeline state.

### Presentation

Implement:

- `ResearchView`
- `ResearchOfferView`
- `ResearchProgressView`
- `DiscoveryNotificationView`
- `ArchiveResearchView`

### Tests

- Core technologies remain available in the pool.
- Prerequisites include or exclude projects correctly.
- Only one active project in the MVP.
- Research rate advances progress deterministically.
- Completing research changes a capability visible through `IEffectQuery`.
- A discovery reveals a path without granting the final technology.
- Completed research is stored in the archive; incomplete progress is
  timeline-owned.

### Deliverable

At least one discovery should make a different economy or future fleet plan
more attractive than the default plan.

## Phase 4: Fleets and Travel

Horizontal slice: build ships at a shipyard, compose fleets by explicit merge and split, and travel at a speed set by the slowest class and by research. Battle waits until after the polish pass that follows this slice.

### In scope

- Three ship classes: screen, line, and carrier
- Shipyard building and a per-colony build queue
- Explicit merge and split
- Travel time from the slowest class and a research travel-speed multiplier

### Deferred

- Battle: engagements, battle simulation, weapons, shields, armor, hull, retreat, debris, defense stations, and combat intel
- Ship upkeep, multi-hop orders, and per-ship damage
- Opposing faction fleets
- Invasion schedule → Phase 5, after the polish pass

### Content assets

Create:

- `ShipClassDefinitionSO`
- `ShipCatalogSO`

Shipyard is a `BuildingDefinitionSO` in the settlement catalog. It has no output. Placing it is not gated by research. Building ships is.

Screen (`starter-screen`) and line require `fleet-foundry`. Carrier requires `screen-doctrine`. Basic travel is a travel-speed multiplier, not a capability id.

### Runtime systems

Implement:

- `FleetService`
- `ShipProductionService`
- `ShipProductionSystem`

`FleetState` replaces the fleet placeholder and holds ship stacks. Build orders live on `ColonyState`. Both are timeline-owned.

Responsibilities:

- Queue spends the class cost. Cancel refunds it. One order builds at a time.
- A finished ship joins an idle fleet at that system, or creates a fleet when none is idle.
- Merge and split are explicit. Merge keeps the target fleet.
- Travel time is the route time divided by the slowest class speed and by the research travel multiplier.

### Presentation

- Shipyard section on the settlement panel
- Fleet panel for the selected system, with merge and split
- Map fleet selection, with the arrival time on the travel line

### Tests

- A queued ship spends its cost and appears only after build time
- Locked classes, a missing shipyard, an outpost, a full queue, and a short stockpile are rejected
- Merge and split keep the ship count and reject an illegal split
- A slow class lengthens the route. Basic travel shortens it
- A new timeline clears fleets and queues and restores the starter fleet

### Deliverable

The player researches basic fleet, builds a shipyard, and queues a line ship. Merging it into the starter screens makes that fleet slower. Basic travel makes the same route faster again.

## Phase 5: Invasion and Crisis

Horizontal slice: learnable escalation using fleets and economy already built.
Introduce outcome reporting here; finalize rewind/end-timeline in Phase 6.

### In scope

- Invasion schedule, warnings, scout, one sabotage crisis, one major wave
- Simple target selection and repair withdrawal
- Crisis windows and recoverable consequences
- Report facts/candidates to a shared outcome evaluator (no per-system
  game-over)

### Deferred

- Voluntary rewind / timeline rebuild UI → Phase 6
- Dynamic enemy redesign → never in first pass
- Secondary fronts / multiple wormholes → deferred content

### Content assets

Create:

- `InvasionDefinition`
- `InvasionPhaseDefinition`
- `InvasionWaveDefinition`
- `SabotageDefinition`
- `CrisisResolutionDefinition`
- `InvasionObjectiveDefinition`
- `RunOutcomeRuleDefinition` (initial home-system / objective checks)

Wire the authored wormhole marker from Phase 1 into live invasion content.

### Runtime systems

Implement:

- `InvasionDirector`, `InvasionScheduleService`, `InvasionTickSystem`
- `InvasionFleetSpawner`, `InvasionTargetService`, `InvasionMovementService`
- `SabotageService`, `CrisisService`, `RepairFleetService`
- `InvasionObjectiveService`
- `TimelineOutcomeEvaluator` (report-only terminal candidates; Phase 6 owns
  finalizing a timeline end)

Responsibilities match the integration invasion loop: fixed high-level enemy
plan, warnings before events, attrition without instant run death from a single
lost battle.

### Presentation

Implement:

- `InvasionStatusView`, `ThreatWarningView`, `CrisisView`
- `InvasionFleetView`, `ObjectiveView`

### Tests

- Milestones and warnings are deterministic.
- Scout/main compositions match authored data.
- Sabotage creates a crisis; resolution/expiry apply documented effects.
- Damaged invasion fleets can withdraw for interception.
- One lost battle or colony does not end the timeline unless rules say so.
- Evaluator emits at most one terminal candidate for a given condition set.

### Deliverable

The player can recognize an approaching threat, prepare a response, suffer
meaningful attrition, and continue after a partial defensive failure.

## Phase 6: Rewind and Rebuild

Horizontal slice: use the archive and timeline boundaries grown by Phases 1–5.
Do not invent a separate restart flow.

### In scope

- Voluntary rewind and defeat-triggered timeline end
- `TimelineEndService` as the only finalizer
- Clear timeline-owned state; keep scenario + archive
- Recreate starting empire via `TimelineFactory`
- Restore completed research and discovered access from the archive
- Rebuild summary UI

### Deferred

- Disk save adapters beyond `IRunStateStore` → later
- Post-run Ascension → out of scope

### Runtime systems

Implement:

- `RewindService`
- `TimelineResetService`
- `TimelineEndService`
- `RunCompletionService`
- Extend `ArchiveService` / `TimelineFactory` as needed for restore

Responsibilities:

- Finalize timeline end once from shared outcome rules.
- Record any pending discoveries before reset.
- Reset colonies, fleets, resources, queues, shipyards, temporary visibility,
  relationships, and active crises.
- Keep scenario geography and event identities stable.
- Rewind budget blocks further rewinds; exhausting it is not itself a defeat.

### Persistence and save boundary

In-memory rewind is enough. Keep `IRunStateStore` as the boundary for later
disk saves (seed, scenario id, archive, timeline, rewind index, version).

### Presentation

Implement:

- `TimelineEndView`
- `RewindConfirmationView`
- `RebuildSummaryView`
- `ArchiveView`

### Tests

- Voluntary rewind consumes one timeline; defeat ends exactly once.
- Scenario stable; archive facts and completed research remain.
- Timeline power resets; new timeline uses starting empire + archive.
- Lost battle ≠ defeated timeline; terminal run outcome emits once.
- No rewind after run complete or budget spent.
- No shared mutable references between old and new timeline state.

### Deliverable

A failed first attempt produces a faster, better-informed second attempt rather
than a restart from identical unknown state.

## Phase 7: Integration Pass

Only after Phases 0–6 each work as thin slices. This phase connects timing and
balance; it does not introduce major new gameplay systems.

### In scope

- Debug controls that call the same services as gameplay
- A few PlayMode end-to-end tests across the connected loop
- Telemetry for balance diagnosis

### Runtime tooling

Implement:

- `IntegrationScenarioRunner`
- `DebugSimulationControls`
- `SimulationInspector`
- `EventLogView`
- `BalanceTelemetry`
- `ScenarioSeedInput`

Useful controls: pause/speed, single tick, reveal map, grant resources,
complete research, enqueue/complete ships, spawn test fleet, trigger crisis,
force rewind.

### End-to-end tests

PlayMode proofs only:

- Explore and settle.
- Complete research.
- Build ships and resolve a battle.
- Trigger an invasion event.
- Rewind and verify persistence boundaries.

Keep detailed rules in EditMode.

### Integration metrics

Record time-to-expansion, settlement cost/output, research choices, shipyard
throughput, battle attrition, invasion milestones, crisis results, timeline/run
outcomes, rewind count, and archive discoveries.

### Deliverable

Repeated full-loop runs answer the integration-plan validation questions well
enough to justify more content or procedural generation.

## Suggested Unity Content Structure

Relative to the authoritative Unity project root `Starwatch/`:

```text
Assets/_Project/
  Scenes/
    Bootstrap.unity
    MainMenu.unity
    Overworld.unity
    Battle.unity
  Runtime/
    Core/
      Bootstrap/
      Run/
      Simulation/
      Events/
      Content/
      Effects/
      Persistence/
    Overworld/
      Map/
      Exploration/
      Travel/
      Planets/
      Economy/
      Research/
      Factions/
      Invasion/
    Battle/
      Fleets/
      Combat/
      Defenses/
      Salvage/
    UI/
      MainMenu/
      Overworld/
      Battle/
  Data/
    Scenarios/
    Systems/
    Planets/
    Economy/
    Research/
    Factions/
    Fleets/
    Invasion/
  Views/
    Overworld/
    Battle/
    UI/
  Art/
    Icons/
    Sprites/
    Prefabs/
      Map/
      BattleTokens/
      UI/
    Materials/
    VFX/
  Tests/
    EditMode/
    PlayMode/
```

Namespaces should mirror these folders according to the project conventions:

- `ProjectStarwatch.Core`
- `ProjectStarwatch.Overworld`
- `ProjectStarwatch.Battle`
- `ProjectStarwatch.UI`
- `ProjectStarwatch.Data`
- `ProjectStarwatch.Shared`

## Package and Project Prerequisites

The current Unity project already includes Unity 6, URP, Input System, AI
Navigation, UI Toolkit support, and the Test Framework.

Before implementing the service composition, add or decide on:

- VContainer for LifetimeScopes and dependency injection.
- UniTask for scene flow, asynchronous operations, and future save operations.
- MessagePipe for typed domain events and intents.

Cinemachine and Addressables are not required for the first campaign loop.
UGUI should not be used to build new gameplay UI even though the package is
present.

## Dependency Order

Implement strictly left-to-right. Each arrow means the previous slice is
playable or headless-proven before the next begins:

```text
Phase 0 Shared spine
  → Phase 1 Map / explore / travel
  → Phase 2 Settlement / economy
  → Phase 3 Research / discovery
  → Phase 4 Shipyards / fleets / battle
  → Phase 5 Invasion / crisis
  → Phase 6 Rewind / rebuild
  → Phase 7 Integration / balance
```

Rules:

- Do not stub Phase N+1 systems while implementing Phase N.
- Later phases may extend earlier state types; they must not require earlier
  phases to invent placeholders for them.
- Phase 6 reuses the scenario / timeline / archive boundaries grown by Phases
  0–5; it is not a separate restart architecture.
- Optional light faction contact may be inserted after Phase 1 if needed for
  map pressure, but full invasion remains Phase 5.

## Explicitly Deferred Engineering

Do not add these until the horizontal loop above works:

- Procedural scenario generation.
- Secondary wormholes or multiple active fronts.
- Full diplomacy simulation and complex NPC development.
- Multiplayer synchronization, shared research, or shared rewinds.
- Post-run progression and Ascension.
- Detailed convoy and cargo simulation.
- The complete ship roster and modular ship designer.
- Dynamic enemy fleet redesign.
- Final art, cinematic battle effects, and production-quality animation.

The engineering goal is to prove each feature as a thin slice, then prove that
those slices connect. Content breadth and presentation expand after that.
