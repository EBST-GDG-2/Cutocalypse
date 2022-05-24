using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Sequences;
using UnityEngine.UIElements;

using TreeView = UnityEditor.IMGUI.Controls.TreeView;

namespace UnityEditor.Sequences
{
    internal partial class StructureTreeView : TreeView
    {
        static class Styles
        {
            public static readonly Color k_InvalidColorLight = new Color32(142, 10, 10, 255);
            public static readonly Color k_InvalidColorDark = new Color32(255, 120, 120, 255);
        }

        VisualElement m_VisualElementContainer;

        [SerializeField]
        List<TreeViewItem> m_Items =  new List<TreeViewItem>();

        // Keep an indexer to assign unique ID to new TreeViewItem.
        // ID starts at 1 as the root item's ID is 0.
        [SerializeField]
        int m_IndexGenerator = 1;

        /// <summary>
        /// Tells if TreeView is in the process of creating a new item.
        /// <seealso cref="TreeViewItemBase.state"/>
        /// </summary>
        bool isCreatingNewItem
        {
            get
            {
                return m_Items.Exists(treeviewItem => (treeviewItem as TreeViewItemBase).state == TreeViewItemBase.State.Creation);
            }
        }

        bool forceEndRename { get; set; }

        public StructureTreeView(TreeViewState state, VisualElement container)
            : base(state)
        {
            m_VisualElementContainer = container;

            Reload();

            getNewSelectionOverride = OnNewSelection;
            SelectionUtility.sequenceSelectionChanged += OnSequenceSelectionChanged;
            SequenceUtility.sequenceCreated += OnSequenceCreated;
            SequenceUtility.sequenceDeleted += OnSequenceDeleted;
            Sequence.sequenceRenamed += OnSequenceRenamed;
        }

        public void Unload()
        {
            SelectionUtility.sequenceSelectionChanged -= OnSequenceSelectionChanged;
            SequenceUtility.sequenceCreated -= OnSequenceCreated;
            SequenceUtility.sequenceDeleted -= OnSequenceDeleted;
            Sequence.sequenceRenamed -= OnSequenceRenamed;
        }

        /// <summary>
        /// Listens for creation of a new Sequence from the API then create a new TreeViewItem when it's necessary.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="masterSequence"></param>
        /// <remarks>
        /// There are three ways to create a Sequence (regardless its level in the hierarchy):
        /// 1. From the Sequences Window.
        /// 2. From the Hierarchy Window.
        /// 3. Directly from the API.
        ///
        /// When this is triggered because of 1., skip this event as the corresponding TreeViewItem is already created.
        /// </remarks>
        void OnSequenceCreated(TimelineSequence sequence, MasterSequence masterSequence)
        {
            // Case 1.
            if (isCreatingNewItem)
                return;

            // Case 2 and 3.
            if (sequence == masterSequence.rootSequence)
            {
                // A MasterSequence asset has been created.
                var masterSequenceItem = CreateNewMasterSequenceFrom(masterSequence);
                rootItem.AddChild(masterSequenceItem);
            }
            else if (sequence.parent == masterSequence.rootSequence)
            {
                // A Sequence asset has been created.
                var masterSequenceItem = GetMasterSequenceItemFrom(masterSequence);
                var sequenceItem = CreateNewSequenceFrom(sequence, masterSequenceItem, masterSequence);
            }
            else
            {
                // A SubSequence asset has been created.
                var sequenceItem = GetSequenceTreeViewItemFrom(sequence.parent as TimelineSequence);
                var subSequenceItem = CreateNewSubSequenceFrom(sequence, sequenceItem, masterSequence);
            }
        }

        /// <summary>
        /// Listens for deletion of a Sequence from in the API.
        /// </summary>
        void OnSequenceDeleted()
        {
            for (int i = m_Items.Count - 1; i > 0; --i)
            {
                EditorialElementTreeViewItem editorialItem = m_Items[i] as EditorialElementTreeViewItem;
                if (editorialItem == null)
                    continue;

                if (TimelineSequence.IsNullOrEmpty(editorialItem.timelineSequence))
                {
                    Detach(editorialItem);
                    Reload();
                }
            }
        }

        void OnSequenceSelectionChanged()
        {
            Sequence sequence = SelectionUtility.activeSequenceSelection;
            if (sequence == null)
            {
                SetSelection(new int[] {}, TreeViewSelectionOptions.None);
                return;
            }

            var foundItem = m_Items.Find(item => (item as EditorialElementTreeViewItem).timelineSequence == sequence);
            if (foundItem != null)
                SetSelection(new int[] { foundItem.id }, TreeViewSelectionOptions.RevealAndFrame);

            Reload();
        }

        void OnSequenceRenamed(Sequence sequence)
        {
            EditorialElementTreeViewItem found = m_Items.Find(x => ((EditorialElementTreeViewItem)x).timelineSequence == sequence) as EditorialElementTreeViewItem;
            if (found != null)
            {
                found.id = SequenceUtility.GetHashCode(found.timelineSequence, found.masterSequence);
                found.displayName = sequence.name;
            }
        }

        void GenerateTreeFromData(IEnumerable<MasterSequence> masterSequences)
        {
            foreach (var masterSequence in masterSequences)
            {
                if (masterSequence.manager != null && masterSequence.manager.count > 0 && masterSequence.rootSequence != null)
                {
                    GenerateMasterSequenceTreeView(masterSequence);
                }
            }
        }

        void GenerateMasterSequenceTreeView(MasterSequence masterSequence)
        {
            var masterSequenceTreeViewItem = CreateNewMasterSequenceFrom(masterSequence);

            if (!masterSequence.rootSequence.hasChildren)
                return;

            foreach (var childSequence in masterSequence.rootSequence.children)
            {
                SequenceTreeViewItem sequenceItem = CreateNewSequenceFrom(childSequence as TimelineSequence, masterSequenceTreeViewItem, masterSequence);
                GenerateSequenceTreeView(childSequence as TimelineSequence, sequenceItem, masterSequence);
            }
        }

        void GenerateSequenceTreeView(TimelineSequence sequence, SequenceTreeViewItem parent, MasterSequence asset)
        {
            if (!sequence.hasChildren)
                return;

            foreach (var child in sequence.children)
            {
                CreateNewSubSequenceFrom((TimelineSequence)child, parent, asset);
            }
        }

        public void OnGUI()
        {
            Event evt = Event.current;
            if (HasFocus() && HasSelection())
            {
                if (evt.isKey && evt.type == EventType.KeyUp && evt.keyCode == KeyCode.Delete)
                {
                    DeleteItems(GetSelection());
                }
            }

            OnGUI(m_VisualElementContainer.contentRect);
        }

        internal void ForceEndRename()
        {
            forceEndRename = true;
            EndRename();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.isRenaming)
            {
                base.RowGUI(args);
                return;
            }
            var item = args.item as EditorialElementTreeViewItem;


            GUIContent itemLabel = new GUIContent(args.label, (args.selected) ? item.iconSelected : item.icon);
            var indentLevel = EditorGUI.indentLevel;

            GUIStyle itemStyle = new GUIStyle(GUI.skin.label);

            if (item.GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                itemStyle.normal.textColor = EditorGUIUtility.isProSkin ? Styles.k_InvalidColorDark : Styles.k_InvalidColorLight;
            else
                itemStyle.normal.textColor = GUI.skin.label.normal.textColor;

            itemLabel.tooltip = "";
            if (item.GetTargetValidity().HasFlag(SequenceUtility.SequenceValidity.MissingGameObject))
                itemLabel.tooltip += "Missing GameObject";

            if (item.GetTargetValidity().HasFlag(SequenceUtility.SequenceValidity.MissingTimeline))
                itemLabel.tooltip += String.IsNullOrEmpty(itemLabel.tooltip) ? "Missing Timeline" : "\nMissing Timeline";

            if (item.GetTargetValidity().HasFlag(SequenceUtility.SequenceValidity.Orphan))
                itemLabel.tooltip += String.IsNullOrEmpty(itemLabel.tooltip) ? "Missing Timeline in a parent Sequence" : "\nMissing Timeline in a parent Sequence";

            EditorGUI.indentLevel = args.item.depth + 1;
            EditorGUI.LabelField(args.rowRect, itemLabel, itemStyle);
            EditorGUI.indentLevel = indentLevel;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem()
            {
                depth = -1
            };
            root.children = new List<TreeViewItem>(m_Items.Count);

            if (m_Items.Count == 0)
            {
                IEnumerable<MasterSequence> existingAssets = SequencesAssetDatabase.FindAsset<MasterSequence>();
                GenerateTreeFromData(existingAssets);

                // When reloading the parent windows, internal states may have expanded items.
                SetExpanded(state.expandedIDs);
            }

            List<EditorialElementTreeViewItem> toDelete = new List<EditorialElementTreeViewItem>();

            for (int i = m_Items.Count - 1; i >= 0; --i)
            {
                if (m_Items[i].depth == 0)
                {
                    var item = (m_Items[i] as EditorialElementTreeViewItem);

                    // Detach invalid MasterSequence items.
                    if (item.state == TreeViewItemBase.State.Ok && item.masterSequence == null)
                    {
                        toDelete.Add(item);
                        continue;
                    }

                    // MasterSequence is valid, add to the tree.
                    root.AddChild(item);
                }
            }

            toDelete.ForEach(x => Detach(x));
            root.children.Sort();

            return root;
        }

        protected override void ContextClickedItem(int id)
        {
            GetItem(id).ContextClicked();
        }

        protected override void DoubleClickedItem(int id)
        {
            GetItem(id).DoubleClicked();
        }

        List<int> OnNewSelection(TreeViewItem clickedItem, bool keepMultiSelection, bool useActionKeyAsShift)
        {
            var item = clickedItem as TreeViewItemBase;
            if (item != null)
            {
                item.Selected();

                return new List<int>() { clickedItem.id };
            }
            return null;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            var editorialItem = item as EditorialElementTreeViewItem;
            if (editorialItem != null && editorialItem.GetTargetValidity() != SequenceUtility.SequenceValidity.Valid)
                return false;

            return true;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            var item = GetItem(args.itemID);
            if (item == null)
            {
                // Reset state as nothing happened.
                forceEndRename = false;
                return;
            }

            if (forceEndRename || args.acceptedRename)
            {
                forceEndRename = false;

                if (item.state == TreeViewItemBase.State.Creation)
                    item.ValidateCreation(args.newName);
                else if (item.state == TreeViewItemBase.State.Ok)
                    item.Rename(args.newName);
            }
            else
            {
                if (item.state == TreeViewItemBase.State.Creation)
                {
                    Detach(item);
                    Reload();
                }
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        /// <summary>
        /// Trigger a delete action request on each item from the given parameter.
        /// Item's implementation is responsible for validating the request.
        /// </summary>
        /// <param name="ids"></param>
        void DeleteItems(IList<int> ids)
        {
            foreach (int id in ids)
            {
                var item = GetItem(id);
                if (item != null)
                    item.Delete();
            }
        }

        MasterSequenceTreeViewItem CreateNewMasterSequenceFrom(MasterSequence masterSequence)
        {
            int newId = SequenceUtility.GetHashCode(masterSequence.rootSequence, masterSequence);

            MasterSequenceTreeViewItem newItem = new MasterSequenceTreeViewItem { id = newId, depth = 0, displayName = masterSequence.rootSequence.name };
            newItem.owner = this;
            newItem.SetSequence(masterSequence.rootSequence, masterSequence);

            m_Items.Add(newItem);

            return newItem;
        }

        SequenceTreeViewItem CreateNewSequenceFrom(TimelineSequence sequence, MasterSequenceTreeViewItem parent, MasterSequence masterSequence)
        {
            int newId = SequenceUtility.GetHashCode(sequence, masterSequence);
            var newItem = new SequenceTreeViewItem { id = newId, depth = parent.depth + 1, displayName = sequence.name };
            newItem.owner = this;
            newItem.SetSequence(sequence, masterSequence);

            m_Items.Add(newItem);
            parent.AddChild(newItem);

            return newItem;
        }

        SubSequenceTreeViewItem CreateNewSubSequenceFrom(TimelineSequence sequence, SequenceTreeViewItem parent, MasterSequence masterSequence)
        {
            int newId = SequenceUtility.GetHashCode(sequence, masterSequence);
            SubSequenceTreeViewItem newItem = new SubSequenceTreeViewItem { id = newId, depth = parent.depth + 1, displayName = sequence.name };
            newItem.owner = this;
            newItem.SetSequence(sequence, masterSequence);

            m_Items.Add(newItem);
            parent.AddChild(newItem);

            return newItem;
        }

        public void CreateNewMasterSequence()
        {
            int newId = GetTemporaryId();

            MasterSequenceTreeViewItem newItem = new MasterSequenceTreeViewItem { id = newId, depth = 0 };
            newItem.owner = this;
            rootItem.AddChild(newItem);
            m_Items.Add(newItem);

            Reload();

            SetSelection(new int[] { newItem.id }, TreeViewSelectionOptions.RevealAndFrame);
            BeginRename(newItem);
        }

        public void CreateNewSequenceInContext(MasterSequenceTreeViewItem parent)
        {
            if (parent is MasterSequenceTreeViewItem)
            {
                int newId = GetTemporaryId();
                var newItem = new SequenceTreeViewItem { id = newId, depth = parent.depth + 1 };
                newItem.owner = this;

                m_Items.Add(newItem);
                parent.AddChild(newItem);
                Reload();

                SetSelection(new int[] { newItem.id }, TreeViewSelectionOptions.RevealAndFrame);
                BeginRename(newItem);
            }
        }

        public void CreateNewSubSequenceInContext(SequenceTreeViewItem parent)
        {
            if (parent is SequenceTreeViewItem)
            {
                int newId = GetTemporaryId();
                SubSequenceTreeViewItem newItem = new SubSequenceTreeViewItem { id = newId, depth = parent.depth + 1 };
                newItem.owner = this;

                m_Items.Add(newItem);
                parent.AddChild(newItem);

                Reload();

                SetSelection(new int[] { newItem.id }, TreeViewSelectionOptions.RevealAndFrame);
                BeginRename(newItem);
            }
        }

        /// <summary>
        /// Detaches the given item from the TreeView without action on the assigned item.
        /// </summary>
        /// <param name="item"></param>
        public void Detach(TreeViewItem item)
        {
            item.parent.children.Remove(item);
            item.parent = null;
            m_Items.Remove(item);
        }

        public bool SelectionContains(Sequence sequence)
        {
            var selections = GetSelection();
            foreach (int id in selections)
            {
                var item = GetItem(id);

                if (item == null)
                    continue;

                if ((item as EditorialElementTreeViewItem).timelineSequence == sequence)
                    return true;
            }
            return false;
        }

        MasterSequenceTreeViewItem GetMasterSequenceItemFrom(MasterSequence masterSequence)
        {
            return m_Items.Find(treeviewItem =>
            {
                return (treeviewItem.depth == 0) && (treeviewItem as MasterSequenceTreeViewItem).masterSequence == masterSequence;
            }) as MasterSequenceTreeViewItem;
        }

        SequenceTreeViewItem GetSequenceTreeViewItemFrom(TimelineSequence sequence)
        {
            return m_Items.Find(treeviewItem =>
            {
                return (treeviewItem.depth == 1) && (treeviewItem as SequenceTreeViewItem).timelineSequence == sequence;
            }) as SequenceTreeViewItem;
        }

        TreeViewItemBase GetItem(int id)
        {
            return m_Items.Find(x => x.id == id) as TreeViewItemBase;
        }

        /// <summary>
        /// Returns a temporary Id until the actual hashcode for an item can be resolved.
        /// Used mostly when creating a new item as the latter needs to wait for the data to be created
        /// in order to move its state to <see cref="TreeViewItemBase.State.Ok"/>.
        /// </summary>
        /// <returns></returns>
        int GetTemporaryId()
        {
            return m_IndexGenerator++;
        }
    }
}
