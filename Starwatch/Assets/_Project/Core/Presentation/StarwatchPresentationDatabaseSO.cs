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

        [field: SerializeField]
        public StarSystemPresentationSO StarSystem { get; private set; }

        /// <summary>
        /// Logs missing child assets once at project bootstrap. Views assume these are wired.
        /// </summary>
        public void ValidateRequired()
        {
            if (StarMap == null)
            {
                Debug.LogError(
                    "StarwatchPresentationDatabaseSO requires an assigned StarMapPresentationSO.",
                    this);
            }

            if (StarSystem == null)
            {
                Debug.LogError(
                    "StarwatchPresentationDatabaseSO requires an assigned StarSystemPresentationSO.",
                    this);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ValidateRequired();
        }
#endif
    }
}
