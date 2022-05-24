namespace UnityEditor.Sequences
{
    internal abstract class SequencesWindowContextMenu<MenuType, TreeViewItemType>
        where MenuType : new()
        where TreeViewItemType : TreeViewItemBase
    {
        static MenuType m_Instance;

        public static MenuType instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new MenuType();
                return m_Instance;
            }
        }

        protected TreeViewItemType target { get; private set; }

        public abstract void Show(TreeViewItemType target);

        protected void SetTarget(TreeViewItemType newTarget)
        {
            target = newTarget;
        }

        protected void ResetTarget()
        {
            target = null;
        }
    }
}
