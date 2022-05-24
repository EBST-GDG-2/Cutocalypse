using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    class SceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty sceneAssetProperty = property.FindPropertyRelative("m_SceneAsset");

            using (new EditorGUILayout.HorizontalScope())
            {
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    var selectedObject = EditorGUI.ObjectField(position, label, sceneAssetProperty.objectReferenceValue, typeof(SceneAsset), false);
                    if (change.changed)
                    {
                        sceneAssetProperty.objectReferenceValue = selectedObject;
                        TimelineEditor.Refresh(RefreshReason.ContentsModified);
                    }
                }

                var scenePath = sceneAssetProperty.objectReferenceValue == null ? string.Empty : AssetDatabase.GetAssetPath(sceneAssetProperty.objectReferenceValue);
                using (new EditorGUI.DisabledScope(SceneManagement.IsLoaded(scenePath)))
                {
                    if (GUILayout.Button("Load", GUILayout.MaxWidth(60)))
                        SceneManagement.OpenScene(scenePath);
                }

                using (new EditorGUI.DisabledScope(!SceneManagement.IsLoaded(scenePath)))
                {
                    if (GUILayout.Button("Unload", GUILayout.MaxWidth(60)))
                        SceneManagement.CloseScene(scenePath);
                }
            }
        }
    }
}
