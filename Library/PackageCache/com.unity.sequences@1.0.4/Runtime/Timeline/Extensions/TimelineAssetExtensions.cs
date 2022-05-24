using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.Sequences.Timeline
{
    /// <summary>
    /// A collection of helpers to manipulate <see cref="TimelineAsset"/> instances.
    /// </summary>
    public static partial class TimelineAssetExtensions
    {
        /// <summary>
        /// Gets a track of type T with the specified <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the track to look for.</typeparam>
        /// <param name="asset">The instance of <see cref="TimelineAsset"/> to look into.</param>
        /// <param name="name">The name the TrackAsset has in Timeline.</param>
        /// <returns>Null when no matching track is found.</returns>
        public static T GetTrack<T>(this TimelineAsset asset, string name) where T : TrackAsset
        {
            var allTracks = asset.GetRootTracks().Concat(asset.GetOutputTracks());
            foreach (var item in allTracks)
            {
                if (item is T && item.name == name)
                    return item as T;
            }

            return null;
        }

        /// <summary>
        /// Gets a track of type T with the specified <paramref name="name"/>.
        /// Creates a new track with the specified <paramref name="name"/> if none is found.
        /// </summary>
        /// <typeparam name="T">The type of the track to look for.</typeparam>
        /// <param name="asset">The instance of <see cref="TimelineAsset"/> to look into.</param>
        /// <param name="name">The name the TrackAsset has in Timeline.</param>
        /// <returns>A valid instance of a <see cref="TrackAsset"/> of type T.</returns>
        public static T GetOrCreateTrack<T>(this TimelineAsset asset, string name)
            where T : TrackAsset, new()
        {
            return asset.GetTrack<T>(name) ?? asset.CreateTrack<T>(name);
        }

        /// <summary>
        /// Finds a PlayableDirector in loaded scenes that references the given <see cref="TimelineAsset"/>.
        /// </summary>
        /// <param name="timeline">The instance of <see cref="TimelineAsset"/> to look for.</param>
        /// <returns>Null when no matching <see cref="PlayableDirector"/> is found.</returns>
        public static PlayableDirector FindDirector(this TimelineAsset timeline)
        {
            if (timeline == null)
                throw new System.NullReferenceException("timeline");

            PlayableDirector[] playables = Resources.FindObjectsOfTypeAll<PlayableDirector>();
            foreach (var playable in playables)
            {
                if (playable.gameObject.scene == default)
                    continue;

                if (playable.playableAsset == timeline)
                    return playable;
            }
            return null;
        }

        /// <summary>
        /// Gets a collection of <see cref="SequenceAssetPlayableAsset"/> found in this instance of TimelineAsset.
        /// </summary>
        /// <param name="timeline">The instance of <see cref="TimelineAsset"/> to look into.</param>
        /// <returns>An enumerator on <see cref="TimelineClip"/> found in this instance.</returns>
        internal static IEnumerable<TimelineClip> GetSequenceAssetClips(this TimelineAsset timeline)
        {
            foreach (var track in timeline.GetOutputTracks())
            {
                if (!(track is SequenceAssetTrack))
                    continue;

                foreach (var clip in track.GetClips())
                {
                    var clipAsset = clip.asset as SequenceAssetPlayableAsset;
                    if (clipAsset != null)
                        yield return clip;
                }
            }
        }

#if TIMELINE_1_6_0_PRE_5_OR_NEWER
        /// <summary>
        /// Gets the frame rate value assigned to this instance of <see cref="TimelineAsset"/>.
        /// </summary>
        /// <param name="timeline">The instance of <see cref="TimelineAsset"/>.</param>
        /// <returns>The frame rate value of this instance.</returns>
        internal static double GetFrameRate(this TimelineAsset timeline)
        {
            return timeline.editorSettings.frameRate;
        }

        /// <summary>
        /// Sets the frame rate value assigned to this instance of <see cref="TimelineAsset"/>.
        /// </summary>
        /// <param name="timeline">The instance of <see cref="TimelineAsset"/> this method applies to.</param>
        /// <param name="fps">The frame rate value to assign.</param>
        internal static void SetFrameRate(this TimelineAsset timeline, double fps)
        {
            timeline.editorSettings.frameRate = fps;
        }

#else
        /// <summary>
        /// Gets the frame rate value assigned to this instance of <see cref="TimelineAsset"/>.
        /// </summary>
        /// <param name="timeline">The instance of <see cref="TimelineAsset"/>.</param>
        /// <returns>The frame rate value of this instance.</returns>
        internal static float GetFrameRate(this TimelineAsset timeline)
        {
            return timeline.editorSettings.fps;
        }

        /// <summary>
        /// Sets the frame rate value assigned to this instance of <see cref="TimelineAsset"/>.
        /// </summary>
        /// <param name="timeline">The instance of <see cref="TimelineAsset"/> this method applies to.</param>
        /// <param name="fps">The frame rate value to assign.</param>
        internal static void SetFrameRate(this TimelineAsset timeline, float fps)
        {
            timeline.editorSettings.fps = fps;
        }

#endif
    }
}
