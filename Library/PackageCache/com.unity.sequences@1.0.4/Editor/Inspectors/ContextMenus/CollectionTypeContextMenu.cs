using UnityEngine;

namespace UnityEditor.Sequences
{
    internal class CollectionTypeContextMenu : SequencesWindowContextMenu<CollectionTypeContextMenu, CollectionTypeTreeViewItem>
    {
        GenericMenu m_Menu;

        public CollectionTypeContextMenu()
        {
            m_Menu = new GenericMenu();
            m_Menu.AddItem(new GUIContent("Create Sequence Asset"), false, CreateSequenceAsset);
        }

        public override void Show(CollectionTypeTreeViewItem newTarget)
        {
            SetTarget(newTarget);
            m_Menu.ShowAsContext();
        }

        void CreateSequenceAsset()
        {
            (target.owner as AssetCollectionsTreeView).CreateSequenceAssetInContext(target.collectionType);
            ResetTarget();
        }
    }
}
