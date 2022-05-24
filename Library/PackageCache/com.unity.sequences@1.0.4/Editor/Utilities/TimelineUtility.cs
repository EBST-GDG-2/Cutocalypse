using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Sequences;
using UnityEngine.Sequences.Timeline;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if !TIMELINE_1_6_0_PRE_5_OR_NEWER
using System;
using System.Reflection;
#endif

namespace UnityEditor.Sequences
{
    internal static class TimelineUtility
    {
#if !TIMELINE_1_6_0_PRE_5_OR_NEWER
        static MethodInfo s_TimelineWindowSetCurrentTimelineMethod;

        static MethodInfo timelineWindowSetCurrentTimelineMethod
        {
            get
            {
                if (s_TimelineWindowSetCurrentTimelineMethod == null)
                {
                    try
                    {
                        var window = TimelineEditor.GetWindow();
                        if (window == null)
                            return null;

                        s_TimelineWindowSetCurrentTimelineMethod = window.GetType().GetMethod("SetCurrentTimeline", new Type[] { typeof(PlayableDirector), typeof(TimelineClip) });
                    }
                    catch (Exception e)
                    {
                        Debug.LogErrorFormat("Exception {0}", e.Message);
                    }
                }
                return s_TimelineWindowSetCurrentTimelineMethod;
            }
        }

        /// <summary>
        /// Check if all reflected methods and properties are available.
        /// Returns true if developer can use breadcrumb. Otherwise, returns false.
        /// In that case, Timeline's API might have changed.
        /// </summary>
        internal static bool breadcrumbIsAvailable => timelineWindowSetCurrentTimelineMethod != null;
#endif

        internal static TimelinePath breadcrumb = new TimelinePath();

        internal class TimelinePath
        {
            internal struct Element
            {
                public PlayableDirector director;
                public TimelineClip hostClip;
            }
            Stack<Element> path;

            internal int count => path.Count;

            public TimelinePath()
            {
                path = new Stack<Element>();
            }

            public Element Pop()
            {
                return path.Pop();
            }

            public void Clear()
            {
                path.Clear();
            }

            public void BuildAndAppend(TimelineSequence destinationClip)
            {
                TimelineSequence current = destinationClip;
                while (current != null)
                {
                    var director = current.timeline.FindDirector();
                    if (director == null)
                        return;

                    TimelineClip hostClip = null;
                    if (current.parent != null)
                    {
                        var track = (current.parent as TimelineSequence).childrenTrack;
                        if (track == null)
                            return;

                        hostClip = track.GetFirstClipWithName(current.name);
                        if (hostClip == null)
                            return;
                    }

                    path.Push(new Element() { director = director, hostClip = hostClip });

                    current = current.parent as TimelineSequence;
                }
            }

            public void Append(PlayableDirector director, TimelineClip hostClip)
            {
                path.Push(new Element() { director = director, hostClip = hostClip });
            }
        }

#if !TIMELINE_1_6_0_PRE_5_OR_NEWER
        static void PushItemIntoBreadcrumb(PlayableDirector director, TimelineClip hostClip)
        {
            var window = TimelineEditor.GetWindow();
            if (window == null)
                return;

            timelineWindowSetCurrentTimelineMethod?.Invoke(window, new object[] { director, hostClip });
        }

#endif

        public static void RefreshBreadcrumb(TimelinePath path = null)
        {
            if (path == null)
                path = breadcrumb;

            TimelinePath.Element parent = default;

#if TIMELINE_1_6_0_PRE_5_OR_NEWER
            TimelineEditorWindow window = TimelineEditor.GetWindow();
            if (window == null)
                return;
#endif
            while (path.count > 0)
            {
                TimelinePath.Element drillInClip = path.Pop();
                if (parent.director != null)
                {
#if TIMELINE_1_6_0_PRE_5_OR_NEWER
                    SequenceContext context = default;

                    // Look for existing context before creating a new one.
                    foreach (var child in window.navigator.GetChildContexts())
                    {
                        if (child.director == drillInClip.director)
                        {
                            context = child;
                            break;
                        }
                    }

                    if (context == default)
                        context = new SequenceContext(drillInClip.director, drillInClip.hostClip);

                    window.navigator.NavigateTo(context);
#else
                    PushItemIntoBreadcrumb(drillInClip.director, drillInClip.hostClip);
#endif
                }
                else
                {
                    // Add a PlayableDirectorInternalState only on the master timeline.
                    if (!drillInClip.director.GetComponent<PlayableDirectorInternalState>())
                        Undo.AddComponent<PlayableDirectorInternalState>(drillInClip.director.gameObject);
#if TIMELINE_1_6_0_PRE_5_OR_NEWER
                    window.SetTimeline(drillInClip.director);
#else
                    PushItemIntoBreadcrumb(drillInClip.director, null);
#endif
                }
                parent = drillInClip;
            }

            if (TimelineEditor.masterDirector
                && TimelineEditor.masterDirector.TryGetComponent<PlayableDirectorInternalState>(out PlayableDirectorInternalState component))
            {
                component.RestoreTimeState();
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }

#if TIMELINE_1_6_0_PRE_5_OR_NEWER
        internal static double GetProjectFrameRate()
        {
            return TimelineProjectSettings.instance.defaultFrameRate;
        }

#else
        internal static double GetProjectFrameRate()
        {
            return TimelineProjectSettings.instance.assetDefaultFramerate;
        }

#endif

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdated;
        }

        static void OnPrefabInstanceUpdated(GameObject prefabInstance)
        {
            RefreshBreadcrumb();
        }
    }
}
