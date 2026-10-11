# Starwatch Engineering Plan

## Purpose

This document translates [integration-plan.md](integration-plan.md) into the
runtime code systems required for the first playable vertical slice.

The goal is not to implement every designed feature. The goal is to establish
the authoritative state model and build the smallest set of systems that can
support:

> Explore → settle → develop → research → fight → rewind → rebuild with
> knowledge.

Balance values, final presentation, procedural content, multiplayer, and
post-run progression are outside the first implementation pass.

## Current Repository Status

The current checkout contains a Unity 6 project under `Starwatch/` with the
template scene and tutorial scripts, but no gameplay runtime code yet.

For implementation, `Starwatch/` is the authoritative Unity project root and
gameplay content belongs under `Starwatch/Assets/_Project/`. The namespace root
remains `EndlessHeros` to follow the existing project conventions; it should
not be inferred from the Unity folder name.

## Engineering Principles

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

## Runtime Architecture

### Layer 1: Content definitions

ScriptableObjects describe authored content:

- `ScenarioDefinition`
- `MapNodeDefinition`
- `SystemDefinition`
- `PlanetDefinition`
- `PlanetArchetypeDefinition`
- `BuildingDefinition`
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

- `BootstrapLoader`
- `ProjectSystems`
- `ProjectLifetimeScope`
- `GameEntryPoint`
- `SceneFlow`
- `SceneId`
- `ISceneLoader`

The flow is:

1. Unity loads `Bootstrap.unity`.
2. `BootstrapLoader` ensures `ProjectSystems` exists and is marked
   `DontDestroyOnLoad`.
3. `ProjectSystems` owns `UpdateManager` and `ProjectLifetimeScope`.
4. `ProjectLifetimeScope` builds the project container.
5. `GameEntryPoint` loads `MainMenu.unity` after the container is ready.
6. `SceneFlow` loads scene-specific scopes as children of the project scope.
7. The main menu starts a run through `RunManager`.
8. `SceneFlow` loads `Overworld.unity` with the active run available.
9. A battle request loads `Battle.unity` additively; the overworld scene and
   scope remain active.
10. Completing the battle unloads the battle presentation and returns focus to
    the overworld without recreating the run.

`SceneFlow` owns scene transitions. Views and feature services must not call
`SceneManager.LoadScene` directly.

### Project lifetime registrations

`ProjectLifetimeScope` registers:

- `SimulationClock`.
- `UpdateManager`.
- `ProjectSystems`.
- MessagePipe or the project event bus.
- `ContentCatalog` and authored content catalogs.
- `RunManager`.
- `SceneFlow`.
- `GameEntryPoint`.
- `ArchiveService`.
- `BattleService`.
- `EngagementService`.
- `TimelineOutcomeEvaluator`.
- `EffectRegistry`.
- `IRunStateStore` implementation.

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
- `SettlementStarted`
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

- `ContentCatalog`
- `ScenarioCatalog`
- `ResearchCatalog`
- `ShipCatalog`
- `BuildingCatalog`
- `ProductionCatalog`
- `FactionCatalog`

The catalog converts stable IDs into ScriptableObject definitions. Runtime
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

## Phase 0: Simulation Contracts

### Runtime systems

Implement:

- `BootstrapLoader`
- `ProjectSystems`
- `GameEntryPoint`
- `SceneFlow`
- `SceneId`
- `ISceneLoader`
- `ProjectLifetimeScope`
- `MainMenuLifetimeScope`
- `OverworldLifetimeScope`
- `BattleLifetimeScope`
- `RunManager`
- `RunState`
- `ScenarioState`
- `TimelineState`
- `RunArchive`
- `TimelineFactory`
- `ProductionQueueState`
- `ShipyardState`
- `TimelineOutcomeEvaluator`
- `RunOutcome`
- `SimulationClock`
- `EventJournal`
- `ContentCatalog`

`RunManager` owns the current run and exposes read-only access to scenario,
timeline, archive, and current rewind index. It delegates mutations to
feature services.

`ScenarioDefinition` contains a `StartingEmpireDefinition` that specifies the
home system, initial colony, starting buildings, resources, and starter fleet.
`TimelineFactory` materializes this setup for a new run or rewind, then applies
the run archive's unlocked capabilities. It must not copy old colonies,
fleets, resources, production queues, or building levels into the new
timeline.

### Data contracts

At minimum:

- `RunState`: seed, scenario, timeline, archive, rewind index.
- `ScenarioState`: generated map, fixed run-level content, and starting empire
  setup.
- `TimelineState`: mutable empire state, including colonies, fleets,
  production queues, shipyards, faction relationships, temporary visibility,
  and active crises.
- `RunArchive`: map knowledge, combat intel, discovered facts, and within-run
  unlocks.
- `EventRecord`: event type, simulation time, source ID, and payload ID.
- `RunOutcome`: continue, timeline defeat, run victory, or run defeat, with an
  explicit reason.

`RunOutcomeRuleDefinition` supplies the scenario-specific conditions. The
initial evaluator can check home-system loss, loss of all viable colonies,
terminal invasion objectives, and final-timeline completion. A lost battle,
lost colony, or exhausted rewind budget is not automatically a timeline
defeat.

### Tests

- Timeline state resets without changing scenario state.
- Archive discoveries survive a timeline reset.
- Completed within-run research remains available after reset.
- Incomplete research progress does not survive reset.
- A new timeline starts with the scenario's defined home system, colony,
  resources, buildings, and starter fleet.
- A new timeline does not share mutable lists, dictionaries, queues, or state
  object references with the previous timeline or scenario state.
- The simulation clock pauses and advances deterministically.
- Events are recorded exactly once where required.
- Invalid commands return stable error codes and do not partially mutate state.
- Losing a battle does not automatically produce a timeline defeat.
- `TimelineOutcomeEvaluator` emits only one terminal outcome for a timeline.
- Bootstrap creates the project container before the main menu loads.
- Project-scoped services survive scene transitions.
- Scene scopes receive the project scope and are disposed correctly.

### Deliverable

A headless test can create a run, advance time, mutate timeline state, rewind,
and verify the three persistence layers without loading a gameplay scene. A
PlayMode smoke test can launch Bootstrap, reach the main menu, start a run, and
load the overworld.

## Phase 1: Authored Scenario and Map Loop

### Application entry and run start

Phase 1 is the first player-facing phase, so it includes the application shell
needed to reach the map:

- Bootstrap scene is build index 0.
- Main menu loads automatically after project initialization.
- `Start Run` creates the authored scenario and initial timeline.
- Overworld loads with the active project and overworld scopes.
- The map is the first interactive screen after starting a run.

Do not add a second run-construction path inside the overworld. The overworld
receives the run created by `RunStartService`.

### Content assets

Create one authored `ScenarioDefinition` containing:

- Four or five important systems.
- One or two non-system nodes.
- Connected routes and one or two chokepoints.
- Planet definitions and local modifiers.
- One primary invasion wormhole.
- One minor faction.
- One authored faction force definition for its defensive or aggressive
  response.
- A `StartingEmpireDefinition` with a home system, initial colony, starter
  resources, buildings, and a small starter fleet.
- Fixed discovery, resource, and challenge IDs.

The first scenario should not use procedural generation. A later generator can
produce the same definitions after the runtime contracts are proven.

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
- `FactionState`
- `FactionRelationshipState`
- `FactionService`
- `FactionContactService`
- `FactionThreatService`

Responsibilities:

- Load the authored scenario into runtime state.
- Validate route connectivity.
- Reveal nodes and broad system composition.
- Reveal planet details after scanning.
- Start and complete fleet travel.
- Record discoveries in the archive.
- Initialize the minor faction's ownership, identity, and relationship state.
- Support basic contact, ignore, trade, or hostile-approach decisions.
- Create a simple faction threat request when the player approaches a hostile
  faction; Phase 4 converts it into a fleet engagement.

Faction relationship and remaining force state belongs to the timeline. Faction
identity, incentives, specialty, and authored territory belong to scenario
state and persistent run knowledge. `FactionState` tracks current ownership,
remaining forces, contact status, and hostile-response cooldowns.

### Presentation

Implement:

- `MainMenuView`
- `MainMenuPresenter`
- `StartRunButton`
- `StarMapView`
- `MapNodeView`
- `RouteView`
- `SystemInspectionView`
- `PlanetInspectionView`
- `FactionContactView`
- `FactionStatusView`
- `FogOfWarView`

The map view observes state and events. It does not own the graph, faction
behavior, or reveal hidden content on its own. `VisibilityService` supplies
known map information from the archive and current sensor coverage.

### Tests

- The application starts at Bootstrap and reaches the main menu.
- `Start Run` creates one run and loads the overworld.
- The authored scenario loads with all required IDs.
- The route graph is connected.
- Exploration reveals only the intended information.
- Scanning reveals planet details and records the discovery.
- Travel takes simulation time and arrives at the correct node.
- Invalid travel and unexplored destinations are rejected.
- Starting state comes from `StartingEmpireDefinition`, not loader constants.
- A faction can be contacted, ignored, or approached hostilely.
- A hostile approach can create a faction sortie without requiring full
  diplomacy simulation.

### Deliverable

The player can start from a defined home system and starter fleet, inspect the
map, explore a route, scan planets, and move toward a meaningful expansion or
minor-faction choice.

## Phase 2: Planet and Economy Loop

### Content assets

Create:

- `ResourceDefinition`
- `PlanetArchetypeDefinition`
- `BuildingDefinition`
- `PlanetChallengeDefinition`
- `ProductionRecipeDefinition`
- `ShipyardDefinition`

The first slice needs only a small resource set:

- Industry.
- Energy.
- Science.
- One strategic material.

### Runtime systems

Implement:

- `SettlementService`
- `OutpostService`
- `ColonyService`
- `BuildingService`
- `EconomyService`
- `EconomyTickSystem`
- `ProductionService`
- `ProductionSystem`
- `ProductionQueueService`
- `ShipyardService`
- `ShipConstructionService`
- `ResourceLedger`
- `FrontierPressureService`
- `ColonyStabilityService`
- `PlanetChallengeService`

Responsibilities:

- Start an outpost or settlement.
- Apply settlement costs and escalating commitment costs.
- Create and remove `ColonyState`.
- Validate building capacity and level upgrades.
- Calculate archetype multipliers without hard-locking other buildings.
- Produce and consume resources on simulation ticks.
- Enqueue ship construction jobs at an active shipyard.
- Spend industry, energy, strategic material, and build time according to the
  selected production recipe.
- Persist active production queues and shipyard state in the timeline.
- Emit `ShipConstructionCompleted` when a job finishes so fleet systems can
  receive the completed ships.
- Apply upkeep, Frontier Pressure, and stability effects.
- Reveal a deterministic local challenge after commitment.
- Resolve, delay, abandon, or partially complete the challenge.

Economy calculations should be pure functions where possible. The tick system
collects production and applies the resulting ledger changes; individual views
should never calculate income.

### Presentation

Implement:

- `PlanetDevelopmentView`
- `BuildingListView`
- `ResourceSummaryView`
- `SettlementCommitmentView`
- `PlanetChallengeView`
- `ColonyStatusView`
- `ShipyardProductionView`

The challenge view must clearly distinguish known warning signs from the
unknown exact challenge.

### Tests

- Settlement consumes the correct cost and creates a colony.
- Settlement cost increases according to the configured curve.
- Building capacity and level requirements are enforced.
- Archetype multipliers affect output without blocking off-specialization
  buildings.
- Production and upkeep update correctly over a simulated interval.
- A shipyard queue spends the configured resources over time.
- A completed ship construction job emits the expected ship output.
- Production queues and partially completed jobs reset on rewind.
- Frontier Pressure changes the intended costs or risks.
- A challenge is deterministic within a run.
- Abandoning a challenge applies the documented sunk cost and state.

### Deliverable

The player must choose between at least two useful planets because settling
both immediately creates a measurable economic or logistical burden. The player
can then turn one developed shipyard into actual fleet strength through a
visible production queue rather than test-only fleet spawning.

## Phase 3: Research and Discovery Loop

### Content assets

Create:

- `ResearchDefinition`
- `ResearchPrerequisite`
- `ResearchLeadDefinition`
- `BlueprintDefinition`
- `TechnologyEffectDefinition`
- `ResearchDomainDefinition`

The first pool should contain:

- Core construction, energy, travel, and fleet technologies.
- One economic specialization.
- One fleet specialization.
- One discovery-gated path.

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
- Start one active project.
- Advance the project from global research rate.
- Refresh offers after selection or completion.
- Add discovered leads with the configured weighting.
- Record blueprints and requirements in the archive.
- Make completed research available across rewinds within the run.
- Register technology capabilities and modifiers through `EffectRegistry`.
- Leave physical labs, production, and resources in timeline state.

Research effects should be represented as data-driven modifiers or unlock IDs,
not hard-coded checks spread across economy and combat services.

### Presentation

Implement:

- `ResearchView`
- `ResearchOfferView`
- `ResearchProgressView`
- `DiscoveryNotificationView`
- `ArchiveResearchView`

The UI should show why an offer is eligible, what it unlocks, and which
prerequisites or discoveries are missing.

### Tests

- Core technologies remain available in the pool.
- Prerequisites include or exclude projects correctly.
- A project cannot be started while another project is active in the MVP.
- Research rate advances progress deterministically.
- Completing research changes the intended capability.
- Production, travel, or combat can observe the completed capability through
  `IEffectQuery` without checking a research ID directly.
- A discovery reveals the correct path without granting the final technology.
- Completed research persists across rewinds.
- Incomplete project progress resets.

### Deliverable

At least one discovery should make a different fleet or economy plan more
attractive than the default plan.

## Phase 4: Fleet and Battle Loop

### Content assets

Create:

- `ShipClassDefinition`
- `WeaponDefinition`
- `DefenseDefinition`
- `FleetRoleDefinition`
- `CombatEffectDefinition`
- `DebrisDefinition`

The first slice can use three roles:

- Screen or fast brawler.
- Heavy line ship or artillery.
- Carrier or strike-craft platform.

Use a small number of weapons and defenses. Shields, armor, hull, ranges,
retreat, and one soft counter are enough to validate the combat structure.

### Runtime systems

Implement:

- `FleetService`
- `FleetCompositionService`
- `FleetMergeService`
- `FleetMovementService`
- `BattleService`
- `EngagementService`
- `BattleSimulation`
- `TargetSelectionService`
- `RangeBandService`
- `DamageResolutionService`
- `RetreatService`
- `RepairService`
- `DebrisService`
- `DefenseStationService`

Responsibilities:

- Add completed ship-construction output to fleets or a shipyard reserve.
- Create and modify fleets from available ships.
- Convert authored faction threat requests into faction fleets.
- Require an explicit merge command.
- Move fleets through the route graph.
- Create a battle when opposing fleets meet or a defense is attacked.
- Track an `ActiveEngagementState` containing separate participating fleet IDs.
- Add an arriving fleet to an active engagement without merging it.
- Select targets according to ship role.
- Advance combat in deterministic simulation ticks.
- Apply shield, armor, hull, penetration, and destruction rules.
- Allow a retreat order subject to the disengagement rule.
- Preserve damaged ships and create debris from destroyed ships.
- Resolve station and planetary defense participation.
- Record observed enemy composition, weapons, defenses, and wreck information
  through `ArchiveService` as soon as it becomes known.

`BattleService` and `ActiveEngagementState` are project-scoped. The overworld
remains loaded while `Battle.unity` is loaded additively as a presentation
scene. `BattleSimulation` subscribes to the existing project
`SimulationClock`; it does not create a second galaxy clock. Overworld travel,
economy, and invasion systems remain registered, so a fleet can arrive during
an engagement and join as a separate participant.

`BattleSimulation` should be a plain C# resolver driven by a managed tick. A
battle view can visualize its events, but visual animation must not determine
the result. `BattleLifetimeScope` registers the battle tickable when the
additive scene loads and unregisters it when the scene unloads. This prevents
duplicate ticks or dropped registrations across repeated engagements. If the
player pauses the simulation, both strategic and battle simulation pause
together.

### Presentation

Implement:

- `FleetMapView`
- `FleetCompositionView`
- `BattleView`
- `BattleEventView`
- `BattleResultView`
- `DebrisView`

The first battle presentation can use simple shapes and health bars. It must
show enough information to explain targets, damage, retreat, destruction, and
debris without requiring individual ship micromanagement.

### Tests

- Fleet composition and explicit merge behavior work correctly.
- Ships produced by a shipyard can enter a fleet without test-only spawning.
- Targeting prefers the configured role targets.
- Range bands affect eligible targets and damage opportunities.
- Shields, armor, hull, and penetration resolve correctly.
- A ship at zero hull is destroyed.
- Retreat removes a surviving fleet according to the disengagement rule.
- Damaged ships remain damaged after battle.
- Destroyed ships produce recoverable debris.
- Station defenses participate without becoming mobile fleets.
- Combat results are deterministic for the same inputs and seed.
- A reinforcement joins an active engagement without merging fleets.
- Unloading the battle presentation does not destroy active engagement state.
- Loading and unloading a battle presentation does not double-register its
  simulation tick.
- A lost battle reports attrition without automatically ending the timeline.
- Combat observations are written to persistent run intel.

### Deliverable

Two differently composed fleets should produce visibly different outcomes
against the same opposing fleet, without requiring mid-battle unit commands.

## Phase 5: Invasion Loop

### Content assets

Create:

- `InvasionDefinition`
- `InvasionPhaseDefinition`
- `InvasionWaveDefinition`
- `SabotageDefinition`
- `CrisisResolutionDefinition`
- `InvasionObjectiveDefinition`

The first scenario needs:

- Signal or warning phase.
- One scout fleet.
- One sabotage crisis.
- One major invasion wave.
- One repair or reinforcement behavior.

### Runtime systems

Implement:

- `InvasionDirector`
- `InvasionScheduleService`
- `InvasionTickSystem`
- `InvasionFleetSpawner`
- `InvasionTargetService`
- `InvasionMovementService`
- `SabotageService`
- `CrisisService`
- `RepairFleetService`
- `InvasionObjectiveService`

Responsibilities:

- Advance fixed escalation milestones.
- Spawn the authored scout and invasion fleets.
- Generate warnings before major events.
- Choose simple eligible targets.
- Move invasion fleets through known routes.
- Create and track a crisis with a response window.
- Apply recoverable sabotage consequences.
- Withdraw damaged fleets toward repair support.
- Keep high-level enemy composition fixed while allowing simple target
  selection and regrouping.
- Report invasion objectives, home-system loss, and other outcome conditions to
  `TimelineOutcomeEvaluator` rather than ending the timeline directly.

The first implementation should not dynamically redesign the enemy fleet to
counter the player.

### Presentation

Implement:

- `InvasionStatusView`
- `ThreatWarningView`
- `CrisisView`
- `InvasionFleetView`
- `ObjectiveView`

Warnings should identify what is known, what is suspected, and how much time
remains without revealing every solution automatically.

### Tests

- Escalation milestones occur at deterministic simulation times.
- Warnings precede the intended events.
- Scout and main fleet compositions match the scenario definition.
- Sabotage creates a crisis instead of applying unexplained immediate damage.
- Crisis resolution and expiry apply the correct consequences.
- Damaged invasion fleets can withdraw and create interception opportunities.
- Minor target-selection variation does not change the invasion's high-level plan.
- Losing one battle or colony does not end the timeline unless the configured
  outcome rules are met.
- A terminal invasion objective produces one run outcome.

### Deliverable

The player can recognize an approaching threat, prepare a response, suffer
meaningful attrition, and continue playing after a partial defensive failure.

## Phase 6: Rewind Loop

### Runtime systems

Implement:

- `RewindService`
- `TimelineResetService`
- `ArchiveService`
- `TimelineFactory`
- `RunCompletionService`
- `TimelineEndService`

Responsibilities:

- End a timeline through defeat or voluntary rewind.
- Evaluate timeline and run outcomes from one shared rule set.
- Record discoveries immediately before reset.
- Dispose or clear timeline-owned state.
- Recreate the starting empire from the scenario and archive.
- Increment the rewind index.
- Restore completed within-run research and discovered access.
- Reset colonies, fleets, resources, production queues, shipyards, faction
  relationships, temporary visibility, and active crises.
- Keep scenario locations, routes, and event identities stable.

The reset should be explicit and testable. Avoid relying on destroying and
reloading arbitrary scene objects as the persistence mechanism.

`TimelineEndService` is the only service that finalizes a timeline. Battle,
invasion, economy, and faction systems report facts or outcome candidates; they
do not each implement their own game-over logic. The rewind budget prevents
additional rewinds, but exhausting it is not itself a defeat outcome.

### Persistence and save boundary

The first vertical slice only needs in-memory rewind state. The runtime should
still expose an abstraction such as `IRunStateStore` so disk saves can be added
without changing feature services.

Later adapters may serialize:

- Scenario seed and scenario ID.
- Archive.
- Current timeline state.
- Rewind index.
- Version metadata.

### Presentation

Implement:

- `TimelineEndView`
- `RewindConfirmationView`
- `RebuildSummaryView`
- `ArchiveView`

The rebuild summary should show what was retained and what must be rebuilt.

### Tests

- Voluntary rewind consumes one timeline.
- Defeat ends the timeline exactly once.
- Timeline state is cleared.
- Scenario state remains stable.
- Archive facts and completed research remain available.
- Resources, colonies, fleets, buildings, and incomplete projects reset.
- Production queues, shipyard state, faction relationships, and temporary
  visibility reset.
- The recreated timeline uses the scenario's starting empire definition.
- A new timeline can use archived information immediately.
- Rewind cannot occur after the run is complete or the rewind budget is spent.
- A lost battle and a defeated timeline are different recorded outcomes.
- A terminal run outcome is emitted once.

### Deliverable

A failed first attempt produces a faster, better-informed second attempt rather
than only a restart from an identical unknown state.

## Phase 7: Integration and Test Harness

### Runtime tooling

Implement:

- `IntegrationScenarioRunner`
- `DebugSimulationControls`
- `SimulationInspector`
- `EventLogView`
- `BalanceTelemetry`
- `ScenarioSeedInput`

Useful debug controls include:

- Pause and change simulation speed.
- Advance one simulation tick.
- Reveal the map.
- Grant resources.
- Complete research.
- Enqueue or complete a ship production job.
- Spawn a test fleet.
- Trigger a crisis.
- Force a rewind.

These controls should be development-only and must call the same services as
normal gameplay.

### End-to-end tests

Add a small number of PlayMode tests that:

- Load the authored scenario.
- Explore and settle a planet.
- Complete a research project.
- Build ships through a shipyard queue and resolve a fleet battle.
- Trigger an invasion event.
- Rewind and verify persistence boundaries.

Keep detailed economic, research, and combat tests in EditMode. Use PlayMode
only to prove that the composed scopes, clock, scenes, and views are wired
correctly.

### Integration metrics

Record enough information to diagnose the slice:

- Simulation time to first expansion.
- Settlement cost and output.
- Research choices and completion times.
- Shipyard queue time, resource spending, and completed ships.
- Fleet strength and battle result.
- Damage, destroyed ships, retreat, and debris.
- Invasion milestone times.
- Crisis resolution and consequences.
- Timeline end reason.
- Run outcome reason.
- Rewind count and archive discoveries.

The metrics are for balancing and debugging, not a player-facing progression
system.

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
  Tests/
    EditMode/
    PlayMode/
```

Namespaces should mirror these folders according to the project conventions:

- `EndlessHeros.Core`
- `EndlessHeros.Overworld`
- `EndlessHeros.Battle`
- `EndlessHeros.UI`
- `EndlessHeros.Data`
- `EndlessHeros.Shared`

## Package and Project Prerequisites

The current Unity project already includes Unity 6, URP, Input System, AI
Navigation, UI Toolkit support, and the Test Framework.

Before implementing the service composition, add or decide on:

- VContainer for LifetimeScopes and dependency injection.
- UniTask for scene flow, asynchronous operations, and future save operations.
- MessagePipe for typed domain events and intents.

Cinemachine and Addressables are not required for the first vertical slice.
UGUI should not be used to build new gameplay UI even though the package is
present.

## Dependency Order

The critical implementation path is:

```text
Phase 0
  ↓
Phase 1 → Phase 2 → Phase 3
                    ↘
Phase 1 → Phase 4 → Phase 5 → Phase 6 → Phase 7
```

Phase 4 can begin once the shared state and clock contracts exist, but its
fleet production and available technologies should be connected after Phases 2
and 3. Phase 6 should not be implemented as a separate restart flow; it must
exercise the same state boundaries established in Phase 0.

## Explicitly Deferred Engineering

Do not add these systems to the first vertical slice:

- Procedural scenario generation.
- Secondary wormholes or multiple active fronts.
- Full diplomacy simulation and complex NPC development.
- Multiplayer synchronization, shared research, or shared rewinds.
- Post-run progression and Ascension.
- Detailed convoy and cargo simulation.
- The complete ship roster and modular ship designer.
- Dynamic enemy fleet redesign.
- Final art, cinematic battle effects, and production-quality animation.

The engineering goal is to prove that the authoritative state model can connect
the systems. Content breadth and presentation can expand after that model has
survived repeated end-to-end runs.
