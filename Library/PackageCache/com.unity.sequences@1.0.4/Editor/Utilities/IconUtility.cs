using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityEditor.Sequences
{
    internal static class IconUtility
    {
        /// <summary>
        /// Store previously loaded icons in this cache for re-usage.
        /// </summary>
        static Dictionary<string, Texture2D> s_CachedIcons = new Dictionary<string, Texture2D>();

        static string k_DefaultCollectionTypeIconName = "CustomType";

        /// <summary>
        /// Indicates if the icon is unique to a skin or common to all.
        /// </summary>
        public enum IconType
        {
            UniqueToSkin,
            CommonToAllSkin
        }

        /// <summary>
        /// Load Icon from Editor Default Resources.
        /// File must contain extension.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Texture2D LoadIcon(string path, IconType type)
        {
            if (string.IsNullOrEmpty(path))
                throw new System.NullReferenceException("path");

            Texture2D icon = null;
            if (s_CachedIcons.TryGetValue(path, out icon))
                return icon;

            string fullPath = BuildFullPath(path, type);

            Texture2D loadedIcon = EditorGUIUtility.Load(fullPath) as Texture2D;

            if (loadedIcon == null)
                return null;

            s_CachedIcons.Add(path, loadedIcon);

            return icon;
        }

        static string BuildFullPath(string path, IconType type)
        {
            string fullIconPath = PackageUtility.editorResourcesFolder;

            string typeFolder = (type == IconType.UniqueToSkin) ? EditorGUIUtility.isProSkin ? "Dark" : "Light" : "Common";

            fullIconPath = Path.Combine(fullIconPath, "Icons");
            fullIconPath = Path.Combine(fullIconPath, typeFolder);
            fullIconPath = Path.Combine(fullIconPath, path);
            fullIconPath += (EditorGUIUtility.pixelsPerPoint > 1.0f) ? "@2x" : "";
            fullIconPath += ".png";

            return fullIconPath;
        }

        /// <summary>
        /// Try to load the associated icon for the given Collection type name.
        /// If it cannot find any valid icon, it returns a generic icon.
        /// </summary>
        /// <param name="name">Collection type name as shown in the Asset Collections tree view.</param>
        /// <returns></returns>
        public static Texture2D LoadAssetCollectionIcon(string name, IconType type)
        {
            if (string.IsNullOrEmpty(name))
                name = k_DefaultCollectionTypeIconName;

            Texture2D icon;

            icon = LoadIcon(Path.Combine("CollectionType", name), type);
            if (icon == null)
                icon = LoadIcon(Path.Combine("CollectionType", k_DefaultCollectionTypeIconName), type);

            return icon;
        }

        public static Texture2D LoadPrefabIcon(PrefabAssetType prefabType)
        {
            var iconName = $"Prefab{prefabType.ToString()} Icon";
            iconName = iconName.Replace("Regular", "");
            return LoadEditorIcon(iconName);
        }

        public static Texture2D LoadEditorIcon(string iconName)
        {
            if (EditorGUIUtility.isProSkin)
                iconName = $"d_{iconName}";

            return (Texture2D)EditorGUIUtility.IconContent(iconName).image;
        }
    }
}
