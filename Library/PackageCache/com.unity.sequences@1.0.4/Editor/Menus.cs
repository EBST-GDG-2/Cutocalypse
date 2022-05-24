using UnityEngine;
using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    internal class Menus
    {
        const string k_ParentMenuName = "Window/Sequencing";

        [MenuItem(k_ParentMenuName + "/Sequences", priority = 3004)]
        static void OpenSequencesWindow()
        {
            var win = EditorWindow.GetWindow<SequencesWindow>();
            win.Show();
        }

        [MenuItem(k_ParentMenuName + "/Sequence Assembly", priority = 3004)]
        static void OpenSequenceAssemblyWindow()
        {
            var win = EditorWindow.GetWindow<SequenceAssemblyWindow>(typeof(SequencesWindow));
            win.Show();
        }

        [MenuItem("GameObject/Sequences/Master Sequence", true, 11)]
        static bool NewMasterSequenceValidate()
        {
            var go = Selection.activeGameObject;
            if (go == null) return true;

            var comp = go.GetComponent<SequenceFilter>();
            return comp == null;
        }

        [MenuItem("GameObject/Sequences/Master Sequence", false, 11)]
        static void NewMasterSequence(MenuCommand command)
        {
            SequenceUtility.CreateMasterSequence(SequenceUtility.k_DefaultMasterSequenceName);
        }

        [MenuItem("GameObject/Sequences/Sequence", true, 11)]
        static bool NewSequenceValidate()
        {
            var go = Selection.activeGameObject;
            if (go == null || EditorUtility.IsPersistent(go))
                return false;

            var comp = go.GetComponent<SequenceFilter>();
            if (comp == null || comp.masterSequence == null)
                return false;

            var sequence = comp.masterSequence.manager.GetAt(comp.elementIndex);
            if (!SequenceUtility.IsValidSequence(sequence as TimelineSequence))
                return false;

            return comp.type == SequenceFilter.Type.MasterSequence || comp.type == SequenceFilter.Type.Sequence;
        }

        [MenuItem("GameObject/Sequences/Sequence", false, 11)]
        static void NewSequence(MenuCommand command)
        {
            var parent = Selection.activeGameObject;
            SequenceFilter parentFilter = parent.GetComponent<SequenceFilter>();
            MasterSequence masterSequence = parentFilter.masterSequence;
            if (parentFilter.type == SequenceFilter.Type.MasterSequence)
                SequenceUtility.CreateSequence(SequenceUtility.k_DefaultSequenceName, masterSequence, masterSequence.rootSequence);
            else
            {
                TimelineSequence sequence = parentFilter.masterSequence.manager.GetAt(parentFilter.elementIndex) as TimelineSequence;
                SequenceUtility.CreateSequence(SequenceUtility.k_DefaultSequenceName, masterSequence, sequence);
            }
        }
    }
}
