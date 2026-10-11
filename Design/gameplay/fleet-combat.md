# Fleet Combat

## Role in the Game

Fleet combat is the consequence of 4X preparation, not a separate tactical game mode. The player makes the important decisions before fleets engage:

- What to research.
- Which ships to build.
- How to compose fleets.
- Where to position them.
- Which routes and systems to defend.
- Whether to commit or retreat.

Once combat begins, ships move, target, and fire autonomously in a readable visual battle.

## General Combat Model

The standard fleet-combat pattern should provide the foundation for Starwatch, while the roguelike layer determines how players exploit and bend those rules.

### Readable Fleet Baseline

The standard pattern should give players an understandable combat language:

- Small ships are fast and numerous.
- Screens protect larger ships, carriers, and support vessels.
- Battleships provide heavy firepower.
- Carriers project strike craft.
- Strike craft counter different ship types.
- Range determines who can threaten whom and how quickly.

Counters should be soft rather than absolute. A counter creates an advantage but does not make the opposing class useless. Composition, numbers, positioning, research, relics, and discoveries should let players overcome a bad matchup.

### Automatic Targeting Behavior

Ships should automatically choose targets according to their role:

- Screens protect carriers, support ships, and capital ships.
- Artillery targets exposed heavy ships or high-value structures.
- Fighters prioritize bombers and enemy strike craft.
- Bombers prioritize capitals, stations, and defenses.
- Raiders target logistics and support ships.

Targeting should be predictable enough for players to understand without requiring individual target commands. The player's main control is building a fleet whose composition produces the desired behavior.

### Engagement Bands

Weapon ranges should use a small number of meaningful engagement bands rather than detailed range simulation:

- **Standoff range:** Artillery, sensors, and long-range weapons operate.
- **Engagement range:** Most line ships fight.
- **Close range:** Corvettes, assault ships, and certain strike craft become more effective.

Range should influence the opening exchange, target access, damage taken, and retreat without completely preventing a fleet from fighting. Research and relics may create exceptions, such as bypassing the normal approach phase or remaining effective at extreme range.

### Missile Warfare

Missiles are ship-launched salvo weapons rather than strike craft. They create a strong saturation and area-damage strategy:

- Effective against clustered fleets, slow ships, capitals, stations, and defenses.
- Capable of damaging multiple nearby ships rather than only the selected target.
- Particularly dangerous when a fleet lacks screening or point defense.
- Less reliable against fast, dispersed targets and fleets with strong interception.
- Limited by reload time, ammunition or resource costs, targeting information, and the risk of having salvos intercepted.

Screens, fighters, interceptors, point defense, and electronic countermeasures should be able to reduce missile damage. However, a large enough salvo may overwhelm those defenses, allowing missiles to remain a meaningful threat without becoming an automatic counter. Missile classes can specialize in different payloads, but the initial system should keep their core identity as high-burst, formation-punishing weapons.

### Roguelike Combat Expression

Starwatch becomes distinctive when discoveries let the player bend the normal rules and create powerful run-specific combinations. Examples include:

- A relic lets bombers bypass screening once per engagement.
- A sensor discovery allows artillery to target enemy carriers before the main line engages.
- A sabotage discovery makes an expensive anti-shield fleet unnecessary.
- Nuclear technology lets a fleet sustain long-range operations and avoid returning to repair as often.
- A rare shipyard makes an unusual ship class cheap enough to build in large numbers.
- A carrier-focused discovery turns a normally defensive fleet into a mobile strike force.

These effects should create new strategic behaviors rather than only provide small statistical increases. Players should feel that they discovered a powerful combination and learned how to exploit it.

### Varied Battle Objectives

Winning does not always mean destroying every enemy ship. Battle objectives may include:

- Intercepting an enemy fleet.
- Delaying an enemy until reinforcements arrive.
- Protecting a carrier, convoy, or support vessel.
- Holding a relay or strategic system.
- Escaping with valuable intelligence.
- Securing a debris field.
- Breaking through a blockade.

The overall goal is standard roles for readability, soft counters for strategic composition, and roguelike discoveries that create exceptions to the normal rules without making every run play the same way.

## Ship Naming and Design Hierarchy

Starwatch should use the following terminology:

- **Category:** A broad grouping such as standard ship, capital ship, support ship, or strike craft.
- **Ship type or role:** A functional type such as corvette, battleship, carrier, scout, or collector.
- **Class:** A distinct named blueprint with a fixed hull, loadout, and strategic identity.

For example, a `Longbow-class Battleship` could be a long-range sniper while a `Bulwark-class Battleship` could be a heavy brawler. Both share the battleship type but are separate classes.

There should not be a separate variant system. If research, a relic, a faction trait, or a recovered blueprint creates a meaningfully different configuration, it should unlock a new named class, potentially based on an earlier class. Smaller improvements can remain modifiers applied to an existing class.

## Ship Model

Each ship class should have a clear default strategic identity defined by:

- Hull class and size.
- Default weapons and defenses.
- Speed and maneuverability.
- Sensors and detection.
- Range or engagement profile.
- Fleet role.
- Construction and upkeep cost.
- Special technology or relic interactions.

Initial roles may include:

- Screening and interception.
- Long-range artillery.
- Direct assault.
- Carrier or support.
- Siege and bombardment.
- Reconnaissance.
- Logistics and repair.

Roles should interact through composition rather than requiring the player to micromanage individual ships.

## Ship Progression and Modifiers

The initial game should not use a Stellaris-style modular ship designer. Each ship class starts with a default hull and loadout. The player's main choices are which classes to build, how to combine them into fleets, and which research or discoveries to pursue.

### Sources of Fleet Identity

Every class should provide a clear baseline identity:

- A heavy battleship may begin with strong armor and slow weapons.
- A sniper class may begin with long-range weapons and weak close defenses.
- A carrier may begin with basic strike-craft capacity.
- A support ship may begin with scouting, repair, or salvage functionality.

Meaningful changes to a design should come from distinct sources:

- **Research:** Provides reliable specialization such as shield regeneration, heavy armor, ion weapons, armor-piercing weapons, missiles, point defense, or engine and targeting disruption. Research can also unlock new named ship classes.
- **Blueprints:** Discoveries from exploration, scouting, combat, or specific locations can unlock alternate named classes, such as a sniper battleship, missile cruiser, or high-capacity carrier.
- **Relics and alien discoveries:** Provide unusual, run-defining effects that bend normal combat rules, such as partially bypassing shields, ignoring screening for an attack, repairing ships in the field, or creating a fleet-wide combat effect.
- **Species or faction identity:** Provides a bias toward a military direction, such as stronger shield technology, more efficient missile production, better carrier logistics, or improved repair and endurance.
- **Strategic resources and infrastructure:** Make particular classes or fleet identities cheaper, stronger, or easier to sustain.

These sources should create meaningful strategic behaviors rather than only small statistical increases. A discovery might allow a fleet to operate farther from supply, bypass a defense type, survive an ambush, or make an otherwise weak ship class valuable.

### Specialized Defensive and Offensive Systems

Classes, research, blueprints, and relics may provide different defense profiles:

- **Fast-recharge shields:** Smaller capacity that recovers quickly during sustained operations.
- **Heavy shields:** Large capacity with slow or limited recharge, strong against opening volleys.
- **Heavy armor:** Reduces damage from weak weapons and strike craft but adds mass or reduces speed.
- **Piercing weapons:** Bypass or reduce part of a shield or armor layer without making that defense irrelevant.
- **Disruption weapons:** Drain shields, slow engines, reduce targeting accuracy, delay weapons, disrupt sensors, or increase repair costs.

The basic counters needed to respond to common threats should remain available in every run. Research, blueprints, relics, and species traits should make a response more efficient or create a powerful alternative rather than being the only possible way to defeat an enemy.

There should be no individual component selection, power-budget management, or custom module fitting in the initial version. Existing ships may require a repair or refit period to receive major upgrades; the exact upgrade application model remains open.

## Fleets

A fleet is the normal strategic unit of combat. It includes:

- A collection of ships.
- An admiral or doctrine.
- A home base or supply connection.
- Current orders.
- Damage, repair, and readiness state.
- Intelligence about the target.

Fleet composition should matter more than a single combat power number. The interface should still provide a useful estimate of likely outcomes, including uncertainty caused by unknown enemy technology.

Fleets can:

- Patrol.
- Escort.
- Intercept.
- Defend a system.
- Attack a target.
- Scout.
- Raid logistics.
- Retreat and repair.
- Join a coordinated operation.

## Auto-Resolving Engagements

An engagement follows a clear strategic sequence:

1. Fleets detect or encounter one another.
2. The player sees known information and estimated strength.
3. The player commits, delays, or attempts to retreat.
4. Ships resolve the battle autonomously.
5. Survivors, damage, wreckage, and system consequences are applied.
6. New combat information enters the archive.

During a fight, the player may have limited strategic controls:

- Retreat.
- Change a fleet-level stance.
- Commit reinforcements if they arrive in time.
- Trigger an emergency fleet ability if the design supports one.

There should be no individual ship movement or action queue.

### Reinforcements and Fleet Merging

Separate fleets do not need to merge in order to support one another. If a fleet arrives in a system while a battle is active, it joins the engagement and begins attacking the opposing side.

Fleets remain separate unless the player explicitly issues a merge order. Fleets should never merge automatically merely because they occupy the same system or arrive at the same battle. Exact command and ownership behavior after a merge can be refined during implementation.

### Post-Initial Feature: Fleet Abilities and Stances

After the initial implementation, special ships, capital ships, admirals, relics, or research may grant limited fleet-level abilities or stances. These should add strategic decisions without turning combat into a tactical micromanagement mode.

Possible abilities include:

- **Emergency burn:** Shortens the retreat charge at an energy, engine, or hull cost.
- **Combat scan:** Reveals another tier of enemy information during an engagement.
- **Emergency repairs:** Restores shields or limited hull health when the fleet has suitable support.
- **Weapon overcharge:** Temporarily increases offensive power at a later repair or resource cost.
- **Decoy signal:** Reduces enemy pursuit effectiveness or improves the chance of a successful retreat.
- **Reinforcement beacon:** Calls an allied or reserve fleet if it can arrive in time.

Stances may trade one fleet property for another, such as improving protection for vulnerable ships while reducing speed or offensive output. These abilities should be limited in number, have clear consequences, and come from the fleet's composition or discoveries rather than requiring individual ship control.

## Combat Resolution

Resolution should consider:

- Ship classes, upgrades, and counters.
- Fleet composition.
- Size and readiness.
- Admirals and doctrines.
- Technology.
- Position and system defenses.
- Supply and repair state.
- Surprise, detection, and ambush.
- Known or unknown enemy traits.

Combat should be understandable and losses must be explainable. A post-battle report should identify major causes:

- Enemy technology or weapon advantage.
- Insufficient screening.
- Poor range profile.
- Outnumbered fleet.
- Lack of supply.
- Late arrival.
- A discovered but unaddressed vulnerability.

## Retreat and Recovery

The player can order a retreat at any point during an engagement, but issuing the order does not guarantee escape.

After a retreat order:

- The fleet enters a short disengagement or thruster-charge state.
- Ships cannot fire offensively during the charge and are more vulnerable.
- The fleet escapes if it survives the charge and has a viable route to safety.
- Enemy pursuit, route control, fleet speed, damage, and nearby safe systems affect the outcome.

A separate minimum battle duration is not required initially; the charge state should provide the risk of an immediate retreat. A short initial contact lock can be added later if instant engagement-and-retreat behavior is too safe.

Retreat should usually cost the player time, position, territory, or damaged ships rather than destroying the entire fleet. A defeated fleet should remain recoverable often enough to support learning and adaptation, but retreating from an objective should have meaningful strategic consequences.

## Ship Damage and Destruction

Ships use temporary defenses and persistent hull health:

- Shields absorb damage and can recover after a ship stops taking fire or returns to safety.
- Hull health can be fractional; damage persists between battles and can reduce combat performance when severe.
- A ship reaching zero hull health is destroyed.
- Damaged ships can be repaired at a suitable base using time and industry.
- Support ships, technologies, or relics may provide limited emergency or in-field repairs.

Disabled or abandoned ships are not part of the normal damage model. They are a future or reach-goal mechanic that could result from boarding, sabotage, reactor failure, crew evacuation, relic effects, or special crises. Such ships could be recovered through salvage or rescue and potentially provide access to a rare or powerful ship.

## Debris and Salvage

Destroyed ships create debris at the battle site. Debris can provide two strategic opportunities:

- **Salvage:** Specialized ships recover industry, strategic materials, trade value, or rare components.
- **Analysis:** Researchers study the wreckage to generate research and reveal enemy weapons, defenses, technology levels, or vulnerabilities.

Salvage should recover only part of a destroyed ship's value, so ship losses still matter. Debris should also require time and protection, decay or become contested, and expose salvage ships to enemy interference. Salvage operations can therefore turn a victory into an economic recovery opportunity without making fleets disposable.

Debris created in a timeline disappears when that timeline resets, but knowledge of the battle, its location, and its technology can persist across rewinds.

## Defenses and Systems

Systems can contribute to combat through:

- Orbital defenses.
- Fortifications.
- Sensor arrays.
- Minefields or route hazards.
- Repair and reinforcement capacity.
- Planetary shields.
- Relays and command structures.

Planetary defense buildings function as stationary combat units attached to their planet. They automatically participate when a hostile fleet engages that system and can contribute attack, health, armor, shields, sensors, or other defensive bonuses.

Defense stations use the same basic rules at the system level. They are stationary units with substantial defensive health, attack, and other bonuses. They can protect a route, relay, or system but cannot pursue or retreat like a fleet.

Defenses should shape the battle and create preparation choices. They can be damaged or destroyed, but should not replace fleets entirely.

## Planet Occupation

After defeating the relevant defenses and controlling a system, a fleet can occupy an enemy planet rather than immediately conquering it.

An occupied planet:

- Diverts a portion of its normal income and resource output to the occupier.
- Denies that diverted output to the original owner.
- Operates below full efficiency while occupied.
- Can be liberated by another fleet.

The diverted output is transferred rather than duplicated. An initial target of approximately half of normal output can be tuned during implementation. Occupation should remain an intentionally lightweight control state rather than introduce a separate population or internal-politics simulation.

## Combat Intelligence

Combat is a major source of persistent knowledge. Better scouting and analysis should reveal increasingly useful information:

1. **Combined fleet strength:** A broad assessment of whether the enemy fleet is weaker, comparable, or stronger.
2. **Ship types:** The classes and general roles present.
3. **Ship counts:** The number of each class, initially hidden or revealed through further intelligence.
4. **Equipment and power level:** The weapons, defenses, technology, relic effects, and vulnerabilities of each class.

The first version should experiment with how exact this information needs to be. When information is unavailable, it should generally be shown as unknown rather than intentionally inaccurate. Exact counts, technical profiles, and operational details can become more precise as the player invests in sensors, scouting, combat observation, wreckage analysis, diplomacy, or research.

Operational intelligence is a related but separate axis. Players may also learn:

- Where the fleet is going.
- How quickly it can arrive.
- Whether it is damaged or supplied.
- Which system it is staging from.
- Whether reinforcements are nearby.

Combat intelligence is stored in the run archive and shared with allies according to the multiplayer information rules. Enemy variants or new discoveries can still create new unknowns.

## Discovery Synergies

Fleet power should come from combinations across the empire:

- Uranium and nuclear research create high-endurance fleets.
- A relic changes the role of a ship class.
- Alien language research enables a joint fleet.
- A sabotage point removes the need for an expensive anti-shield design.
- A rare shipyard makes an otherwise impractical hull affordable.

These combinations should be strong enough to support a power fantasy while remaining conditional on the current timeline's resources and discoveries.

## Multiplayer Fleet Operations

Each player owns their fleets. Cooperation can happen through:

- Fleet support.
- Shared routes and staging bases.
- Coordinated arrival times.
- Combined task forces.
- Escort and supply agreements.
- Multiple simultaneous engagements.
- Coalition fleet projects.

An allied fleet may contribute to a battle without transferring permanent ownership. Fleets do not merge automatically; an explicit merge order is required.

## Battle Presentation and Reports

Combat should make major interactions and outcomes understandable, but the exact visual presentation is an implementation concern. The initial design requirements are:

- Players can understand the major reason a battle was won or lost.
- Post-battle reports show ships deployed, destroyed, and surviving by class.
- Reports show damage, repairs, debris, and newly discovered enemy information.
- Enemy details remain limited by the player's available intelligence.

The specific use of health bars, damage numbers, effects, camera behavior, and report layout should be tested during implementation.

## Final Offensive

The final offensive should be a sequence of strategically connected fleet operations:

- Assemble the required fleet and supplies.
- Secure the approach route and a suitable staging area.
- Disable relays or defenses.
- Survive the main enemy fleet.
- Destroy or decisively cripple the main invasion fleet.

Earlier discoveries should make this operation more efficient, but the player still needs enough timeline power to execute the plan.

## Open Questions

- How many ship classes are needed for the first prototype?
- Which upgrades apply automatically, and which require a refit?
- How much visual detail should battles show before returning control to the empire map? This is an implementation and playtesting question.
- What ownership and command behavior should follow an explicit fleet merge?
- How exact should each intelligence tier be in practice?
- What emergency controls are allowed during auto-resolution?
- How are carrier, siege, repair, and logistics roles represented?
- What exactly ends an engagement when one side is clearly losing?
- How long should debris remain available, and how contested should salvage be?
