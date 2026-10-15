# Universe and Travel

## Role in the Game

The universe is a compact, readable 4X space rather than a full galaxy simulator. Its purpose is to create meaningful routes, discoveries, territorial decisions, and fronts for the invasion.

The player should understand the important shape of the map across a run while still discovering enough hidden information to make exploration valuable.

## Universe Structure

The map is organized into:

- Star systems.
- Other star-map locations such as asteroid belts, gas regions, hazards, and anomalies.
- Routes between nodes.
- Planets, stations, and other locations within systems.
- Chokepoints, relays, gates, and frontier corridors.
- Faction territories and contested regions.
- Relic, ruin, and anomaly sites.
- Invasion wormhole and enemy staging area.

## Star Map Location Kinds

Not every graph node needs to be a star system. The generated map can mix several kinds of nodes while keeping all of them connected through the same FTL corridor graph.

Possible node types include:

- **Star systems:** Contain planets, moons, settlements, stations, and major infrastructure.
- **Resource regions:** Asteroid belts, gas clouds, debris regions, or other locations that support limited extraction without normal planetary settlement.
- **Hazard regions:** Nebulae, radiation zones, unstable corridors, or environments that affect travel, sensors, supply, or combat.
- **Discovery regions:** Derelicts, anomalies, relic sites, ruins, or hidden research opportunities.
- **Transit nodes:** Mostly empty space that creates junctions, alternate routes, ambush locations, or strategic chokepoints.

Non-system nodes generally do not support population, ordinary buildings, or full settlement. They may still support specialized extraction, temporary staging, salvage, environmental effects, or route infrastructure.

Every important node should provide at least one meaningful strategic property:

- Route value or geographic control.
- A useful resource or extraction opportunity.
- An environmental risk or combat modifier.
- A discovery, relic, blueprint, or intelligence opportunity.

Purely empty transit nodes are acceptable in moderation when they create useful pacing, distance, or route choices, but they should not make travel feel like filler. Node types, locations, and contents remain randomized within the generator's solvability rules.

Important systems can have identities such as:

- Resource-rich but exposed colony.
- Research station with a unique branch.
- Ancient ruin containing a relic.
- Shipyard with unusual production capability.
- Diplomatic contact or alien society.
- Defensive chokepoint.
- Invasion relay or sabotage target.

The map should contain enough systems and non-system nodes for meaningful expansion, travel, and alternate routes, but not so many that players cannot remember what matters across five timelines.

## Rule-Driven Procedural Content

The universe uses a procedural scenario structure built from authored content templates.

The game authors the possible content:

- System roles and planet types.
- Relics, blueprints, and anomalies.
- Faction archetypes and diplomatic opportunities.
- Invasion fleet types and technology.
- Sabotage events and prevention methods.
- Main fleet variants and victory conditions.

The run generator determines which content appears, where it appears, and how it connects:

- Important systems, non-system nodes, and routes.
- Major relics and blueprints.
- Invasion wormhole system and major fleet beats.
- Key factions and diplomatic opportunities.
- Main sabotage targets.
- Final offensive objectives.
- Resource deposits and production sites.

Every run should therefore have a distinct strategic setup. The player may know what kinds of things can exist, but must explore to learn which specific relic, faction, route, weakness, or objective was generated this time.

The generator must enforce strict rules rather than pure randomness:

- The map remains connected and supports multiple viable expansion routes.
- Required discoveries are reachable and have readable clues.
- Resources and research opportunities can support at least one winning plan.
- Major threats arrive within the intended timeline windows.
- Factions have enough space and resources to pursue their roles.
- The final objective is difficult but not dependent on an unknowable hidden requirement.
- Randomization creates tradeoffs and variation without producing unwinnable setups.

The campaign mystery comes from learning the generated scenario, not from following one authored map. The player should learn the rules and content pool while still needing to analyze every new run.

## Exploration and Discovery

Unexplored space begins under fog of war. Exploration can reveal:

- System identity and ownership.
- Route connections.
- Resource deposits.
- Fleet movement or signals.
- Anomalies and ruins.
- Faction presence.
- Relic signatures.
- Invasion traces.

Different discovery actions should provide different levels of certainty:

- Basic survey reveals broad system information.
- Deep scanning reveals resources, defenses, and hidden locations.
- Reconnaissance observes fleets and movement.
- Archaeology or research uncovers relics, blueprints, and historical clues.
- Espionage reveals faction plans and enemy weaknesses.

Discovery should cost time, fleet capacity, research, or safety. The player must decide whether to explore broadly or act on partial information.

Discoveries are recorded immediately in the run archive. A player does not need to survive a timeline for observed information to persist within that run.

## Radar and Visibility

Enemy movement visibility is governed by radar and sensor coverage rather than a simple visible/hidden state.

Radar quality can determine:

- How many systems away movement can be detected.
- Whether the player sees a vague signal, fleet presence, or confirmed fleet composition.
- How early an invasion route is identified.
- Whether civilian and military movement can be tracked.
- How accurately arrival times and destinations can be estimated.

Radar coverage can come from:

- Scout ships.
- Orbital sensor arrays.
- Defensive relays.
- Research stations.
- Allied intelligence.
- Relics or specialized ship modules.

Poor radar should still provide useful warnings, such as unexplained signals, disrupted routes, or probable movement direction. Better radar turns those warnings into actionable information.

The enemy can respond with:

- Stealth technology.
- Sensor jamming.
- Decoy fleets.
- False signals.
- Attacks on sensor arrays and relays.

This creates a visibility arms race. Investing in radar should improve planning without revealing the entire generated scenario immediately.

## Travel

The empire simulation runs continuously with pause and speed controls. Fleets and civilian expeditions take meaningful time to move between systems.

Travel should be affected by:

- Route length.
- Ship and fleet speed.
- FTL technology.
- Navigation hazards.
- Enemy control or disruption of a route.
- Supply infrastructure and fleet upkeep.
- Fleet composition.

FTL is a research progression rather than a fixed number of tiers. Each level should improve travel speed, upkeep, reliability, sensor performance, or fleet capabilities. FTL research can also unlock ships and technologies that require more advanced drives, but it should not silently create new conventional map connections. The number of levels should be determined by pacing, readability, and how many meaningful fleet and research milestones the game needs.

Travel time creates strategic pressure:

- A fleet may arrive too late even if it is strong enough.
- A shortcut may expose the fleet to an ambush.
- A relic may be powerful but expensive to reach.
- A known invasion route can be prepared for in advance.
- A destroyed relay can delay or redirect enemy movement.

The player should have enough information from radar, scouting, and known routes to make informed travel decisions without seeing every future event.

## Natural FTL and Relay Infrastructure

Every generated graph connection is a valid natural FTL corridor and can be traveled without a relay. Natural FTL corridors are the foundation of exploration and expansion.

Natural corridors should have baseline costs and limitations:

- Longer travel times.
- Higher fleet upkeep during transit.
- Less efficient repairs and reinforcement.
- Lower radar coverage.
- Greater exposure to navigation hazards or interception.

Relays enhance existing corridors rather than making normal exploration possible. A relay can provide:

- Faster travel.
- Lower upkeep.
- Better radar coverage and earlier warnings.
- Higher fleet throughput.
- Improved repairs and reinforcement.
- A safer staging point for nearby operations.

If a relay is damaged or destroyed, the underlying corridor remains usable but loses its bonuses. An advanced gate or rare technology may eventually create an explicit shortcut, but that connection must be visible, discoverable, and represented in the same shared graph for every faction. Ordinary exploration should not depend on gates.

## Escape Drive

The player may eventually research a high-level **Escape Drive** as an emergency alternative to the normal corridor graph. It can temporarily create a one-use FTL corridor between systems based on the fleet's FTL capability and the distance between the systems.

The Escape Drive should be a last resort rather than a normal travel option:

- It is restricted to sufficiently advanced FTL technology.
- It is primarily usable by military fleets, not routine civilian traffic.
- The destination must be known and within the drive's maximum emergency distance.
- The temporary corridor collapses after the transit.
- It does not alter the generated map or create a permanent route for any faction.

The cost should be severe enough that using it is a meaningful decision:

- Critical ship or drive damage.
- Lost or disabled modules.
- Fleet casualties or ships left behind.
- Major resource expenditure.
- Long repair or cooldown period.
- Poor combat readiness on arrival.
- A detectable signature that may attract enemy attention.

The Escape Drive creates a player-controlled exception to learned chokepoints without invalidating them. It can save a fleet, reinforce an isolated front, or enable a desperate final operation, but it should never be the most efficient way to travel.

## Routes and Chokepoints

Routes are strategic objects rather than invisible movement lines.

The corridor graph is generated once for the run and remains stable across its five timelines. FTL research, relay construction, and route control change how efficiently a faction uses the graph, not which hidden conventional routes exist. This allows players to learn and defend chokepoints without an enemy unexpectedly bypassing them through a different version of the map.

Important routes may include:

- Stable FTL corridors.
- Dangerous shortcut routes.
- Gate networks.
- Narrow defensive chokepoints.
- Supply corridors.
- Invasion-only approach vectors.

Controlling a route can provide:

- Faster travel.
- Safer logistics.
- Early warning.
- Fleet interception opportunities.
- Access to a region or final offensive staging area.

Losing a route should change the strategic situation without automatically ending the timeline. Players should be able to reroute, negotiate, sabotage, or abandon the affected region.

Players can build and repair gates, relays, and other route infrastructure when they have the required resources, technology, construction capacity, and time. Repairing an existing network should usually be more practical than creating an entirely new route.

Permanently closing or redirecting a route is not a normal player action. It may be possible through an extremely rare resource, relic, or endgame technology, but it should have major strategic and diplomatic consequences.

## Frontier Pressure

Frontier Pressure represents the broader diseconomies and risks of expanding beyond the empire's established core. It is a soft expansion pressure rather than a hard empire limit.

Frontier Pressure can increase with:

- The number of controlled systems.
- The number of settled planets.
- Distance from the established core.
- Long or poorly supported routes.
- Rapid expansion.
- Exposed outposts and frontier colonies.
- Missing relays, defenses, or logistics infrastructure.

At lower levels, pressure primarily creates economic and logistical costs:

- Higher global upkeep.
- More expensive reinforcement and repairs.
- Greater route maintenance.
- Increased defense and sensor requirements.
- Reduced efficiency in distant colonies.

At higher levels, it can create external pressure:

- Frontier raids.
- Minor faction sorties.
- Exploration crises.
- More dangerous supply routes.
- Increased vulnerability to sabotage.
- Colony stability problems.

Players can manage Frontier Pressure by consolidating territory, building relays and defenses, improving FTL and logistics, negotiating with nearby factions, or using stealth and low-signature technologies. Pressure should be dynamic: an established route and supported colony can become less burdensome than a newly exposed outpost.

Frontier Pressure is separate from the invasion escalation clock, faction-specific threat, and colony stability. It can contribute to local instability and create opportunities for invasion pressure, but it should not rewrite the invasion's learnable timing or high-level composition.

## Logistics and Supply

Fleets should not operate indefinitely far from friendly infrastructure without consequences. Supply can be represented strategically rather than through detailed fuel simulation.

The initial design default is strategic upkeep. Fleets pay ongoing upkeep based on their size, composition, technology, and operating conditions. Supply infrastructure, friendly territory, and relays can reduce that cost or prevent penalties. Explicit convoys and detailed range simulation remain possible future extensions.

Possible supply effects include:

- Reduced combat effectiveness.
- Slower repairs and reinforcement.
- Higher fleet upkeep.
- Restricted ability to remain in hostile space.
- Increased vulnerability to sabotage.

Supply should make bases, shipyards, relays, and allied territory valuable. It should not create constant maintenance busywork.

## Invasion Movement

The invasion follows a learnable strategic structure:

1. Signals and anomalies reveal possible activity.
2. Scouts identify routes, signals, and the likely location of the invasion wormhole.
3. Incursions test frontier defenses.
4. Major fleets exploit weak routes and disrupted logistics.
5. The main fleet emerges through a primary wormhole at one specific star system, while a secondary wormhole may create a separate front.
6. The final offensive intercepts and defeats the main invasion fleet.

The primary and secondary wormhole locations are generated for each run and remain consistent across that run's timelines. Players should be able to locate them through enough scouting, radar coverage, and investigation rather than being expected to guess randomly. The secondary wormhole exists primarily to divide the player's attention; its activation and reinforcement behavior remain open. The invasion's major timing beats remain consistent within a run, while minor movement, target selection, and low-level behavior can vary.

The invasion may use its dedicated wormhole as an asymmetric movement capability, but it should not receive hidden conventional corridors unavailable to the player. Any other bypass, gate, or route exception should be an explicit discoverable capability with warning signs and counterplay.

## Multiplayer Considerations

Each player controls an independent territory and fleet network. Travel and geography should create natural responsibilities:

- One empire controls a frontier route.
- Another owns a central gate.
- Another has the nearest shipyard or resource.
- Another discovers the invasion vector.

Players should be able to share route information, request fleet support, trade access, and coordinate movement. A shared route should create dependence without requiring a common empire treasury.

## Open Questions

- What density of important systems produces meaningful choices? Start testing with at least four or five, then adjust based on how rare and strategically important each system is.
- Which map-generation rules guarantee readable and solvable route networks?
- What radar levels and visibility details are needed for planning without eliminating uncertainty?
- What are the resource, technology, and time costs for building or repairing route infrastructure?
- Under what exceptional conditions can a player permanently close or redirect a route?
