using System;
using System.IO;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Sequences;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace UnityEditor.Sequences
{
    /// <summary>
    /// Utility functions to manipulate MasterSequence assets and Sequence objects.
    /// </summary>
    public static class SequenceUtility
    {
        /// <summary>
        /// Mask to define the validity state of a Sequence.
        /// One single Sequence can be invalid for four reasons: its GameObject is missing in the Hierarchy view, its
        /// Timeline asset was deleted, its parent Sequence is invalid (Orphan), its MasterSequence asset itself is
        /// missing.
        /// </summary>
        [Flags]
        internal enum SequenceValidity
        {
            Valid = 0,
            MissingMasterSequence = 1,
            MissingGameObject = 2,
            MissingTimeline = 4,
            Orphan = 8
        }

        internal static readonly string k_DefaultMasterSequenceName = "New Master Sequence";
        internal static readonly string k_DefaultSequenceName = "New Sequence";

        /// <summary>
        /// Each TimelineSequence creation on disk triggers this event.
        /// </summary>
        public static event Action<TimelineSequence, MasterSequence> sequenceCreated;

        /// <summary>
        /// Each TimelineSequence deletion from disk triggers this event.
        /// </summary>
        public static event Action sequenceDeleted;

        /// <summary>
        /// Creates a new MasterSequence and saves it on disk.
        /// </summary>
        /// <param name="name">The created MasterSequence name.</param>
        /// <param name="fps">The created MasterSequence frame rate. If you don't specify a framerate, the
        /// MasterSequence uses by default the Timeline framerate value from the Project Settings.</param>
        /// <returns>The newly created MasterSequence asset.</returns>
        public static MasterSequence CreateMasterSequence(string name, float fps = -1.0f)
        {
            Undo.SetCurrentGroupName("Create Master Sequence");
            var groupIndex = Undo.GetCurrentGroup();

            fps = fps < 0.0 ? (float)TimelineUtility.GetProjectFrameRate() : fps;
            var masterSequence = MasterSequence.CreateInstance(name, fps);
            masterSequence.Save();

            sequenceCreated?.Invoke(masterSequence.rootSequence, masterSequence);

            Undo.CollapseUndoOperations(groupIndex);

            return masterSequence;
        }

        /// <summary>
        /// Creates a new Sequence in the specified MasterSequence asset. Also saves the Sequence TimelineAsset and the
        /// updated MasterSequence on disk.
        /// </summary>
        /// <param name="name">The name of the created Sequence.</param>
        /// <param name="masterSequence">The MasterSequence asset to add the created Sequence to.</param>
        /// <param name="parent">An optional parent Sequence for the created one.</param>
        /// <returns>The newly created TimelineSequence.</returns>
        public static TimelineSequence CreateSequence(string name, MasterSequence masterSequence, TimelineSequence parent = null)
        {
            Undo.SetCurrentGroupName("Create Sequence");
            var groupIndex = Undo.GetCurrentGroup();

            var sequence = masterSequence.NewSequence(name, parent);

            masterSequence.Save(); // Save the updated SequenceManager structure.
            sequence.Save(); // Save the sequence TimelineAsset on disk.
            masterSequence.Save(); // Save the MasterSequence asset with the new sequence TimelineAsset correctly bind.

            sequenceCreated?.Invoke(sequence, masterSequence);

            Undo.CollapseUndoOperations(groupIndex);

            return sequence;
        }

        /// <summary>
        /// Removes the specified Sequence and all its sub-Sequences from the specified MasterSequence asset. This also
        /// removes from disk each corresponding Sequence TimelineAsset, and saves the updated MasterSequence asset.
        /// </summary>
        /// <param name="sequence">The Sequence to delete.</param>
        /// <param name="masterSequence">The MasterSequence to remove the Sequence from.</param>
        public static void DeleteSequence(TimelineSequence sequence, MasterSequence masterSequence)
        {
            Undo.SetCurrentGroupName("Delete Sequence");
            var groupIndex = Undo.GetCurrentGroup();

            if (TimelineEditor.selectedClip == sequence.editorialClip)
                TimelineEditor.selectedClip = null;

            var sequenceFolderPath = SequencesAssetDatabase.GetSequenceFolder(sequence);
            foreach (var removedSequence in masterSequence.RemoveSequence(sequence))
            {
                removedSequence.Delete();
            }

            if (!string.IsNullOrEmpty(sequenceFolderPath) && SequencesAssetDatabase.IsEmpty(sequenceFolderPath, true))
                AssetDatabase.DeleteAsset(sequenceFolderPath);

            masterSequence.Save();
            sequenceDeleted?.Invoke();

            Undo.CollapseUndoOperations(groupIndex);
        }

        /// <summary>
        /// Gets the Sequence associated to the specified TimelineAsset.
        /// </summary>
        /// <param name="timeline">The TimelineAsset the Sequence to search is associated to.</param>
        /// <returns>The TimelineSequence associated with the given TimelineAsset.</returns>
        internal static TimelineSequence GetSequenceFromTimeline(TimelineAsset timeline)
        {
            var masterSequences = SequencesAssetDatabase.FindAsset<MasterSequence>();
            foreach (var masterSequence in masterSequences)
            {
                foreach (var sequence in masterSequence.manager.sequences)
                {
                    var timelineSequence = sequence as TimelineSequence;
                    if (!TimelineSequence.IsNullOrEmpty(timelineSequence) && timelineSequence.timeline == timeline)
                        return timelineSequence;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the specified Sequence is not null and has a valid TimelineAsset associated to it. Also checks the
        /// validity of all the parent Sequences.
        /// </summary>
        /// <param name="sequence">The Sequence to check.</param>
        /// <returns>true if the specified Sequence and all its parent Sequences are valid. Otherwise, false.</returns>
        internal static bool IsValidSequence(TimelineSequence sequence)
        {
            var isValid = !TimelineSequence.IsNullOrEmpty(sequence);
            if (!isValid)
                return false;

            var parentSequence = sequence.parent as TimelineSequence;
            while (isValid && parentSequence != null)
            {
                isValid = !TimelineSequence.IsNullOrEmpty(parentSequence);
                parentSequence = parentSequence.parent as TimelineSequence;
            }

            return isValid;
        }

        /// <summary>
        /// Gets the validity of the specified Sequence. A Sequence is valid if its GameObject is present in the scene,
        /// if its Timeline asset exists and if it's parent Sequence is valid as well.
        /// </summary>
        /// <param name="sequence">The Sequence to get validity from.</param>
        /// <param name="masterSequence">The MasterSequence of the specified sequence. This is used to check if the
        /// whole MasterSequence is loaded in a Scene or not. If it's not, there is no need to check if the Sequence
        /// GameObject exists.</param>
        /// <returns>A <see cref="SequenceValidity"/> mask.</returns>
        internal static SequenceValidity GetSequenceValidity(TimelineSequence sequence, MasterSequence masterSequence)
        {
            if (masterSequence == null)
                return SequenceValidity.MissingMasterSequence;

            SequenceValidity result = SequenceValidity.Valid;

            var parentSequence = sequence.parent as TimelineSequence;
            if (parentSequence != null && !IsValidSequence(parentSequence))
                result |= SequenceValidity.Orphan;

            if (TimelineSequence.IsNullOrEmpty(sequence))
                result |= SequenceValidity.MissingTimeline;

            if (!IsMasterSequenceInScene(masterSequence))
                return result;

            var gameObject = GetSequenceGameObject(sequence);
            if (gameObject == null)
                result |= SequenceValidity.MissingGameObject;

            return result;
        }

        /// <summary>
        /// Gets the GameObject in the current loaded Scenes that corresponds to the specified Sequence.
        /// </summary>
        /// <param name="sequence">The TimelineSequence to look the GameObject for.</param>
        /// <returns>The GameObject that corresponds to the specified Sequence.</returns>
        internal static GameObject GetSequenceGameObject(TimelineSequence sequence)
        {
            var sequenceFilters = ObjectsCache.FindObjectsFromScenes<SequenceFilter>();
            foreach (var sequenceFilter in sequenceFilters)
            {
                // Might happen when instantiating a Sequence prefab under a MasterSequence GameObject.
                if (sequenceFilter.masterSequence == null)
                    continue;

                if (sequenceFilter.masterSequence.manager.GetAt(sequenceFilter.elementIndex) == sequence)
                {
                    return sequenceFilter.gameObject;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the specified MasterSequence asset is loaded in the Hierarchy view.
        /// </summary>
        /// <param name="masterSequence">The MasterSequence asset to look for.</param>
        /// <returns>True if the MasterSequence is loaded in the Hierarchy view. False otherwise.</returns>
        static bool IsMasterSequenceInScene(MasterSequence masterSequence)
        {
            var sequenceFilters = ObjectsCache.FindObjectsFromScenes<SequenceFilter>();
            foreach (var sequenceFilter in sequenceFilters)
            {
                if (sequenceFilter.masterSequence == masterSequence)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets an unique hashcode from the provided sequence.
        /// It gets it by building the sequence path. Renaming a Sequence or the MasterSequence will yield a new hashcode.
        /// </summary>
        /// <param name="sequence">The Sequence the returned hashcode belongs to.</param>
        /// <param name="masterSequence">The MasterSequence the <paramref name="sequence"/> belongs to.</param>
        /// <returns>An unique integer</returns>
        internal static int GetHashCode(TimelineSequence sequence, MasterSequence masterSequence)
        {
            string path = string.Empty;

            TimelineSequence current = sequence;
            while (current != null)
            {
                if (current.parent == null)
                    path = Path.Combine(AssetDatabase.GetAssetPath(masterSequence), path);
                else
                    path = Path.Combine(
                        masterSequence.manager.GetIndex(sequence).ToString(),
                        "_",
                        current.name,
                        path);

                current = current.parent as TimelineSequence;
            }
            return path.GetHashCode();
        }
    }
}
