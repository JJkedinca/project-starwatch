# Invasion and Sabotage

## Role in the Game

The invasion is an active opponent with its own goals, intelligence, and strategy. It should pressure the player's empire before the main fleet arrives, undermine promising plans, and force the player to adapt.

The enemy should feel more like a civilization executing a long-term plan than a sequence of combat encounters.

## Enemy Goals

The invaders may try to:

- Establish an entry route.
- Scout the player's technology and fleet composition.
- Destroy or compromise research.
- Deny rare resources.
- Isolate player empires from one another.
- Manipulate or recruit local factions.
- Disrupt supply and production.
- Destroy relays and staging areas.
- Force the player into bad defensive positions.
- Prepare the final offensive.

Enemy actions should be connected. A scout may identify a uranium world, a later sabotage operation may target its logistics, and the main fleet may arrive after the player has been forced to divert its defenders.

## Invasion Intelligence Model

The enemy should have incomplete information about the player and learn over time.

It may observe:

- Fleet movement.
- Research direction.
- Resource concentration.
- Diplomatic relationships.
- Defensive weaknesses.
- Player response to previous attacks.

The player should also be able to mislead, conceal, or redirect enemy attention through:

- Sensor coverage.
- Decoy fleets.
- Disinformation.
- Diplomatic secrecy.
- Hidden ship designs.
- Alternate routes.

The first prototype can keep enemy intelligence simple, but the design should support an opponent that responds to the player's strongest visible plan.

## Escalation Structure

The invasion follows a broad sequence:

1. Strange signals and unexplained anomalies.
2. Reconnaissance and observation.
3. Scout fleets and probing attacks.
4. Sabotage and small incursions.
5. Coordinated attacks on frontier systems.
6. Main invasion fleet.
7. Multi-front collapse or final offensive.

Major beats should be stable enough to learn across timelines. Minor attacks, fleet compositions, target selection, and sabotage events can vary.

The escalation clock should create urgency without being opaque. Players should understand that delay matters even when they do not yet know exactly where the main fleet will arrive.

## Invasion Waves and Wormholes

The initial invasion can use a primary wormhole and a secondary wormhole. Both locations are fixed once generated for a run. The secondary wormhole exists primarily to split the player's attention and create a second front; its exact activation and reinforcement behavior remains a later tuning question.

The major invasion can arrive in two or three waves:

- Earlier scouts and probes establish the threat and provide intelligence.
- Major waves emerge as powerful combined forces before dispersing toward player territory.
- The final wave contains the strongest fleet and a unique boss capital ship mechanic.

The combined force should be too strong to defeat casually at the wormhole. After it begins spreading, the player can use defenses to chip away at the invasion, intercept separated fleets, and choose where to commit the main fleet.

Wormhole territory cannot be conquered or converted into normal player territory. The player may raid staging fleets and attack invasion infrastructure, but cannot settle the region or build permanent defenses there. Permanent defenses must be established along approach routes and in player-controlled systems.

## Repair Fleets and Attrition

Some major invasion fleets can be followed by mobile repair fleets or support groups. When a major fleet is sufficiently damaged, it may withdraw toward a repair fleet or staging area instead of continuing its advance.

This creates an opportunity for the player to:

- Intercept the repair fleet.
- Attack the damaged main fleet before it recovers.
- Force the invasion to remain weakened.
- Use defenses and smaller engagements to accumulate attrition.

Defenses should be able to inflict meaningful but not total damage before being overrun. The player should be able to weaken an invasion over several engagements rather than lose one battle and immediately lose the campaign.

## Frontier Pressure and Invasion

Frontier Pressure is an expansion and logistics pressure that exists alongside the invasion escalation clock. It represents the attention, exposure, and cost created by maintaining a large or distant empire.

Higher Frontier Pressure can make frontier systems more vulnerable to:

- Local raids and sorties.
- Minor sabotage.
- Supply disruption.
- Exploration crises.
- Colony stability problems.

This pressure should create side threats and opportunity costs rather than changing the invasion's main timing, wormhole location, high-level fleet composition, or overall objective. A player may reduce it by consolidating territory, improving infrastructure, securing routes, negotiating with nearby factions, or using stealth technology.

## Sabotage and Crisis Pool

Some crises should come from a pool so the player does not know which specific problem will happen in a timeline.

Major sabotage events should usually trigger at a recognizable escalation milestone, campaign year, or Frontier Pressure threshold. Minor strength-targeted sabotage may trigger when the player exposes a specific weakness or relies heavily on one strategy. The event should begin as a crisis with warning and a resolution window rather than applying immediate arbitrary damage.

Possible sabotage categories include:

- **Research interference:** A scientist disappears, a lab is compromised, or a research branch is delayed.
- **Resource disruption:** A mine, convoy, or extraction site is attacked.
- **Relay sabotage:** A gate, route, or defense network is disabled.
- **Infiltration:** A faction, colony, or ally becomes compromised.
- **Misinformation:** False signals point the player toward the wrong route or counter-tech.
- **Diplomatic manipulation:** The invaders provoke conflict between the player and a potential ally.
- **Fleet disruption:** A shipyard, repair base, or supply hub is targeted.
- **Population destabilization:** Refugees, biological threats, or cultural conflict create internal pressure.

Each event should have specific prevention or mitigation mechanics that are difficult to know initially but discoverable through clues, scouting, diplomacy, research, or failure.

## Colony Stability and the Alien-Aligned Underground

The invaders are supported by an underground movement within settled space that welcomes, assists, or worships the alien overlords. This is not a conventional diplomatic faction and should not create a separate internal-politics simulation.

For the initial design, underground activity is one source of **colony stability/internal pressure** rather than a separate empire-level subversion meter. The player manages one local condition that can be caused by:

- Major military defeats or abandoned colonies.
- Shortages, poor supply, or visible overextension.
- Alien propaganda, contact, or cultural influence.
- Enemy victories nearby.
- Repeatedly exposing the same strategic weakness.
- An activated underground cell.

Low colony stability or internal pressure may cause:

- Reduced output or delayed construction.
- Intelligence leaks and false sensor reports.
- Research, shipyard, or logistics disruption.
- Unrest during settlement or expansion.
- Local sabotage events.
- Increased vulnerability to invasion pressure.

An underground cell can remain hidden inside a stable colony. If it activates, it creates an event or stability loss rather than requiring a second visible meter. The source of the problem should determine the appropriate counter: supply and infrastructure for shortages, security and investigation for infiltration, and defenses or fleets for direct invasion pressure.

Counterplay may include counterintelligence, investigations, improved supply and stability, exposing the movement's connection to the invaders, and protecting likely targets. Investigating or surviving a cell should reveal useful information about enemy methods, future targets, infiltration routes, or sabotage weaknesses. A player may even tolerate a low-level cell temporarily to gain intelligence, accepting short-term damage for a long-term advantage.

## Fairness Rules

Sabotage should be surprising but not arbitrary.

Every serious crisis should provide at least one of:

- A warning signal.
- A suspicious pattern.
- A visible prerequisite.
- A partial defense.
- A recoverable loss.
- A discovery that makes future timelines safer.

The player should be able to say:

> I did not know this could happen, but now I understand what warning I missed and how to prevent it next time.

Avoid destroying a unique required location with no warning or counterplay. A sabotage event can still be severe, but the player needs a meaningful response window.

## Enemy Adaptation

For the initial version, enemy intelligence should remain simple. The invasion's high-level composition should remain mostly fixed within a run. Each wave should have a learnable identity:

- Ship classes and broad fleet composition.
- Weapon and defense types.
- Technology level.
- Main fleet capabilities.
- Intended escalation role.

The same invasion fleet can still vary its low-level behavior from battle to battle:

- Choose different eligible targets.
- Split or concentrate its forces.
- Pursue or retreat.
- Prioritize carriers, defenses, or logistics.
- Attempt to regroup.
- Take different routes within the known invasion structure.

The enemy should not dynamically redesign its fleet to hard-counter the player's composition. A player who learns how to defeat a scout or main fleet should be able to rely on that knowledge across later timelines in the same run.

The invasion does not permanently learn the player's strategy or construct a new counter-composition. Its low-level target selection, splitting, regrouping, and pursuit behavior can vary within the fixed invasion structure.

### Strength-Targeted Minor Sabotage

The invasion may respond to the player's strongest visible strategy through smaller sabotage events rather than fleet redesign:

- A carrier build suffers delayed strike-craft replenishment or damaged carrier logistics.
- A sniper build has sensors or targeting infrastructure disrupted.
- A brawler build has repair bases or supply routes attacked.
- A missile build loses access to ammunition production or industrial capacity.
- A research-heavy empire has a key lab delayed or temporarily weakened.
- A single-anchor economy has its supporting route or infrastructure disrupted.

These minor sabotages should be temporary or recoverable, clearly telegraphed, and targeted at supporting systems rather than directly invalidating the player's fleet. They should teach the player how the invasion responds without changing the invasion's core composition or overall objective.

## Local Hostiles Versus Invaders

Neighboring hostile aliens are distinct from the invasion force.

Minor and local factions:

- Teach the early map and fleet basics.
- Compete for nearby resources and territory.
- May be negotiated with, conquered, or manipulated.
- Can serve as a temporary buffer if the player chooses to protect them.

The invaders:

- Have a separate origin and strategic objective.
- Follow the campaign escalation.
- Use asymmetrical technology and sabotage.
- Become the focus of Rewinds 1–4.

The distinction helps Rewind 0 introduce military danger without revealing the full invasion.

### Minor Factions in the Invasion

In the initial design, the invasion treats minor factions as territorial occupants rather than giving them a separate invasion alignment or campaign. Their systems can be invaded and overrun like other strategic territory.

If the player provides no support:

- The faction's finite fleets and defenses are overwhelmed relatively quickly.
- Its territory, resources, and specialties may be lost.
- The player learns that the faction cannot solve the invasion independently.

If the player protects the faction:

- It can slowly replenish its forces.
- It may survive another local invasion wave.
- It can continue acting as a limited buffer or preserve its strategic benefits.

Protected minor factions should still have a strength ceiling. They can delay or weaken the invasion, but cannot scale indefinitely or hold off the invasion force alone. Invasion-aligned or puppeted minor factions are deferred to a future expansion.

## Counterplay

Players can respond to enemy pressure through:

- Scouting and sensor coverage.
- Fleet positioning.
- Fortification and redundancy.
- Diplomacy and faction protection.
- Counterintelligence.
- Research.
- Decoy operations.
- Resource stockpiles.
- Evacuation or abandonment.
- Attacking enemy logistics and relays.

There should rarely be only one response. The best response depends on the current economy, discoveries, alliances, and available time.

## Sabotage as Discovery

Surviving or suffering a crisis should reveal information:

- The event's trigger.
- The enemy's objective.
- The target's vulnerability.
- The required prevention technology.
- A safer route or alternate solution.

For example, a player may learn that a shield network is supported by a remote relay. Later timelines can skip the anti-shield technology branch and destroy the relay instead.

The player may also deliberately allow a controlled crisis to learn more, accepting short-term damage for long-term intelligence.

## Multiplayer Pressure

Co-op should increase the number of simultaneous problems rather than only increasing enemy statistics.

The invasion can:

- Attack different player empires at once.
- Target the player holding a unique resource.
- Disrupt a joint project.
- Create a crisis that requires fleets from two empires.
- Spread misinformation between allies.
- Force one empire to choose between defending itself and supporting another.
- Exploit an empire with high internal pressure as an entry point against shared infrastructure.

Each empire can have its own subversion pressure, while shared intelligence reveals known cells and sabotage patterns to all players. Cooperation comes from assigning counterintelligence, protecting exposed targets, and accepting opportunity costs.

## Final Offensive

The main invasion fleet should have a vulnerable strategic structure that can be learned over multiple timelines. The initial game's final objective is to destroy or decisively cripple the invasion's meaningful fleet capacity and territory-taking force.

The final operation may also involve supporting objectives:

- Collapsing a relay network.
- Breaking a supply or reinforcement route.
- Disabling a defense or staging system.
- Protecting the approach route.

The final wave should contain the strongest invasion fleet and a unique boss capital ship mechanic. Destroying the boss should weaken enemy coordination, reinforcement, or repair, but should not automatically end the run. The player must complete a finite cleanup operation against the remaining meaningful invasion fleets or force their collapse; hunting every individual straggler is not required.

The final offensive should combine fleet power, discovered weaknesses, route knowledge, diplomacy, and logistics. It should be possible to win through more than one defense plan, even if some plans are more efficient.

## Open Questions

- What exact role and activation pattern should the secondary wormhole have?
- How much information does the enemy begin with?
- How much time should exist between major waves?
- How large should the sabotage event pool be?
- Which sabotage slots are required by the escalation rules, and which event identities can be randomized?
- How are warnings communicated without making every crisis obvious?
- Can the player permanently eliminate an enemy capability?
- How should repair fleets choose when and where to withdraw?
- What support objectives make the main fleet battle feel like a strategic operation rather than one isolated engagement?
