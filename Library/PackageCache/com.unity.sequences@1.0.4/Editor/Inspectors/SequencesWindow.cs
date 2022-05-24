using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Sequences
{
    internal class SequencesWindow : EditorWindow
    {
        static readonly string k_UXMLFilePath = "Packages/com.unity.sequences/Editor/UI/SequencesWindow.uxml";

        class Styles
        {
            public static readonly string k_StructureContentViewPath = "structure_content";
            public static readonly string k_AssetCollectionsContentViewPath = "asset_collections_content";
            public static readonly string k_SequencesWindowAddDropdownViewPath = "add_dropdown";
        }

        [SerializeField]
        TreeViewState m_State = new TreeViewState();

        StructureTreeView m_Structure;

        [SerializeField]
        TreeViewState m_AssetCollectionsState =  new TreeViewState();

        AssetCollectionsTreeView m_AssetCollectionsTreeView;

        IMGUIContainer m_StructureTreeViewContainer;
        IMGUIContainer m_AssetCollectionsTreeViewContainer;

        SequencesWindowAddMenu m_MainMenu;

        internal StructureTreeView structureTreeView => m_Structure;

        void OnEnable()
        {
            titleContent = new GUIContent("Sequences", IconUtility.LoadIcon("MasterSequence/MasterSequence", IconUtility.IconType.UniqueToSkin));

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Minimum window size
            minSize = new Vector2(200.0f,  250.0f);

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_UXMLFilePath);
            visualTree.CloneTree(root);

            // Set style
            StyleSheetUtility.SetStyleSheets(root);
            StyleSheetUtility.SetIcon(root.Q<Label>(null, "seq-create-add-new"), "CreateAddNew");
            StyleSheetUtility.SetIcon(root.Q<Label>(null, "seq-dropdown"), "icon dropdown");

            // Header, search
            Button addDropdownButton = root.Q<Button>(Styles.k_SequencesWindowAddDropdownViewPath);
            addDropdownButton.clicked += OnAddMenuClicked;

            // Hierarchy
            m_StructureTreeViewContainer = root.Q<IMGUIContainer>(Styles.k_StructureContentViewPath);
            m_Structure = new StructureTreeView(m_State, m_StructureTreeViewContainer);
            m_StructureTreeViewContainer.onGUIHandler = m_Structure.OnGUI;

            // Asset Collections
            m_AssetCollectionsTreeViewContainer = root.Q<IMGUIContainer>(Styles.k_AssetCollectionsContentViewPath);
            m_AssetCollectionsTreeView = new AssetCollectionsTreeView(m_AssetCollectionsState, m_AssetCollectionsTreeViewContainer);
            m_AssetCollectionsTreeViewContainer.onGUIHandler = m_AssetCollectionsTreeView.OnGUI;

            // Popup menus
            m_MainMenu = new SequencesWindowAddMenu();
            m_MainMenu.userClickedOnCreateMasterSequence += m_Structure.CreateNewMasterSequence;
            m_MainMenu.userClickedOnCreateSequenceAsset += m_AssetCollectionsTreeView.CreateSequenceAssetInContext;
        }

        void OnAddMenuClicked()
        {
            m_Structure.ForceEndRename();
            m_AssetCollectionsTreeView.ForceEndRename();

            m_MainMenu.Show();
        }

        void OnDestroy()
        {
            m_Structure.Unload();
            m_AssetCollectionsTreeView.Unload();
        }
    }
}
