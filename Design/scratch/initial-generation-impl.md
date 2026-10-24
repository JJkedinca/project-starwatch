# Initial Generation Implementation

## Purpose

This document lists the concrete content and rules for the first procedural galaxy. It replaces the fixed vertical-slice map.

The high-level intent lives in the design docs and is not repeated here:

- [universe-and-travel.md](Design/gameplay/universe-and-travel.md)
- [star-systems.md](Design/gameplay/star-systems.md)
- [planets.md](Design/gameplay/planets.md)
- [economy-and-building.md](Design/gameplay/economy-and-building.md)
- [fleet-combat.md](Design/gameplay/fleet-combat.md)

Every number here is a starting value for playtesting, not final balance.

## Goals

- Every run offers several viable expansion directions.
- The early region always supports a basic economy. It rarely contains a jackpot.
- Rare and build-defining content sits farther out, behind more travel and risk.
- Planets have strong identities. Local modifiers add variety without erasing them.
- The same run seed always produces the same galaxy, so the map stays stable across all five timelines.

## Content Catalog

### Resources

| Resource | Category | Stockpiled | Source | Main uses |
| --- | --- | --- | --- | --- |
| Industry | Baseline | Yes | Industry Works, Heavy Foundry | Construction, colonies, ships |
| Energy | Baseline | Yes | Power Plant, Fusion Reactor | Building upkeep, later fleet upkeep |
| Science | Baseline | No (a rate) | Laboratory, Research Institute | Research progress |
| Strategic | Specialized | Yes | Extractor, boosted by Strategic deposits | Advanced buildings, larger ships, upgrades |
| Uranium | Build-defining | Yes | Uranium Mine on a Uranium deposit | Nuclear power, nuclear-drive ships |
| Exotic Crystals | Build-defining | Yes | Crystal Harvester on a Crystals deposit | Energy weapons, shields, advanced energy |
| Precursor Alloy | Build-defining | Yes | Precursor Excavation on a Precursor deposit | Relic buildings, blueprint ship classes |

Deferred:

- **Food or life support:** waits for colony stability and population.
- **Influence:** waits for diplomacy.

Neither has anything meaningful to spend on until those systems exist.

### Deposits

- **Strategic deposit:** adds +1 to the planet's Strategic multiplier.
- **Rare deposit (Uranium, Exotic Crystals, Precursor Alloy):** allows that resource's extraction building on the planet. It adds no multiplier, and planet archetypes have no multiplier for rare resources.
- A rare resource can only be produced on a colony that has its deposit.
- All deposits are hidden until the planet is scanned.

### Planet Archetypes

The archetype is a planet's strongest modifier. Its output multipliers are fixed. Variation comes from the star, the system, and local modifiers.

| Archetype | Tier | Industry / Energy / Science / Strategic | Footprint | Strategic deposit chance |
| --- | --- | --- | --- | --- |
| Homeworld (home only) | — | 1 / 1 / 1 / 1 | 10 | 0% |
| Barren rock | Common | 1 / 1 / 0.75 / 1.25 | 2–4 | 20% |
| Gas giant | Common | 0.5 / 2.5 / 1 / 0.5 | 2–4 | 0% |
| Mining world | Common | 1 / 0.75 / 0.5 / 2 | 4–6 | 40% |
| Ice world | Common | 0.75 / 0.5 / 1.5 / 1.25 | 4–6 | 15% |
| Asteroid cluster | Common | 0.75 / 0.5 / 0.5 / 2.5 | 2 | 50% |
| Industrial world | Uncommon | 2 / 0.75 / 0.5 / 1 | 6–8 | 25% |
| Oceanic world | Uncommon | 0.5 / 1 / 2 / 0.75 | 6–8 | 0% |
| Volcanic world | Uncommon | 1.25 / 2 / 0.5 / 1.25 | 4–6 | 35% |
| Temperate world | Uncommon | 1 / 1 / 1 / 1 | 8–12 | 10% |
| Ruined world | Rare | 0.5 / 1 / 2.5 / 1 | 4–6 | 10% |

- Footprint rolls in steps of two, because every building occupies two.
- Every archetype can be colonized. Gas giants and Asteroid clusters are orbital stations, which is why their footprint is small.
- Every Ruined world has the Precursor ruins modifier as an archetype trait. It does not use one of the planet's modifier rolls.
- Shipyard and Fortress worlds need ship-production and defense stats that do not exist yet. They become archetypes when those stats exist.

### Planet Modifiers

Each planet rolls zero to two modifiers by tier, with no duplicates. A modifier with eligible archetypes listed can only roll on those archetypes.

| Modifier | Tier | Eligible archetypes | Effect |
| --- | --- | --- | --- |
| Rich deposit | Common | Any | Adds a Strategic deposit |
| Mineral veins | Common | Any | Industry +0.25 |
| Thin crust | Common | Any | Footprint −2 (minimum 2) |
| Geothermal | Uncommon | Any | Energy +0.5 |
| Ancient infrastructure | Uncommon | Any | Footprint +2 |
| Harsh environment | Uncommon | Any | Colony cost +25% |
| Uranium seam | Rare | Volcanic, Mining, Barren rock, Asteroid cluster | Adds a Uranium deposit |
| Crystal formations | Rare | Ice, Barren rock, Asteroid cluster | Adds an Exotic Crystals deposit. Energy +0.5 |
| Precursor ruins | Rare | Any except Gas giant | Adds a Precursor Alloy deposit |
| Precursor archive | Rare | Any | Science +1. Later also reveals a research lead |
| Abandoned shipyard | Rare | Any | The colony starts with a Shipyard |
| Reactor core | Exceptional | Volcanic, Mining | Energy doubled after archetype. Adds a Uranium deposit |
| Forge relic | Exceptional | Any | Industry +2 |

- Additive effects (+0.25, +1) apply to the archetype multiplier. Multiplicative effects apply after them.
- Crystal formations roll at ×2 weight in Pulsar systems.
- Exceptional modifiers create the build-defining worlds. A run is never required to find one.

### Star Types

| Star | Tier | Effect |
| --- | --- | --- |
| Yellow | Common | Neutral |
| Red dwarf | Common | Energy ×0.8. Rocky planets weighted up |
| Binary | Uncommon | +1 planet |
| Blue giant | Uncommon | Energy ×1.4. Footprint −1 on Temperate and Oceanic planets |
| Pulsar | Rare | Science ×1.3. Footprint −2. Ice and Ruined planets weighted up |

### Star System Archetypes

System archetypes use their own per-ring weights instead of rarity tiers.

| Archetype | Inner / Middle / Outer / Rim weight | Planets | Effect |
| --- | --- | --- | --- |
| Normal | 55 / 35 / 25 / 15 | 2–4 | None |
| Big | 5 / 10 / 10 / 10 | 5–6 | More competing choices in one place |
| Dead | 10 / 10 / 10 / 15 | 1–2 | Mostly barren planets. Reserved for a hidden discovery later |
| Frontier | 0 / 20 / 30 / 30 | 3–4 | Planet rolls shift one tier toward rarer. Local hostiles later |
| Anomaly | 0 / 10 / 15 / 20 | 2–3 | One system-wide rule (see below) |

**Junction** is a tag, not a rolled archetype. Any star system with four or more routes is a junction, and the map shows it as a chokepoint. A junction keeps its rolled archetype.

Faction, Relay, Warzone, and Invasion archetypes wait until diplomacy, route infrastructure, and the invasion exist.

An Anomaly system rolls one rule that applies to every planet in it:

| Rule | Effect |
| --- | --- |
| Ion storm | Science ×1.25, footprint −1 |
| Dense nebula | Strategic ×1.25. Planet archetypes stay hidden until each planet is scanned |
| Gravity well | Industry ×1.25. Travel time on this system's routes is +25% |
| Radiant core | Energy ×1.5, footprint −1 |

### Non-System Nodes

Non-system nodes have no planets and cannot be settled. Until their mechanics exist, they act as route nodes only.

| Node | Tier | Later role |
| --- | --- | --- |
| Asteroid field | Common | Remote extraction |
| Empty transit | Common | Route junction or pacing. At most two in a row along any route |
| Nebula | Uncommon | Travel or sensor hazard |
| Derelict | Rare | Research lead or blueprint |

### Buildings

Every building has a footprint of two. Rates are per second at level 1 and scale linearly with level. Planet multipliers change output, not upkeep.

| Building | Output | Upkeep | Cost | Max level / upgrade costs | Requires |
| --- | --- | --- | --- | --- | --- |
| Industry Works | 1 Industry | 0.25 Energy | 40 Industry | 3 / 60, 120 Industry | — |
| Power Plant | 1.5 Energy | — | 30 Industry | 3 / 45, 90 Industry | — |
| Laboratory | 0.5 Science | 0.5 Energy | 50 Industry | 3 / 80 Industry; 150 Industry + 10 Strategic | — |
| Extractor | 0.5 Strategic | 0.25 Energy | 40 Industry | 2 / 80 Industry | — |
| Shipyard | — | 1 Energy | 80 Industry | 1 | — |
| Uranium Mine | 0.2 Uranium | 0.5 Energy | 60 Industry + 10 Strategic | 2 / 100 Industry + 20 Strategic | Uranium deposit |
| Crystal Harvester | 0.2 Exotic Crystals | 1 Energy | 60 Industry + 10 Strategic | 2 / 100 Industry + 20 Strategic | Exotic Crystals deposit |
| Precursor Excavation | 0.1 Precursor Alloy | 0.5 Energy | 80 Industry | 2 / 120 Industry + 20 Strategic | Precursor Alloy deposit |
| Fusion Reactor | 4 Energy | 0.25 Strategic | 100 Industry + 20 Strategic | 2 | Fusion research |
| Research Institute | 1.5 Science | 1 Energy | 120 Industry + 15 Strategic | 2 | Research Institute research |
| Heavy Foundry | 3 Industry | 1 Energy + 0.25 Strategic | 150 Industry + 30 Strategic | 2 | Heavy Industry research |

- The home colony starts with Industry Works and a Power Plant. That leaves six footprint free for the player's first choice.
- Rare extraction buildings need their deposit but no research. Research controls what rare resources can be spent on.
- At level 1, a Uranium Mine yields 12 Uranium per minute. Specialized buildings and ships should cost tens of a rare resource, not hundreds. One extraction colony supports a strategy; a second speeds it up.

### Ships

Each class has a name and a combat role. The role drives targeting and composition. Later classes can share a role, such as a missile cruiser in the Line role.

| Class | Role | Cost | Build time | Speed | Requires |
| --- | --- | --- | --- | --- | --- |
| Survey Corvette | Scout | 20 Industry | 15 seconds | 1.5 | — |
| Destroyer | Screen | 30 Industry | 20 seconds | 1.0 | Basic fleet research |
| Cruiser | Line | 60 Industry + 10 Strategic | 40 seconds | 0.5 | Basic fleet research |
| Carrier | Carrier | 80 Industry + 20 Strategic | 50 seconds | 0.75 | Carrier research |

The starting fleet is two Survey Corvettes and one Destroyer. A large galaxy makes exploration speed an early decision.

## Generation Rules

### Pipeline

Generation runs in fixed stages. Only the first stage depends on the galaxy shape; every later stage works on the resulting graph.

1. **Layout (shape-specific).** Place node positions, choose the home node, label each node's sector, and propose candidate routes with costs.
2. **Connect routes.** Pick routes from the candidates so the graph is connected and each node reaches its degree target.
3. **Assign rings.** Measure each node's distance from home along routes and cut the nodes into ring bands.
4. **Reserve roles.** Place the home system, the hostile-neighbor region, and the wormhole systems.
5. **Apply sector bias.** Give each sector a content family.
6. **Roll node content.** For each node, roll its kind (star system or non-system node), then its archetype, then its star type.
7. **Roll planets.** Roll each system's planet count, then each planet's archetype, footprint, and Strategic deposit.
8. **Roll planet modifiers.** Roll zero to two modifiers on each planet, respecting eligible archetypes.
9. **Name everything.** Generate system and planet names from syllable tables.
10. **Validate.** Check every guarantee. If any fails, regenerate with the next attempt number. Log an error after repeated failures.

### Seeds

- Every random choice draws from the run seed. Generation must not read the clock or any other unseeded source.
- Each stage gets its own random stream, derived from the run seed, the stage name, and the attempt number.
- Layout and route streams are independent of content streams. Tuning content weights keeps the same map for a seed, which makes balancing easier.

### Rings

A ring is a progression tier, not a geometric circle. It is measured by travel distance from home: route jumps first, then total travel time to break ties. This keeps "rarer content is farther away" true for every galaxy shape.

Nodes are sorted by distance and cut into bands by share of the total node count:

| Ring | Share of nodes | At 30 nodes | Non-system share | Role |
| --- | --- | --- | --- | --- |
| Home (0) | 1 node | 1 | — | Fixed home system |
| Inner (1) | ~18% | 5–6 | ~20% | Safe baseline economy |
| Middle (2) | ~30% | 8–10 | ~30% | Contested choices and the first real finds |
| Outer (3) | ~35% | 9–12 | ~35% | Rare content, hostile neighbors, and the secondary wormhole |
| Rim (4) | ~17% | 4–6 | ~40% | Build-defining content and the primary wormhole |

The target is 27–35 nodes, which yields about 20–25 star systems and roughly 65–75 planets.

In a circle galaxy, rings look like circles. In a linear galaxy, they are segments along the line. In a spiral, they are stretches along the arms.

### Galaxy Shapes

The galaxy shape is a swappable asset. A shape answers four questions:

1. Where can nodes be placed?
2. Where is home, and which region is "far" (a hint for the primary wormhole)?
3. How are nodes grouped into sectors?
4. Which routes are natural, and what does each one cost?

Every other stage is shared. Adding a shape means adding one new shape asset type, with no changes to routes, rings, roles, content, or validation.

| Shape | Layout | Home | Sectors | Route preference | Feel |
| --- | --- | --- | --- | --- | --- |
| Circle | Evenly spaced points in a disc | Center | 3–4 angular wedges | Nearest neighbors in any direction | Open, many expansion directions |
| Spiral | A small core plus 2–4 curved arms, nodes jittered along each arm | Partway along one arm, not the core | One per arm | Cheap along an arm, expensive across. 1–2 bridges per neighboring arm pair | Lanes joined by chokepoint bridges |
| Linear | A long band two or three lanes wide, with occasional side pockets | One end | One per lane; side pockets join the nearest lane | Forward and back along the band, few lateral links | One clear front. The wormhole sits at the far end |
| Ring (later) | A hollow donut | One side of the ring | The two halves | Around the ring; rare links across the hollow | Two fronts that meet opposite home |
| Clusters (later) | 4–6 islands of 4–7 nodes | Inside one cluster | One per cluster | Dense inside a cluster, single bridges between clusters | Strong, memorable chokepoints |

Shared parameters live on the generation-settings asset: target node count, minimum spacing between nodes, ring shares, route degree range, target seconds per jump, and the weighted list of shapes a run can roll.

### Implementing Shapes

A shape describes geometry only. Placing nodes, building candidate routes, and connecting them are shared algorithms that call into the shape.

#### Shape Contract

Every shape derives from one abstract `GalaxyShapeSO`. It works in normalized space: the shape fits inside a unit box, and the generator scales the result to map coordinates at the end.

| Member | Required | Purpose |
| --- | --- | --- |
| `Density(point)` | Yes | Returns 0 to 1. 0 means no node can sit there. Higher values attract more nodes |
| `HomeAnchor` | Yes | The point where the home node is placed |
| `FarAnchor` | Yes | The point the primary wormhole should be near. Rings still come from route distance |
| `Describe(point)` | Yes | Returns the node's sector and shape tags: arm or lane index, and progress along it from 0 to 1 |
| `RouteCost(a, b)` | No | Cost multiplier for a candidate route. Default 1. Infinity forbids the route |
| `RouteGroup(a, b)` | No | Optional group key, such as an arm pair, with a cap on how many routes the group may keep |
| `PlaceNodes(...)` | No | Override only when density sampling is not enough, such as Clusters |
| `MinOutwardRoutes` | No | Minimum routes from each ring into the next. Default 3 |

Each shape asset is configured in the Inspector. Adding a new shape means adding one subclass and one asset.

#### Shared Node Placement

Nodes are placed with best-candidate sampling, which spreads points evenly and always produces exactly the target count:

1. Place the home node exactly at `HomeAnchor`.
2. For each remaining node, draw about 12 candidate points. Draw each by picking a random point in the unit box and keeping it with probability `Density(point)`.
3. Keep the candidate farthest from every existing node. A density bias lets denser areas win ties, so arms and lanes fill first.
4. Reject the layout, and move to the next attempt, if any two nodes end up closer than the minimum spacing.
5. Round positions to three decimal places, then call `Describe` on each node.

#### Shared Route Candidates

1. **Triangulate:** build a Delaunay triangulation of all nodes (Bowyer–Watson; about 35 points is trivial). Its edges are natural neighbor links and never cross, so later steps need no crossing checks.
2. **Prune long edges:** drop any edge longer than 2.5 times the median edge length. This removes the long slivers along a band's edges and across a ring's hollow.
3. **Apply shape rules:** multiply each edge's length by `RouteCost(a, b)` and drop forbidden edges.
4. **Connect:** run Kruskal's algorithm (minimum spanning tree) on cost. If the tree cannot reach every node, reject the layout.
5. **Fill:** add the cheapest remaining edges until each node has two to four routes, respecting each `RouteGroup` cap.

#### Circle

- **Density:** 1 inside a disc of radius 1, fading to 0.6 near the edge (`edgeFalloff`) so the rim is slightly sparser.
- **Home anchor:** the center.
- **Far anchor:** a point on the rim at a seeded random angle, so the wormhole's direction changes per run.
- **Sectors:** the angle from the center, split into `wedgeCount` equal wedges. The whole wedge pattern is rotated by a seeded random offset.
- **Routes:** every cost is 1.
- **Parameters:** `wedgeCount`, `edgeFalloff`.

#### Spiral

- **Arm curves:** each arm `i` is a curve with progress `t` from 0 to 1:
  - radius = `coreRadius + t × (1 − coreRadius)`
  - angle = `armOffset(i) + twist × 2π × t`, where `armOffset(i) = 2π × i / armCount` plus a seeded rotation
- **Density:** for each arm, find the distance `d` to the arm curve by sampling the curve at 64 points. Arm density is `exp(−(d / width(t))²)`, where the width grows slightly toward the tips. The core disc has density 0.6. A point takes the highest density of the core and all arms.
- **Describe:** the sector is the nearest arm. Progress is that arm's `t` at the nearest curve sample. Core nodes are tagged as core and take the sector of their nearest arm.
- **Home anchor:** arm 0 at `t = homeArmPosition` (default 0.45). Starting mid-arm gives the player two directions along their own arm, plus the core.
- **Far anchor:** the tip (`t = 1`) of the arm most opposite arm 0.
- **Routes:**
  - Same arm: cost 1.
  - Core to any arm: cost 1.5. This makes the core a natural junction.
  - Between neighboring arms: cost 4, grouped by arm pair, and capped at `bridgesPerArmPair`.
  - Between non-neighboring arms: forbidden.
- **Parameters:** `armCount`, `twist`, `armWidth`, `coreRadius`, `homeArmPosition`, `bridgesPerArmPair`.

#### Linear

- **Centerline:** a gentle curve from left to right, `y = 0.5 + bend × sin(2π × waves × x + phase)`, with a seeded phase.
- **Lanes:** `laneCount` parallel lanes offset from the centerline by `laneSpacing`.
- **Density:** about 1 on each lane, 0.3 between lanes, and 0 beyond the band. Pockets are small seeded blobs just outside the band, each placed with `sidePocketChance` per tenth of the length.
- **Describe:** the sector is the nearest lane. Progress is `x`. Pocket nodes take the sector of their nearest lane.
- **Home anchor:** the left end on the middle lane. **Far anchor:** the right end on the middle lane.
- **Routes:**
  - Mostly along the band (more change in `x` than in `y`): cost 1.
  - Mostly across lanes: cost `lateralCost` (default 3).
  - Pocket to its own lane: cost 1. Pocket to any other lane: forbidden.
- **Minimum outward routes:** 2.
- **Parameters:** `laneCount`, `laneSpacing`, `bend`, `waves`, `sidePocketChance`, `lateralCost`.

#### Ring (later)

- **Density:** 1 between an inner radius (`hollowRadius`, default 0.5) and 1; 0 in the hollow.
- **Home anchor:** angle 0 at the middle radius. **Far anchor:** the opposite side, at angle π.
- **Sectors:** the two halves on either side of the line from home to the far anchor.
- **Routes:** the long-edge prune removes most links across the hollow. Any that survive form one group capped at `hollowBridges` (default 1).

#### Clusters (later)

- **Overrides `PlaceNodes`:**
  1. Place `clusterCount` centers with best-candidate sampling at a large spacing.
  2. Split the node count across clusters, at least 4 per cluster.
  3. Place each cluster's nodes inside a small disc around its center, using the shared sampler.
- **Sectors:** the cluster index.
- **Home anchor:** the center of a seeded cluster. **Far anchor:** the cluster center farthest from home.
- **Routes:** within a cluster, cost 1. Between clusters, cost 5, grouped by cluster pair and capped at 1, so every link between clusters is a chokepoint.

#### Determinism

- Shapes draw randomness only from the layout stream passed in. Iteration order is fixed by node index, never by dictionary or hash-set order.
- Rounding positions before triangulation keeps small floating-point differences from changing which edges exist. Cross-platform determinism for multiplayer may later need fixed-point math.

#### Preview and Tests

- **Preview window (editor only):** pick a shape asset and a seed, then step through seeds. It draws the density field, nodes colored by sector or ring, candidate routes versus kept routes, and the home and far anchors.
- **Tests for each shape:**
  - The exact node count is produced, and minimum spacing is respected.
  - Home sits at the anchor, and every sector has at least three nodes.
  - The graph is connected, and route-group caps are respected.
  - The same seed gives the same layout.

#### Code Layout

All of this lives under `Core/Generation/`:

- `GalaxyShapeSO`, with `CircleGalaxyShapeSO`, `SpiralGalaxyShapeSO`, and `LinearGalaxyShapeSO`.
- `BestCandidateSampler`, `DelaunayTriangulator`, `CandidateRouteBuilder`, and `RouteConnector`.
- `GalaxyLayout` (the shape stage's output): positions, sector and shape tags, home and far nodes, and the chosen routes.

### Routes

The route connector is shared by every shape:

1. **Candidates:** Delaunay triangulation edges, with long edges pruned and the shape's cost and group rules applied (see [Shared Route Candidates](#shared-route-candidates)).
2. **Connect:** a minimum spanning tree over the candidates guarantees a connected graph with the fewest long links.
3. **Fill:** add the cheapest remaining candidates until each node has two to four routes. Triangulation edges never cross, so no crossing check is needed.
4. **Check outward routes:** after rings are assigned, each ring needs a minimum number of routes into the next ring, spread across sectors. This minimum is a shape parameter: 3 by default, 2 for Linear.

Route rules:

- Routes mostly join nodes in the same ring or adjacent rings. A ring-skipping route is a rare, visible shortcut, capped at two per galaxy.
- Travel time is scaled so the median route takes about 40 seconds at baseline speed, whatever the shape's raw map size. Routes that cross between rings take 30% longer.
- At baseline speed, the trip from home to the Rim takes roughly four to five minutes.
- The corridor graph is fixed for the run. Research and relays change travel speed, not which routes exist.

### Sectors

The shape labels each node's sector. Each sector then gets a ×1.5 weight bias toward one content family, chosen with the content random stream:

- **Industry:** Industrial, Mining, Volcanic.
- **Science:** Oceanic, Ice, Ruined.
- **Energy:** Gas giant, Volcanic.
- **Frontier:** more Frontier systems and more rare rolls.

Rules:

- Industry and Science are always assigned first.
- If a shape has fewer sectors than families, Frontier is dropped first, then Energy.
- If it has more sectors than families, families repeat, but neighboring sectors never share one.

Sector bias gives each expansion direction a readable flavor. It never removes an archetype from a sector.

### Reserved Roles

Some nodes get a role before content is rolled:

- **Home system:** Yellow star, a Homeworld, and two more Common planets. One of those two is a Gas giant or Mining world.
- **Hostile-neighbor region:** Two or three adjacent Outer-ring systems in one sector. This is the planned Rewind 0 failure. Until neighboring factions exist, these systems use the Frontier archetype.
- **Primary wormhole:** One Rim-ring system.
- **Secondary wormhole:** One Outer- or Rim-ring system in a different sector from the primary.

The wormhole systems roll normal content for now. Their invasion behavior arrives with the invasion phase. Their locations stay stable for the run.

### Rarity

A template's selection weight is its tier's base weight times the ring multiplier. This applies to star types, planet archetypes, planet modifiers, and non-system nodes.

| Tier | Base weight | Inner | Middle | Outer | Rim |
| --- | --- | --- | --- | --- | --- |
| Common | 60 | ×1.2 | ×1 | ×0.8 | ×0.6 |
| Uncommon | 25 | ×1 | ×1 | ×1 | ×1 |
| Rare | 12 | ×0.4 | ×1 | ×1.75 | ×2.25 |
| Exceptional | 3 | ×0 | ×0 | ×1 | ×4 |

Expected content per galaxy:

- About 8–12 Rare planets or Rare modifiers.
- One to three Exceptional modifiers, always in the Outer or Rim ring.
- Two to five deposits of each rare resource, mostly in the Middle ring and beyond.

### Visibility

- **Before exploration:** the node is fog. Its ring and route links may be known.
- **Exploring a system:** reveals its star, archetype, anomaly rule, and each planet's archetype.
- **Scanning a planet:** reveals its footprint, deposits, and modifiers.

### Guarantees

The validator rejects a galaxy that breaks any of these:

- The graph is connected. Each ring meets the shape's minimum number of outward routes, spread across different sectors.
- Within two jumps of home there is at least one Industrial or Mining world, one Gas giant or Volcanic world, and one planet with Science 1.5 or higher.
- The Inner and Middle rings together contain at least three Strategic deposits and at least two planets with Science 1.5 or higher.
- No Exceptional content appears in the Inner or Middle ring.
- No system holds more than one Exceptional modifier.
- The galaxy holds one to three Exceptional modifiers.
- Every sector contains at least one Rare planet or Rare modifier by the Outer ring.
- Each rare resource has two to five deposits in the galaxy.
- At least two of the three rare resources have a deposit in the Middle ring or closer.
- No rare deposit appears within one jump of home.
- All reserved roles are placed.

## Implementation Notes

- **Templates are ScriptableObjects:** planet archetypes, planet modifiers, star types, system archetypes, anomaly rules, non-system node types, galaxy shapes, and one generation-settings asset for node count, ring shares, tier weights, ring multipliers, sector bias, guarantees, route timing, and the weighted shape list. All are referenced from the object database.
- **Galaxy shapes** derive from one abstract shape asset with a single layout method. It takes the shared settings and a random stream and returns node positions, the home node, sector labels, candidate routes with costs, and an optional far-region hint. Circle, Spiral, and Linear are the first three subclasses.
- **The generator is plain C#.** An orchestrator runs one small class per stage (spacing sampler, route connector, ring assigner, sector bias, role reservation, content roller, name generator, validator). It takes the run seed and templates and produces the scenario consumed by the universe loader.
- **An editor-only preview window** draws a galaxy for a chosen shape and seed. It lets shapes be tuned without starting a run.
- **Generated planets belong to the run,** not to the object database. Settlement, production, and UI look planets up through the run's universe. This is the largest ripple of the change.
- **Resource kinds grow** to include the three rare resources. Planet output multipliers need to hold any number of resource kinds instead of one field per resource.
- **Planet modifiers** carry a list of eligible archetypes.
- **Buildings** gain an optional required deposit and an optional required research capability, matching ship classes.
- **Ship roles** gain a Scout value. Existing ship assets are renamed to Destroyer and Cruiser, and a Survey Corvette asset is added.
- **Removed:** the fixed vertical-slice scenario factory and its star system and planet databases.
- **Tests** build small hand-made scenarios instead of relying on slice ids. Generator tests cover seed determinism, each guarantee, the rarity caps, and modifier eligibility. Each shape is also tested across many seeds for connectivity, ring shares within tolerance, and the share of first attempts that pass validation.

## Open Questions

- Is 27–35 nodes memorable across five timelines, or should the Rim shrink?
- Is the galaxy shape rolled per run, chosen by the player, or unlocked through post-run progression?
- Should a shape be able to override ring shares, for example a longer Rim in a Linear galaxy?
- Should sector bias be visible to the player, or only felt through content?
- Do Gas giants and Asteroid clusters stay full colonies, or become outpost-only once remote extraction exists?
- How many anomaly rules does the first version need?
- Should the hostile-neighbor region always sit in the sector farthest from the best early research world?
- What regeneration limit is acceptable before a seed is rejected outright?
- Should Strategic be renamed to something more concrete, such as Alloys?
- Should Precursor Excavation deplete over time, unlike other extraction?
