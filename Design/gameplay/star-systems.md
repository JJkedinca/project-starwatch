# Star Systems

## Role in the Game

A star system is the main local container for planets, stations, resources, factions, discoveries, and strategic sites.

The system sits between the map and the economy:

- [universe-and-travel.md](universe-and-travel.md) defines the system's location, graph connections, routes, radar, and travel.
- This document defines what is generated inside the system.
- [planets-and-economy.md](planets-and-economy.md) defines how planets, populations, infrastructure, and resources function after generation.

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

Not every system needs all of these elements. The generator should create focused systems with a few meaningful properties rather than filling every system with content.

## System Identity

Every important system should have at least one strong reason for the player to care about it:

- Rare resource.
- Valuable relic or blueprint.
- Research opportunity.
- Unique shipyard.
- Diplomatic contact.
- Strategic route or chokepoint.
- Defensive position.
- Invasion or sabotage significance.
- Population or cultural importance.

Some systems can combine several identities, but stacking too many unique rewards in one location should be rare. The player should have to decide which systems are worth prioritizing.

## System Archetypes

The generator can select from authored archetypes and combine them with local modifiers.

Possible archetypes include:

- **Frontier system:** Exposed, lightly developed, and useful for expansion.
- **Resource system:** Contains one or more valuable deposits.
- **Research system:** Provides unusual science, specialists, or technology clues.
- **Industrial system:** Supports construction, shipyards, or advanced production.
- **Relic system:** Contains a ruin, artifact, or dangerous excavation.
- **Diplomatic system:** Hosts an alien society, faction, or cultural opportunity.
- **Fortress system:** Controls a route and can anchor a defensive line.
- **Anomaly system:** Contains unusual rules, risks, or discoveries.
- **Invasion system:** Connected to the wormhole, enemy staging network, or a major threat.
- **Dead system:** Offers little immediate value but may contain hidden information or a future opportunity.

Archetypes should affect what can be found, not completely determine the outcome. A resource system can still contain a diplomatic complication, and a relic system can still be politically contested.

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

## Planet and Site Distribution

Planet and site generation should create meaningful choices within a system:

- A habitable planet may be safe but lack rare resources.
- An industrial world may be productive but politically unstable.
- A dangerous moon may contain the only valuable isotope.
- A ruined planet may be difficult to settle but contain a powerful relic.
- A gas giant may support an unusual energy or fuel economy.

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

- How many planets or sites should a typical important system contain?
- How many system archetypes are needed for the first playable version?
- Are stars visually and mechanically distinct, or mostly environmental modifiers?
- How many resources can appear in one system before its identity becomes overloaded?
- How often should a system contain multiple major discoveries?
- How are system names, histories, and flavor generated?
- How much of a system is visible before survey or scanning?
- Can a system's archetype change after major player actions?
