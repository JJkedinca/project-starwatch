using ProjectStarwatch.Core.Simulation;

namespace ProjectStarwatch.Core.Events
{
    /// <summary>
    /// A fact that already happened. Publishers send it after state is committed.
    /// The journal stores this value so replay can read every field.
    /// </summary>
    public interface IDomainEvent
    {
        GameEventType EventType { get; }

        GameTime Time { get; }
    }
}
