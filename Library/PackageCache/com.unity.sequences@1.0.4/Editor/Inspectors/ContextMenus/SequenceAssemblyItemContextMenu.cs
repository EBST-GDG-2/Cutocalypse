using UnityEngine;

#if UNITY_2022_1_OR_NEWER
using PopupField = UnityEngine.UIElements.PopupField<UnityEngine.GameObject>;
#else
using PopupField = UnityEditor.UIElements.PopupField<UnityEngine.GameObject>;
#endif

namespace UnityEditor.Sequences
{
    /// <summary>
    /// The context menu for each Sequence Asset item of the Sequence Assembly window.
    /// </summary>
    class SequenceAssemblyItemContextMenu
    {
        static SequenceAssemblyItemContextMenu m_Instance;

        public static SequenceAssemblyItemContextMenu instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new SequenceAssemblyItemContextMenu();

                return m_Instance;
            }
        }

        GenericMenu m_Menu;
        GameObject variant { get; set; }
        GameObject source { get; set; }
        PopupField variantSelector { get; set; }

        public void Show(PopupField selector, GameObject target)
        {
            Initialize(selector, target);
            m_Menu = new GenericMenu();

            m_Menu.AddItem(new GUIContent("Create new variant"), false, CreateNewVariant);

            if (source == target)
            {
                m_Menu.AddDisabledItem(new GUIContent("Duplicate current variant"));
                m_Menu.AddSeparator("");
                m_Menu.AddDisabledItem(new GUIContent("Delete current variant"));
            }
            else
            {
                m_Menu.AddItem(new GUIContent("Duplicate current variant"), false, DuplicateVariant);
                m_Menu.AddSeparator("");
                m_Menu.AddItem(new GUIContent("Delete current variant"), false, DeleteVariant);
            }

            m_Menu.ShowAsContext();
        }

        void Initialize(PopupField selector, GameObject newTarget)
        {
            variant = newTarget;
            source = SequenceAssetUtility.GetSource(newTarget);
            variantSelector = selector;
        }

        void CreateNewVariant()
        {
            SequenceAssetUtility.CreateVariant(source);
        }

        void DuplicateVariant()
        {
            SequenceAssetUtility.DuplicateVariant(variant);
        }

        void DeleteVariant()
        {
            if (!UserVerifications.ValidateSequenceAssetDeletion(variant))
                return;

            variantSelector.value = source;
            SequenceAssetUtility.DeleteVariantAsset(variant);
        }
    }
}
