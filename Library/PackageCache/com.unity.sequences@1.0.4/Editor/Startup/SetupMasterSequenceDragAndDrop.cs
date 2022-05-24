using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Sequences;
using System.Collections.Generic;
#if !UNITY_2021_2_0_A18_OR_NEWER
using System;
using System.Reflection;
using UnityEditorInternal;
#endif

namespace UnityEditor.Sequences.Startup
{
    [InitializeOnLoad]
    class SetupMasterSequenceDragAndDrop
    {
#if UNITY_2021_2_0_A18_OR_NEWER
        static SetupMasterSequenceDragAndDrop()
        {
            DragAndDrop.AddDropHandler(HierarchyDropHandler);
            MasterSequence.sequenceAdded += RefreshTimelineEditor;
            MasterSequence.sequenceRemoved += RefreshTimelineEditor;
        }

        static DragAndDropVisualMode HierarchyDropHandler(int dropTargetInstanceID,
            HierarchyDropFlags dropFlags, Transform parentForDraggedObjects, bool perform)
        {
            MasterSequence masterSequence = null;
            foreach (var objectReference in DragAndDrop.objectReferences)
            {
                if (objectReference is MasterSequence)
                {
                    masterSequence = objectReference as MasterSequence;
                    break;
                }
            }

            if (masterSequence == null)
                return DragAndDropVisualMode.None;

            if (perform)
            {
                var sequenceFilters = ObjectsCache.FindObjectsFromScenes<SequenceFilter>();
                foreach (var sequenceFilter in sequenceFilters)
                {
                    if (sequenceFilter.masterSequence == masterSequence)
                    {
                        Debug.Log("MasterSequence \"" + masterSequence.name + "\" already exists in the scene.");
                        return DragAndDropVisualMode.Generic;
                    }
                }
                // parentForDraggedObjects is always null in this call. We expect it to vary when the cinemactic is dragged and dropped under
                // different objects in the hierarchy. It will remain for now, but make sure to double-check it when te API releases.
                SequenceFilter.GenerateSequenceRepresentation(masterSequence, masterSequence.rootSequence, parentForDraggedObjects);
            }

            return DragAndDropVisualMode.Generic;
        }

#else
        static SetupMasterSequenceDragAndDrop()
        {
            Assembly editor = typeof(UnityEditor.Editor).Assembly;
            Type dragAndDropService = editor.GetType("UnityEditor.DragAndDropService");

            Type hierarchyDropHandler = editor.GetType("UnityEditor.DragAndDropService+HierarchyDropHandler");
            Delegate hierarchyDropHandlerDelegate = Delegate.CreateDelegate(hierarchyDropHandler, typeof(SetupMasterSequenceDragAndDrop), "HierarchyDropHandler");

            MethodInfo addDropHandler = dragAndDropService.GetMethod("AddDropHandler",
                BindingFlags.Static | BindingFlags.Public, null, new Type[] {hierarchyDropHandler}, null);
            addDropHandler.Invoke(null, new object[] { hierarchyDropHandlerDelegate });

            MasterSequence.sequenceAdded += RefreshTimelineEditor;
            MasterSequence.sequenceRemoved += RefreshTimelineEditor;
        }

        static DragAndDropVisualMode HierarchyDropHandler(int dropTargetInstanceID,
            InternalEditorUtility.HierarchyDropMode dropMode, Transform parentForDraggedObjects, bool perform)
        {
            MasterSequence masterSequence = null;
            foreach (var objectReference in DragAndDrop.objectReferences)
            {
                if (objectReference is MasterSequence)
                {
                    masterSequence = objectReference as MasterSequence;
                    break;
                }
            }

            if (masterSequence == null)
                return DragAndDropVisualMode.None;

            if (perform)
            {
                IReadOnlyCollection<SequenceFilter> sequenceFilters = ObjectsCache.FindObjectsFromScenes<SequenceFilter>();
                foreach (var sequenceFilter in sequenceFilters)
                {
                    if (sequenceFilter.masterSequence == masterSequence)
                    {
                        Debug.Log("MasterSequence \"" + masterSequence.name + "\" already exists in the scene.");
                        return DragAndDropVisualMode.Generic;
                    }
                }
                // parentForDraggedObjects is always null in this call. We expect it to vary when the cinemactic is dragged and dropped under
                // different objects in the hierarchy. It will remain for now, but make sure to double-check it when te API releases.
                SequenceFilter.GenerateSequenceRepresentation(masterSequence, masterSequence.rootSequence, parentForDraggedObjects);
            }

            return DragAndDropVisualMode.Generic;
        }

#endif

        static void RefreshTimelineEditor(MasterSequence masterSequence, Sequence sequence)
        {
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }
    }
}
