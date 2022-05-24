using System.Diagnostics;

namespace UnityEngine.Sequences
{
#if UNITY_2021_1_OR_NEWER
    [Conditional("UNITY_EDITOR")]
    class ComponentHelpURLAttribute : HelpURLAttribute
    {
        // Usage: [ComponentHelpURLAttribute("some-component")]
        public ComponentHelpURLAttribute(string componentTitle)
            : base(HelpURL(componentTitle)) {}

        static string HelpURL(string componentTitle)
        {
            return DocumentationInfo.baseURL +
                DocumentationInfo.version +
                DocumentationInfo.manual +
                DocumentationInfo.components +
                DocumentationInfo.ext +
                DocumentationInfo.titleRef +
                componentTitle;
        }
    }
#endif

    class DocumentationInfo
    {
        public const string baseURL = "https://docs.unity3d.com/Packages/com.unity.sequences@";
        public const string manual = "/manual/";
        public const string components = "/ref-components";
        public const string ext = ".html";
        public const string titleRef = "#";

        const string fallbackVersion = "1.0";
#if UNITY_2021_1_OR_NEWER
        public static string version
        {
            get
            {
#if UNITY_EDITOR
                UnityEditor.PackageManager.PackageInfo packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(DocumentationInfo).Assembly);
                if (packageInfo == null)
                    return fallbackVersion;

                var splitVersion = packageInfo.version.Split('.');
                return $"{splitVersion[0]}.{splitVersion[1]}";
#else
                return fallbackVersion;
#endif
            }
        }
#else
        public const string version = fallbackVersion;
#endif
    }
}
