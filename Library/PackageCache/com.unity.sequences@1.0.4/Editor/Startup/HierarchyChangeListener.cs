using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    /// <summary>
    /// Class in charge of reflecting changes from the Hierarchy window
    /// to the data.
    /// </summary>
    [InitializeOnLoad]
    class HierarchyChangeListener
    {
        static HierarchyChangeListener()
        {
            EditorApplication.hierarchyChanged += UpdateMasterSequence;
        }

        static void UpdateMasterSequence()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var sequenceFilters = ObjectsCache.FindObjectsFromScenes<SequenceFilter>();
            foreach (var sequenceFilter in sequenceFilters)
            {
                if (sequenceFilter.masterSequence == null)
                    continue;

                // Process names
                var sequence = sequenceFilter.masterSequence.manager.GetAt(sequenceFilter.elementIndex) as TimelineSequence;
                if (sequence != null && sequenceFilter.gameObject.name != sequence.name)
                {
                    if (sequence == sequenceFilter.masterSequence.rootSequence)
                        sequenceFilter.masterSequence.Rename(sequenceFilter.gameObject.name);
                    else
                        sequence.Rename(sequenceFilter.gameObject.name);
                }
            }
        }
    }
}
