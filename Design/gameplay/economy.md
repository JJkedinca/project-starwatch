# Economy

## Role in the Game

The economy converts the player's chosen planets, populations, infrastructure, discoveries, and relationships into the resources needed to survive and defeat the invasion.

The intended approach is strategic abstraction:

- Resources have clear strategic roles.
- Populations are represented by broad groups, needs, identities, and political interests.
- Production and logistics create opportunity costs.
- The economy supports run-specific synergies without becoming a detailed spreadsheet.

Planet identity, settlement, hidden challenges, and colonization decisions are defined in [planets-and-economy.md](planets-and-economy.md).

## Infrastructure

Infrastructure converts a planet's potential into strategic output.

Possible infrastructure includes:

- Mines and extraction sites.
- Industrial facilities.
- Research institutions.
- Shipyards.
- Power systems.
- Habitat expansion.
- Defensive platforms.
- Sensor arrays.
- Diplomatic centers.
- Logistics hubs.

Infrastructure should compete for:

- Construction capacity.
- Population or workforce.
- Energy and strategic materials.
- Research prerequisites.
- Time before the invasion arrives.

Infrastructure should respect the planet's archetype. Off-specialization buildings can be constructed when useful, but they should have reduced effectiveness, higher costs, lower capacity, or limited access to advanced tiers. A mining world can produce some energy, but a dedicated energy world should be substantially better at it and support the best energy infrastructure.

The player should often have to choose between making a planet stronger economically, militarily, scientifically, or diplomatically.

## Resource Categories

The economy should use a limited set of broad resources with distinct strategic functions.

Candidate categories include:

- **Industry:** Construction, infrastructure, and basic ship production.
- **Energy:** Powering colonies, research, fleets, and advanced systems.
- **Science:** Research progress and analysis of discoveries.
- **Strategic materials:** Rare inputs for advanced ships, relics, and weapons.
- **Food or life support:** Population support and colony stability.
- **Influence:** Diplomacy, treaties, faction pressure, and political projects.

Resources should fall into three practical categories:

- **Baseline resources:** Common enough that every empire can function.
- **Specialized resources:** Distributed across different planets and useful for particular strategies.
- **Build-defining resources:** Rare combinations that can create exceptional run-specific builds.

Rare deposits should create opportunities rather than mandatory recipes. Uranium, isotopes, crystals, alien biological material, and ancient components can enable specialized strategies, but missing one should encourage a pivot rather than make the timeline unwinnable.

## Production and Tradeoffs

The economy should force decisions about where to spend limited output:

- Expand into a new system or fortify the current frontier.
- Build a shipyard or increase research.
- Produce a large conventional fleet or save rare material for an elite design.
- Support an ally or prepare for internal instability.
- Invest in a relic expedition or defend an existing colony.

Production should be understandable enough that players can identify the bottleneck in a plan. The UI should make it clear whether a failed fleet was caused by industry, research, resources, time, or positioning.

## Populations

Populations are represented by broad groups rather than individual citizens.

Each group may have:

- Species or biological identity.
- Cultural or faction identity.
- Skills or production affinities.
- Needs and expectations.
- Loyalty or trust.
- Migration preferences.
- Political demands.

Population groups can affect:

- Research output.
- Industrial production.
- Diplomacy.
- Stability.
- Recruitment and fleet support.
- Access to certain technologies or discoveries.

The player should not manage individual jobs or citizens. Population decisions should remain at the level of policy, migration, rights, settlement, and strategic allocation.

## Stability and Internal Politics

Population groups should create internal pressure without becoming a second diplomacy simulator.

Possible pressures include:

- Neglected needs.
- Cultural or species discrimination.
- Wartime conscription.
- Resource shortages.
- Abandonment of a colony.
- Cooperation with alien factions.
- Dangerous research policies.

Responses may include:

- Granting autonomy.
- Investing in infrastructure.
- Relocating populations.
- Offering political concessions.
- Enforcing emergency measures.
- Accepting unrest to meet an invasion deadline.

Internal politics should create meaningful costs and opportunities. It should not punish every strategic choice with constant random rebellions.

## Logistics and Inter-Empire Trade

Resources should need routes, access, or agreements to move between planets and player empires.

Important logistics may include:

- Trade routes.
- Supply hubs.
- Protected shipping corridors.
- Blockades.
- Resource transfers between allies.
- Shared shipyard access.
- Convoys supporting distant fleets.

The initial supply model is strategic upkeep. Fleets pay ongoing upkeep based on their size, composition, technology, and operating conditions. Supply infrastructure, friendly territory, and relays can reduce that cost or prevent penalties.

In multiplayer, each empire should retain its own economy. Coalition-wide scarcity creates reasons to trade and cooperate, while separate ownership preserves player agency.

## Economy and Discovery Synergies

Discoveries should alter what the economy can do:

- A uranium deposit plus nuclear research enables a specialized fleet.
- An ancient forge enables production that bypasses normal industrial limits.
- A friendly alien species provides a resource that removes the need to conquer a region.
- A relic changes how energy converts into fleet endurance.
- A sabotage discovery makes an expensive anti-shield economy unnecessary.

The current map and available deposits determine which synergies are practical. The player should not be able to force the same economy every run.

## Loss and Recovery

Colonies should be valuable but replaceable enough that losing one does not always end a timeline.

Loss may cause:

- Resource and production reduction.
- Refugees or displaced populations.
- A damaged research branch.
- A broken trade route.
- A new enemy approach.
- Political instability.

Some worlds should be irreplaceable because of unique relics, shipyards, or research. Those locations should be clearly valuable once discovered, and losing them should teach the player something rather than feel arbitrary.

## Multiplayer Considerations

Players should have separate economies with opportunities for:

- Resource trade.
- Research cooperation.
- Shared infrastructure.
- Fleet support contracts.
- Access to shipyards and routes.
- Coalition projects using several empires' outputs.

The strongest economy should not be able to solve every problem alone. Unique deposits, production sites, and research opportunities should be geographically distributed.

## Open Questions

- What is the final resource list for the first prototype?
- Are populations attached to planets, species, factions, or all three?
- How much migration and displacement is needed to make populations matter?
- Are planets built from slots, districts, modifiers, or a simpler specialization choice?
- How explicit are supply chains and trade routes?
- Can players sell or transfer planets to one another?
- How much internal unrest is fun before it becomes a distraction?
- Which resources are common, rare, and run-defining?
