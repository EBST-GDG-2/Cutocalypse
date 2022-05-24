using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityEditor.Sequences
{
    internal sealed class SubSequenceContextMenu : SequencesWindowContextMenu<SubSequenceContextMenu, SubSequenceTreeViewItem>
    {
        GenericMenu m_Menu;
        internal bool isItemValid;

        public override void Show(SubSequenceTreeViewItem target)
        {
            SetTarget(target);

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

            if (IsTargetInScene() && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                m_Menu.AddDisabledItem(new GUIContent("Delete"), false);
            }
            else
            {
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

        void DeleteAction()
        {
            target.Delete();
            ResetTarget();
        }

        void RecordAction()
        {
            target.timelineSequence.Record();
            ResetTarget();
        }

        void RecordAsAction()
        {
            target.timelineSequence.Record(true);
            ResetTarget();
        }

        bool IsTargetInScene()
        {
            var playableDirectors = ObjectsCache.FindObjectsFromScenes<PlayableDirector>();
            foreach (var playableDirector in playableDirectors)
            {
                if (playableDirector.playableAsset == target.timelineSequence.timeline)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
