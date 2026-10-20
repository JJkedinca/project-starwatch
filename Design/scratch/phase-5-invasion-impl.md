# Phase 5 Invasion Implementation

## Purpose

This document plans Phase 5: hostile pressure, the invasion clock, and the first crisis. It follows the procedural generation work in [initial-generation-impl.md](initial-generation-impl.md) and expands the Phase 5 outline in [engineering-plan.md](engineering-plan.md).

The high-level intent lives in the design docs and is not repeated here:

- [invasion-and-sabotage.md](Design/gameplay/invasion-and-sabotage.md)
- [fleet-combat.md](Design/gameplay/fleet-combat.md)
- [README.md](Design/gameplay/README.md) (campaign milestones per rewind)

Every number here is a starting value for playtesting, not final balance.

## Goals

- The player sees threats coming and has time to prepare.
- Rewind 0 usually ends against the neighboring hostiles, as the campaign milestones intend. The invasion appears as signals and a scout.
- One lost battle or colony never ends the timeline by itself.
- The invasion's major beats are fixed per run, so a later timeline can plan around them.
- Phase 5 reports terminal conditions. Phase 6 ends the timeline and rewinds.

## Prerequisites

- **Procedural generation is in place.** It provides the reserved hostile-neighbor region and both wormhole nodes.
- **Combat foundation.** Phase 4 deferred battle, so Phase 5 starts by adding it (slice 5A). Without combat, no threat in this phase can do anything.

## Slices

Phase 5 is built in five slices. Each one ends playable.

| Slice | Adds | Playable result |
| --- | --- | --- |
| 5A Combat foundation | Ship combat stats, fleet owners, engagements, damage, retreat, repair, defense platforms | Two fleets meeting in a system fight and leave readable results |
| 5B Hostile neighbors | A local faction in the reserved region, raids, colony raid and loss consequences | The Rewind 0 failure: local raids escalate into a push the player must prepare for |
| 5C Invasion clock | Fixed schedule, warnings, signals, scout fleet, first major wave with repair withdrawal | The real enemy appears on a learnable timetable |
| 5D Sabotage crisis | One crisis with a warning, response window, and recoverable consequences | The enemy hits a supporting system, and the player can prevent or soften it |
| 5E Outcome reporting and intel | Terminal-condition reports and archive entries for everything observed | Phase 6 can end and rewind the timeline, and the next attempt knows more |

## 5A: Combat Foundation

### Ownership

- Every fleet gets an owner: the player, a hostile faction, or the invasion.
- Two fleets with hostile owners in the same system start an engagement.
- A fleet that arrives at a system holding a hostile fleet stops there; it cannot pass through.

### Ship Combat Stats

Ship classes gain hull, damage, and a role behavior. The role rules come from [fleet-combat.md](Design/gameplay/fleet-combat.md): screens protect larger ships and carriers project strike craft.

| Class | Owner | Role | Hull | Damage per round | Role behavior |
| --- | --- | --- | --- | --- | --- |
| Survey Corvette | Player | Scout | 20 | 2 | Withdraws at the start of any engagement if a route out is open |
| Destroyer | Player | Screen | 60 | 8 | Absorbs half of the damage aimed at non-screen ships while any screen survives |
| Cruiser | Player | Line | 150 | 20 | None |
| Carrier | Player | Carrier | 100 | 14 | Strike craft ignore enemy screens |
| Raider | Hostile neighbor | Screen | 50 | 7 | Same as Destroyer |
| Marauder | Hostile neighbor | Line | 130 | 16 | None |
| Seeker | Invasion | Scout | 40 | 10 | Withdraws once it has fought one round |
| Lancer | Invasion | Line | 160 | 24 | None |
| Bastion | Invasion | Line (heavy) | 400 | 30 | Takes 25% less damage |
| Tender | Invasion | Support | 120 | 0 | Repairs nearby invasion fleets. Never joins an attack |

### Defense Platform

A new building. It fights in every engagement at its colony and cannot move.

| Building | Hull per level | Damage per level | Upkeep | Cost | Max level / upgrade costs |
| --- | --- | --- | --- | --- | --- |
| Defense Platform | 200 | 15 | 0.5 Energy | 80 Industry + 10 Strategic | 3 / 120 Industry + 20 Strategic, 200 Industry + 40 Strategic |

A destroyed platform is disabled, not removed. It repairs to full over two minutes once no hostile fleet is in the system.

### Engagement Resolution

Engagements resolve on the strategic layer as a pure, seeded calculation. The future Battle scene can visualize the same result; it does not decide it.

1. A round runs every 2 seconds of simulation time.
2. Each side totals its damage and spreads it across enemy ships by target priority: line and carrier ships first, then screens, then scouts.
3. Screen protection and carrier strike craft apply, using the role behaviors above.
4. Damage fills a damage pool on each ship stack. Each time the pool reaches one ship's hull, that ship is destroyed.
5. A side withdraws when its remaining hull drops below its retreat threshold: 30% for the player by default, and per fleet for enemies. It leaves by the route it arrived on.
6. The engagement ends when one side is destroyed, has withdrawn, or has no ship that can deal damage.

Before an engagement starts, the player sees both sides' estimated strength and can order a retreat. The player cannot give orders during an engagement.

### Repair

- Player ships repair at any colony with a Shipyard at 5 hull per second per stack, as long as no hostile fleet is present.
- Invasion fleets repair only near a Tender (see 5C).
- Hostile-neighbor fleets repair in their own territory.

### Results

Every engagement publishes one result event with the location, both sides, losses, survivors, withdrawals, and duration. The engagement summary view reads it.

## 5B: Hostile Neighbors

### Territory

- The hostile faction owns the two or three reserved Outer-ring systems.
- The player cannot settle owned systems. Conquest is deferred.
- The faction does not build an economy. It has a home fleet and spawns new fleets from its schedule.

### Faction Schedule

Times are simulation minutes from the start of the timeline. Each range is rolled once per run, so it repeats in every timeline of that run.

| Time | Beat | Force | Target |
| --- | --- | --- | --- |
| 8–12 | First contact | 2 Raiders | The player colony or fleet nearest their territory |
| 15–20 | Raid | 4 Raiders + 1 Marauder | The nearest player colony |
| 25–30 | Raid | 5 Raiders + 2 Marauders | The highest-value colony within four jumps |
| 35–45 | Push | 8 Raiders + 4 Marauders, in two fleets | Two colonies. The second fleet heads for the home system |
| After the push | Repeating raids every 6–8 minutes | Scales with time | Weakest colony within reach |

The push is the intended Rewind 0 failure. A prepared player can hold it with platforms and a fleet of about 6 Destroyers and 3 Cruisers.

The player can buy time by destroying raid fleets. A fleet that withdraws heads home to repair and rejoins the next raid.

### Colony Consequences

A hostile fleet that holds a player colony with no defenders left begins a siege.

| Siege time | Consequence |
| --- | --- |
| Immediately | The colony stops producing. Its ship queue pauses |
| Every 60 seconds | The highest-level building loses one level. A level-1 building is disabled |
| 3 minutes | The colony is lost and becomes unclaimed. Outposts are lost immediately |

Breaking the siege restores production and repairs disabled buildings over two minutes. Lost levels stay lost.

Losing the home colony is reported as a terminal candidate (see 5E). Losing any other colony is not.

### Enemy Movement

- Enemy fleets move along routes with the existing travel rules.
- They choose a path with the fewest jumps, then the shortest travel time. Multi-hop paths are needed for AI even though player multi-hop orders stay deferred.
- An enemy fleet re-picks its target if the target is lost, or if a stronger player fleet blocks the path.

## 5C: Invasion Clock

### Invasion Plan

Generation rolls one invasion plan per run and stores it in the scenario:

- Primary wormhole node (generated) and the secondary wormhole node (generated but inactive in Phase 5).
- The rolled time for each beat.
- The rolled sabotage event from the pool (one entry for now).
- Wave compositions, chosen from authored waves with a small roll on counts.

The plan uses its own random stream, so it does not shift when content weights change.

### Schedule

| Time | Beat | Player sees |
| --- | --- | --- |
| 15–18 | Signals | A warning that names the wormhole's ring and sector, not the system |
| 20–24 | Scout | 3 Seekers come out of the primary wormhole and visit two or three player-explored systems, then return |
| 25–35 | Sabotage crisis | See 5D |
| 45–50 | Wave warning | Warning that a major wave is gathering. The wormhole system becomes known |
| 50–55 | Major wave 1 | Gathers at the wormhole, then advances |
| Later | Major wave 2 and main fleet | Deferred to a later content pass |

Rewind 0 usually ends against the hostile neighbors before major wave 1 arrives. Later rewinds reach it.

### Scout Behavior

- Seekers withdraw after one round of combat, so killing one takes preparation (enough damage to finish it in one round).
- A destroyed Seeker leaves intel: the invasion's ship classes go into the archive.
- Seekers that return home do nothing else. Their visit only marks which systems the invasion has seen.

### Major Wave 1

- **Composition:** 2 Bastions, 6 Lancers, 6 Seekers, and 1 Tender in a separate repair fleet.
- **Gather:** the wave waits at the wormhole for 3 minutes. It is too strong to defeat there.
- **Split:** it splits into two or three fleets.
- **Target:** each fleet scores reachable player colonies by value (output and buildings), weakness (defense strength), and distance, then picks the best.
- **Withdraw:** a fleet below 50% hull withdraws toward the Tender. It repairs 10 hull per second per stack while it shares a system with the Tender.
- **Tender:** the Tender stays one or two jumps behind the wave. Destroying it stops wave repairs for the rest of the timeline.

## 5D: Sabotage Crisis

Phase 5 ships one crisis from the pool so the whole crisis loop works end to end. More entries come in a content pass.

### Research Interference

- **Trigger:** the scheduled crisis time (25–35 minutes).
- **Target:** the colony with the highest Science output. This is strength-targeted sabotage in a small form.
- **Warning:** 2 minutes before it starts, "Unusual signals near <colony>."
- **Window:** 3 minutes after it starts.

| Response | Effect |
| --- | --- |
| Station any player fleet at the colony before the window closes | Crisis prevented |
| Pay 60 Industry for a security sweep | Lab output is halved for 2 minutes |
| No response | Labs at the colony are disabled for 4 minutes, and the highest-level lab loses one level |

- **Discovery:** after the crisis ends, the archive records its trigger time, its targeting rule, and the fleet response. The next timeline can position a fleet in advance.

## 5E: Outcome Reporting and Intel

### Terminal Candidates

The outcome evaluator watches committed events and reports terminal candidates. It never ends the timeline; Phase 6 does.

| Candidate | Condition |
| --- | --- |
| Defeat | The home colony is lost |
| Defeat | The player has no colonies and no fleets |
| Victory (later) | The main invasion fleet is destroyed or broken. Not reachable in Phase 5 |

The evaluator reports each condition at most once.

### Intel Recorded in the Archive

| Observation | When it is recorded |
| --- | --- |
| Hostile-neighbor territory | When one of its systems is explored |
| Hostile-neighbor beat times and forces | When the beat is observed |
| Wormhole location | At the wave warning, or when explored |
| Invasion ship classes | When a Seeker is destroyed, or any invasion ship is seen in an engagement |
| Wave composition | When the wave is seen at the wormhole or in an engagement |
| Tender's role | The first time a fleet withdraws to it |
| Crisis trigger, target rule, and responses | When the crisis ends |

Archive entries feed the warnings in later timelines. For example, a known wave shows its arrival time and composition instead of a vague signal.

## Visibility

- Enemy fleets are visible in explored systems that hold a player colony or fleet, and in systems one route away from them.
- A **Sensor Array** building (footprint 2, 0.5 Energy upkeep, 60 Industry) extends that range by one more route around its colony. It is the first radar step from [universe-and-travel.md](Design/gameplay/universe-and-travel.md).
- Warnings always fire, even when the source is out of sight. Visibility only affects how much detail they show.

## Content Assets

These follow the names in the engineering plan, using the project's `SO` suffix:

- `FactionDefinitionSO`: owner identity, ship classes, retreat threshold, and the hostile-neighbor schedule.
- `InvasionDefinitionSO`: beat list with time ranges, warning lead times, and wave references.
- `InvasionWaveDefinitionSO`: compositions, gather time, split count, and withdraw threshold.
- `SabotageDefinitionSO`: trigger, targeting rule, warning lead, window, responses, and consequences. Responses are embedded, not separate assets.
- `RunOutcomeRuleDefinitionSO`: the terminal conditions above.
- `ShipClassDefinitionSO` gains hull, damage, role behavior, and the owners allowed to build or field it.
- New buildings: Defense Platform and Sensor Array.

## Runtime Systems

All gameplay changes are committed by services, which then publish typed domain events. Nothing discovers changes by polling the simulation tick.

| System | Responsibility |
| --- | --- |
| `EngagementService` | Starts engagements when hostile owners share a system |
| `EngagementResolver` | Pure, seeded round-by-round resolution |
| `EngagementTickSystem` | Advances active engagements every round |
| `FleetRepairService` | Applies shipyard, territory, and Tender repairs |
| `ColonySiegeService` | Siege timers, building damage, and colony loss |
| `ThreatScheduleService` | Holds the run's rolled beats in a time-sorted queue. Fires each beat once when its time passes |
| `HostileFactionDirector` | Turns hostile-neighbor beats into fleet spawns and targets |
| `InvasionDirector` | Turns invasion beats into signals, the scout, and the wave |
| `EnemyTargetService` | Scores targets and re-targets |
| `EnemyMovementService` | Plans paths for enemy fleets and issues travel through the existing travel service |
| `CrisisService` | Warning, window, responses, and consequences for sabotage |
| `IntelArchiveService` | Writes observed intel into the run archive |
| `TimelineOutcomeEvaluator` | Reports terminal candidates |

The schedule only ever checks the earliest item in its queue, so it does not scan every beat each tick.

New domain events include engagement started, engagement resolved, fleet withdrew, colony besieged, colony lost, threat warning, enemy fleet spawned, crisis started, crisis resolved, intel recorded, and terminal candidate reported.

## Presentation

UI Toolkit views with presenters, following the existing overworld screens:

- **Threat panel:** known upcoming beats with time estimates and confidence.
- **Enemy fleet markers** on the map, using the visibility rules.
- **Pre-engagement prompt:** estimated strength on both sides, with fight or retreat.
- **Engagement summary:** losses, survivors, withdrawals.
- **Crisis panel:** the warning, the window timer, and the response options.
- **Colony siege state** on the settlement panel.

## Tests

- The same seed rolls the same threat schedule, invasion plan, and wave compositions.
- An engagement with the same inputs and seed gives the same result.
- Screens absorb damage aimed at other ships. Carriers bypass screens. Fleets withdraw at their threshold.
- Defense platforms fight and repair. Shipyards repair player ships only when no hostile fleet is present.
- A siege removes production immediately, lowers building levels on schedule, and loses the colony at 3 minutes. Breaking it restores production.
- Each beat fires once at its rolled time. Warnings come before their beats.
- Enemy fleets path along routes, re-target when blocked, and never pass a hostile fleet.
- Wave fleets withdraw to the Tender below 50% hull. Destroying the Tender stops repairs.
- Each crisis response applies its documented effect, and an ignored crisis applies the full consequence.
- Losing a non-home colony or a battle never reports a terminal candidate. Losing the home colony reports exactly one.
- Each observation writes its intel entry once.

## Deliverable

In a generated galaxy, the player meets the hostile neighbors, survives or loses raids, sees invasion signals and the scout, and can respond to the research sabotage. Rewind 0 usually ends at the neighbors' push. The archive then records when they push, how strong they are, and what the invasion looks like.

## Deferred

- Visual battles in the Battle scene. They will read engagement results rather than replace them.
- Shields, armor, weapon types, and debris.
- Conquering hostile-neighbor systems, diplomacy with them, and their economy.
- Secondary wormhole activity, major wave 2, the main fleet, and the boss capital ship.
- The rest of the sabotage pool, colony stability, and the alien-aligned underground.
- Frontier Pressure-driven raids.
- Voluntary rewind and the timeline-end flow (Phase 6).

## Open Questions

- Should engagements always resolve on the strategic layer, with the Battle scene as a viewer only? Or should important battles load the Battle scene and let its simulation decide?
- Is 3 minutes of siege the right window to respond from one or two jumps away?
- Should the hostile-neighbor push be tuned to beat an unprepared player, or any player, in Rewind 0?
- Should Seekers be hard to kill on purpose, so that getting scout intel is its own small goal?
- How much warning detail should a first timeline give compared with a timeline that has archive intel?
- Should the player be able to set their own retreat threshold per fleet?
