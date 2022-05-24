using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Sequences;
using UnityEngine.UIElements;

using TreeView = UnityEditor.IMGUI.Controls.TreeView;

namespace UnityEditor.Sequences
{
    internal class AssetCollectionsTreeView : TreeView
    {
        VisualElement m_VisualElementContainer;

        [SerializeField]
        List<TreeViewItem> m_Items = new List<TreeViewItem>();

        // Keep an indexer to assign unique ID to new TreeViewItem.
        // ID starts at 1 as the root item's ID is 0.
        [SerializeField]
        int m_IndexGenerator = 1;

        bool isCreatingNewItem
        {
            get
            {
                return m_Items.Exists(treeviewItem => (treeviewItem as TreeViewItemBase).state == TreeViewItemBase.State.Creation);
            }
        }

        bool forceEndRename { get; set; }

        public AssetCollectionsTreeView(TreeViewState state, VisualElement container)
            : base(state)
        {
            m_VisualElementContainer = container;

            Reload();

            getNewSelectionOverride = OnNewSelection;

            SequenceAssetIndexer.sequenceAssetImported += OnSequenceAssetImported;
            SequenceAssetIndexer.sequenceAssetDeleted += OnSequenceAssetDeleted;
            SequenceAssetIndexer.sequenceAssetUpdated += OnSequenceAssetUpdated;
        }

        public void Unload()
        {
            SequenceAssetIndexer.sequenceAssetImported -= OnSequenceAssetImported;
            SequenceAssetIndexer.sequenceAssetDeleted -= OnSequenceAssetDeleted;
            SequenceAssetIndexer.sequenceAssetUpdated -= OnSequenceAssetUpdated;
        }

        /// <summary>
        /// Listens for new import of Sequence Assets from the AssetDatabase.
        /// </summary>
        /// <param name="gameObject"></param>
        void OnSequenceAssetImported(GameObject gameObject)
        {
            // Case 1. Sequence Asset is created from the Sequence Window.
            if (isCreatingNewItem)
                return;

            // Case 2. Duplicated Prefab or Prefab variant created from the Project View.
            if (SequenceAssetUtility.IsSource(gameObject))
            {
                if (GetSequenceAssetItemFrom(gameObject) != null)
                    return;

                string type = SequenceAssetUtility.GetType(gameObject);
                var root = GetCollectionAssetItemFrom(type);

                CreateSequenceAssetTreeViewItem(gameObject, root as CollectionTypeTreeViewItem);
            }
            else if (SequenceAssetUtility.IsVariant(gameObject))
            {
                if (GetSequenceAssetVariantItemFrom(gameObject) != null)
                    return;

                GameObject baseObject = SequenceAssetUtility.GetSource(gameObject);
                TreeViewItem root = GetSequenceAssetItemFrom(baseObject);
                CreateSequenceAssetVariantTreeViewItem(gameObject, root as SequenceAssetTreeViewItem);
            }

            Reload();
        }

        /// <summary>
        /// Listens for deletion of Sequence Assets in the AssetDatabse.
        /// It detaches TreeView items affected by this deletion.
        /// </summary>
        /// <param name="gameObject"></param>
        void OnSequenceAssetDeleted()
        {
            // Deletion from the asset database.
            // Go over all the items and delete the ones with an invalid prefab reference.
            for (int i = m_Items.Count - 1; i > 0; --i)
            {
                var sequenceAssetItem = m_Items[i] as SequenceAssetTreeViewItem;
                if (sequenceAssetItem != null && sequenceAssetItem.asset == null)
                {
                    Detach(sequenceAssetItem);
                    continue;
                }

                var sequenceAssetVariant = m_Items[i] as SequenceAssetVariantTreeViewItem;
                if (sequenceAssetVariant != null && sequenceAssetVariant.asset == null)
                    Detach(sequenceAssetVariant);
            }
        }

        void OnSequenceAssetUpdated(GameObject sequenceAsset)
        {
            TreeViewItem item = null;

            if (SequenceAssetUtility.IsSource(sequenceAsset))
                item = GetSequenceAssetItemFrom(sequenceAsset);
            else if (SequenceAssetUtility.IsVariant(sequenceAsset))
                item = GetSequenceAssetVariantItemFrom(sequenceAsset);

            if (item != null)
                item.displayName = sequenceAsset.name;
        }

        TreeViewItem GetCollectionAssetItemFrom(string type)
        {
            return m_Items.Find(treeviewItem =>
            {
                return (treeviewItem.depth == 0) && (treeviewItem as CollectionTypeTreeViewItem).collectionType == type;
            });
        }

        TreeViewItem GetSequenceAssetItemFrom(GameObject gameObject)
        {
            return m_Items.Find(treeviewItem =>
            {
                return (treeviewItem.depth == 1) && (treeviewItem as SequenceAssetTreeViewItem).asset == gameObject;
            });
        }

        TreeViewItem GetSequenceAssetVariantItemFrom(GameObject gameObject)
        {
            return m_Items.Find((treeviewItem) =>
            {
                var variantItem = treeviewItem as SequenceAssetVariantTreeViewItem;
                return variantItem != null && variantItem.asset == gameObject;
            });
        }

        void GenerateTreeFromData(GameObject[] sequenceAssets)
        {
            foreach (var userType in CollectionType.instance.types)
                GenerateAssetCollectionTreeView(userType, sequenceAssets);
        }

        void GenerateAssetCollectionTreeView(string collectionType, GameObject[] assets)
        {
            CollectionTypeTreeViewItem collectionTypeTreeViewItem = CreateAssetCollectionTreeViewItem(collectionType);

            var content = (assets != null && assets.Length > 0) ? Array.FindAll(
                assets,
                a => SequenceAssetUtility.GetType(a) == collectionType) : null;

            if (content == null || content.Length == 0)
                return;

            foreach (var sequenceAsset in content)
                GenerateSequenceAssetTreeView(sequenceAsset, collectionTypeTreeViewItem);
        }

        void GenerateSequenceAssetTreeView(GameObject asset, CollectionTypeTreeViewItem parent)
        {
            var sequenceAssetTreeViewItem = CreateSequenceAssetTreeViewItem(asset, parent);

            foreach (var variant in SequenceAssetUtility.GetVariants(asset))
                CreateSequenceAssetVariantTreeViewItem(variant, sequenceAssetTreeViewItem);
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

        internal void CreateSequenceAssetInContext(string collectionType)
        {
            var parent = m_Items.Find(x => (x is CollectionTypeTreeViewItem) && (x as CollectionTypeTreeViewItem).collectionType == collectionType);
            if (parent == null)
            {
                parent = CreateAssetCollectionTreeViewItem(collectionType);
            }

            if (parent != null)
            {
                int newId = GetNextId();
                var newItem = new SequenceAssetTreeViewItem { id = newId, depth = parent.depth + 1, displayName = $"{collectionType}Asset"};
                newItem.owner = this;
                m_Items.Add(newItem);
                parent.AddChild(newItem);

                Reload();

                SetSelection(new int[] { newItem.id }, TreeViewSelectionOptions.RevealAndFrame);
                BeginRename(newItem);
            }
        }

        internal void CreateSequenceAssetVariantInContext(GameObject source)
        {
            // This methods runs differently from other TreeViewItems.
            // UX: The user cannot set a custom name to a new Asset Variant.
            // However, we want the creation process to follow the same rules as the others TreeViewItems.
            // This is done in three steps.

            var root = GetSequenceAssetItemFrom(source);
            int newId = GetNextId();

            // 1.Create an empty TreeViewItem for the new variant.
            // Let its internal state set to State.Creation.
            SequenceAssetVariantTreeViewItem newItem = new SequenceAssetVariantTreeViewItem() { id = newId, depth = 2 };
            newItem.owner = this;
            m_Items.Add(newItem);
            root.AddChild(newItem);

            // 2. Request the creation of an Asset Variant.
            GameObject variant = SequenceAssetUtility.CreateVariant(source);

            // 3. Assign the new Asset Variant to the empty TreeViewItem.
            // Internal state then becomes State.Ok.
            newItem.SetSequenceAssetVariant(variant);
            newItem.displayName = variant.name;

            // Repaint the view.
            Reload();

            SetSelection(new int[] { newItem.id }, TreeViewSelectionOptions.RevealAndFrame);
        }

        CollectionTypeTreeViewItem CreateAssetCollectionTreeViewItem(string name)
        {
            int newId = GetNextId();

            CollectionTypeTreeViewItem newItem = new CollectionTypeTreeViewItem() { id = newId, depth = 0, displayName = name };
            newItem.SetCollectionType(name);
            newItem.owner = this;
            m_Items.Add(newItem);

            return newItem;
        }

        SequenceAssetTreeViewItem CreateSequenceAssetTreeViewItem(GameObject asset, CollectionTypeTreeViewItem parent)
        {
            int newId = GetNextId();

            SequenceAssetTreeViewItem newItem = new SequenceAssetTreeViewItem() { id = newId, depth = 1, displayName = asset.name };
            newItem.SetSequenceAsset(asset);
            newItem.owner = this;
            m_Items.Add(newItem);
            parent.AddChild(newItem);

            return newItem;
        }

        void CreateSequenceAssetVariantTreeViewItem(GameObject prefab, SequenceAssetTreeViewItem parent)
        {
            int newId = GetNextId();

            SequenceAssetVariantTreeViewItem newItem = new SequenceAssetVariantTreeViewItem() { id = newId, depth = 2, displayName = prefab.name };
            newItem.SetSequenceAssetVariant(prefab);
            newItem.owner = this;
            m_Items.Add(newItem);
            parent.AddChild(newItem);
        }

        protected override void ContextClickedItem(int id)
        {
            GetItem(id).ContextClicked();
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
            if (item is CollectionTypeTreeViewItem)
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
                {
                    if (item.ValidateCreation(args.newName))
                        Reload();
                    else
                        DeleteItems(new int[] { args.itemID });
                }
                else if (item.state == TreeViewItemBase.State.Ok)
                {
                    item.Rename(args.newName);
                    Reload();
                }
            }
            else
            {
                if (item.state == TreeViewItemBase.State.Creation)
                    Detach(item);
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
                // item can be null if the tree view is in focus but no item is selected
                if (item is object)
                    item.Delete();
            }
        }

        TreeViewItemBase GetItem(int id)
        {
            return m_Items.Find(x => x.id == id) as TreeViewItemBase;
        }

        int GetNextId()
        {
            return m_IndexGenerator++;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem()
            {
                depth = -1
            };

            if (m_Items.Count == 0)
            {
                GameObject[] sequenceAssets = SequenceAssetUtility.FindAllSources().ToArray();
                GenerateTreeFromData(sequenceAssets);

                // When reloading the parent windows, internal states may have expanded items.
                SetExpanded(state.expandedIDs);
            }

            foreach (var item in m_Items)
            {
                if (item.depth == 0)
                    root.AddChild(item);
            }

            return root;
        }

        protected override void DoubleClickedItem(int id)
        {
            TreeViewItemBase item = GetItem(id);
            item.DoubleClicked();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var collectionTypeItem = args.item as CollectionTypeTreeViewItem;

            /// When collectionTypeItem is null, it means the treeview is diplaying a Prefab or a Prefab Variant.
            /// In that case, we fallback to the default drawing method.
            if (args.isRenaming || collectionTypeItem == null)
            {
                base.RowGUI(args);
                return;
            }

            GUIContent gui = new GUIContent(args.label, (args.selected) ? collectionTypeItem.iconSelected : collectionTypeItem.icon);
            var indentLevel = EditorGUI.indentLevel;

            GUIStyle itemStyle = new GUIStyle(GUI.skin.label);
            itemStyle.normal.textColor = GUI.skin.label.normal.textColor;

            EditorGUI.indentLevel = args.item.depth + 1;
            EditorGUI.LabelField(args.rowRect, gui, itemStyle);
            EditorGUI.indentLevel = indentLevel;
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

            Reload();
        }
    }
}
