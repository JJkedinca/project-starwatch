namespace ProjectStarwatch.Core.Commands
{
    /// <summary>
    /// Stable machine-readable reasons a player command was rejected.
    /// </summary>
    public enum CommandErrorCode
    {
        None = 0,
        InvalidArgument = 1,
        NotAllowed = 2,
        InsufficientResources = 3,
        InvalidRoute = 4,
        PrerequisiteNotMet = 5,
    }
}
