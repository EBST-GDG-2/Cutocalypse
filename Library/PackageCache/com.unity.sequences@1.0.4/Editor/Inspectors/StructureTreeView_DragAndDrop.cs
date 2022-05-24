using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.Sequences
{
    internal interface IEditorialDraggable
    {
        bool CanBeParentedWith(TreeViewItem parent);
    }

    internal partial class StructureTreeView : TreeView
    {
        // Dragging
        //-----------

        const string k_GenericDragID = "GenericDragColumnDragging";
        public event Action<IList<TreeViewItem>> beforeDroppingDraggedItems;

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (hasSearch)
                return;

            DragAndDrop.PrepareStartDrag();
            var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
            DragAndDrop.SetGenericData(k_GenericDragID, draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] {};  // this IS required for dragging to work
            string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag(title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            // Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
            var draggedRows = DragAndDrop.GetGenericData(k_GenericDragID) as List<TreeViewItem>;
            if (draggedRows == null)
                return DragAndDropVisualMode.None;

            // Parent item is null when dragging outside any tree view items.
            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.BetweenItems:
                {
                    bool validDrag = ValidDrag(args.parentItem, draggedRows);
                    if (args.performDrop && validDrag)
                    {
                        var parentData = (args.parentItem);
                        OnDropDraggedElementsAtIndex(draggedRows, parentData, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
                    }
                    return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                }

                case DragAndDropPosition.OutsideItems:
                {
                    bool validDrag = ValidDrag(rootItem, draggedRows);

                    if (args.performDrop && validDrag)
                    {
                        OnDropDraggedElementsAtIndex(draggedRows, rootItem, rootItem.children.Count);
                    }

                    return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                }
                default:
                    Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                    return DragAndDropVisualMode.None;
            }
        }

        protected virtual void OnDropDraggedElementsAtIndex(List<TreeViewItem> draggedRows, TreeViewItem parent, int insertIndex)
        {
            if (beforeDroppingDraggedItems != null)
                beforeDroppingDraggedItems(draggedRows);

            var draggedElements = new List<TreeViewItem>();
            foreach (var x in draggedRows)
                draggedElements.Add(x);

            var selectedIDs = draggedElements.Select(x => x.id).ToArray();
            MoveElements(parent, insertIndex, draggedElements);
            SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
        }

        void MoveElements(TreeViewItem parentElement, int insertionIndex, List<TreeViewItem> elements)
        {
            if (insertionIndex < 0)
                throw new ArgumentException("Invalid input: insertionIndex is -1, client needs to decide what index elements should be reparented at");

            // Invalid reparenting input
            if (parentElement == null)
                return;

            // We are moving items so we adjust the insertion index to accomodate that any items above the insertion index is removed before inserting
            if (insertionIndex > 0)
                insertionIndex -= parentElement.children.GetRange(0, insertionIndex).Count(elements.Contains);

            // Remove draggedItems from their parents
            foreach (var draggedItem in elements)
            {
                draggedItem.parent.children.Remove(draggedItem);    // remove from old parent
                draggedItem.parent = parentElement;                 // set new parent
            }

            if (parentElement.children == null)
                parentElement.children = new List<TreeViewItem>();

            // Insert dragged items under new parent
            parentElement.children.InsertRange(insertionIndex, elements);

            StructureTreeView.UpdateDepthValuesTreeView(rootItem);

            Reload();
        }

        static void UpdateDepthValuesTreeView(TreeViewItem root)
        {
            if (root == null)
                throw new ArgumentNullException("root", "The root is null");

            if (!root.hasChildren)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                if (current.children != null)
                {
                    foreach (var child in current.children)
                    {
                        child.depth = current.depth + 1;
                        stack.Push(child);
                    }
                }
            }
        }

        bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
        {
            TreeViewItem currentParent = parent;

            var item = draggedItems[0] as IEditorialDraggable;

            if (currentParent != null && item != null)
            {
                if (draggedItems.Contains(currentParent))
                    return false;

                return (item.CanBeParentedWith(currentParent));
            }
            return true;
        }
    }
}
