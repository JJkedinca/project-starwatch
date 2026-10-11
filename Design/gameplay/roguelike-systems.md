# Roguelike Systems

## Role in the Game

The roguelike structure turns a compressed 4X campaign into a process of learning, experimentation, and execution.

The player's goal is not simply to become numerically stronger. It is to understand the current setup, identify the best available plan, and optimize that plan before the invasion arrives.

## Terminology

- **Run:** One complete campaign or new game, targeted at roughly six to eight hours.
- **Timeline:** One empire attempt within a run.
- **Rewind:** Resetting the current timeline and moving to the next attempt.
- **Rewinds 0–4:** The five total timelines in one run.
- **Timeline power:** Empire state built during the current timeline.
- **Run knowledge:** Information discovered during the current run and carried across its timelines.
- **Post-run progression:** Unlocks available in future runs.

## Timeline Lifecycle

Each timeline follows a broad loop:

1. Establish the empire and inspect the available setup.
2. Explore, expand, research, and develop infrastructure.
3. Identify resources, factions, relics, and enemy behavior.
4. Commit to a strategic plan or pivot when new information appears.
5. Respond to local threats and invasion escalation.
6. Reach a milestone, lose, or decide to rewind.
7. Preserve discoveries immediately.
8. Rebuild with more knowledge and a better route.

Players may voluntarily rewind whenever they want. This consumes the current attempt and ends the timeline, so early rewinding is convenient but gives up the opportunity to discover additional information.

Defeat also ends the timeline. Discoveries are recorded immediately when observed and do not require surviving the timeline.

## Five-Timeline Campaign Arc

The intended milestones are soft gates:

- **Rewind 0:** Neighboring hostile aliens expose the initial map and local threats.
- **Rewind 1:** The player reaches the invasion scouts, loses, and gains key intelligence.
- **Rewind 2:** The player defeats the scouts and expands into the midgame.
- **Rewind 3:** The player reaches the main fleet, loses or wins, and learns what the final defense requires.
- **Rewind 4:** The player executes the final defense and offensive plan.

Skilled players may reach later milestones early. The game should not force a scripted loss or block content to preserve the intended order.

## Persistence Layers

### Timeline power

Resets after a rewind:

- Colonies and territory.
- Fleets and ship designs.
- Resources.
- Incomplete research project progress.
- Temporary alliances.
- Fortifications.
- Current production and logistics.
- Research buildings and infrastructure.
- Current Frontier Pressure.
- Colony stability and internal pressure.
- Current invasion fleet positions and active crises.
- Current minor-faction relationship states.

### Run knowledge

Persists across timelines in the same run:

- Map and route discoveries.
- Relic locations.
- Invasion routes and timing.
- Enemy fleet observations.
- Alien weaknesses.
- Faction incentives and betrayals.
- Sabotage points.
- Anomaly and event triggers.
- Blueprints and research requirements.
- Frontier Pressure rules and known escalation triggers.

### Permanent within-run unlocks

Persist across the run once discovered or completed:

- Completed research and fully understood technologies.
- Revealed research branches and faction specialties.
- Ship classes.
- Doctrines.
- Defensive structures.
- Diplomatic options.
- Ways to interact with anomalies.

These unlocks provide knowledge and access, not physical timeline power. The player must still rebuild the infrastructure, resources, shipyards, and fleets needed to exploit them after a rewind.

### Post-run progression

Persists between separate runs:

- New races or civilizations.
- New ships and technologies.
- Artifacts and modifiers.
- Starting options.
- Power-up investments.
- Higher difficulty levels.

The exact persistence of run-specific facts between separate runs remains a separate design decision.

For the initial design, no research branches or completed technologies from the current run carry into a separate run. Higher Ascension levels may later reduce how much research remains available between rewinds, while preserving the player's factual knowledge in the archive.

## Discovery and Synergy Model

The strongest run-specific combos should usually have four parts:

1. **Enabler:** A resource, relic, ally, discovery, or location.
2. **Converter:** Research, infrastructure, doctrine, or diplomatic knowledge.
3. **Payoff:** A powerful fleet, economic advantage, diplomatic shortcut, or defense.
4. **Consequence:** Something expensive becomes unnecessary, or a new route becomes viable.

Examples:

- Uranium deposit + reactor relic + nuclear research → high-endurance fleet.
- Scout wreckage + shield relay location + fast ships → bypass the anti-shield branch.
- Alien language + friendly species + shared threat → alliance fleet and a secure flank.
- Ancient blueprint + rare shipyard + industrial doctrine → mass production of a unique hull.

The player should not be able to force the same combo every run. The available map, resources, factions, and threats determine which combinations are practical.

## Rule-Bending Technologies and Artifacts

Some technologies, relics, and artifacts should give the player powerful features that bend or temporarily rewrite normal system rules. These are a major source of agency and should let players build a run around an unusual opportunity.

Examples:

- **Escape Drive:** Temporarily bypass the corridor graph at extreme cost.
- **Relay Hijacker:** Convert an enemy relay into a temporary player route.
- **Diplomatic Translator:** Establish contact with a species without conquering its border.
- **Ancient Forge:** Produce one advanced ship class without its normal industrial chain.
- **Invasion Scrambler:** Delay or redirect one invasion wave.
- **Gravitic Weapon:** Defeat a defense that would otherwise require an entire research branch.

These effects should be powerful rather than perfectly efficient. They are balanced through constraints:

- Narrow use cases.
- Significant resource, research, or political costs.
- Fleet damage, casualties, instability, or other risks.
- Limited uses, cooldowns, or recovery periods.
- Opportunity costs within the current timeline.
- Clear discovery requirements and counterplay.

Rule-bending effects should not permanently change the generated map or erase the broader strategic problem. They create temporary exceptions, alternate solutions, or new routes through a problem while preserving the rest of the game.

The player should be able to recognize a potential power fantasy and commit to it:

> This changes what is possible. I can build the rest of this run around it.

Not every artifact needs to be equally valuable. Its value depends on the current map, resources, factions, enemy behavior, and the player's chosen plan. Missing one component should encourage a pivot rather than make the timeline unwinnable.

## Knowledge and Uncertainty

The player should gradually move from:

- Unknown location.
- Possible lead.
- Observed fact.
- Confirmed weakness.
- Reliable plan.

The archive should distinguish observation from certainty. A first wreck analysis may suggest a weakness, while a later encounter confirms it.

Knowledge should always change a future decision. A discovery that only adds flavor text is not meaningful progression.

## Run Variation

The campaign is generated from authored content pools and strict scenario rules. Major content is not fixed to one map or one run.

Stable rules should define:

- Number and approximate size of important systems.
- Route connectivity and expansion requirements.
- Number and tiers of relics, factions, sabotage events, and objectives.
- Timeline windows for scouts, main fleets, and the final offensive.
- Required clues, counters, and recovery paths.
- Minimum conditions that guarantee a solvable defense plan.

The run generator determines:

- Map topology and important routes.
- Which systems receive special roles.
- Resource deposits and production sites.
- Which relics and blueprints appear.
- Faction identities, locations, and starting attitudes.
- Invasion wormhole location and fleet compositions.
- Sabotage events and their targets.
- The final objective and the enemy's vulnerable structure.

The desired result is a player who understands the rules and content pool well enough to optimize, but must still analyze the current setup rather than follow one solved checklist.

## Failure and Rewards

Every failed timeline should provide useful information:

- Cause of loss.
- Enemy composition and weakness.
- Route or timing information.
- Completed objectives.
- Recovered relic or blueprint data.
- New research or fleet options.
- Faction knowledge.

The player should be able to explain why the timeline failed and how the next attempt can be better.

## Multiplayer Rewinds

In co-op:

- The run and rewind budget are shared.
- All player empires reset together.
- General discoveries enter the shared archive immediately.
- Blueprints and physical discoveries remain owned by the player who found or recovered them for now.
- A voluntary rewind should require group confirmation.
- A player losing an empire should create a crisis before causing automatic elimination.

The group should be able to choose between continuing a damaged timeline for more intel or rewinding for a cleaner execution.

## Final Offensive

The final timeline should culminate in a multi-stage operation:

- Secure the required resources and allies.
- Position fleets and staging bases.
- Disable relays or sabotage points.
- Intercept and defeat or decisively cripple the main invasion fleet.

The final offensive should test execution, not merely check whether the player found the correct answer.

## Open Questions

- What exact knowledge persists between separate runs?
- Are five timelines always available, or can post-run modifiers change the count?
- How much scenario variation can the campaign support without undermining learning?
- Which discoveries grant immediate run unlocks versus only archive knowledge?
- Can a run be won before Rewind 4?
- What fleet composition, support network, and conditions make defeating the main fleet a satisfying final objective?
- How are partial discoveries and uncertain hypotheses represented?
