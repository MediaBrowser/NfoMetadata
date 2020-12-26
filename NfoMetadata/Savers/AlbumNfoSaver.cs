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

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer)
        {
            var album = (MusicAlbum)item;

            foreach (var artist in album.Artists)
            {
                writer.WriteElementString("artist", artist.Name);
            }

            foreach (var artist in album.AlbumArtists)
            {
                writer.WriteElementString("albumartist", artist.Name);
            }

            var tracks = album.GetTracks(new InternalItemsQuery()
            {
                EnableTotalRecordCount = false

            }).Items;

            AddTracks(tracks, writer);
        }

        private void AddTracks(BaseItem[] tracks, XmlWriter writer)
        {
            foreach (var track in tracks)
            {
                writer.WriteStartElement("track");

                if (track.IndexNumber.HasValue)
                {
                    writer.WriteElementString("position", track.IndexNumber.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (!string.IsNullOrEmpty(track.Name))
                {
                    writer.WriteElementString("title", track.Name);
                }

                if (track.RunTimeTicks.HasValue)
                {
                    var time = TimeSpan.FromTicks(track.RunTimeTicks.Value).ToString(@"mm\:ss");

                    writer.WriteElementString("duration", time);
                }

                writer.WriteEndElement();
            }
        }

        protected override List<string> GetTagsUsed(BaseItem item)
        {
            var list = base.GetTagsUsed(item);
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
