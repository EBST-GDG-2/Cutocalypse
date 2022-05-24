using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Sequences;
using UObject = UnityEngine.Object;

namespace UnityEditor.Sequences
{
    /// <summary>
    /// Interface to help retrieve cached objects.
    /// Caches are invalidated at every Editor tick.
    /// </summary>
    static class ObjectsCache
    {
        /// <summary>
        /// Interface to implement for any caches that would be compatible with <see cref="ObjectsCacheSystem"/>
        /// </summary>
        interface ICache
        {
            IReadOnlyCollection<UObject> references { get; }

            bool shouldRebuild { get; }

            void InvalidateData();

            void Build();

            void RemoveInvalidReferences();
        }

        /// <summary>
        /// Generic implementation of <see cref="ICache"/>.
        /// It holds references living in loaded scenes.
        /// </summary>
        /// <typeparam name="T">Type derived from <see cref="UnityEngine.Object"/>.</typeparam>
        class SceneObjectsCache<T> : ICache where T : UObject
        {
            List<T> m_References = new List<T>();

            bool ICache.shouldRebuild => m_References.Count == 0;

            IReadOnlyCollection<UObject> ICache.references => m_References;

            void ICache.InvalidateData()
            {
                m_References.Clear();
            }

            void ICache.Build()
            {
                m_References.AddRange(UObject.FindObjectsOfType<T>(true));
            }

            void ICache.RemoveInvalidReferences()
            {
                m_References.RemoveAll(obj => obj == null);
            }
        }

        /// <summary>
        /// System holding the various caches.
        /// </summary>
        class ObjectsCacheSystem
        {
            Dictionary<Type, ICache> m_Caches = new Dictionary<Type, ICache>();

            internal IReadOnlyCollection<T> GetObjects<T>() where T : UObject
            {
                Type type = typeof(T);

                // Create cache for the given type if it does not exist.
                if (!m_Caches.ContainsKey(type))
                    m_Caches.Add(type, new SceneObjectsCache<T>());

                ICache cache = m_Caches[type];

                // Rebuild cache if it has been invalidated or is empty.
                if (cache.shouldRebuild)
                    cache.Build();
                else
                    cache.RemoveInvalidReferences();


                return cache.references as IReadOnlyCollection<T>;
            }

            internal void InvalidateCaches()
            {
                foreach (KeyValuePair<Type, ICache> cache in m_Caches)
                    cache.Value.InvalidateData();
            }
        }

        static ObjectsCacheSystem m_Internal;

        /// <summary>
        /// Initialize the internal cache system when the Editor loads.
        /// </summary>
        [InitializeOnLoadMethod]
        static void InitializeReferencesCache()
        {
            m_Internal = new ObjectsCacheSystem();

            EditorApplication.update += m_Internal.InvalidateCaches;

            // Listen to the following events in case data manipulation is happening from script in the same editor tick.
            SequenceUtility.sequenceCreated += OnSequenceCreated;
            SequenceUtility.sequenceDeleted += OnSequenceDeleted;
            SequenceAssetUtility.sequenceAssetAssignedTo += SequenceAssetUtility_sequenceAssetAssignedTo;
            SequenceAssetUtility.sequenceAssetRemovedFrom += SequenceAssetUtility_sequenceAssetRemovedFrom;
            EditorSceneManager.sceneLoaded += EditorSceneManager_sceneLoaded;
            EditorSceneManager.sceneUnloaded += EditorSceneManager_sceneUnloaded;
            EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
        }

        /// <summary>
        /// Returns a collection of references from matching Objects from loaded scenes.
        /// </summary>
        /// <typeparam name="T">Type of the requested objects. Must derived from <see cref="UnityEngine.Object" /></typeparam>
        /// <returns>A read only collection of objects found.</returns>
        internal static IReadOnlyCollection<T> FindObjectsFromScenes<T>() where T : UObject
        {
            return m_Internal.GetObjects<T>();
        }

        static void OnSequenceCreated(TimelineSequence sequence, MasterSequence masterSequence)
        {
            m_Internal.InvalidateCaches();
        }

        static void OnSequenceDeleted()
        {
            m_Internal.InvalidateCaches();
        }

        static void SequenceAssetUtility_sequenceAssetRemovedFrom(PlayableDirector obj)
        {
            m_Internal.InvalidateCaches();
        }

        static void SequenceAssetUtility_sequenceAssetAssignedTo(
            GameObject prefab,
            GameObject instance,
            PlayableDirector sequenceDirector)
        {
            m_Internal.InvalidateCaches();
        }

        static void EditorSceneManager_sceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            m_Internal.InvalidateCaches();
        }

        static void EditorSceneManager_sceneUnloaded(Scene scene)
        {
            m_Internal.InvalidateCaches();
        }

        static void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
        {
            m_Internal.InvalidateCaches();
        }
    }
}
