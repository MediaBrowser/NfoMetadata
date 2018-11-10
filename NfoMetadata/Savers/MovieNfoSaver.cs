﻿using MediaBrowser.Controller.Configuration;
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
using MediaBrowser.Model.Xml;

namespace NfoMetadata.Savers
{
    public class MovieNfoSaver : BaseNfoSaver
    {
        protected override string GetLocalSavePath(BaseItem item)
        {
            var paths = GetMovieSavePaths(new ItemInfo(item), FileSystem);
            return paths.Count == 0 ? null : paths[0];
        }

        public static List<string> GetMovieSavePaths(ItemInfo item, IFileSystem fileSystem)
        {
            var list = new List<string>();

            var isDvd = string.Equals(item.Container, MediaContainer.Dvd, StringComparison.OrdinalIgnoreCase);

            if (isDvd)
            {
                var path = item.ContainingFolderPath;

                list.Add(Path.Combine(path, "VIDEO_TS", "VIDEO_TS.nfo"));
            }

            if (isDvd || string.Equals(item.Container, MediaContainer.Bluray, StringComparison.OrdinalIgnoreCase))
            {
                var path = item.ContainingFolderPath;

                list.Add(Path.Combine(path, Path.GetFileName(path) + ".nfo"));
            }
            else
            {
                // http://kodi.wiki/view/NFO_files/Movies
                // movie.nfo will override all and any .nfo files in the same folder as the media files if you use the "Use foldernames for lookups" setting. If you don't, then moviename.nfo is used
                //if (!item.IsInMixedFolder && item.ItemType == typeof(Movie))
                //{
                //    list.Add(Path.Combine(item.ContainingFolderPath, "movie.nfo"));
                //}

                list.Add(Path.ChangeExtension(item.Path, ".nfo"));

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
            if (!item.SupportsLocalMetadata)
            {
                return false;
            }

            var video = item as Video;

            // Check parent for null to avoid running this against things like video backdrops
            if (video != null && !(item is Episode) && !video.ExtraType.HasValue)
            {
                return updateType >= MinimumUpdateType;
            }

            return false;
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
                foreach (var artist in musicVideo.Artists)
                {
                    writer.WriteElementString("artist", artist);
                }
                if (!string.IsNullOrEmpty(musicVideo.Album))
                {
                    writer.WriteElementString("album", musicVideo.Album);
                }
            }

            var movie = item as Movie;

            if (movie != null)
            {
                if (!string.IsNullOrEmpty(movie.CollectionName))
                {
                    writer.WriteElementString("set", movie.CollectionName);
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
                "set",
                "id"
            });
            return list;
        }

        public MovieNfoSaver(IFileSystem fileSystem, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger, IXmlReaderSettingsFactory xmlReaderSettingsFactory) : base(fileSystem, configurationManager, libraryManager, userManager, userDataManager, logger, xmlReaderSettingsFactory)
        {
        }
    }
}
