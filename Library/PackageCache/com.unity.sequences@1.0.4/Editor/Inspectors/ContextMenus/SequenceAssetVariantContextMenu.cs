using UnityEngine;

namespace UnityEditor.Sequences
{
    internal sealed class SequenceAssetVariantContextMenu : SequencesWindowContextMenu<SequenceAssetVariantContextMenu, SequenceAssetVariantTreeViewItem>
    {
        GenericMenu m_Menu;

        public SequenceAssetVariantContextMenu()
        {
            m_Menu = new GenericMenu();
            m_Menu.AddItem(new GUIContent("Duplicate"), false, DuplicateAction);
            m_Menu.AddItem(new GUIContent("Delete"), false, DeleteAction);
        }

        public override void Show(SequenceAssetVariantTreeViewItem target)
        {
            SetTarget(target);
            m_Menu.ShowAsContext();
        }

        void DuplicateAction()
        {
            SequenceAssetUtility.DuplicateVariant(target.asset);
            ResetTarget();
        }

        void DeleteAction()
        {
            target.Delete();
            ResetTarget();
        }
    }
}
