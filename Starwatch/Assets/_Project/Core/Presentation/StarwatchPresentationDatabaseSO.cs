using ProjectStarwatch.Overworld.Map;
using UnityEngine;

namespace ProjectStarwatch.Core.Presentation
{
    /// <summary>
    /// Root aggregator for authored presentation style assets (sibling to ObjectDatabaseSO).
    /// </summary>
    [CreateAssetMenu(fileName = "StarwatchPresentationDatabase", menuName = "Starwatch/Presentation Database")]
    public sealed class StarwatchPresentationDatabaseSO : ScriptableObject
    {
        [field: SerializeField]
        public StarMapPresentationSO StarMap { get; private set; }
    }
}
