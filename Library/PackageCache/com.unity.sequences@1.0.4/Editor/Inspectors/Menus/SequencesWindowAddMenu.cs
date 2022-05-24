using UnityEngine;
using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    internal class SequencesWindowAddMenu
    {
        public event System.Action userClickedOnCreateMasterSequence;
        public event System.Action<string> userClickedOnCreateSequenceAsset;

        GenericMenu m_Menu;

        public SequencesWindowAddMenu()
        {
            RefreshData();
        }

        void RefreshData()
        {
            m_Menu = new GenericMenu();
            m_Menu.AddItem(new GUIContent("Create Master Sequence"), false, CreateMasterSequence);
            m_Menu.AddSeparator("/");
            PopulateWithUserTypes();
        }

        void PopulateWithUserTypes()
        {
            foreach (string type in CollectionType.instance.types)
            {
                m_Menu.AddItem(new GUIContent("New Sequence Asset/" + type), false, CreateSequenceAsset, type);
            }
        }

        public void Show()
        {
            m_Menu.ShowAsContext();
        }

        void CreateMasterSequence()
        {
            userClickedOnCreateMasterSequence?.Invoke();
        }

        void CreateSequenceAsset(object type)
        {
            string typeAsString = type as string;
            userClickedOnCreateSequenceAsset?.Invoke(typeAsString);
        }
    }
}
