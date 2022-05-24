#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Sequences
{
    partial class SequenceAssemblyWindow
    {
        internal SequenceAssemblyInspector cachedEditor => m_CachedEditor;
    }

    partial class SequenceAssemblyInspector
    {
        internal List<AssetCollectionList> assetCollectionListsCache => m_AssetCollectionListsCache;
    }

    partial class SequenceAssetFoldoutItem
    {
        internal BasicSequenceAssetView basicView => m_SequenceAssetView as BasicSequenceAssetView;
        internal GameObject sourceAsset => m_SequenceAssetSource;
        internal GameObject selectedAsset => m_SequenceAssetSelected;
        internal GameObject selectedInstance => m_SequenceAssetSelectedInstance;
    }

    internal partial class BasicSequenceAssetView
    {
        internal GameObject selected => variantsSelector.value;
#if UNITY_2021_2
        internal List<GameObject> variants => variantsSelector.choices;
#endif
    }
}
#endif
