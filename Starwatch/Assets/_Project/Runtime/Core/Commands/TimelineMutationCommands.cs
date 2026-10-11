using EndlessHeros.Core.Identifiers;
using EndlessHeros.Core.Run;

namespace EndlessHeros.Core.Commands
{
    /// <summary>
    /// Tiny command helpers used to prove CommandResult and timeline isolation in Phase 0.
    /// Feature phases replace these with real services.
    /// </summary>
    public static class TimelineMutationCommands
    {
        /// <summary>
        /// Adds a placeholder fleet to timeline-owned state only.
        /// Location comes from the caller (typically scenario home), not a timeline copy.
        /// </summary>
        public static CommandResult<EntityId> AddPlaceholderFleet(
            TimelineState timeline,
            ContentId classId,
            int shipCount,
            ContentId locationSystemId)
        {
            if (timeline == null)
            {
                return CommandResult<EntityId>.Fail(
                    CommandErrorCode.InvalidArgument,
                    "Timeline is required.");
            }

            if (classId.IsEmpty)
            {
                return CommandResult<EntityId>.Fail(
                    CommandErrorCode.InvalidArgument,
                    "Fleet class id is required.");
            }

            if (locationSystemId.IsEmpty)
            {
                return CommandResult<EntityId>.Fail(
                    CommandErrorCode.InvalidArgument,
                    "Location system id is required.");
            }

            if (shipCount <= 0)
            {
                return CommandResult<EntityId>.Fail(
                    CommandErrorCode.NotAllowed,
                    "Ship count must be positive.");
            }

            if (!timeline.TryAddFleet(classId, shipCount, locationSystemId, out var id))
            {
                return CommandResult<EntityId>.Fail(
                    CommandErrorCode.NotAllowed,
                    "Fleet could not be added.");
            }

            return CommandResult<EntityId>.Ok(id);
        }

        /// <summary>
        /// Records an explored node on the archive only.
        /// </summary>
        public static CommandResult RecordExploredNode(RunArchive archive, ContentId nodeId)
        {
            if (archive == null)
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Archive is required.");
            }

            if (!archive.TryAddExploredNode(nodeId))
            {
                return CommandResult.Fail(CommandErrorCode.InvalidArgument, "Node id is required.");
            }

            return CommandResult.Ok();
        }
    }
}
