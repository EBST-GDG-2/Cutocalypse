using UnityEngine;

namespace UnityEditor.Sequences
{
    internal class CollectionTypeTreeViewItem : TreeViewItemBase
    {
        public override Texture2D icon => IconUtility.LoadAssetCollectionIcon(collectionType, IconUtility.IconType.UniqueToSkin);
        public override Texture2D iconSelected => IconUtility.LoadAssetCollectionIcon(collectionType + "-selected", IconUtility.IconType.CommonToAllSkin);

        public string collectionType { get; private set; }

        public override void ContextClicked()
        {
            CollectionTypeContextMenu.instance.Show(this);
        }

        public override void Delete()
        {
        }

        public override void Selected()
        {
        }

        public override bool ValidateCreation(string newName)
        {
            if (string.IsNullOrEmpty(newName))
                return false;

            SetCollectionType(newName);

            return true;
        }

        public void SetCollectionType(string collectionTypeName)
        {
            collectionType = collectionTypeName;
            state = State.Ok;
        }

        public override void DoubleClicked()
        {
        }
    }
}
