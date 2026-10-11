# Star Systems

## Role in the Game

A star system is the main local container for planets, stations, resources, factions, discoveries, and strategic sites.

The system sits between the map and the economy:

- [universe-and-travel.md](universe-and-travel.md) defines the system's location, graph connections, routes, radar, and travel.
- This document defines what is generated inside the system.
- [planets.md](planets.md) defines planet identity, settlement, and local challenges after generation. [economy-and-building.md](economy-and-building.md) defines populations, infrastructure, resources, and production.

A system should be more than a collection of generic planets. It should have a recognizable strategic identity that changes how the player evaluates and uses it.

## System Anatomy

Each system may contain:

- A star and environmental profile.
- A set of planets or habitats.
- Moons, asteroid belts, or gas giants.
- Colonies and populations.
- Stations and shipyards.
- Resource deposits.
- Relics, ruins, and anomalies.
- Faction presence or ownership.
- Defenses and military installations.
- Sensor, relay, or gate infrastructure.
- Local hazards and special rules.

Not every system needs all of these elements. Most systems should contain approximately three to five meaningful planets, moons, habitats, or sites. This is a content target rather than a requirement to fill every orbit.

An important system should not automatically contain more local bodies. Its importance should come from what those bodies contain, how they combine, or how the system connects to the wider map. A larger system with unusually many bodies can exist as a rare system archetype, but its value should be tested against the complexity it introduces.

## System Identity

Every important system should have at least one strong reason for the player to care about it:

- Strategic route, chokepoint, or junction.
- Faction territory, capital, or diplomatic contact.
- Frontier, invasion, or sabotage significance.
- System-wide anomaly, hazard, or special rule.
- A valuable planet or site contained within the system.

Some systems can combine several identities, but stacking too many unique system-level rewards should be rare. A system can be important because of its overall position while also containing a planet-level opportunity.

## System Archetypes

The generator can select from authored archetypes and combine them with local modifiers.

The initial target is approximately seven to ten system archetypes. Possible archetypes include:

- **Normal/Default system:** Nothing special.
- **Frontier system:** Exposed to local hostiles and useful for expansion.
- **Junction system:** Connects several routes or provides alternate paths through the map.
- **Faction system:** Contains a faction capital, border, diplomatic hub, or contested territory.
- **Anomaly system:** Has system-wide environmental rules, risks, or discoveries.
- **Invasion system:** Connected to the wormhole, enemy staging network, or a major threat.
- **Big system:** Contains unusually many bodies or sites and may support several competing opportunities.
- **Relay system:** Contains infrastructure that improves travel, radar, or logistics across nearby routes.
- **Warzone system:** Has active conflict, unstable ownership, or repeated military pressure.
- **Dead system:** Offers little immediate value but may contain hidden information or a future opportunity.

System archetypes should affect map role, access, and local conditions rather than completely determine the contents of every planet. A frontier system can contain a research world, and a faction system can contain a contested relic site.

System archetypes are stable for the run. Player actions can change ownership, development, defenses, and local conditions, but they do not transform a junction into a resource system or an anomaly system into a normal one.

## Planet and Site Roles

Planet and site roles belong to the local contents of a system and are defined in [planets.md](planets.md).

Possible roles include:

- Resource world.
- Research world.
- Industrial world.
- Shipyard world.
- Relic or ruin site.
- Diplomatic or cultural center.
- Fortress world.
- Anomaly site.

A system archetype and a planet or site role can combine. For example, a chokepoint system may contain a fortress world, while a faction system may control a rare-resource planet. The system determines the strategic context; the planet type determines the planet's strongest gameplay identity.

Planet archetypes are also stable. Players can build off-specialization infrastructure on a planet, but it is less effective and may not support the strongest or most advanced buildings for that role. A mining world can produce some energy, but it should not become as efficient as a dedicated energy world.

## Procedural Generation

The system generator should create content in stages:

1. Select the system's role within the generated map.
2. Choose the star and environmental profile.
3. Generate planets, belts, moons, or habitats.
4. Assign habitability and population opportunities.
5. Place common and rare resource deposits.
6. Add infrastructure, faction presence, or ownership.
7. Select relics, ruins, anomalies, or research opportunities.
8. Add local hazards, defenses, and discovery requirements.
9. Validate that the system contributes to a viable and readable run.

The possible content is authored, but the specific system, placement, and combination are generated for each run.

## Weighted Rarity

Resource deposits and major discoveries should use weighted randomness rather than hard per-system caps. A system can rarely contain several valuable resources or discoveries, but the probability of that outcome should be very low.

Rarity should be influenced by the system's map position:

- Home and inner-ring systems have a lower chance of rare resources, relics, and multiple major discoveries.
- Middle-ring systems offer more meaningful opportunities and contested combinations.
- Outer systems have higher chances of rare or build-defining content, balanced by longer travel, greater danger, and higher settlement cost.

The early region should still guarantee enough common resources and practical worlds for the player to begin. It should simply be less likely to produce an exceptional build-defining jackpot.

Multiple major discoveries in one system should be possible but unusual. When it happens, the system can become a high-value strategic target without making that outcome necessary for victory.

## Stars, Names, and Flavor

For the initial game, stars are primarily environmental modifiers rather than deep simulation objects. Star properties can affect habitability, energy, hazards, resource likelihood, or local visibility without requiring every star to have a unique mechanical system.

Visual distinction can support readability and atmosphere, but deeper star mechanics can be expanded later.

System names, planet names, and local flavor should be procedurally generated. Histories are not required for the initial design. Names and short descriptive details should communicate enough identity for the player to remember a location without requiring authored lore for every generated system.

## System and Planet Visibility

Entering a system reveals its broad local composition:

- The star and environmental profile.
- The number and general type of planets, moons, habitats, or sites.
- Obvious system-wide hazards or infrastructure.
- Broad ownership or faction presence when detectable.

Individual planet modifiers remain hidden until the planet is scanned or investigated. A player may recognize that a world is unusually promising from its visible type or appearance, while a hidden local modifier may only be revealed through a dedicated scan.

Examples of hidden details include:

- An unusually rich resource deposit.
- Ancient infrastructure.
- A rare population trait.
- A relic or blueprint.
- A severe environmental hazard.
- A unique technology interaction.

Advance intelligence can reveal some of this information before the player enters or scans the system. Possible sources include:

- Diplomatic exchanges.
- Purchased or traded star maps.
- Espionage.
- Scout reports.
- Recovered data.
- Research or relic effects.

This creates a distinction between discovering a system's shape and understanding the value of each planet inside it.

## Planet and Site Distribution

Planet generation should create meaningful choices within a system. A planet's world type should provide its largest modifiers, while local modifiers add run-specific variation:

- A research world should be substantially better at science than a general-purpose colony.
- An industrial world should support much stronger construction and production.
- A dangerous moon may have a resource-world identity plus a local hazard.
- A ruined planet may have a relic-world identity plus a difficult excavation condition.
- A gas giant may have an energy-world identity plus a rare local modifier.

Local modifiers can add positive or negative variation:

- Unusually rich deposits.
- Hostile environment.
- Strange population traits.
- Ancient infrastructure.
- Political instability.
- Reduced construction cost.
- A unique technology interaction.

Local modifiers should influence the main world identity rather than erase it. Players should recognize what a planet is best at while still evaluating whether its particular modifiers make it worth claiming.

Habitable planets should be relatively common. The scarcity should come from the player's ability to settle, develop, supply, and defend them, not from finding a habitable planet at all. A system may offer several viable candidates, but their archetypes, local modifiers, distance, and strategic roles should make them meaningfully different choices.

The player should not need to colonize every planet. Some locations are better left as:

- Resource outposts.
- Research sites.
- Military bases.
- Observation posts.
- Diplomatic territories.
- Abandoned or quarantined sites.

## Resources and Discoveries

Resources can be:

- Visible after basic exploration.
- Revealed through scanning.
- Hidden behind archaeology, research, or diplomacy.
- Controlled by a faction.
- Dangerous to extract.
- Available only after a local event or infrastructure project.

Special discoveries should be tied to system identity and local conditions:

- A reactor relic on a radioactive world.
- A blueprint hidden in an abandoned shipyard.
- A language archive within an ancient ruin.
- A sabotage clue inside an alien relay.
- A friendly species living in a system that appears resource-poor.

The generator should avoid making required discoveries completely invisible. If a system contains a major opportunity, there should be clues, signals, or plausible reasons to investigate it.

Some rare planets should be powerful enough to define a run-specific build. These worlds may combine a strong archetype with an unusual deposit, relic, population trait, or production rule:

- A radioactive reactor world that enables nuclear fleets.
- A crystal world that transforms energy weapons or shields.
- An ancient forge world that unlocks a unique ship class.
- A translation archive world that enables a major diplomatic strategy.

Rare build-defining worlds should be uncommon, discoverable, and optional. The generator must provide alternative viable plans so a run is not lost because one rare planet is inaccessible.

## Factions and Ownership

A system may be:

- Unclaimed.
- Owned by a player empire.
- Controlled by a non-player faction.
- Contested.
- Shared through an agreement.
- Occupied or blockaded.
- Abandoned after a crisis.

Ownership and system identity should interact. A faction may control the only uranium deposit in a region, while an ancient ruin may be inside a supposedly friendly empire's territory.

Players should have several ways to interact with a valuable system:

- Colonize.
- Trade for access.
- Negotiate ownership.
- Establish an outpost.
- Conquer it.
- Sabotage its infrastructure.
- Ignore it and pursue another plan.

## Local Threats and Events

Systems can generate local pressure:

- Hostile fleets.
- Pirates or raiders.
- Environmental hazards.
- Population unrest.
- Infiltration.
- Rival expeditions.
- Anomalies.
- Invasion probes.

Local threats should be tied to the system's identity and generated setup. They may make a valuable system expensive to claim or create a discovery opportunity if the player takes the risk.

## Generation Constraints

The generator should enforce:

- A connected starting map.
- Multiple viable expansion directions.
- Plenty of habitable candidates, with strong differences in value and settlement cost.
- At least four or five strategically important systems as an initial testing target, adjusted according to how rare and valuable they are.
- Enough common resources for basic empire development.
- At least one viable path to advanced fleet development.
- Discoverable alternatives when a major location is inaccessible.
- Meaningful but not overwhelming local variation.
- No single randomly selected system that is mandatory for victory without warning.

The map should remain recognizable across timelines within one run. Rewinding resets ownership and development, not the generated system contents or locations.

## Multiplayer Considerations

Important systems should be distributed across player starting regions and contested areas:

- One player's frontier may contain the rare resource.
- Another player's region may contain the unique shipyard.
- A third may discover the diplomatic contact.
- A fourth may control the route needed to connect them.

The system generator should create interdependence without assigning one player all the necessary pieces of a winning strategy.

## Open Questions

- Which seven to ten system archetypes are needed for the first playable version?
- What ring-based rarity weights produce useful early choices without making rare outer systems mandatory?
- Is proximity between planets, moons, and sites within a system mechanically meaningful?
- If proximity matters, should it affect travel time, defenses, logistics, colonization, or only presentation?
