using UnityEngine;

namespace UnityEditor.Sequences
{
    /// <summary>
    /// The Sequence Asset context menu in the Asset Collections panel of the Sequences window.
    /// </summary>
    internal sealed class SequenceAssetContextMenu : SequencesWindowContextMenu<SequenceAssetContextMenu, SequenceAssetTreeViewItem>
    {
        GenericMenu m_Menu;

        public SequenceAssetContextMenu()
        {
            m_Menu = new GenericMenu();
            m_Menu.AddItem(new GUIContent("Create Variant"), false, CreateVariant);
            m_Menu.AddItem(new GUIContent("Delete"), false, Delete);
        }

        public override void Show(SequenceAssetTreeViewItem target)
        {
            SetTarget(target);
            m_Menu.ShowAsContext();
        }

        void CreateVariant()
        {
            (target.owner as AssetCollectionsTreeView).CreateSequenceAssetVariantInContext(target.asset);
            ResetTarget();
        }

        void Delete()
        {
            target.Delete();
            ResetTarget();
        }
    }
}
