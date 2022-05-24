using UnityEngine;
using UnityEngine.Sequences;
using UnityEngine.Playables;

namespace UnityEditor.Sequences
{
    partial class SequenceAssemblyWindow : EditorWindow
    {
        SequenceAssemblyInspector m_CachedEditor;
        bool m_PlayMode;

        void OnEnable()
        {
            titleContent = new GUIContent("Sequence Assembly", IconUtility.LoadIcon("MasterSequence/Shot", IconUtility.IconType.UniqueToSkin));
            minSize = new Vector2(250.0f,  230.0f); // Minimum window size

            m_PlayMode = EditorApplication.isPlayingOrWillChangePlaymode;
            BuildUI();

            EditorApplication.playModeStateChanged += Rebuild;
        }

        void OnDisable()
        {
            SelectionUtility.playableDirectorChanged -= ShowSelection;
            EditorApplication.playModeStateChanged -= Rebuild;
            ClearView();
        }

        void Rebuild(PlayModeStateChange stateChange)
        {
            rootVisualElement.Clear();

            if (stateChange == PlayModeStateChange.EnteredPlayMode)
            {
                m_PlayMode = true;
            }
            else if (stateChange == PlayModeStateChange.EnteredEditMode)
            {
                m_PlayMode = false;
            }
            BuildUI();
        }

        void BuildUI()
        {
            SelectionUtility.playableDirectorChanged += ShowSelection;
            ShowSelection();
        }

        void ShowSelection()
        {
            var director = SelectionUtility.activePlayableDirector;
            if (director == null || director.playableAsset == null)
                return;

            // The new PlayableDirector selected is already shown or is not the one of a Sequence so there is no
            // need to change the view.
            if (IsAlreadyShown(director) || director.gameObject.GetComponent<SequenceFilter>() == null)
                return;

            ClearView();
            CreateView(director);
        }

        bool IsAlreadyShown(PlayableDirector target)
        {
            return (m_CachedEditor && m_CachedEditor.target == target);
        }

        void CreateView(PlayableDirector data)
        {
            if (!m_PlayMode)
                m_CachedEditor = SequenceAssemblyInspector.CreateEditor(data, typeof(SequenceAssemblyInspector)) as SequenceAssemblyInspector;
            else
                m_CachedEditor = SequenceAssemblyInspector.CreateEditor(data, typeof(SequenceAssemblyPlayModeInspector)) as SequenceAssemblyPlayModeInspector;

            rootVisualElement.Add(m_CachedEditor.CreateInspectorGUI());
        }

        void ClearView()
        {
            if (m_CachedEditor)
            {
                DestroyImmediate(m_CachedEditor);
                rootVisualElement.Clear();
            }
        }

        void OnFocus()
        {
            if (m_CachedEditor != null)
                m_CachedEditor.SelectPlayableDirector();
        }

        void OnHierarchyChange()
        {
            // This callback is called even when nothing else than the selection changed in the Hierarchy.
            // This means that `m_CachedEditor.Refresh` is called way too often. This causes UX discomfort in some
            // selection scenario.
            if (m_CachedEditor != null)
            {
                var director = SelectionUtility.activePlayableDirector;
                if (director == null)
                    ClearView();
                else
                    m_CachedEditor.Refresh();
            }
        }
    }
}
