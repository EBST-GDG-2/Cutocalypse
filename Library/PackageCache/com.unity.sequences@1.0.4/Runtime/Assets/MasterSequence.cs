using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.Sequences
{
    /// <summary>
    /// The MasterSequence ScriptableObject serializes a hierarchy of Sequences and allows their creation and deletion.
    /// </summary>
    public class MasterSequence : ScriptableObject
    {
        [FormerlySerializedAs("m_RootCinematicIndex")][SerializeField] int m_RootIndex = -1;
        [SerializeReference] SequenceManager m_Manager;

        /// <summary>
        /// Event invoked when a Sequence is removed from a MasterSequence.
        /// </summary>
        public static event Action<MasterSequence, TimelineSequence> sequenceRemoved;

        /// <summary>
        /// Event invoked when a new Sequence is added to a MasterSequence.
        /// </summary>
        public static event Action<MasterSequence, TimelineSequence> sequenceAdded;

        internal int rootIndex
        {
            get => m_RootIndex;
            private set => m_RootIndex = value;
        }

        internal SequenceManager manager
        {
            get
            {
                // TODO (FTV-581): When doing this Jira, this could require some refactoring.
                if (m_Manager != null && m_Manager.count > 1)
                {
                    // The manager is not serialized in Sequence.
                    // This could also only be done once on deserialize and help make the code clearer and simpler.
                    EnsureUniformManager();
                }

                return m_Manager;
            }
            private set
            {
                m_Manager = value;
                EnsureUniformManager();
            }
        }

        /// <summary>
        /// The root Sequence of the structure defined in the MasterSequence.
        /// </summary>
        public TimelineSequence rootSequence
        {
            get => manager.GetAt(rootIndex) as TimelineSequence;
            private set => rootIndex = manager.GetIndex(value);
        }

        /// <summary>
        /// Creates a new MasterSequence instance.
        /// </summary>
        /// <param name="name">The name of the created MasterSequence.</param>
        /// <param name="fps">The framerate of the created MasterSequence.</param>
        /// <returns>The newly created MasterSequence.</returns>
        public static MasterSequence CreateInstance(string name, float fps = 24.0f)
        {
            var masterSequence = ScriptableObject.CreateInstance<MasterSequence>();
            masterSequence.name = name;
            masterSequence.manager = new SequenceManager();

            var sequence = TimelineSequence.CreateInstance(masterSequence.manager);
            sequence.name = name;
            sequence.fps = fps;
            sequence.childrenTrackName = "Sequences";

            masterSequence.rootSequence = sequence;

            return masterSequence;
        }

        /// <summary>
        /// Creates and adds a new Sequence to this MasterSequence.
        /// </summary>
        /// <param name="clipName">The name of the created Sequence.</param>
        /// <param name="parent">The parent is the root sequence by default if you don't specify a value for this parameter.</param>
        /// <returns>The newly created Sequence.</returns>
        /// <remarks>This function invokes the <see cref="sequenceAdded"/> event.</remarks>
        public TimelineSequence NewSequence(string clipName, TimelineSequence parent = null)
        {
            var sequence = TimelineSequence.CreateInstance(manager);
            sequence.name = clipName;
            sequence.childrenTrackName = "Sequences";

            if (parent == null)
                sequence.parent = rootSequence;
            else
                sequence.parent = parent;

            sequenceAdded?.Invoke(this, sequence);
            return sequence;
        }

        /// <summary>
        /// Remove the specified Sequence and all its children from this MasterSequence.
        /// </summary>
        /// <param name="sequence">The Sequence to remove.</param>
        /// <returns>Returns the list of sequences that was removed from this MasterSequence. That can be used for
        /// any post-processing like removing the associated assets from disk.</returns>
        /// <remarks>This function invokes the <see cref="sequenceRemoved"/> event.</remarks>
        public IEnumerable<TimelineSequence> RemoveSequence(TimelineSequence sequence)
        {
            sequence.parent = null;
            sequenceRemoved?.Invoke(this, sequence);

            m_Manager.Remove(sequence);
            yield return sequence;

            foreach (var childSequence in sequence.GetChildren())
            {
                foreach (var removedSequence in RemoveSequence(childSequence as TimelineSequence))
                {
                    yield return removedSequence;
                }
            }
        }

        void EnsureUniformManager()
        {
            if (m_RootIndex < 0)
                return;

            var rootSequence = m_Manager.GetAt(m_RootIndex);
            if (rootSequence == null || rootSequence.manager == m_Manager)
                return;

            rootSequence.manager = m_Manager;
            UniformizeManagerFrom(rootSequence);
        }

        void UniformizeManagerFrom(Sequence sequence)
        {
            foreach (var child in sequence.children)
            {
                child.manager = sequence.manager;
                UniformizeManagerFrom(child);
            }
        }
    }
}
