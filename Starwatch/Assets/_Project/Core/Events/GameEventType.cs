namespace ProjectStarwatch.Core.Events
{
    /// <summary>
    /// Closed set of domain events recorded in the journal and published across systems.
    /// </summary>
    public enum GameEventType
    {
        None = 0,
        NodeExplored = 1,
        PlanetScanned = 2,
        MapKnowledgeChanged = 3,
        SettlementFounded = 4,
        ChallengeRevealed = 5,
        ResourceChanged = 6,
        ResearchCompleted = 7,
        FleetArrived = 8,
        BattleStarted = 9,
        BattleEnded = 10,
        CrisisStarted = 11,
        CrisisResolved = 12,
        TimelineEnded = 13,
        RewindCompleted = 14,
        ShipConstructionCompleted = 15,
        FactionContacted = 16,
        FleetJoinedEngagement = 17,
        CombatIntelRecorded = 18,
        TimelineOutcomeChanged = 19,
        FleetTravelStarted = 20,
        BuildingChanged = 21,
        SettlementAbandoned = 22,
        ResearchStarted = 23,
        ShipQueued = 24,
        FleetCreated = 25,
        FleetMerged = 26,
        FleetSplit = 27,
    }
}
