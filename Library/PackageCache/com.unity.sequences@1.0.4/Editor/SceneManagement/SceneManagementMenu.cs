using System.IO;
using UnityEngine;
using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    internal class SceneManagementMenu
    {
        internal class ContextInfo
        {
            public MasterSequence masterSequence;
            public TimelineSequence sequence;
        }

        internal static void AppendMenuFrom(ContextInfo context, GenericMenu destinationMenu)
        {
            if (context.sequence.HasScenes() && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                destinationMenu.AddItem(new GUIContent("Load Scenes"), false, LoadAllScenes, context);
            }
            else
            {
                destinationMenu.AddDisabledItem(new GUIContent("Load Scenes"));
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                destinationMenu.AddDisabledItem(new GUIContent("Load specific Scene"));
            }
            else
            {
                foreach (string path in context.sequence.GetRelatedScenes())
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    if (SceneManagement.IsLoaded(path))
                    {
                        destinationMenu.AddDisabledItem(new GUIContent(string.Format("Load specific Scene/{0}", fileName)),
                            true);
                    }
                    else
                    {
                        destinationMenu.AddItem(new GUIContent(string.Format("Load specific Scene/{0}", fileName)), false,
                            LoadScene, path);
                    }
                }
            }


            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                destinationMenu.AddDisabledItem(new GUIContent("Create Scene..."), false);
            }
            else
            {
                destinationMenu.AddItem(new GUIContent("Create Scene..."), false, AddNewScene, context);
            }
        }

        static void LoadScene(object pathObject)
        {
            string path = pathObject as string;
            SceneManagement.OpenScene(path, true);
        }

        static void LoadAllScenes(object contextObject)
        {
            ContextInfo context = contextObject as ContextInfo;
            SceneManagement.OpenAllScenes(context.sequence, true);
        }

        static void AddNewScene(object contextObject)
        {
            ContextInfo context = contextObject as ContextInfo;
            SceneManagement.AddNewScene(context.sequence);
        }
    }
}
