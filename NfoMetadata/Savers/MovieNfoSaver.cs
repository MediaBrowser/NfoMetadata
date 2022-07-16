using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;
using NfoMetadata.Providers;

namespace NfoMetadata.Savers
{
    public class MovieNfoSaver : BaseNfoSaver
    {
        public MovieNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, libraryMonitor, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
        }

        protected override string GetSavePath(BaseItem item, LibraryOptions libraryOptions)
        {
            var paths = GetMovieSavePaths(new ItemInfo(item), FileSystem);
            return paths.Count == 0 ? null : paths[0];
        }

        public static FileSystemMetadata GetMovieNfo(ItemInfo item, IDirectoryService directoryService)
        {
            var container = item.Container.AsSpan();

            var isDvd = container.Equals(MediaContainer.Dvd.Span, StringComparison.OrdinalIgnoreCase);
            var isBluray = container.Equals(MediaContainer.Bluray.Span, StringComparison.OrdinalIgnoreCase);

            if (isDvd)
            {
                var path = item.ContainingFolderPath;

                var file = Helpers.GetFileInfo(directoryService, Path.Combine(path, "VIDEO_TS"), "VIDEO_TS.nfo");
                if (file != null)
                {
                    return file;
                }
            }
            else if (isBluray)
            {
                var path = item.ContainingFolderPath;

                var file = Helpers.GetFileInfo(directoryService, Path.Combine(path, "BDMV"), "index.nfo");
                if (file != null)
                {
                    return file;
                }
            }

            if (isDvd || isBluray)
            {
                var path = item.ContainingFolderPath;

                var file = Helpers.GetFileInfo(directoryService, path, Path.GetFileName(path) + ".nfo");
                if (file != null)
                {
                    return file;
                }
            }
            else
            {
                var path = item.Path;

                if (string.IsNullOrEmpty(path) || BaseItem.MediaSourceManager.GetPathProtocol(path.AsSpan()) != MediaBrowser.Model.MediaInfo.MediaProtocol.File)
                {
                    return null;
                }

                // http://kodi.wiki/view/NFO_files/Movies
                // movie.nfo will override all and any .nfo files in the same folder as the media files if you use the "Use foldernames for lookups" setting. If you don't, then moviename.nfo is used
                //if (!item.IsInMixedFolder && item.ItemType == typeof(Movie))
                //{
                //    list.Add(Path.Combine(item.ContainingFolderPath, "movie.nfo"));
                //}

                var file = directoryService.GetFile(Path.ChangeExtension(path, ".nfo"));
                if (file != null)
                {
                    return file;
                }

                if (!item.IsInMixedFolder)
                {
                    file = Helpers.GetFileInfo(directoryService, item.ContainingFolderPath, "movie.nfo");
                    if (file != null)
                    {
                        return file;
                    }
                }
            }

            return null;
        }

        public static List<string> GetMovieSavePaths(ItemInfo item, IFileSystem fileSystem)
        {
            var list = new List<string>();

            var container = item.Container.AsSpan();

            var isDvd = container.Equals(MediaContainer.Dvd.Span, StringComparison.OrdinalIgnoreCase);
            var isBluray = container.Equals(MediaContainer.Bluray.Span, StringComparison.OrdinalIgnoreCase);

            if (isDvd)
            {
                var path = item.ContainingFolderPath;

                list.Add(Path.Combine(path, "VIDEO_TS", "VIDEO_TS.nfo"));
            }
            else if (isBluray)
            {
                var path = item.ContainingFolderPath;

                list.Add(Path.Combine(path, "BDMV", "index.nfo"));
            }

            if (isDvd || isBluray)
            {
                var path = item.ContainingFolderPath;

                list.Add(Path.Combine(path, Path.GetFileName(path) + ".nfo"));
            }
            else
            {
                var path = item.Path;

                if (string.IsNullOrEmpty(path) || BaseItem.MediaSourceManager.GetPathProtocol(path.AsSpan()) != MediaBrowser.Model.MediaInfo.MediaProtocol.File)
                {
                    return list;
                }

                // http://kodi.wiki/view/NFO_files/Movies
                // movie.nfo will override all and any .nfo files in the same folder as the media files if you use the "Use foldernames for lookups" setting. If you don't, then moviename.nfo is used
                //if (!item.IsInMixedFolder && item.ItemType == typeof(Movie))
                //{
                //    list.Add(Path.Combine(item.ContainingFolderPath, "movie.nfo"));
                //}

                list.Add(Path.ChangeExtension(path, ".nfo"));

                if (!item.IsInMixedFolder)
                {
                    list.Add(Path.Combine(item.ContainingFolderPath, "movie.nfo"));
                }
            }

            return list;
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
            return true;
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer)
        {
            var imdb = item.GetProviderId(MetadataProviders.Imdb);

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

        protected override List<string> GetTagsUsed(BaseItem item)
        {
            var list = base.GetTagsUsed(item);
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
