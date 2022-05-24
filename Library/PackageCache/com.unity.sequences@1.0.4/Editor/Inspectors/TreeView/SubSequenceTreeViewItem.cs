using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    internal class SubSequenceTreeViewItem : EditorialElementTreeViewItem, IEditorialDraggable
    {
        public override Texture2D icon => IconUtility.LoadIcon("MasterSequence/Shot", IconUtility.IconType.UniqueToSkin);
        public override Texture2D iconSelected => IconUtility.LoadIcon("MasterSequence/Shot-selected", IconUtility.IconType.CommonToAllSkin);

        public SubSequenceTreeViewItem()
            : base()
        {
            displayName = "New Sequence";
        }

        public bool CanBeParentedWith(TreeViewItem parent)
        {
            return (parent is SequenceTreeViewItem);
        }

        public override void Selected()
        {
            if (GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                return;

            SelectionUtility.TrySelectSequence(timelineSequence);
        }

        public override void ContextClicked()
        {
            if (masterSequence == null || (parent as SequenceTreeViewItem).GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                return;

            SubSequenceContextMenu.instance.isItemValid = GetTargetValidity() == SequenceUtility.SequenceValidity.Valid;
            SubSequenceContextMenu.instance.Show(this);
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

            displayName = newName;

            MasterSequence masterSequenceAsset = (parent.parent as MasterSequenceTreeViewItem).masterSequence;
            TimelineSequence sequence = (parent as SequenceTreeViewItem).timelineSequence;

            SetSequence(SequenceUtility.CreateSequence(newName, masterSequenceAsset, sequence), masterSequenceAsset);
            id = SequenceUtility.GetHashCode(timelineSequence, masterSequence);
            return true;
        }

        public override void Delete()
        {
            if (masterSequence == null || (parent as SequenceTreeViewItem).GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                return;

            if (!UserVerifications.ValidateSequenceDeletion(timelineSequence))
                return;

            MasterSequence masterSequenceAsset = (parent.parent as MasterSequenceTreeViewItem).masterSequence;
            SequenceUtility.DeleteSequence(timelineSequence, masterSequenceAsset);
        }
    }
}
