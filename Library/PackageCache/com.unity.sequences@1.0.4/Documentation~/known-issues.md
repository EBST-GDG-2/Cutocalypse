# Known issues and limitations

This page lists some known issues and limitations that you might experience with the Sequences package. It also gives basic instructions to help you work around them when possible.

#### Functionality limitations when converting a Sequence into a Prefab

Unity doesn’t prevent you from converting a Sequence GameObject into a Prefab. However, it is currently not recommended to do it. Such a conversion might bring unwanted side-effects in your workflow, due to the inherent behaviors of Unity Prefabs that are not yet supported in Sequences. For example, you can’t use the Sequence Assembly window to remove Sequence Assets from a Prefab-converted Sequence.

#### Limited Editorial clip manipulation in Timeline

Some Editorial clip manipulations in Timeline are not recommended as they might cause binding loss. For example: moving an Editorial clip from one track to another, or manually removing an Editorial clip from its track.

#### Game view not always updated on Sequence Asset Variant swap

When you swap Variants of a Sequence Asset that is currently framed in the Game view, you might not always be able to see the expected visual result of the swap.

To see the actual result of the Variant swap, you need to slightly scrub the playhead in the Timeline window.
