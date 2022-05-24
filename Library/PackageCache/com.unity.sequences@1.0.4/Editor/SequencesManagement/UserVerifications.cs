using UnityEngine;
using UnityEngine.Sequences;

namespace UnityEditor.Sequences
{
    internal static class UserVerifications
    {
        /// <summary>
        /// Skips the Editor popups asking users for confirmation.
        /// Set it to True when methods are called from automation.
        /// </summary>
        internal static bool skipUserVerification = false;

        internal static bool ValidateSequenceDeletion(Sequence deletedSequence)
        {
            if (skipUserVerification)
                return true;

            var deleteAssets = EditorUtility.DisplayDialog(
                "Sequence deletion",
                $"Do you want to delete the \"{deletedSequence.name}\" Sequence and its children?\n\n" +
                "You cannot undo this action.",
                "Delete",
                "Cancel"
            );

            return deleteAssets;
        }

        internal static bool ValidateSequenceAssetDeletion(GameObject deletedSequenceAsset)
        {
            if (skipUserVerification)
                return true;

            var hasVariantMessage = "";
            if (SequenceAssetUtility.HasVariants(deletedSequenceAsset))
                hasVariantMessage = " and its Variants";

            var deleteAssets = EditorUtility.DisplayDialog(
                "Sequence Asset deletion",
                $"Do you want to delete the \"{deletedSequenceAsset.name}\" Sequence Asset{hasVariantMessage}?\n\n" +
                "You cannot undo this action.",
                "Delete",
                "Cancel"
            );

            return deleteAssets;
        }

        internal static bool ValidateInstanceChange(GameObject instance)
        {
            if (!PrefabUtility.HasPrefabInstanceAnyOverrides(instance, false))
                return true;

            // Deactivating an instance counts as an override, so ignore that case
            bool hasNonDefaultOverrides = false;
            if (PrefabUtility.GetAddedComponents(instance).Count > 0 ||
                PrefabUtility.GetAddedGameObjects(instance).Count > 0)
            {
                hasNonDefaultOverrides = true;
            }
            else
            {
                foreach (var modification in PrefabUtility.GetPropertyModifications(instance))
                {
                    if (!PrefabUtility.IsDefaultOverride(modification) &&
                        // m_InitialState is controlled by "Play on Awake" that is changed when the playable director is
                        // targeted by a SequenceAsset clip (i.e. it's a nested timeline).
                        !modification.propertyPath.Equals("m_InitialState"))
                    {
                        hasNonDefaultOverrides = true;
                    }
                }
            }

            if (!hasNonDefaultOverrides)
                return true;

            if (skipUserVerification)
                return true;

            var result = EditorUtility.DisplayDialogComplex(
                "Sequence Asset instance has been modified",
                $"Do you want to save the changes you made on \"{instance.name}\"?\n\n" +
                "Your changes will be lost if you don't save them.",
                "Save Changes",
                "Cancel",
                "Discard Changes"
            );

            switch (result)
            {
                case 0:  // Apply Overrides
                    PrefabUtility.ApplyPrefabInstance(
                        instance,
                        InteractionMode.AutomatedAction);

                    return true;

                case 1:  // Cancel
                    return false;

                case 2:  // Discard Changes
                    return true;

                default:
                    return false;
            }
        }
    }
}
