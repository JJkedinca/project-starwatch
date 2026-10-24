# Star Map 3D Migration and Camera

## Purpose

Move the star map from absolutely positioned UI Toolkit elements to a 3D world-space map with a panning camera. This happens before procedural generation, so generated galaxies go straight onto a map that can pan, and the generator preview can reuse the real renderer.

## Decisions

- **Gameplay data stays 2D.** Map positions, routes, travel time, and rings are unchanged. Height is never gameplay.
- **Camera:** perspective, looking straight down, at a fixed height.
  - No tilt, rotation, or zoom for now.
  - Pan only, by mouse drag, keyboard, or touch drag.
  - Perspective (not orthographic) so background layers below the map plane move slower than the nodes, which gives depth while panning.
- **Depth comes from presentation only:** a starfield and nebula layers below the plane, star glow, and moving fleet markers.
- **The star system view is 3D** (hard-swap via `OverworldSpaceSwitcher`, shared map camera, no tilt). Smooth zoom from galaxy into a system stays deferred until after system space exists.
- **Text stays in UI Toolkit.** Node labels and the hover tooltip are drawn in the UI layer, positioned over their world points.

## Pattern

The 3D map stays in MVP. It is a view of player knowledge that sends intents, the same as the UI map it replaces. The rule against presenters for units and combat applies to the Battle scene, not to map markers.

- **Presenter:** `StarMapPresenter` builds models, reacts to domain events, and sends intents to services. It never touches GameObjects, cameras, or transforms.
- **Contract:** `IStarMapView` is unchanged. The 3D map is another implementation of it.
- **View:** `StarMapView` passes each model to the world view, the label overlay, and the inspection card. It raises the same select, travel, and hover events.
- **View-local behavior:** camera panning, picking, and fleet-marker movement between updates stay inside the view. Shared pointer gestures live in `Core/WorldInput` so Battle can reuse them.
- **Logic stays testable:** any rule about what to show, such as fog filtering or node state, belongs in `StarMapQuery` or the presenter, not in the renderer.

## What Stays and What Changes

**Unchanged:**

- `IStarMapView` and its events (node selected, travel requested, node hovered).
- `StarMapPresenter`, `StarMapQuery`, `StarMapModel`, and the model structs (`StarMapNodeModel`, `MapRouteSegment`, `FleetMarkerModel`, `MapNodePresentationState`).
- Presenter tests and their fake views.
- `StarSystemInspectionView` (the selected-node card).

**Changes:**

- `StarMapView` still implements `IStarMapView`, but drives the 3D world view and label overlay instead of the old UITK map view.
- The `map-surface` element in `Overworld.uxml` is a transparent region that ignores picking, so the 3D map shows through it. It still hosts the label overlay. HUD panels block map input via `UiPointerBlocker` (picking mode), not a name list.

## Map Space

- One static `MapSpace` converts between map and world coordinates: world = (x × `kWorldUnitsPerMapUnit`, 0, y × `kWorldUnitsPerMapUnit`).
- `kWorldUnitsPerMapUnit` starts at 10. The current slice positions (0–6) then cover about 60 world units.
- The generator later scales its unit-box output so routes match their target length. The renderer never needs to know which shape made the map.

## Scene Setup

Everything is authored in the Overworld scene and on prefabs, then wired with `[SerializeField]`:

- **Map camera:** perspective, rotated straight down, at a fixed height. It has `MapCameraController` and `WorldPointerInput`.
- **Map root:** holds `StarMapWorldView` / `MapPointerInput` and the parent transforms for nodes, routes, and fleet markers.
- **Background:** a starfield quad far below the plane and one or two nebula layers between it and the plane. Shared materials only.
- **Prefabs:**
  - Map node (`MapNode` layer): star glow, selection ring, fleet-here indicator, trigger sphere collider.
  - Route: a line with a shared material.
  - Fleet marker (`MapFleet` layer): chevron, trigger collider, kinematic rigidbody.
- **Input:** `Assets/_Project/Input/StarwatchInput.inputactions` (`WorldPointer` + `MapCamera` maps).

## Components

| Component | Kind | Responsibility |
| --- | --- | --- |
| `MapSpace` | Static helper | Map ↔ world conversion |
| `WorldPointerInput` | MonoBehaviour | Reads pointer actions, UI-blocks, runs `PointerGestureTracker`, re-raises click/drag/long-press/hover |
| `PointerGestureTracker` | Pure C# | Single place that classifies click vs drag vs long press |
| `UiPointerBlocker` | Static helper | Panel pick with Y flip; document roots pass through; children block unless `picking-mode="Ignore"` |
| `WorldPicker` | Static helper | Exact physics ray, then pixel-tolerance sphere cast, by layer mask |
| `MapCameraController` | MonoBehaviour | Consumes `WorldPointerInput` drags + `MapCamera/Pan`; clamps bounds; raises moved |
| `MapCameraBounds` | Pure helper | Node bounds plus margin, clamp camera position |
| `MapPointerInput` | MonoBehaviour | Consumes click/long-press/hover; picks `MapNode` via `WorldPicker` |
| `StarMapWorldView` | MonoBehaviour | Pools node/route/fleet views; binds node ids; places fleets |
| `StarSystemWorldView` | MonoBehaviour | Pools star/planet/orbit-ring views for the open system |
| `OverworldSpaceSwitcher` | MonoBehaviour | Galaxy vs system roots, pointer consumer, camera save/restore |
| `MapNodeView`, `MapRouteView`, `FleetMarkerView` | Prefab views | Apply tints from colors the world view resolves |
| `SystemStarView`, `SystemPlanetView`, `SystemOrbitRingView` | Prefab views | System-space visuals |
| `WorldLabelOverlay` / `MapLabelOverlay` | UI Toolkit | Shared world labels; galaxy hover tooltip adapter |
| `StarwatchPresentationDatabaseSO` / `StarMapPresentationSO` / `StarSystemPresentationSO` | ScriptableObjects | Authored map and system colors/sizes |

### Camera Controller

- **Mouse / touch drag:** comes from `WorldPointerInput` (shared drag threshold). A drag never clicks.
- **Keyboard:** WASD and arrows via `MapCamera/Pan`.
- **Bounds:** camera stays within node bounds plus a margin.
- **Start:** centers on the home system when Overworld loads.
- **Updates:** registers as `IFrameTickable` on `IUpdateManager` only while dragging, keyboard pan is held, or easing.

### Picking

- Collider pick on the `MapNode` layer (exact ray, then ~24px sphere cast). Fleet colliders exist but are not in the map pick mask yet.
- `MapNodeView.Bind(NodeId)` so hits resolve without a pick-point list.
- A pointer over blocking UI (anything under a document root that is not `Ignore`) never starts a map gesture.
- **UI authoring:** full-screen layout wrappers must use `picking-mode="Ignore"`. Hide panels with `display: none`, not `opacity: 0`.

### Node and Route States

| Model state | Visual |
| --- | --- |
| Unknown | Hidden |
| Known, not explored | Dim glow, no label |
| Explored | Full glow with label |
| Selected | Selection ring |
| Player fleet here | Fleet indicator |
| Route known or explored | Faint or solid line |
| Course preview (hover) | Dashed or highlighted line |
| Active travel | Bright line from origin to destination |

All of these are `MaterialPropertyBlock` changes on shared materials. Nothing creates a material at runtime.

### Fleet Markers

- A parked fleet sits next to its node.
- A transit marker is placed at the fraction of the trip already done along the route.
- Moving markers advance from the simulation clock, not real time.

### Labels and Tooltip

- Labels show for explored nodes only. Each label is placed with `RuntimePanelUtils.CameraTransformWorldToPanel`.
- Positions update when the camera moves or nodes are rebuilt, not every frame.
- The hover tooltip keeps today's content and anchors next to the hovered node.

## Tests

- `PointerGestureTracker` EditMode: click, drag, UI-blocked press, long press, hover rules.
- `WorldPicker` PlayMode: exact hit, tolerance, layer mask, higher collider wins.
- `MapSpace`, `MapCameraBounds`, `MapPanMath`, transit placement EditMode tests.
- Existing presenter tests unchanged.
- PlayMode smoke: world presenter binds one active node per model node.

## Performance

- Pooled prefabs; shared materials with property blocks.
- Camera and labels update only while something is moving.

## Deferred

- Zoom, then tilt tied to zoom (lands on existing 3D system space).
- Camera rotation.
- Smooth zoom from the galaxy into a system (requires system space — done as hard-swap first).
- Edge-of-screen panning, a minimap, and centering on events.
- Final node art, star types shown visually, and route effects.
- Input mode service (Overworld / Battle / modal action maps).
- Fleet picking on the map (`MapFleet` in the pick mask).
- `BattlePointerInput` on the same `WorldPointerInput`.

## Open Questions

- Should desktop panning use left-drag (with a threshold), middle-drag, or both?
- Should labels show for every explored node, or only near the hovered or selected one once the galaxy is larger?
- Should keyboard pan speed be in screen space (constant on any map size) or world space?
- On touch, travel is ordered by long press (maps to right-click / travel intent) or the inspection Travel button.
