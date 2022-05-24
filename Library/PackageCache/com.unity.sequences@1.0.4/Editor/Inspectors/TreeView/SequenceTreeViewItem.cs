using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    internal class SequenceTreeViewItem : EditorialElementTreeViewItem, IEditorialDraggable
    {
        public override Texture2D icon => IconUtility.LoadIcon("MasterSequence/Sequence", IconUtility.IconType.UniqueToSkin);
        public override Texture2D iconSelected => IconUtility.LoadIcon("MasterSequence/Sequence-selected", IconUtility.IconType.CommonToAllSkin);

        public SequenceTreeViewItem()
        {
            displayName = "New Sequence";
        }

        public bool CanBeParentedWith(TreeViewItem parent)
        {
            return (parent is MasterSequenceTreeViewItem);
        }

        public override void Selected()
        {
            if (GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                return;

            SelectionUtility.TrySelectSequence(timelineSequence);
        }

        public override void ContextClicked()
        {
            if ((parent as MasterSequenceTreeViewItem).GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                return;

            SequenceContextMenu.instance.isItemValid = GetTargetValidity() == SequenceUtility.SequenceValidity.Valid;
            SequenceContextMenu.instance.Show(this);
        }

        public override void DoubleClicked()
        {
            if (GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                return;

            SelectionUtility.TrySelectSequence(timelineSequence);
            SelectionUtility.SelectTimeline(timelineSequence.timeline);
        }

        public override void Rename(string newName)
        {
            if (GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                return;

            base.Rename(newName);
            timelineSequence.Rename(newName);
        }

        public override bool ValidateCreation(string newName)
        {
            if (string.IsNullOrEmpty(newName))
                newName = displayName;

            MasterSequence masterSequenceAsset = (parent as MasterSequenceTreeViewItem).masterSequence;

            SetSequence(SequenceUtility.CreateSequence(newName, masterSequenceAsset, masterSequenceAsset.rootSequence), masterSequenceAsset);
            displayName = timelineSequence.name;
            id = SequenceUtility.GetHashCode(timelineSequence, masterSequence);
            return true;
        }

        public override void Delete()
        {
            if ((parent as MasterSequenceTreeViewItem).GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                return;

            if (!UserVerifications.ValidateSequenceDeletion(timelineSequence))
                return;

            MasterSequence masterSequenceAsset = (parent as MasterSequenceTreeViewItem).masterSequence;
            SequenceUtility.DeleteSequence(timelineSequence, masterSequenceAsset);
        }
    }
}
