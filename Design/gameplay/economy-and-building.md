# Economy and Building

## Role in the Game

The economy converts the player's chosen planets, populations, infrastructure, discoveries, and relationships into the resources needed to survive and defeat the invasion.

The intended approach is strategic abstraction:

- Resources have clear strategic roles.
- Population is represented abstractly, with species traits affecting broad outputs and colony needs.
- Production and logistics create opportunity costs.
- The economy supports run-specific synergies without becoming a detailed spreadsheet.

Planet identity, settlement, hidden challenges, and colonization decisions are defined in [planets.md](planets.md).

## Infrastructure

Infrastructure converts a planet's potential into strategic output.

Possible infrastructure includes:

- Economy And Resource Extraction
    - Mines and extraction sites.
    - Economic Centers
    - Industrial facilities.
    - Spaceports and Trade

- Planet Infra
    - Housing and Population Centers
    - Power Systems
    - Habitat Expansions
    - Relic Level

- Defense/Intelligence
    - Defensive platforms
    - Sensor arrays.
    - Logistics & jump gates 
    - Diplomatic centers.
    - Research institutions.

- Production
    - Shipyards
    - Weapon centers?

Infrastructure should compete for:

- Construction capacity.
- Population or workforce.
- Energy and strategic materials.
- Research prerequisites.
- Time before the invasion arrives.

Infrastructure should respect the planet's archetype without hard-locking its options. Any planet can build research, energy, industry, defense, or other infrastructure if the player meets the normal requirements. Off-specialization buildings may have weaker multipliers, higher operating costs, greater infrastructure footprint, or lower population efficiency. A mining world can become a research world if the player wants, but a dedicated research world should produce more science from the same investment.

The player should often have to choose between making a planet stronger economically, militarily, scientifically, or diplomatically.

## Planet Capacity and Archetype Benefits

Planets should have a meaningful amount of development space rather than an unlimited building list. Building footprint, population capacity, and available infrastructure should be influenced by:

- Planet size.
- Terraforming level.
- Habitability and environment.
- Planet archetype.
- Local modifiers.

Larger planets should generally provide more building space per terraforming level. Terraforming can increase the amount of usable space and population capacity, but should require time, resources, and a meaningful development commitment.

World types should provide strong benefits to particular outputs:

- Solar or gas worlds can generate more energy per development level.
- Mining worlds can generate more strategic and production resources per development level.
- Oceanic worlds can provide strong research or science potential.
- Industrial worlds can produce more construction output.
- Shipyard worlds can support more advanced fleet production and repair.
- Fortress worlds can derive more value from defensive infrastructure.

These are efficiency advantages, not hard restrictions. Any planet can support other infrastructure if the player accepts weaker multipliers, higher costs, or greater building footprint. Local modifiers and rare discoveries can further amplify or redirect a planet's normal strengths.

## Tall and Wide Development

Planet development should support both broad expansion and deep investment.

### Wide development

The player can settle several planets and develop each to a low or medium level:

- Establish multiple specialized outputs.
- Create redundancy across the empire.
- Claim several strategic positions.
- Accept lower efficiency and limited development at each world.

### Tall development

The player can invest heavily in a smaller number of planets:

- Upgrade major infrastructure through multiple levels.
- Unlock stronger output and advanced production capabilities.
- Build exceptional research worlds, shipyards, industrial centers, or fortresses.
- Protect a small number of strategic anchors.

The intended building model is a hybrid of limited development capacity and vertical levels. A planet has finite building space or development tracks, while major facilities can be upgraded over time. A track may represent research, industry, energy, shipyard capacity, defense, or logistics.

Higher levels should require escalating time, construction capacity, resources, and infrastructure investment. They may improve output, increase capacity, or unlock advanced buildings and production, while the research system controls the technology prerequisites.

All planets can invest in the same broad tracks, but archetype multipliers determine how effective that investment is. A mining world can develop a high-level research facility, but it will generally require more space or cost more for less output than the same facility on a research world.

Early colonies should often become the empire's strongest developed worlds because they have had more time to grow. A rare late-discovered planet can surpass them if its archetype and local modifiers justify redirecting investment.

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

## Stable Needs, Variable Solutions

Every successful empire needs access to basic economic functions:

- Construction and industry.
- Energy and life support.
- Research.
- Fleet production and maintenance.
- Defense and logistics.

The player should not need the same planet layout to provide those functions. Each run should offer several possible solutions:

- Research can come from a dedicated research world, several smaller facilities, an oceanic planet, an ancient observatory, a relic, or an alien research alliance.
- Mining can come from a specialized mining world, remote extraction stations, an ally's territory, a rare deposit, or an industrial relic.
- Fleet production can come from a tall shipyard world, several smaller shipyards, an ancient forge, an industrial planet, or allied fleet support.

The recurring need is stable; the source and ratio are variable. A run may require a basic economy, but it should not require one research world, one mining world, and one production world in the same order every time.

The map generator should guarantee access to viable baseline functions while allowing the current resources, planet archetypes, discoveries, diplomacy, and challenges to determine the most attractive solution. The player is choosing a subset of many opportunities rather than following a universal build order.

## Production and Tradeoffs

The economy should force decisions about where to spend limited output:

- Expand into a new system or fortify the current frontier.
- Build a shipyard or increase research.
- Produce a large conventional fleet or save rare material for an elite design.
- Support an ally or stabilize a vulnerable colony.
- Invest in a relic expedition or defend an existing colony.

Production should be understandable enough that players can identify the bottleneck in a plan. The UI should make it clear whether a failed fleet was caused by industry, research, resources, time, or positioning.

## Populations

Each run uses one player species or civilization. The player does not manage multiple internal species or population factions.

The species may have a small set of strengths and weaknesses:

- Research or technology affinity.
- Fleet, weapon, or energy specialization.
- Diplomatic, language, or exploration advantage.
- Environmental tolerance or settlement preference.
- A modest population growth, colony stability, or resource output modifier.
- A meaningful drawback or constraint.

Population is an abstract planet-level quantity rather than a simulation of individual citizens. It can affect:

- Available workforce and output.
- Colony development speed.
- Basic settlement viability.

Species traits can meaningfully influence playstyle and enable different run strategies. The specific effects on population growth, colony stability, resource output, and planet suitability should stay small. A species may have a strong research or fleet identity, but it should not be locked out of planet archetypes or require a separate population-management game.

## Colony Stability

Stability is an aggregate condition of a colony, not a simulation of internal factions or politics.

Possible pressures include:

- Resource or life-support shortages.
- Environmental hazards.
- Invasion damage or blockade.
- Insufficient infrastructure.
- Species/planet incompatibility.
- Abandonment or loss of supply.

Responses may include:

- Investing in infrastructure.
- Restoring resources or supply.
- Adding defenses or support.
- Applying a technology or species trait.
- Abandoning or evacuating the colony.

Low stability can reduce output, slow development, weaken fleet support, or make a local challenge harder. It should not create individual political factions, migration management, or constant internal rebellions.

## Logistics and Inter-Empire Trade

Resources should need routes, access, or agreements to move between planets and player empires.

Important logistics may include:

- Trade routes.
- Protected shipping corridors.
- Blockades.
- Shared shipyard access.
- Convoys supporting distant fleets.

The initial supply model is strategic upkeep. Fleets pay ongoing upkeep based on their size, composition, technology, and operating conditions. Supply infrastructure, friendly territory, and relays can reduce that cost or prevent penalties.

The initial trade model can remain implicit: establish a trade agreement or route, pay the appropriate access and upkeep costs, and receive the resulting resource flow or income. Players should not need to manage individual convoys at first. Explicit convoys, interception, and route-level cargo risk can be added later if the economy needs more strategic depth.

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
- Population or workforce loss.
- A broken trade route.
- Reduced colony stability.
- Destroyed / Downgraded buildings

Some worlds should be irreplaceable because of unique relics, shipyards, or research. Those locations should be clearly valuable once discovered, and losing them should teach the player something rather than feel arbitrary.

## Multiplayer Considerations

Players should have separate economies with opportunities for:

- Resource trade.
- Research cooperation.
- Shared infrastructure.
- Shared building/development

The strongest economy should not be able to solve every problem alone. Unique deposits, production sites, and research opportunities should be geographically distributed.

## Open Questions

- What is the final resource list for the first prototype?
- Are development tracks represented by slots, districts, or a shared capacity budget?
- How many building levels can a timeline support before development becomes too slow?
- Do higher building levels primarily improve output, unlock new infrastructure, or both?
- How much recovery should be possible after losing a highly developed planet?
- Which resources are common, rare, and run-defining?
