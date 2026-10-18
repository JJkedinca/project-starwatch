namespace ProjectStarwatch.Core.Commands
{
    /// <summary>
    /// Structured rejection for a player command.
    /// </summary>
    public readonly struct CommandError
    {
        public CommandErrorCode Code { get; }

        public string Message { get; }

        public CommandError(CommandErrorCode code, string message)
        {
            Code = code;
            Message = message ?? string.Empty;
        }
    }
}
