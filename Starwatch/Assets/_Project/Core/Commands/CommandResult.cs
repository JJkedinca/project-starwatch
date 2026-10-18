namespace ProjectStarwatch.Core.Commands
{
    /// <summary>
    /// Success/failure result for a player command with no payload.
    /// </summary>
    public readonly struct CommandResult
    {
        public bool Success { get; }

        public CommandError Error { get; }

        private CommandResult(bool success, CommandError error)
        {
            Success = success;
            Error = error;
        }

        /// <summary>
        /// Creates a successful command result.
        /// </summary>
        public static CommandResult Ok()
        {
            return new CommandResult(true, new CommandError(CommandErrorCode.None, string.Empty));
        }

        /// <summary>
        /// Creates a failed command result with a stable error code.
        /// </summary>
        public static CommandResult Fail(CommandErrorCode code, string message)
        {
            return new CommandResult(false, new CommandError(code, message));
        }
    }

    /// <summary>
    /// Success/failure result for a player command that returns a value.
    /// </summary>
    public readonly struct CommandResult<T>
    {
        public bool Success { get; }

        public T Value { get; }

        public CommandError Error { get; }

        private CommandResult(bool success, T value, CommandError error)
        {
            Success = success;
            Value = value;
            Error = error;
        }

        /// <summary>
        /// Creates a successful command result carrying a value.
        /// </summary>
        public static CommandResult<T> Ok(T value)
        {
            return new CommandResult<T>(true, value, new CommandError(CommandErrorCode.None, string.Empty));
        }

        /// <summary>
        /// Creates a failed command result with a stable error code.
        /// </summary>
        public static CommandResult<T> Fail(CommandErrorCode code, string message)
        {
            return new CommandResult<T>(false, default, new CommandError(code, message));
        }
    }
}
