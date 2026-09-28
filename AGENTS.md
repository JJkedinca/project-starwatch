# Project Instructions

## Overview

EndlessHeros is a fantasy roguelike: FTL-style overworld adventure plus auto-battle RTS combat (short-term top-down squad placeholders; target presentation is 2.5D isometric).

- **Active project:** [`EndlessHerosUnity/`](EndlessHerosUnity/) (Unity 6, URP).
- **Legacy reference:** Unreal (`Source/`, `Content/`) is frozen for behavior/design reference only — do not extend it unless explicitly asked.
- **Presentation (short-term battle):** full top-down camera; squads as selectable combat entities with up to 10–20 shape/icon members + world HP/energy bars. Continuous NavMesh battle space (no hex grid). Later: isometric + sprite art.
- **Platform:** Desktop for iteration; architect for mobile (draw calls, pooling, quality tiers).

## Code Style

- Clean C# following common Unity conventions (PascalCase types/methods, private fields use `_camelCase` prefix).
- `const` names use a `k` prefix (`kMaxMembers`, `kTickInterval`).
- Member order in a type: fields / properties / events first, then lifetime methods in lifecycle order (`Awake` → `OnEnable` → `Start` → `Update` / `ManagedUpdate` → `OnDisable` → `OnDestroy`; include `Construct` / `Configure` with begin when present), then other methods.
- Single responsibility — each class does one job.
- Favor composition: interfaces, small components, injected services.
- Prefer ScriptableObjects for content data; avoid hard-coding balance numbers in MonoBehaviours.
- Do not abuse `Update` / `FixedUpdate`. Prefer `UpdateManager` + `ITickable.ManagedUpdate`, MessagePipe events, or UniTask delays/timers.
- Log unexpected edge cases and silent failures (`Debug.LogWarning` / `LogError`) that would break gameplay.
- Comments explain purpose when non-obvious — not line-by-line narration.
- Extract a small options/params struct when a method grows past ~5 parameters.
- Prefer enums or option structs over ambiguous boolean parameters.
- [SerializeField] or other attributes should appear on their own line above the member. Prefer `[field: SerializeField] public T Name { get; private set; }` for Inspector-editable, externally read-only data.
- All function or variable should have access level defined

## Code Organization Guardrails

- Game content lives under `EndlessHerosUnity/Assets/_Project/`.
- Namespaces mirror folders: `EndlessHeros.Core`, `EndlessHeros.Battle`, `EndlessHeros.Overworld`, `EndlessHeros.UI`, `EndlessHeros.Data`, `EndlessHeros.Shared`.
- If a change would add more than ~150 lines to one existing `.cs` file, pause and extract a helper/coordinator.
- Keep managers/services orchestration-focused; move feature state machines into dedicated helpers.
- Prefer clear type names (`Unit`, `RunManager`). Use an `EH` / `I` prefix only where it avoids collisions or marks engine-facing contracts.
- Tests mirror production folders under `EndlessHerosUnity/Assets/_Project/Tests/` (see Tests / Build).

## DI and Patterns

- **VContainer** LifetimeScopes: project scope (Bootstrap / DontDestroyOnLoad) for core managers; scene scopes for Overworld and Battle.
- **MVP for UI only** — Presenters call services; Views stay thin. Do not wrap units/combat in Presenters.
- **UI:** UI Toolkit (UXML + USS + `UIDocument`). Do not build UGUI Canvas/Button hierarchies in code.
- **Gameplay:** systems + MonoBehaviour views (`BattleSimulation`, `CombatService`, `Unit` as squad). Squads (per-member HP summed on the squad, shared energy, member placeholders), not one independent agent per sprite.
- **MessagePipe** for typed cross-system events/intents.
- **UniTask** for async (scene flow, saves, delays).
- **No Unity DOTS/ECS** unless profiling later demands it. Optional plain C# buffers for hot sim state are fine.

## Content / “Data Tables”

- No UE-style DataTables. Use ScriptableObject definitions for units (`UnitDefinition`) plus a `ContentCatalog` for id lookup. Embed small combat data (e.g. `AttackData`) on those definitions rather than separate attack assets.
- Unit variety: MaterialPropertyBlock tints first; masked recolor shader later. Shared materials — do not create unique materials per unit (see Art / Presentation prefab rule).

## Architecture

- Bootstrap scene (`Assets/_Project/Scenes/Bootstrap.unity`, build index 0) runs `BootstrapLoader`, which creates DontDestroyOnLoad `ProjectSystems` (`UpdateManager` + `ProjectLifetimeScope`).
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

- Tests live under `EndlessHerosUnity/Assets/_Project/Tests/` (`EditMode/` for pure logic, `PlayMode/` when Unity frame/scene lifetime is required).
- **Every functional / abstractable type needs tests** — services, managers, registries, coordinators, catalog lookups, combat/resolve helpers, presenters’ logic (not thin MonoBehaviour views, pure DTOs/enums, or empty LifetimeScope shells). Add or extend tests in the same change as the production code.
- Prefer EditMode tests; use PlayMode only when behavior depends on `Update`, scenes, physics, or NavMesh.
- Mirror production folders under Tests (`Core/`, `Battle/`, …).
- No need to run Unity tests or force a compile unless the user asks. User builds / runs Test Runner in the Editor.

## Other Rules

- Do not rename variables or add functionality outside what the user asked.
- Do not move functions or variables unless asked.
- Large future TODOs go in [`TODOs.md`](TODOs.md); small todos can be inline comments.
- Do not need to run linters.
