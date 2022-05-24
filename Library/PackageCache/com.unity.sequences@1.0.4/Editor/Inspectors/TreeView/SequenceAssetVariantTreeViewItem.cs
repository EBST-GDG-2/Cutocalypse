using UnityEngine;

namespace UnityEditor.Sequences
{
    internal class SequenceAssetVariantTreeViewItem : TreeViewItemBase
    {
        public override Texture2D icon => IconUtility.LoadPrefabIcon(PrefabAssetType.Variant);

        public GameObject asset { get; private set; }

        public override void ContextClicked()
        {
            SequenceAssetVariantContextMenu.instance.Show(this);
        }

        public override void Delete()
        {
            if (!UserVerifications.ValidateSequenceAssetDeletion(asset))
                return;

            SequenceAssetUtility.DeleteVariantAsset(asset);
        }

        public override void Selected()
        {
            SelectionUtility.SetSelection(asset);
        }

        public override bool ValidateCreation(string newName)
        {
            return true;
        }

        public void SetSequenceAssetVariant(GameObject variant)
        {
            asset = variant;
            state = State.Ok;
        }

        public override void DoubleClicked()
        {
            AssetDatabase.OpenAsset(asset);
        }

        public override void Rename(string newName)
        {
            var actualNewName = SequenceAssetUtility.Rename(asset, asset.name, newName);
        }
    }
}
