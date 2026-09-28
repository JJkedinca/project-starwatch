# Project Starwatch — Gameplay Design

## Working Premise

Project Starwatch is a roguelike 4X space strategy game about preparing a civilization for an inevitable alien invasion.

The primary play surface is the empire layer: exploration, expansion, research, diplomacy, fleet building, and defensive planning. Fleet combat auto-resolves as a consequence of those decisions rather than as a separate tactical game.

The player knows that the invasion is coming, but does not initially know:

- Where it will arrive.
- What the invaders want.
- How their technology works.
- Which factions can be trusted.
- What can actually stop them.

Each timeline is an attempt to build an empire, discover the invasion's structure, and execute a viable defense plan. The player will usually fail at first, but each failure provides new information and unlocks new tools.

The intended experience combines:

- Stellaris-style 4X empire play: exploration, research, expansion, diplomacy, and auto-resolving fleet combat.
- Edge of Tomorrow-style repeated timelines and learning through failure.
- Three Body Problem-style scientific mystery, asymmetrical technology, and civilization-scale preparation.

## Core Fantasy

> Discover the plan required to defeat an invasion that has been preparing for longer than your civilization has existed.

The player should feel like they are:

- Building a civilization under a deadline.
- Discovering resources, techs, and contacts that open unexpected strategies.
- Testing hypotheses about an unknowable enemy.
- Making difficult choices about what to protect.
- Learning from doomed timelines.
- Eventually executing a plan that was impossible to see at the beginning.

## Core Gameplay Loop

### Timeline layer

Each timeline is one attempt to prepare for and survive the invasion.

The player:

1. Explores a partially unknown star map.
2. Expands through connected systems.
3. Discovers resources, anomalies, ruins, and alien contacts.
4. Develops research and infrastructure that unlock new strategic options.
5. Builds fleets and doctrines around what the timeline actually offers.
6. Forms alliances and manages rival factions.
7. Responds to alien probes, anomalies, and early incursions.
8. Wins or loses fleet engagements based on preparation, composition, and positioning.
9. Learns more about the invasion.
10. Is eventually overwhelmed, or chooses to rewind.

### Rewind layer

The player has a limited number of timeline attempts. Five rewinds are the current design target.

A rewind resets the current empire state but preserves selected forms of progression:

- Discovered map information.
- Alien weaknesses and behavior.
- Invasion routes and event timing.
- Research blueprints.
- Permanent technology or ship unlocks.
- Faction knowledge.
- New starting options.

The player should carry forward knowledge and access to tools, not a complete finished empire.

### Final attempt

The final successful attempt should feel like the result of accumulated understanding:

- The player knows which systems matter.
- The player understands the invasion's timing.
- The required technologies are available.
- The correct factions or resources can be secured.
- The player can field the right fleets in the right places.
- The player still needs to execute the plan successfully.

## The Three Progression Layers

### Knowledge

Knowledge is what the player learns by observing the timeline.

Examples:

- The invasion enters through a hidden route.
- A research colony is attacked after a specific trigger.
- A faction is compromised or unreliable.
- A certain alien ship class has a specific vulnerability.
- An alien relay must be destroyed before the final invasion.
- A technology requires a particular artifact or scientist.

Knowledge should be recorded in an intelligence archive so the player can reference important discoveries in later timelines.

### Permanent power

Permanent progression should primarily unlock new options rather than provide unlimited flat stat bonuses.

Examples:

- New research branches.
- New ship classes or weapon systems.
- New admirals or fleet doctrines.
- New starting doctrines.
- New defensive structures.
- New faction relationships.
- New ways to interact with anomalies.

Some vertical improvements are appropriate, such as a modest research-speed bonus or an additional starting option. However, victory should not require endless grinding.

### Timeline power

Timeline power is built during the current attempt and resets after a rewind.

Examples:

- Colonies.
- Fleets.
- Resources.
- Current research projects.
- Temporary alliances.
- Fortifications.
- Ship designs and fleet compositions.
- Stationed defenses.

The relationship should be:

> Knowledge tells the player what to pursue. Permanent progression provides more tools. Timeline power lets the player execute the plan.

## Strategic Empire Layer

The empire / 4X layer is the core of the game. Most of the player's time and skill expression should live here.

The scope can still be tighter than a full Stellaris campaign so that a timeline remains readable and rewindable, but the systems should feel like real 4X play: exploring, claiming systems, researching, managing factions, building fleets, and racing an escalation clock.

The player should make meaningful decisions about:

- Which systems to explore.
- Where to expand.
- Which research branch to pursue.
- Which factions to ally with or oppose.
- Which routes to fortify.
- Which resources to prioritize.
- Which systems can be abandoned.
- Where to station fleets before the invasion arrives.

The map should stay small enough to understand across multiple timelines. A limited number of important systems and routes is preferable to simulating hundreds of planets.

Possible system types include:

- Colony.
- Research station.
- Resource world.
- Military outpost.
- Ancient ruin.
- Alien anomaly.
- Diplomatic faction.
- Shipyard.
- Defensive relay.
- Invasion entry point.

Combat outcomes should primarily reflect strategic preparation: ship designs, fleet strength, tech, fortifications, and whether the player arrived in time. The player should not need micromanagement during the fight itself.

## Discovery Synergies

Even without tactical cards, the roguelike 4X loop should create fun strategic combos. Exploration, research, and diplomacy should unlock paths that feel specific to what the player found — not just flat +10% bonuses.

Discoveries should open options. Research should convert those options into strategies. Empire choices should commit to a plan.

### Example combos

- Heavy uranium worlds + nuclear research → nuclear-powered fleets with high endurance or unique weapon profiles.
- Alien language research → better negotiations, treaties, or joint fleets with friendly species.
- Ancient ruin survey → unlock a relic that enables a specialized ship doctrine.
- Crystal-rich systems + energy weapons research → laser-focused fleet builds that counter shielded invaders.
- First-contact anthropology + a peaceful neighbor → early alliance that holds a flank the player cannot garrison.
- Anomaly study of invader debris → research branch that counters a specific alien ship class.
- Rare isotope stockpile + industrial doctrine → mass-produce a ship class that would otherwise be too expensive.

### Design rules for synergies

- Combos should span at least two systems: a discovery, a research choice, and a payoff in fleets, diplomacy, economy, or defense.
- Each major discovery should suggest a direction without forcing it. The player can ignore uranium and pursue diplomacy instead.
- Synergies should be readable: the player should understand why finding uranium makes nuclear fleets attractive.
- Multiple viable combo families should exist so different timelines and different knowledge states produce different winning plans.
- Permanent unlocks may reveal that a combo exists; the current timeline still has to secure the resources, research, and partners to use it.

The fantasy is:

> I found something rare, researched the right branch, and suddenly my empire can do a thing it could not do before.

## Invasion Structure

The invasion should create an unavoidable escalation clock.

Possible phases include:

1. Strange signals and unexplained anomalies.
2. Alien probes and reconnaissance.
3. Sabotage and small incursions.
4. Coordinated attacks on frontier systems.
5. Major invasion fleets.
6. Multi-front collapse.
7. Final offensive.

The player may delay, redirect, or weaken the invasion, but cannot ignore it forever.

The major invasion events should be consistent enough to learn between timelines. Smaller skirmishes and minor enemy fleet compositions can remain variable.

## Fleet Combat

Fleet combat supports the 4X layer. It should feel consequential and readable, but it is not a second primary game mode.

Important conflicts are resolved through Stellaris-style fleet battles: ships auto-target and fight once fleets engage.

The player's skill expression is before the battle, not during it:

- Ship design and weapon loadouts.
- Fleet composition and size.
- Technology and doctrines.
- Admirals or fleet bonuses.
- Whether the fleet arrives in time.
- Optional retreat before annihilation.

During the fight, ships move, target, and fire autonomously. The player watches the outcome, may issue a retreat order, and then continues on the empire map.

### Engagement flow

1. Fleets meet in a system, or an invasion force attacks a defended location.
2. The player sees relative strength, composition, and any known alien traits.
3. The player commits to fight or attempts to withdraw if possible.
4. Fleets auto-resolve in a short visual battle.
5. Surviving ships, wreckage, and strategic consequences update the empire state.

Battles should end when one side is destroyed, retreats, or a clear strength threshold makes the remaining fight irrelevant. The player should not need to eliminate every last ship for the outcome to resolve.

### What the player learns from combat

Fleet battles are a primary source of knowledge about the invasion:

- Which alien ship classes appear in each wave.
- Which weapons or defenses are effective.
- Whether a system can be held with the current fleet.
- How much lead time is needed to reinforce a front.
- Which technologies make later engagements winnable.

### Fleet build

The run-level military build can include:

- Ship classes and designs.
- Weapon and defense systems.
- Admirals.
- Research.
- Doctrines.
- Relics or artifacts.
- Stationed defenses and fortifications.
- Allied fleet support.

Counterplay should come from matching preparation to discovered alien weaknesses, not from mid-battle micromanagement.

## Learning Through Failure

Every failed timeline should reveal something meaningful.

Good failure:

> The northern colony fell because the player did not secure the relay before the second invasion wave.

Bad failure:

> A random event destroyed the only required research site before the player had any way to know it mattered.

The game should record:

- What caused the loss.
- What new information was discovered.
- Which objectives were completed.
- Which alien ship types and technologies were observed.
- Which new research or fleet options became available.

Once the player understands the early timeline, solved sections should become faster through:

- Faster travel.
- Skip options.
- Automatic resolution of low-risk events.
- Better starting options.
- Known map routes.

## Prototype Scope

The first playable prototype should focus on proving the 4X preparation loop and rewind learning, not combat spectacle.

Suggested prototype components:

- One small star map with meaningful expansion choices.
- One invasion route and escalation clock.
- Exploration, colonization, and basic resource pressure.
- At least two discovery synergies (for example: uranium → nuclear fleets, language research → better alien diplomacy).
- Three ship classes and simple auto-resolving fleet combat.
- One or two research choices that clearly change available strategies.
- Basic fortifications or system defenses.
- A few enemy fleet compositions.
- A simple rewind and discovery archive.

The prototype can use:

- Icons.
- Colored markers.
- Simple shapes.
- Placeholder ships.
- Basic VFX.
- Abstract planets and star systems.

Final art, faction identity, and detailed visual presentation can be developed after the strategic loop and fleet combat feel are proven.

## Design Principles

1. The empire / 4X layer is the primary game; combat is a consequence of preparation.
2. Exploration, research, and diplomacy should create readable strategic combos.
3. The invasion creates urgency but should remain understandable.
4. The player should gain both information and power between timelines.
5. Major strategic events should be learnable.
6. Fleet combat should auto-resolve based on preparation, not micromanagement.
7. Ship design, composition, tech, and positioning decide battles.
8. The player may commit or retreat, but does not control individual ships in combat.
9. Failure should reveal information or unlock options.
10. Permanent progression should expand possibilities more than it removes challenge.
11. Gameplay data should remain separate from visual presentation.
12. The project should validate the core loop before committing to final art or a large content roster.

## Open Questions

- What exactly causes a rewind: defeat, a voluntary reset, or a timeline resource?
- Are there exactly five timelines, or is five the initial campaign target?
- Which empire actions should resolve abstractly versus through visible fleet battles?
- What information should be deterministic between timelines?
- How much of the invasion plan should be discoverable in each attempt?
- What types of permanent power should carry across timelines?
- Should the player choose one of several viable defense plans?
- How many discovery synergy families does a campaign need to feel expressive?
- Which discoveries are fixed on the map versus randomized per timeline?
- How much combat detail should the player see: full visual fight, summary screen, or both?
- Can the player issue any orders during a battle beyond retreat?
- How should ship design depth compare to Stellaris without becoming a full ship designer?
