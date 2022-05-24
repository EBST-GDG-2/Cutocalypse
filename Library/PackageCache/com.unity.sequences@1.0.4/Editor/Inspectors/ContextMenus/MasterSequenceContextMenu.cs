using UnityEngine;
using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    internal sealed class MasterSequenceContextMenu : SequencesWindowContextMenu<MasterSequenceContextMenu, MasterSequenceTreeViewItem>
    {
        GenericMenu m_Menu;
        internal bool isItemValid;

        public override void Show(MasterSequenceTreeViewItem targetItem)
        {
            SetTarget(targetItem);
            m_Menu = new GenericMenu();
            PopulateMenu();
            m_Menu.ShowAsContext();
        }

        void PopulateMenu()
        {
            if (isItemValid)
                PopulateMenuForValidItem();
            else
                PopulateMenuForInvalidItem();
        }

        void PopulateMenuForValidItem()
        {
            var context = new SceneManagementMenu.ContextInfo();
            context.masterSequence = target.masterSequence;
            context.sequence = target.timelineSequence;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                m_Menu.AddDisabledItem(new GUIContent("Create Sequence"), false);
                m_Menu.AddDisabledItem(new GUIContent("Delete"), false);
            }
            else
            {
                m_Menu.AddItem(new GUIContent("Create Sequence"), false, CreateSequenceAction);
                m_Menu.AddItem(new GUIContent("Delete"), false, DeleteAction);
            }

            m_Menu.AddSeparator("");
            SceneManagementMenu.AppendMenuFrom(context, m_Menu);

            m_Menu.AddSeparator("");
            if (IsTargetInScene() && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                m_Menu.AddItem(new GUIContent("Record"), false, RecordAction);
                m_Menu.AddItem(new GUIContent("Record As..."), false, RecordAsAction);
            }
            else
            {
                m_Menu.AddDisabledItem(new GUIContent("Record"));
                m_Menu.AddDisabledItem(new GUIContent("Record As..."));
            }
        }

        void PopulateMenuForInvalidItem()
        {
            m_Menu.AddItem(new GUIContent("Delete"), false, DeleteAction);
        }

        void CreateSequenceAction()
        {
            (target.owner as StructureTreeView).CreateNewSequenceInContext(target);
            ResetTarget();
        }

        void DeleteAction()
        {
            target.Delete();
            ResetTarget();
        }

        void RecordAction()
        {
            target.masterSequence.rootSequence.Record();
            ResetTarget();
        }

        void RecordAsAction()
        {
            target.masterSequence.rootSequence.Record(true);
            ResetTarget();
        }

        bool IsTargetInScene()
        {
            var sequenceFilters = ObjectsCache.FindObjectsFromScenes<SequenceFilter>();
            foreach (var sequenceFilter in sequenceFilters)
            {
                if (sequenceFilter.masterSequence == target.masterSequence)
                    return true;
            }

            return false;
        }
    }
}
