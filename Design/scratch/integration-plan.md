# Starwatch Integration Plan

## Purpose

This document connects the gameplay design documents into an implementation and
validation sequence. It is intended to prove the core Starwatch loop before
building the full content roster, procedural generation, multiplayer, or final
presentation.

The integration target is:

> Explore, commit to expansion, develop a run-specific build, survive a
> learnable invasion, rewind with new knowledge, and execute a better plan.

The detailed rules remain in:

- [Universe and Travel](Design/gameplay/universe-and-travel.md)
- [Star Systems](Design/gameplay/star-systems.md)
- [Planets](Design/gameplay/planets.md)
- [Economy and Building](Design/gameplay/economy-and-building.md)
- [Research and Technology](Design/gameplay/research-and-technology.md)
- [Fleet Combat](Design/gameplay/fleet-combat.md)
- [Politics and Diplomacy](Design/gameplay/politics-and-diplomacy.md)
- [Roguelike Systems](Design/gameplay/roguelike-systems.md)
- [Invasion and Sabotage](Design/gameplay/invasion-and-sabotage.md)

## Integration Principles

- The empire layer is the primary game. Combat resolves as a consequence of
  exploration, economy, research, and fleet preparation.
- The scenario is generated once per run and remains stable across timelines.
- Timeline power resets after a rewind.
- Run knowledge and selected completed discoveries persist within the run.
- The simulation runs continuously, with pause and speed controls.
- Randomness should create different viable plans, not hidden unwinnable states.
- The first implementation should use authored content and deterministic
  scenarios before adding procedural generation.

## State Boundaries

The implementation should separate three kinds of state.

### Scenario state

Generated once when a run begins:

- Map nodes, systems, planets, and routes.
- Resource deposits and local modifiers.
- Faction placement and starting attitudes.
- Relic, blueprint, and anomaly locations.
- Invasion wormhole, timing, and major fleet structure.
- Planet and sabotage challenge identities.

Scenario state is preserved across rewinds.

### Timeline state

Reset when the player rewinds:

- Colonies, ownership, and development.
- Buildings, research infrastructure, and defenses.
- Fleets, ships, debris, and production queues.
- Resources, upkeep, supply, and Frontier Pressure.
- Colony stability/internal pressure.
- Current fleet positions and active crises.
- Temporary diplomatic relationships.

### Run archive

Updated immediately when discovered:

- Explored map and route information.
- Enemy compositions, observed weapons, and weaknesses.
- Research leads, prerequisites, and blueprints.
- Faction identities, incentives, and specialties.
- Invasion timing, targets, and sabotage triggers.
- Completed research and fully understood technologies.

The archive must be usable by later timelines without restoring the previous
timeline's physical infrastructure or resources.

## First Vertical Slice

The first playable slice should use a small authored scenario:

- One player species.
- Four or five important star systems.
- A small number of non-system nodes such as an asteroid region or anomaly.
- One primary invasion wormhole.
- One minor faction.
- Several viable expansion directions.
- One or two meaningful routes or chokepoints.
- Two or three timeline attempts during a test session, while the production
  target remains five rewinds.

The slice should include:

- Exploration, scanning, and travel.
- Planet settlement and one hidden local challenge.
- Settlement commitment, abandonment, and consequence handling.
- A small economy with industry, energy, science, and one strategic material.
- Planet capacity, archetype multipliers, and one or two building levels.
- Frontier Pressure and basic colony stability.
- Three research offers with one active project.
- One discovery-gated research path.
- Three readable ship roles with shields, armor, hull, retreat, and debris.
- One station or planetary defense type.
- One minor faction interaction.
- A scout attack, one sabotage crisis, and one main invasion wave.
- Rewind, timeline reset, and archive persistence.

The slice does not need final art. Map markers, simple planet views, placeholder
ships, a battle summary, and an archive panel are sufficient.

## End-to-End Scenarios

The slice should prove these scenarios from beginning to end.

### Expansion commitment

1. The player explores two promising planets.
2. The player chooses one to settle.
3. The local challenge is revealed after commitment.
4. The player resolves it, delays it, or abandons the settlement.
5. The choice changes available resources, pressure, and future fleet options.

### Run-specific build

1. The player discovers a resource, relic, or blueprint.
2. The discovery changes the research offers or reveals a research path.
3. Research and infrastructure convert it into a fleet or economic advantage.
4. The resulting build is stronger in one area but carries an opportunity cost.

### Invasion and attrition

1. The player receives learnable warnings about an approaching scout or wave.
2. A fleet and a defense station engage the invasion.
3. The battle produces losses, damage, retreat opportunities, and debris.
4. The player chooses between repairing, intercepting, expanding, or consolidating.
5. The invasion remains dangerous without making one lost battle immediately fatal.

### Rewind learning

1. The timeline ends through defeat or voluntary rewind.
2. Timeline power is removed.
3. The archive retains discovered information and within-run unlocks.
4. The player reaches the earlier decision faster.
5. The player uses knowledge to choose a different route, settlement, research
   plan, or fleet composition.

## Implementation Sequence

### Phase 0: Simulation contracts

Define the data and service boundaries before adding presentation:

- Run, scenario, timeline, and archive state.
- Simulation clock and pause/speed behavior.
- Node, system, planet, fleet, research, and invasion identifiers.
- Reset and persistence rules.
- Event log for discoveries, battles, crises, and rewinds.

This phase should make it possible to save, reset, and inspect state without
depending on the final UI.

### Phase 1: Authored scenario and map loop

Implement one deterministic scenario:

- Map nodes and connected routes.
- Fog of war and basic exploration.
- Scanning and discovery records.
- Fleet travel and arrival.
- System and planet inspection.

Success means the player can identify a promising route and make an informed
expansion decision.

### Phase 2: Planet and economy loop

Add:

- Outposts and settlement.
- Settlement cost and commitment.
- Planet archetype modifiers.
- Building capacity and simple vertical upgrades.
- Resource production and upkeep.

Success means the player must choose which opportunities to commit to rather
than settling every available planet.

### Phase 3: Research and discovery loop

Add:

- Global research rate from research infrastructure.
- Three eligible offers.
- One active project.
- Prerequisite validation.
- Discovery-gated research.
- Completed research persistence across rewinds.

Success means research changes the player's economic or fleet plan instead of
acting as a sequence of automatic upgrades.

### Phase 4: Fleet and travel loop

Add:

- Three ship classes built at a shipyard.
- One build queue per colony.
- Explicit fleet merge and split.
- Travel time from the slowest class and from research.

Success means composing a fleet changes how fast it travels, and a shipyard turns research into ships on the map.

### Phase 5: Invasion loop

Add:

- Escalation clock.
- Scout fleet.
- One warning-driven sabotage crisis.
- One major invasion wave.
- Fixed high-level enemy composition.
- Simple target selection and reinforcement behavior.

Success means the invasion creates urgency and forces preparation without
requiring a hidden counter or a single all-or-nothing battle.

### Phase 6: Rewind loop

Add:

- Voluntary rewind.
- Defeat-triggered timeline end.
- Timeline reset.
- Archive persistence.
- Completed research and discovered ship access within the run.
- A short rebuild flow using the known scenario.

Success means the second attempt is meaningfully more informed and efficient,
not merely numerically stronger.

### Phase 7: Integration pass

Run the complete slice repeatedly and adjust the connections between systems:

- Travel time versus invasion timing.
- Settlement cost versus available resource output.
- Research rate versus meaningful project count.
- Fleet production versus expected losses.
- Defense attrition versus invasion strength.
- Archive usefulness versus repetitive solved steps.

Only after this pass should the project expand content or procedural variation.

## Deferred Until the Slice Works

- Procedural map and scenario generation.
- Secondary wormhole and multiple invasion fronts.
- Full minor-faction roster and complex diplomacy.
- Multiplayer research, economy, and rewind coordination.
- Post-run progression and Ascension modifiers.
- Detailed convoys, cargo interception, and supply simulation.
- Complete ship roster, advanced ship classes, and all fleet keywords.
- Large sabotage and crisis event pools.
- Final art, cinematic battle presentation, and extensive VFX.

## Validation Questions

The vertical slice is successful if playtesting can answer “yes” to most of
these questions:

- Does expansion feel attractive before it becomes burdensome?
- Does settling a planet create a meaningful commitment?
- Can the player understand why a planet or fleet is valuable?
- Does research create opportunity cost and different plans?
- Can the player identify why a battle was won or lost?
- Do defenses enable attrition and smaller engagements?
- Does the invasion remain learnable across timelines?
- Does a rewind convert failure into useful knowledge?
- Can at least two different plans survive the authored scenario?

Exact rates, costs, research durations, damage values, and invasion timings
remain implementation and balance questions rather than integration blockers.
