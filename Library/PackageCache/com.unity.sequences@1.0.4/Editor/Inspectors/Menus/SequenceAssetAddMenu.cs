using System.Linq;
using UnityEngine;

namespace UnityEditor.Sequences
{
    class SequenceAssetAddMenu
    {
        public event System.Action<string> userClickedOnCreateSequenceAsset;
        public event System.Action<GameObject> userClickedOnAddSequenceAsset;

        GenericMenu m_Menu;
        string m_Type;

        public SequenceAssetAddMenu()
        {
            SequenceAssetIndexer.sequenceAssetImported += Refresh;
            SequenceAssetIndexer.sequenceAssetUpdated += Refresh;
        }

        void Refresh(GameObject importedPrefab)
        {
            Populate(m_Type);
        }

        public void Populate(string type)
        {
            m_Type = type;
            m_Menu = new GenericMenu();

            var sequenceAssets = SequenceAssetUtility.FindAllSources(type).ToList();
            foreach (var sequenceAsset in sequenceAssets)
                m_Menu.AddItem(new GUIContent(sequenceAsset.name), false, AddSequenceAsset, sequenceAsset);

            if (sequenceAssets.Any())
                m_Menu.AddSeparator("/");

            m_Menu.AddItem(new GUIContent("Create Sequence Asset"), false, CreateSequenceAsset, type);
        }

        public void Show()
        {
            m_Menu.ShowAsContext();
        }

        void AddSequenceAsset(object sequenceAsset)
        {
            var sequenceAssetGo = sequenceAsset as GameObject;
            userClickedOnAddSequenceAsset?.Invoke(sequenceAssetGo);
        }

        void CreateSequenceAsset(object type)
        {
            string typeAsString = type as string;
            userClickedOnCreateSequenceAsset?.Invoke(typeAsString);
        }
    }
}
