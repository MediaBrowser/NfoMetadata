using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using NfoMetadata.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Configuration;

namespace NfoMetadata.Savers
{
    public class ArtistNfoSaver : BaseNfoSaver
    {
        public ArtistNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, libraryMonitor, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
        }

        protected override string GetSavePath(BaseItem item, LibraryOptions libraryOptions)
        {
            var artist = item as MusicArtist;

            var path = artist.GetMediaContainingFolderPath(libraryOptions);

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return Path.Combine(path, "artist.nfo");
        }

        protected override string GetRootElementName(BaseItem item)
        {
            return "artist";
        }

        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            return item is MusicArtist && updateType >= MinimumUpdateType;
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer)
        {
            var artist = (MusicArtist)item;

            if (artist.EndDate.HasValue)
            {
                var formatString = ConfigurationManager.GetNfoConfiguration().ReleaseDateFormat;

                writer.WriteElementString("disbanded", artist.EndDate.Value.LocalDateTime.ToString(formatString));
            }

            var albums = LibraryManager.GetItemList(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { typeof(MusicAlbum).Name },
                AlbumArtistIds = new[] { artist.InternalId },
                OrderBy = new[] { new ValueTuple<string, SortOrder>(ItemSortBy.SortName, SortOrder.Ascending) }
            });

            AddAlbums(albums, writer);
        }
        
        private void AddAlbums(BaseItem[] albums, XmlWriter writer)
        {
            foreach (var album in albums)
            {
                writer.WriteStartElement("album");

                if (!string.IsNullOrEmpty(album.Name))
                {
                    writer.WriteElementString("title", album.Name);
                }

                if (album.ProductionYear.HasValue)
                {
                    writer.WriteElementString("year", album.ProductionYear.Value.ToString(CultureInfo.InvariantCulture));
                }

                writer.WriteEndElement();
            }
        }

        protected override List<string> GetTagsUsed(BaseItem item, XbmcMetadataOptions options)
        {
            var list = base.GetTagsUsed(item, options);
            list.AddRange(new string[]
            {
                "album",
                "disbanded"
            });
            return list;
        }
    }
}