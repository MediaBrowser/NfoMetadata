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

            return paths.Select(p => System.IO.Path.Combine(p.Directory, p.FileName)).FirstOrDefault();
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
