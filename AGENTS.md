# Project Instructions

## Overview

ProjectStarwatch is a fantasy roguelike: FTL-style overworld adventure plus auto-battle RTS combat (short-term top-down squad placeholders; target presentation is 2.5D isometric).

- **Active project:** [`Starwatch/`](Starwatch/) (Unity 6, URP).
- **Legacy reference:** Unreal (`Source/`, `Content/`) is frozen for behavior/design reference only — do not extend it unless explicitly asked.
- **Presentation (short-term battle):** full top-down camera; squads as selectable combat entities with up to 10–20 shape/icon members + world HP/energy bars. Continuous NavMesh battle space (no hex grid). Later: isometric + sprite art.
- **Platform:** Desktop for iteration; architect for mobile (draw calls, pooling, quality tiers).

## Code Style

- Clean C# following common Unity conventions (PascalCase types/methods, private fields use `_camelCase` prefix).
- Name a variable for what it holds. A service variable includes `Service` (`_fleetService`, `travelService`). Be precise and descriptive with naming but not too overboard. Do not use single letter variable names.
- `const` names use a `k` prefix (`kMaxMembers`, `kTickInterval`).
- Member order in a type: fields / properties / events first, then lifetime methods in lifecycle order (`Awake` → `OnEnable` → `Start` → `Update` / `ManagedUpdate` → `OnDisable` → `OnDestroy`; include `Construct` / `Configure` with begin when present), then other methods.
- Single responsibility — each class does one job.
- Favor composition: interfaces, small components, injected services.
- Prefer ScriptableObjects for content data; avoid hard-coding balance numbers in MonoBehaviours.
- Static definitions and scene setup belong on assets, not in startup code. Author content as ScriptableObjects. Author known components on prefabs or scenes and assign them with `[SerializeField]`. Use `AddComponent`, `CreateInstance`, or factory construction only when the object must exist at runtime and cannot be authored ahead of time. Tests may still construct stand-ins.
- Do not abuse `Update` / `FixedUpdate`. Prefer `UpdateManager` + `IFrameTickable.ManagedUpdate` (presentation / real time), `ISimulationSystem` via `ISimulationUpdateManager` (gameplay), MessagePipe events, or UniTask delays/timers.
- Log unexpected edge cases and silent failures (`Debug.LogWarning` / `LogError`) that would break gameplay.
- Comments explain purpose when non-obvious — not line-by-line narration.
- Extract a small options/params struct when a method grows past ~5 parameters.
- Prefer enums or option structs over ambiguous boolean parameters.
- [SerializeField] or other attributes should appear on their own line above the member. Prefer `[field: SerializeField] public T Name { get; private set; }` for Inspector-editable, externally read-only data.
- Every type member must declare an access level. Prefer the narrowest that works: `private` by default, `internal` for same-assembly collaborators (tests via `InternalsVisibleTo`), `public` only for intentional cross-boundary API. Do not default new helpers, constants, or view fields to `public`.

## Code Organization Guardrails

- Game content lives under `Starwatch/Assets/_Project/`.
- Namespaces mirror folders: `ProjectStarwatch.Core`, `ProjectStarwatch.Battle`, `ProjectStarwatch.Overworld`, `ProjectStarwatch.UI`, `ProjectStarwatch.Data`, `ProjectStarwatch.Shared`.
- If a change would add more than ~150 lines to one existing `.cs` file, pause and extract a helper/coordinator.
- Keep managers/services orchestration-focused; move feature state machines into dedicated helpers.
- Prefer clear type names (`Unit`, `RunManager`). Use an `EH` / `I` prefix only where it avoids collisions or marks engine-facing contracts.
- Tests mirror production folders under `Starwatch/Assets/_Project/Tests/` (see Tests / Build).

## DI and Patterns

- **VContainer** LifetimeScopes: project scope (Bootstrap / DontDestroyOnLoad) for core managers; scene scopes for Overworld and Battle.
- **MVP** — Presenters call services; Views stay thin.
- **UI:** UI Toolkit (UXML + USS + `UIDocument`). Do not build UGUI Canvas/Button hierarchies in code.
- The screen UXML is the `UIDocument` source. It owns layout (where regions sit) and instances view templates. A view class that owns a fixed set of controls has its own UXML template; `Bind` takes that template root. Regions code fills at runtime (map nodes, planet icons) stay empty hosts on the screen UXML. Tests may build a stand-in tree with the same element names.
- **Gameplay:** systems + MonoBehaviour views (`BattleSimulation`, `CombatService`, `Unit` as squad). Squads (per-member HP summed on the squad, shared energy, member placeholders), not one independent agent per sprite.
- **DomainEventBus** is the domain event bus. After a service commits state (travel, fleets, exploration, buildings, enemy actions), it publishes a typed event from `DomainEvents`. The bus appends the journal and notifies subscribers on that call. Subscribers react. Do not poll the simulation tick to discover those changes.
- **UniTask** for async (scene flow, saves, delays).
- **No Unity DOTS/ECS** unless profiling later demands it. Optional plain C# buffers for hot sim state are fine.
- **Prefer No Ticking**: Use events when possible instead of lazy refreshing/checks. When a frame tick is required, register an `IFrameTickable` with `IUpdateManager` only while active; do not put presentation work on the simulation clock.

## Content / “Data Tables”

- No UE-style DataTables. Use ScriptableObject definitions for units (`UnitDefinition`) plus an `ObjectDatabaseSO` for id lookup. Embed small combat data (e.g. `AttackData`) on those definitions rather than separate attack assets.
- Unit variety: MaterialPropertyBlock tints first; masked recolor shader later. Shared materials — do not create unique materials per unit (see Art / Presentation prefab rule).
- **Authored config is the contract.** Required ScriptableObject / catalog wiring must be set by the developer on assets. Assert required references and default entries at bootstrap (`ValidateRequired` / `Assert`); do not add soft fallback chains, “resolve to first valid,” or extra lookup helpers that paper over missing static configuration. Look up by the requested id; if it is missing after validation, that is a bug — fail loudly.

## Architecture

- Bootstrap scene (`Assets/_Project/Scenes/Bootstrap.unity`, build index 0) contains DontDestroyOnLoad `ProjectLifetimeScope` and `UpdateManager`. The scope is assigned the `ObjectDatabaseSO`.
- `GameEntryPoint` (`IStartable`) loads MainMenu after the project container builds.
- `SceneFlow` switches MainMenu ↔ Overworld ↔ Battle and carries `EncounterSetup`. Overworld/Battle loads enqueue `ProjectLifetimeScope` as VContainer parent.
- Overworld owns the node/run loop; Battle owns continuous auto-battle simulation on NavMesh.
- Structure code so multiplayer coop can be added later: UI sends intents; simulation/spawn go through services/registry (no NGO in v1).

## Art / Presentation

- **Prefabs over procedural visuals:** author meshes, materials, icons, and world UI (e.g. squad members, HP/energy bars) as Prefabs + shared Material assets under `Assets/_Project/`. Wire them via `[SerializeField]` on spawners/views. Do **not** build visuals with `GameObject.CreatePrimitive`, `new Material(...)`, or `Shader.Find` in gameplay code (tests may still construct minimal stand-ins).
- Tint with `MaterialPropertyBlock` on shared materials — never unique materials per instance.
- URP 3D. Battle v1: top-down + prefab placeholder squad members / bars. Later: isometric + `SpriteRenderer` / 4-dir sheets.
- Animation impact / projectile spawn via Animation Events or SO markers — not magic frame indices in combat code.
- Keep mobile budgets in mind: atlases, pooling, one sim ticker.

## Packages (planned / preferred)

- VContainer, UniTask, MessagePipe (+ VContainer integration), Cinemachine.
- Keep: URP, Input System, AI Navigation, UGUI, Test Framework.
- Later if needed: Addressables, spreadsheet→SO importer, tween library.

## Tests / Build

- Tests live under `Starwatch/Assets/_Project/Tests/` (`EditMode/` for pure logic, `PlayMode/` when Unity frame/scene lifetime is required).
- **Every functional / abstractable type needs tests** — services, managers, registries, coordinators, catalog lookups, combat/resolve helpers, presenters’ logic (not thin MonoBehaviour views, pure DTOs/enums, or empty LifetimeScope shells). Add or extend tests in the same change as the production code.
- UI tests target logic, not the visual tree. Test query/model builders, selection/navigation state, and presenter intent → service / domain event → refresh wiring (against a fake view). Do not assert on visual-tree element names, USS classes, or exact display strings, and do not test thin views. At most one end-to-end smoke test per screen.
- Prefer EditMode tests; use PlayMode only when behavior depends on `Update`, scenes, physics, or NavMesh.
- Mirror production folders under Tests (`Core/`, `Battle/`, …).
- No need to run Unity tests or force a compile unless the user asks. User builds / runs Test Runner in the Editor.

## Other Rules

- Do not rename variables or add functionality outside what the user asked.
- Do not move functions or variables unless asked.
- Large future TODOs go in [`TODOs.md`](TODOs.md); small todos can be inline comments.
- Do not need to run linters.
