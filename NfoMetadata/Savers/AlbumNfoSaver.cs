using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Configuration;
using NfoMetadata.Configuration;

namespace NfoMetadata.Savers
{
    public class AlbumNfoSaver : BaseNfoSaver
    {
        public AlbumNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, libraryMonitor, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
        }

        protected override string GetSavePath(BaseItem item, LibraryOptions libraryOptions)
        {
            var album = item as MusicAlbum;

            var path = album.GetMediaContainingFolderPath(libraryOptions);

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return Path.Combine(path, "album.nfo");
        }

        protected override string GetRootElementName(BaseItem item)
        {
            return "album";
        }

        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            return item is MusicAlbum && updateType >= MinimumUpdateType;
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer, int nodeIndex, int numNodes)
        {
            var album = (MusicAlbum)item;

            foreach (var artist in album.ArtistItems)
            {
                writer.WriteElementString("artist", artist.Name);
            }

            foreach (var artist in album.AlbumArtistItems)
            {
                writer.WriteElementString("albumartist", artist.Name);
            }
        }

        protected override List<string> GetTagsUsed(BaseItem item, XbmcMetadataOptions options)
        {
            var list = base.GetTagsUsed(item, options);
            list.AddRange(new string[]
            {
                "track",
                "artist",
                "albumartist"
            });
            return list;
        }
    }
}
