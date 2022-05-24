using System.Collections.Generic;
using System.IO;
using UnityEngine.UIElements;

namespace UnityEditor.Sequences
{
    internal static class StyleSheetUtility
    {
        static readonly string k_StyleSheetFolderPath =
            Path.Combine("Packages", "com.unity.sequences", "Editor", "UI");

        static readonly string k_CommonFileName = "Common";
        static readonly string k_CommonLightFileName = "CommonLight";
        static readonly string k_CommonDarkFileName = "CommonDark";

        public static StyleSheet commonStyleSheet => GetStyleSheet(k_CommonFileName);
        public static StyleSheet commonLightStyleSheet => GetStyleSheet(k_CommonLightFileName);
        public static StyleSheet commonDarkStyleSheet => GetStyleSheet(k_CommonDarkFileName);

        public static StyleSheet GetStyleSheet(string name)
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(Path.Combine(k_StyleSheetFolderPath, $"{name}.uss"));
        }

        public static IEnumerable<StyleSheet> GetStyleSheets(
            string ussName = null,
            string ussDarkName = null,
            string ussLightName = null)
        {
            yield return commonStyleSheet;

            if (EditorGUIUtility.isProSkin)
                yield return commonDarkStyleSheet;
            else
                yield return commonLightStyleSheet;

            if (ussName == null)
                yield break;

            var customStyle = GetStyleSheet(ussName);
            if (customStyle != null)
                yield return customStyle;

            if (EditorGUIUtility.isProSkin)
            {
                var darkCustomStyle = GetStyleSheet(ussDarkName ?? ussName + "Dark");
                if (darkCustomStyle != null)
                    yield return darkCustomStyle;
            }
            else
            {
                var lightCustomStyle = GetStyleSheet(ussLightName ?? ussName + "Light");
                if (lightCustomStyle != null)
                    yield return lightCustomStyle;
            }
        }

        public static void SetStyleSheets(
            VisualElement visualElement,
            string ussName = null,
            string ussDarkName = null,
            string ussLightName = null)
        {
            foreach (var styleSheet in GetStyleSheets(ussName, ussDarkName, ussLightName))
            {
                visualElement.styleSheets.Add(styleSheet);
            }
        }

        public static void SetIcon(VisualElement visualElement, string iconName)
        {
            visualElement.style.backgroundImage = new StyleBackground(IconUtility.LoadEditorIcon(iconName));
        }
    }
}
