using UnityEngine;

namespace UnityEditor.Sequences
{
    internal class SequenceAssetTreeViewItem : TreeViewItemBase
    {
        /// <summary>
        /// Sequence asset source prefab.
        /// </summary>
        public GameObject asset { get; private set; }

        public override Texture2D icon => IconUtility.LoadPrefabIcon(PrefabAssetType.Regular);

        public override bool ValidateCreation(string newName)
        {
            string collectionType = (parent as CollectionTypeTreeViewItem).collectionType;
            if (string.IsNullOrEmpty(newName))
                newName = $"{collectionType}Asset";

            var sequenceAsset = SequenceAssetUtility.CreateSource(newName, collectionType);
            SetSequenceAsset(sequenceAsset);
            displayName = sequenceAsset.name;

            return true;
        }

        public override void ContextClicked()
        {
            SequenceAssetContextMenu.instance.Show(this);
        }

        public override void Selected()
        {
            SelectionUtility.SetSelection(asset);
        }

        public override void Delete()
        {
            if (!UserVerifications.ValidateSequenceAssetDeletion(asset))
                return;

            SequenceAssetUtility.DeleteSourceAsset(asset);
        }

        public override void Rename(string newName)
        {
            var actualNewName = SequenceAssetUtility.Rename(asset, asset.name, newName);
        }

        public void SetSequenceAsset(GameObject sequenceAsset)
        {
            asset = sequenceAsset;
            state = State.Ok;
        }

        public override void DoubleClicked()
        {
            AssetDatabase.OpenAsset(asset);
        }
    }
}
