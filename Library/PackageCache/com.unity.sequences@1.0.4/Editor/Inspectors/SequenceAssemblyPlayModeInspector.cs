using System.Collections.Generic;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Sequences
{
    internal class SequenceAssemblyPlayModeInspector : SequenceAssemblyInspector
    {
        void OnEnable()
        {
            Initialize();
        }

        void OnDisable()
        {
            ClearCollectionsCache();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = base.CreateInspectorGUI();
            var playmodeMessage = new Label("Read-only in Play mode.");
            playmodeMessage.AddToClassList("seq-warning-label");
            root.Insert(0, playmodeMessage);

            return root;
        }

        protected override void SetAssetCollection(string type, IEnumerable<GameObject> sequenceAssetSelections)
        {
            AssetCollectionList newAssetCollectionList = new AssetCollectionList(m_Director, type, true);
            AddCollectionList(sequenceAssetSelections, newAssetCollectionList);
        }

        protected override void  AddSequenceAssetToCollection(IEnumerable<GameObject> sequenceAssetSelections, AssetCollectionList assetCollectionList)
        {
            foreach (var selection in sequenceAssetSelections)
                assetCollectionList.AddSequenceAssetSelection<SequenceAssetInstanceItem>(selection);
        }
    }
}
