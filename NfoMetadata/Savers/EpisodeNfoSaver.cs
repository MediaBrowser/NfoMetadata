﻿using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using NfoMetadata.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;

namespace NfoMetadata.Savers
{
    public class EpisodeNfoSaver : BaseNfoSaver
    {
        protected override string GetLocalSavePath(BaseItem item)
        {
            return Path.ChangeExtension(item.Path, ".nfo");
        }

        protected override string GetRootElementName(BaseItem item)
        {
            return "episodedetails";
        }

        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            if (!item.SupportsLocalMetadata)
            {
                return false;
            }

            return item is Episode && updateType >= MinimumUpdateType;
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer)
        {
            var episode = (Episode)item;

            if (episode.IndexNumber.HasValue)
            {
                writer.WriteElementString("episode", episode.IndexNumber.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (episode.IndexNumberEnd.HasValue)
            {
                writer.WriteElementString("episodenumberend", episode.IndexNumberEnd.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (episode.ParentIndexNumber.HasValue)
            {
                writer.WriteElementString("season", episode.ParentIndexNumber.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (episode.PremiereDate.HasValue)
            {
                var formatString = ConfigurationManager.GetNfoConfiguration().ReleaseDateFormat;

                writer.WriteElementString("aired", episode.PremiereDate.Value.LocalDateTime.ToString(formatString));
            }

            if (!episode.ParentIndexNumber.HasValue || episode.ParentIndexNumber.Value == 0)
            {
                if (episode.AirsAfterSeasonNumber.HasValue && episode.AirsAfterSeasonNumber.Value != -1)
                {
                    writer.WriteElementString("airsafter_season", episode.AirsAfterSeasonNumber.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (episode.AirsBeforeEpisodeNumber.HasValue && episode.AirsBeforeEpisodeNumber.Value != -1)
                {
                    writer.WriteElementString("airsbefore_episode", episode.AirsBeforeEpisodeNumber.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (episode.AirsBeforeSeasonNumber.HasValue && episode.AirsBeforeSeasonNumber.Value != -1)
                {
                    writer.WriteElementString("airsbefore_season", episode.AirsBeforeSeasonNumber.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (episode.AirsBeforeEpisodeNumber.HasValue && episode.AirsBeforeEpisodeNumber.Value != -1)
                {
                    writer.WriteElementString("displayepisode", episode.AirsBeforeEpisodeNumber.Value.ToString(CultureInfo.InvariantCulture));
                }

                var specialSeason = episode.AiredSeasonNumber;
                if (specialSeason.HasValue && specialSeason.Value != -1)
                {
                    writer.WriteElementString("displayseason", specialSeason.Value.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        protected override List<string> GetTagsUsed(BaseItem item)
        {
            var list = base.GetTagsUsed(item);
            list.AddRange(new string[]
            {
                "aired",
                "season",
                "episode",
                "episodenumberend",
                "airsafter_season",
                "airsbefore_episode",
                "airsbefore_season",
                "displayseason",
                "displayepisode"
            });
            return list;
        }

        public EpisodeNfoSaver(IFileSystem fileSystem, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
        }
    }
}