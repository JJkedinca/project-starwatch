# Planets and Settlement

## Role in the Game

Planets provide the strategic foundation for settlement, discovery, specialization, and expansion. The simulation should make choosing and committing to planets meaningful without becoming a detailed colony spreadsheet.

The intended approach is strategic abstraction:

- Planets have a small number of meaningful properties and infrastructure choices.
- Planet archetypes create strong specializations and opportunity costs.
- Settlement and development can reveal local challenges.

System-level procedural generation is defined in [star-systems.md](star-systems.md). Economy, production, populations, logistics, and trade are defined in [economy.md](economy.md). This document focuses on planet identity, settlement, exploration commitment, and local challenges during a timeline.

## Planet Model

Each important planet or colony can be described by:

- Environment and habitability.
- Population capacity.
- Existing infrastructure.
- Resource deposits.
- Production specialization.
- Faction or cultural identity.
- Strategic position.
- Defenses and vulnerability.
- Relics, ruins, or anomalies.

## Planet Archetypes and Identity

A planet's world type should provide its largest gameplay modifiers. Planet archetypes are not cosmetic labels; they determine what the planet is naturally good at, what infrastructure it supports, and which strategies it can enable.

Examples:

- A **research world** has strong science output, specialist capacity, and research infrastructure, but may have weaker industry.
- An **industrial world** has strong construction and production potential, but may require more energy or create more pollution and unrest.
- A **resource world** has valuable extraction opportunities, but may be remote, exposed, or difficult to inhabit.
- A **shipyard world** supports advanced fleet construction and repair, but may be expensive to establish and strategically vulnerable.
- A **fortress world** is naturally suited to defenses and garrisons, but may have limited economic output.
- A **diplomatic or cultural center** improves influence, language, and faction relations, but may depend heavily on population stability.

The archetype should determine the planet's baseline:

- Primary outputs.
- Infrastructure options.
- Population affinities.
- Capacity and habitability.
- Construction and upkeep modifiers.
- Strategic vulnerabilities.

Most planets should be strongly specialized rather than slightly above or below average at everything. A general-purpose colony can still be useful, but it should not make every specialized world interchangeable.

## Local Modifiers

After selecting the planet archetype, the generator can apply a small number of local modifiers. These create run-specific variation without erasing the planet's core identity.

Possible modifiers include:

- Rich or depleted deposits.
- Ancient infrastructure.
- Hostile environment.
- Unusual population traits.
- Political instability.
- Reduced construction cost.
- Increased research potential.
- Unique technology or relic interaction.
- Increased vulnerability to invasion or sabotage.

For example, two research worlds may both be the best places to conduct science, but one may have an ancient observatory while the other has a hostile population and a rare alien language archive.

Local modifiers should make choosing between similar worlds interesting. They should not turn a fortress world into a better research world or make the planet's defining role irrelevant.

## Build-Defining Worlds

Rare planets can combine a strong archetype with an unusually powerful resource, relic, population trait, or production rule. These worlds should be capable of defining a run-specific build:

- A radioactive reactor world enables a nuclear fleet strategy.
- A crystal world transforms energy weapons or shield production.
- An ancient forge world unlocks or mass-produces a unique ship class.
- A translation archive world enables a major diplomatic strategy.

These planets should be:

- Uncommon.
- Discoverable through exploration or scanning.
- Expensive or risky to claim.
- Optional rather than mandatory.
- Strong enough to reward a major strategic commitment.

The scenario generator must provide alternate viable strategies. Missing a build-defining world should change the player's plan, not make the timeline unwinnable.

## Abundant Habitable Worlds and Settlement Cost

Habitable worlds should be common enough that discovering one does not automatically make the decision to settle it obvious. Habitability means a world is a candidate; it does not mean the world is worth the investment.

The player should evaluate each candidate based on:

- World archetype and baseline output.
- Local modifiers.
- Rare resources or build-defining opportunities.
- Distance and travel time.
- Strategic position and defensive requirements.
- Population capacity and compatibility.
- Infrastructure requirements.
- Current empire capacity and competing projects.

Settling a planet should be a major commitment rather than a default expansion action. The cost should escalate sharply as an empire takes on more colonies. This can be a strongly convex or exponential-style curve rather than a flat per-colony fee.

Settlement cost may include:

- Colonization ships or population.
- Upfront industry and strategic materials.
- Time before the colony becomes productive.
- Ongoing administration and upkeep.
- Defense and supply requirements.
- Political or cultural integration.
- Opportunity cost from delaying another project.

The first few settlements should be practical. Later settlements should require a clear reason, such as a rare resource, a build-defining planet, a critical route, or a major strategic advantage. Expansion should remain possible, but indiscriminate growth should become increasingly inefficient and dangerous.

Players should have alternatives to full settlement:

- Survey and reserve the world.
- Establish a cheap outpost.
- Extract a resource remotely.
- Use a faction or ally to access it.
- Build a research or military station.
- Leave it unclaimed until the empire can support it.
- Abandon or evacuate it after its value changes.

The strategic question is not:

> Can I colonize this habitable world?

It is:

> Is this world worth consuming one of my increasingly expensive settlement commitments?

### Abundance versus commitment

Starwatch should not primarily force players to survive with whatever resources happen to exist. Habitable planets and useful resources should be plentiful enough to support several viable plans.

The constraint is the player's ability to commit to those opportunities:

- Settling and developing a planet consumes escalating empire capacity.
- Distant planets require more time, upkeep, logistics, and defense.
- Each settlement delays other colonies, fleets, research, or diplomatic projects.
- Build-defining planets can be powerful without being mandatory.

The player is choosing a subset of many viable opportunities rather than trying to colonize everything. A run's map should present several possible plans, while its archetypes, local modifiers, and discovery combinations determine which plan is most attractive.

## Exploration and Settlement Challenges

The exploration loop should parallel a glade-style commitment structure:

- **Enter a system:** Reveal its broad composition and the available planets or sites.
- **Scan a planet:** Reveal more of its archetype, local modifiers, resources, and possible risks.
- **Establish an outpost or begin settlement:** Pay an initial cost and commit to the location.
- **Reveal the local challenge:** The exact challenge may not be known until the commitment begins.
- **Resolve, delay, abandon, or suffer the consequences:** Use the current build to decide whether the planet is worth completing.

The player should usually see warning signs before committing, but not necessarily the exact challenge. Examples include unusual signals, unstable readings, political tension, missing survey data, or signs of enemy activity.

Challenges should be generated from mechanical requirements rather than only from narrative themes. A local challenge may require:

- Military protection or patrol capacity.
- Industry, energy, or strategic materials.
- Research, specialists, or scanning.
- Influence, language, or diplomatic concessions.
- Population support, food, or stability.
- Time, supply, route access, or relay infrastructure.

The same fiction can support multiple solutions. A dangerous mining world might be secured with a fleet, protected by an ally, stabilized through research, or exploited remotely at lower efficiency.

Challenges should not instantly destroy a new colony. The player should receive a response window and be able to:

- Commit additional resources.
- Delay the project.
- Seek an alternative solution.
- Abandon the settlement after paying sunk costs.
- Accept a partial success or reduced output.

Major planet challenges should be deterministic within a run. If settling a planet reveals a particular crisis on Rewind 1, later timelines should preserve that crisis so the player can prepare for it or avoid the planet. The discovery should enter the archive immediately.

System-wide challenges belong to system archetypes such as anomalies, warzones, faction capitals, or invasion systems. Planet-specific challenges should be the primary expansion commitment because players choose which planets within a system to settle.

Planet types may include:

- Core world.
- Frontier colony.
- Research world.
- Industrial world.
- Resource world.
- Shipyard world.
- Fortress world.
- Diplomatic or cultural center.
- Ruined or abandoned world.
- Alien habitat.

A planet should usually have a reason to exist beyond producing generic income. Its identity may come from a unique resource, location, research branch, faction, or defensive role.

## Related Economy Design

Economy, infrastructure, resources, production, populations, stability, logistics, trade, and economic discovery synergies now live in [economy.md](economy.md).

Planet design still determines which economic roles and infrastructure are effective, but the economy document defines how those outputs function across the empire.
