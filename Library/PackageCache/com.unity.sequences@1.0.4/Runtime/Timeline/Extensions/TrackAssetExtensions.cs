using System.Linq;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace UnityEngine.Sequences.Timeline
{
    /// <summary>
    /// A collection of helpers to manipulate <see cref="TrackAsset"/> instances.
    /// </summary>
    public static class TrackAssetExtensions
    {
        /// <summary>
        /// Gets a child <see cref="TrackAsset"/> whose name matches the specified string.
        /// Creates a new instance when no matching track is found.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="TrackAsset"/> to look for.</typeparam>
        /// <param name="asset">The instance of <see cref="TrackAsset"/> this method applies to.</param>
        /// <param name="name">The name of the <see cref="TrackAsset"/> instance to look for.</param>
        /// <returns>A valid instance of <see cref="TrackAsset"/>.</returns>
        public static T GetOrCreateSubTrackByName<T>(this TrackAsset asset, string name)
            where T : TrackAsset, new()
        {
            foreach (var item in asset.GetChildTracks())
            {
                if (item is T && item.name == name)
                    return item as T;
            }
            return asset.timelineAsset.CreateTrack<T>(asset, name);
        }

        /// <summary>
        ///  Gets the first <see cref="TimelineClip"/> found in this instance of <see cref="TrackAsset"/>.
        ///  Creates a new clip instance if no matching type is found.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="TimelineClip"/> to look for.</typeparam>
        /// <param name="track">The instance of <see cref="TrackAsset"/> this method applies to.</param>
        /// <returns>A valid instance of <see cref="TimelineClip"/>.</returns>
        public static T GetOrCreateFirstAssetClipOfType<T>(this TrackAsset track)
            where T : PlayableAsset
        {
            foreach (var clip in track.GetClips())
            {
                if (clip.asset is T)
                    return clip.asset as T;
            }

            return track.CreateClip<T>().asset as T;
        }

        /// <summary>
        /// Gets the first clip found in this instance of <paramref name="track"/>.
        /// </summary>
        /// <param name="track">The instance of <see cref="TrackAsset"/> this method applies to.</param>
        /// <returns>Null if there is no clip in this track.</returns>
        public static TimelineClip GetFirstClip(this TrackAsset track)
        {
            return track.GetClips().FirstOrDefault();
        }

        /// <summary>
        /// Gets the first clip whose display name matches the specified string.
        /// </summary>
        /// <param name="track">The instance of <see cref="TrackAsset"/> this method applies to.</param>
        /// <param name="name">The display name of the clip to look for.</param>
        /// <returns>Null if no matching clip is found.</returns>
        public static TimelineClip GetFirstClipWithName(this TrackAsset track, string name)
        {
            return track.GetClips().FirstOrDefault(clip => clip.displayName == name);
        }

        /// <summary>
        /// Gets the binding to this instance of <see cref="TrackAsset"/> from the specified <paramref name="director"/> and casts it to the specified type T.
        /// </summary>
        /// <typeparam name="T">The type used to cast the object to.</typeparam>
        /// <param name="track"><see cref="TrackAsset"/> instance to look for in the <paramref name="director"/> instance.</param>
        /// <param name="director">The instance of <see cref="PlayableDirector"/> where to look for the track instance.</param>
        /// <returns>Null when no binding is found.</returns>
        public static T GetBinding<T>(this TrackAsset track, PlayableDirector director) where T : Object
        {
            if (!track.outputs.Any())
                return null;

            return director.GetGenericBinding(track) as T;
        }

        /// <summary>
        /// Gets the generated binding name for the specified <paramref name="director"/> from <paramref name="track"/>.
        /// </summary>
        /// <param name="track">The instance of <see cref="TrackAsset"/> this method applies to.</param>
        /// <param name="director">The instance of <see cref="PlayableDirector"/> this methods will the binding name of.</param>
        /// <returns>Returns the track's name when no binding is found.</returns>
        public static string GetBindingName(this TrackAsset track, PlayableDirector director)
        {
            var binding = track.GetBinding<Object>(director);
            return binding == null ? track.name : binding.name;
        }
    }
}
