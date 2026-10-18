using ProjectStarwatch.Core.Identifiers;

namespace ProjectStarwatch.Core.Data
{
    /// <summary>
    /// Read check for completed research ids. Keeps a definition free of the archive type's mutators.
    /// </summary>
    public interface IResearchKnowledgeQuery
    {
        bool Contains(ContentId researchId);
    }
}
