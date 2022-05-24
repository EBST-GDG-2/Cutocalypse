#if UNITY_INCLUDE_TESTS
using UnityEditor.IMGUI.Controls;
using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    /// <summary>
    /// Utilities used in internal tests.
    /// </summary>
    internal partial class StructureTreeView : TreeView
    {
        internal void ClearItems()
        {
            m_Items.Clear();
        }

        internal bool HasTreeViewItem(MasterSequence masterSequence, TimelineSequence timelineSequence)
        {
            var result = m_Items.Find(treeviewItem =>
            {
                return (treeviewItem as EditorialElementTreeViewItem).timelineSequence == timelineSequence;
            }) as EditorialElementTreeViewItem;
            return result != null;
        }

        internal EditorialElementTreeViewItem GetTreeViewItem(MasterSequence masterSequence, TimelineSequence timelineSequence)
        {
            var result = m_Items.Find(treeviewItem =>
            {
                return (treeviewItem as EditorialElementTreeViewItem).timelineSequence == timelineSequence;
            }) as EditorialElementTreeViewItem;
            return result;
        }
    }
}
#endif
