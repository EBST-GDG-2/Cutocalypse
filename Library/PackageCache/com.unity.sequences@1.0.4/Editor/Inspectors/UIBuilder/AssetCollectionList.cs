using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.Sequences.Timeline;

namespace UnityEditor.Sequences
{
    class AssetCollectionList : VisualElement
    {
        static readonly string k_UXMLFilePath = "Packages/com.unity.sequences/Editor/UI/AssetCollectionList.uxml";

        VisualElement m_RootVisualElement;
        SelectableScrollView m_ContentContainer;

        SequenceAssetAddMenu m_AddMenu;
        PlayableDirector m_Director;
        TimelineAsset m_Timeline;
        string m_CollectionType;

        List<GameObject> m_Items;

        internal string collectionType => m_CollectionType;

        public AssetCollectionList(PlayableDirector director, string type, bool playMode = false)
        {
            m_Director = director;
            m_Timeline = m_Director.playableAsset as TimelineAsset;
            m_CollectionType = type;
            m_Items = new List<GameObject>();

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UXMLFilePath);
            m_RootVisualElement = visualTree.CloneTree();

            // Set style
            StyleSheetUtility.SetStyleSheets(m_RootVisualElement, "AssetCollectionList");

            m_ContentContainer = m_RootVisualElement.Q<SelectableScrollView>("seq-sequence-asset-list");

            Label label = m_RootVisualElement.Q<Label>("seq-asset-collection");
            label.text = type;

            if (playMode)
            {
                var footer = m_RootVisualElement.Q<VisualElement>("seq-list-footer");
                footer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            else
            {
                Button button = m_RootVisualElement.Q<Button>("seq-sequence-asset-add");
                button.clicked += OpenAddMenu;

                Button removeButton = m_RootVisualElement.Q<Button>("seq-sequence-asset-remove");
                removeButton.clicked += RemoveSelectedSequenceAsset;

                m_AddMenu = new SequenceAssetAddMenu();
                m_AddMenu.Populate(type);
                m_AddMenu.userClickedOnCreateSequenceAsset += CreateSequenceAssetAndInstantiate;
                m_AddMenu.userClickedOnAddSequenceAsset += InstantiateSequenceAsset;
            }

            Add(m_RootVisualElement);
        }

        public void Dispose()
        {
            for (int i = 0; i < m_ContentContainer.childCount; ++i)
                (m_ContentContainer[i] as SelectableScrollViewItem).Dispose();

            RemoveAllItems();
        }

        public void RemoveAllItems()
        {
            m_Items.Clear();
            m_ContentContainer.Refresh();
        }

        public void AddSequenceAssetSelection<T>(GameObject selection) where T : SelectableScrollViewItem, new()
        {
            m_Items.Add(selection);
            var foldout = new T();
            foldout.BindItem(m_Director, m_Items[m_Items.Count - 1]);
            m_ContentContainer.AddItem(foldout);
        }

        void RemoveSelectedSequenceAsset()
        {
            if (m_ContentContainer.selectedIndex < 0)
                return;

            GameObject selection = m_Items[m_ContentContainer.selectedIndex];
            SequenceAssetUtility.RemoveFromSequence(selection, m_Timeline.FindDirector());
        }

        void CreateSequenceAssetAndInstantiate(string type)
        {
            var builder = SequenceAssetBuilder.GetBuilder(typeof(GameObject));

            var prefab = builder.CreateSequenceAsset(type);
            SequenceAssetUtility.InstantiateInSequence(prefab, m_Timeline.FindDirector());
        }

        void InstantiateSequenceAsset(GameObject prefab)
        {
            SequenceAssetUtility.InstantiateInSequence(prefab, m_Timeline.FindDirector());
        }

        void OpenAddMenu()
        {
            m_AddMenu.Show();
        }
    }
}
