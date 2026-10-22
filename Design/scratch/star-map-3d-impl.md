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
- **The star system view stays in UI Toolkit.** It moves to 3D when zoom exists and a smooth zoom into a system is worth building.
- **Text stays in UI Toolkit.** Node labels and the hover tooltip are drawn in the UI layer, positioned over their world points.

## Pattern

The 3D map stays in MVP. It is a view of player knowledge that sends intents, the same as the UI map it replaces. The rule against presenters for units and combat applies to the Battle scene, not to map markers.

- **Presenter:** `StarMapPresenter` builds models, reacts to domain events, and sends intents to services. It never touches GameObjects, cameras, or transforms.
- **Contract:** `IStarMapView` is unchanged. The 3D map is another implementation of it.
- **View:** `StarMapScreen` passes each model to the renderer, the label overlay, and the inspection card. It raises the same select, travel, and hover events.
- **View-local behavior:** camera panning, picking math, and fleet-marker movement between updates stay inside the view. If game logic later needs to move the camera, add one intent to the contract (for example `FocusNode(id)`) rather than giving the presenter a camera.
- **Logic stays testable:** any rule about what to show, such as fog filtering or node state, belongs in `StarMapQuery` or the presenter, not in the renderer.

## What Stays and What Changes

**Unchanged:**

- `IStarMapView` and its events (node selected, travel requested, node hovered).
- `StarMapPresenter`, `StarMapQuery`, `StarMapModel`, and the model structs (`StarMapNodeModel`, `MapRouteSegment`, `FleetMarkerModel`, `MapNodePresentationState`).
- Presenter tests and their fake views.
- `StarSystemInspectionView` (the selected-node card).

**Changes:**

- `StarMapScreen` still implements `IStarMapView`, but drives the 3D renderer and label overlay instead of `StarMapView`.
- `StarMapView`, `MapNodeView`, `RouteView`, `FleetMarkerView`, and `MapLayout` (fitting nodes into the panel) are removed once the 3D map covers them, along with their map USS.
- The `map-surface` element in `Overworld.uxml` becomes a transparent region that ignores picking, so the 3D map shows through it. It still hosts the label overlay. The surrounding UI panels keep blocking map input.

## Map Space

- One static `MapSpace` converts between map and world coordinates: world = (x × `kWorldUnitsPerMapUnit`, 0, y × `kWorldUnitsPerMapUnit`).
- `kWorldUnitsPerMapUnit` starts at 10. The current slice positions (0–6) then cover about 60 world units.
- The generator later scales its unit-box output so routes match their target length. The renderer never needs to know which shape made the map.

## Scene Setup

Everything is authored in the Overworld scene and on prefabs, then wired with `[SerializeField]`:

- **Map camera:** perspective, rotated straight down, at a fixed height. It has the camera controller component.
- **Map root:** holds the renderer component and the parent transforms for nodes, routes, and fleet markers.
- **Background:** a starfield quad far below the plane and one or two nebula layers between it and the plane. Shared materials only.
- **Prefabs:**
  - Map node: star glow, a selection ring, and a fleet-here indicator.
  - Route: a line with a shared material.
  - Fleet marker: an arrow or chevron with a count badge.

The Overworld lifetime scope registers the scene components so presenters and screens can receive them.

## Components

| Component | Kind | Responsibility |
| --- | --- | --- |
| `MapSpace` | Static helper | Map ↔ world conversion |
| `MapCameraController` | MonoBehaviour | Reads pan input, moves the camera, clamps it to the map bounds, and raises a moved event. Can center on a node |
| `MapCameraBounds` | Pure helper | The node bounds plus a margin, and clamping a camera position into them |
| `MapPointerInput` | MonoBehaviour | Turns pointer input into node clicks, right-clicks, and hover. Ignores pointers over UI panels |
| `MapPicker` | Pure helper | Intersects the pointer ray with the map plane and returns the nearest node within a pick radius |
| `StarMapWorldPresenter` | MonoBehaviour | Pools node, route, and fleet-marker prefabs. Rebuilds nodes, sets routes, applies node state, and places fleet markers |
| `MapNodeView`, `MapRouteView`, `FleetMarkerView` | MonoBehaviour on each prefab | Apply state with `MaterialPropertyBlock` tints on shared materials |
| `MapLabelOverlay` | UI Toolkit view | Node labels and the hover tooltip, positioned over world points |

### Camera Controller

- **Mouse:** drag pans the map. A small drag threshold separates a drag from a click, so left-click still selects nodes and right-click still orders travel.
- **Keyboard:** WASD and arrow keys pan at a fixed screen-relative speed.
- **Touch:** a one-finger drag pans. A tap selects.
- **Input System:** a `Map` action map with `Pan` (keyboard vector), `Drag` (pointer delta while held), and `Point` (pointer position).
- **Bounds:** the camera stays within the node bounds plus a margin. Bounds update when nodes are rebuilt.
- **Start:** the camera centers on the home system when the Overworld loads.
- **Updates:** runs through `UpdateManager` only while input is active or the camera is easing to a target. It sleeps otherwise.
- **Event:** raises a moved event, so the label overlay repositions only when the camera actually moves.

### Picking

- Picking uses plane intersection plus the nearest node within a radius. Nodes need no physics colliders, it suits touch, and it is easy to test.
- A pointer over any UI Toolkit panel (checked by picking on the panel) never reaches the map.
- Picking clicks a node, right-clicks a node, and sets or clears hover, exactly as the UI node elements do today.

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
- A transit marker is placed at the fraction of the trip already done: (`TotalSeconds` − `RemainingSeconds`) ÷ `TotalSeconds`, along the route.
- Moving markers advance from the simulation clock, not real time, so they follow pause and speed changes. Today's USS transition uses real time and drifts when the game is paused or sped up.
- Marker movement only updates while at least one fleet is in transit.

### Labels and Tooltip

- Labels show for explored nodes only. Each label is placed with `RuntimePanelUtils.CameraTransformWorldToPanel`.
- Positions update when the camera moves or nodes are rebuilt, not every frame.
- The hover tooltip keeps today's content (destination name and travel time) and anchors next to the hovered node.

## Steps

Each step leaves the game playable.

1. **Map space, camera, and background.** Add the map camera, background layers, `MapSpace`, `MapCameraBounds`, and the camera controller with pan input. The UI map still draws on top.
2. **Nodes and picking.** Add `StarMapWorldPresenter` with pooled node prefabs, node states, `MapPicker`, and `MapPointerInput`. Switch `StarMapScreen` to the world presenter for nodes. Make `map-surface` transparent and non-picking.
3. **Routes.** Draw routes, course previews, and active travel lines in 3D.
4. **Fleet markers.** Place parked and transit markers in 3D, driven by the simulation clock.
5. **Labels and tooltip.** Add `MapLabelOverlay` and move the hover tooltip into it.
6. **Cleanup.** Remove `StarMapView`, `MapNodeView`, `RouteView`, `FleetMarkerView`, `MapLayout`, and their USS. Confirm every UI panel blocks map input.

## Tests

These follow the project's UI test rules: logic only, no visual-tree assertions.

- `MapSpace` conversion round-trips.
- `MapPicker` picks the nearest node within the radius, picks nothing outside it, and breaks ties consistently.
- `MapCameraBounds` clamps positions and handles a map smaller than the screen.
- Pan math turns a pointer delta into the matching world delta at the camera's height, so the map stays under the cursor.
- The transit marker position is correct at the start, middle, and end of a trip, and does not move while paused.
- Existing presenter tests pass unchanged.
- One PlayMode smoke test: the Overworld loads, the renderer shows one node per model node, and clicking a node's screen position selects it.

## Performance

- Pooled prefabs: about 35 nodes, about 70 routes, and a few fleet markers.
- Shared materials with property blocks only.
- The camera, labels, and markers update only while something is moving.

## Deferred

- Zoom, then tilt tied to zoom.
- Camera rotation.
- A 3D star system view and a smooth zoom from the galaxy into a system.
- Edge-of-screen panning, a minimap, and centering on events (for example, a threat warning).
- Final node art, star types shown visually, and route effects.

## Open Questions

- Should desktop panning use left-drag (with a threshold), middle-drag, or both?
- Should labels show for every explored node, or only near the hovered or selected one once the galaxy is larger?
- Should keyboard pan speed be in screen space (constant on any map size) or world space?
- On touch, how is travel ordered without a right-click: a button on the selected-node card, or a long press?
