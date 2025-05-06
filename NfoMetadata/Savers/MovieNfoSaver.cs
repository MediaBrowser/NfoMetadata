using System.Xml;
using System.Linq;
using System.Collections.Generic;

using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;

using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;

using NfoMetadata.Configuration;

namespace NfoMetadata.Savers
{
    public class MovieNfoSaver : BaseNfoSaver
    {
        public MovieNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, libraryMonitor, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
        }

        protected override string GetSavePath(BaseItem item, LibraryOptions libraryOptions)
        {
            var options = ConfigurationManager.GetNfoConfiguration();
            var paths = Helpers.GetMovieSavePaths(new ItemInfo(item), options);

            // Attempt to save back to whatever file we found, without considering the order. If we do not
            // find a file, then we use the preferred location.
            using (var enumerator = paths.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    // Hold on to the first, this will be our fallback if we do not find a file
                    var first = enumerator.Current;

                    do
                    {
                        var current = enumerator.Current;
                        var file = Helpers.GetFileInfo(FileSystem, current.Directory, current.FileName);

                        if (file != null)
                            return file.FullName;
                    }
                    while (enumerator.MoveNext());

                    // If we did not find a file, then return the first path
                    return System.IO.Path.Combine(first.Directory, first.FileName);
                }
            }

            // This is not expected to happen, but if it does, throw an exception. It means that GetMovieSavePaths
            // has a serious problem.
            throw new System.Exception("Could not determine save path");
        }

        protected override string GetRootElementName(BaseItem item)
        {
            return item is MusicVideo ? "musicvideo" : "movie";
        }

        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            if (!item.IsFileProtocol)
            {
                return false;
            }

            var video = item as Video;

            if (video != null && !(item is Episode))
            {
                var extraType = video.ExtraType;

                // Avoid running this against things like video backdrops
                if (!extraType.HasValue || IsSupportedExtraType(extraType.Value))
                {
                    return updateType >= MinimumUpdateType;
                }
            }

            return false;
        }

        private static bool IsSupportedExtraType(ExtraType type)
        {
            if (type == ExtraType.ThemeSong)
            {
                return false;
            }
            if (type == ExtraType.ThemeVideo)
            {
                return false;
            }
            if (type == ExtraType.Trailer)
            {
                return false;
            }
            if (type == ExtraType.AdditionalPart)
            {
                return false;
            }
            return true;
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer, int nodeIndex, int numNodes)
        {
            var imdb = item.GetProviderId(MetadataProviders.Imdb);

            // TODO: Deprecate once most other tools are no longer using this
            if (!string.IsNullOrEmpty(imdb))
            {
                writer.WriteElementString("id", imdb);
            }

            var musicVideo = item as MusicVideo;

            if (musicVideo != null)
            {
                foreach (var artist in musicVideo.ArtistItems)
                {
                    writer.WriteElementString("artist", artist.Name);
                }
                if (!string.IsNullOrEmpty(musicVideo.Album))
                {
                    writer.WriteElementString("album", musicVideo.Album);
                }
            }
        }

        protected override List<string> GetTagsUsed(BaseItem item, XbmcMetadataOptions options)
        {
            var list = base.GetTagsUsed(item, options);
            list.AddRange(new string[]
            {
                "album",
                "artist",
                "id"
            });
            return list;
        }
    }
}
