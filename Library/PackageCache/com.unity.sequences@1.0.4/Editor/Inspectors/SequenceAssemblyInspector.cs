using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Sequences;
using UnityEngine.Sequences.Timeline;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace UnityEditor.Sequences
{
    internal partial class SequenceAssemblyInspector : Editor
    {
        class Styles
        {
            public static readonly string k_UXMLFilePath = "Packages/com.unity.sequences/Editor/UI/SequenceAssemblyInspector.uxml";
        }

        protected VisualElement m_RootVisualElement;
        protected PlayableDirector m_Director;
        protected TimelineAsset m_Timeline;
        protected List<AssetCollectionList> m_AssetCollectionListsCache;

        void OnEnable()
        {
            Initialize();
            SequenceAssetIndexer.sequenceAssetImported += OnSequenceAssetImported;
            SequenceAssetIndexer.sequenceAssetUpdated += OnSequenceAssetUpdated;
            SequenceAssetIndexer.sequenceAssetDeleted += OnSequenceAssetDeleted;
        }

        protected void Initialize()
        {
            m_AssetCollectionListsCache = new List<AssetCollectionList>();

            if (target == null)
                return;

            m_Director = (target as PlayableDirector);
            m_Timeline = m_Director.playableAsset as TimelineAsset;
        }

        void OnDisable()
        {
            SequenceAssetIndexer.sequenceAssetImported -= OnSequenceAssetImported;
            SequenceAssetIndexer.sequenceAssetUpdated -= OnSequenceAssetUpdated;
            SequenceAssetIndexer.sequenceAssetDeleted -= OnSequenceAssetDeleted;
            ClearCollectionsCache();
        }

        protected void ClearCollectionsCache()
        {
            foreach (var assetCollectionList in m_AssetCollectionListsCache)
                assetCollectionList.Dispose();

            m_AssetCollectionListsCache.Clear();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Styles.k_UXMLFilePath);
            m_RootVisualElement = visualTree.CloneTree();
            StyleSheetUtility.SetStyleSheets(m_RootVisualElement, "SequenceAssemblyInspector");

            SetSequenceAssemblyData();

            return m_RootVisualElement;
        }

        void SetSequenceAssemblyData()
        {
            m_RootVisualElement.Bind(new SerializedObject(m_Director.gameObject));

            TextField editName = m_RootVisualElement.Q<TextField>("seq-sequence-name");
            editName.isDelayed = true;

            ObjectField setReference = m_RootVisualElement.Q<ObjectField>("seq-sequence-set");
            setReference.objectType = typeof(SceneAsset);

            var sequenceDirector = m_Timeline.FindDirector();
            if (sequenceDirector == null)
                return;

            var instances = SequenceAssetUtility.GetInstancesInSequence(sequenceDirector);
            SetAssetCollections(instances);
        }

        void SetAssetCollections(IEnumerable<GameObject> sequenceAssetSelections)
        {
            foreach (string collectionType in CollectionType.instance.types)
            {
                var sequenceAssetOfType = sequenceAssetSelections.Where(value => SequenceAssetUtility.GetType(value) == collectionType);
                SetAssetCollection(collectionType, sequenceAssetOfType);
            }
        }

        void OnSequenceAssetImported(GameObject sequenceAsset)
        {
            Refresh();
        }

        void OnSequenceAssetUpdated(GameObject sequenceAsset)
        {
            Refresh();
        }

        void OnSequenceAssetDeleted()
        {
            Refresh();
        }

        protected virtual void SetAssetCollection(string type, IEnumerable<GameObject> sequenceAssetSelections)
        {
            AssetCollectionList newAssetCollectionList = new AssetCollectionList(m_Director, type);
            AddCollectionList(sequenceAssetSelections, newAssetCollectionList);
        }

        protected void AddCollectionList(IEnumerable<GameObject> sequenceAssetSelections, AssetCollectionList newAssetCollectionList)
        {
            AddSequenceAssetToCollection(sequenceAssetSelections, newAssetCollectionList);
            VisualElement assetCollectionsRoot = m_RootVisualElement.Q<VisualElement>("seq-asset-collections");
            assetCollectionsRoot.Add(newAssetCollectionList);
            m_AssetCollectionListsCache.Add(newAssetCollectionList);
        }

        internal void Refresh()
        {
            if (m_Director == null || (m_Director.playableAsset as TimelineAsset) == null)
                return;

            VisualElement assetCollectionsRoot = m_RootVisualElement.Q<VisualElement>("seq-asset-collections");
            foreach (var child in assetCollectionsRoot.Children())
            {
                var assetCollectionList = child as AssetCollectionList;
                if (assetCollectionList == null)
                    continue;

                assetCollectionList.RemoveAllItems();

                var instances = SequenceAssetUtility.GetInstancesInSequence(m_Director, assetCollectionList.collectionType);
                AddSequenceAssetToCollection(instances, assetCollectionList);
            }
        }

        protected virtual void AddSequenceAssetToCollection(IEnumerable<GameObject> sequenceAssetSelections, AssetCollectionList assetCollectionList)
        {
            foreach (var selection in sequenceAssetSelections)
                assetCollectionList.AddSequenceAssetSelection<SequenceAssetFoldoutItem>(selection);
        }

        internal void SelectPlayableDirector()
        {
            if (m_Director != null)
                Selection.activeGameObject = m_Director.gameObject;
        }
    }
}
