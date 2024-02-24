using MediaBrowser.Controller.Configuration;
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
using MediaBrowser.Model.Configuration;
using System;
using System.Linq;

namespace NfoMetadata.Savers
{
    public class EpisodeNfoSaver : BaseNfoSaver
    {
        public EpisodeNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, libraryMonitor, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
        }

        protected override string GetSavePath(BaseItem item, LibraryOptions libraryOptions)
        {
            return Path.ChangeExtension(item.Path, ".nfo");
        }

        protected override string GetRootElementName(BaseItem item)
        {
            return "episodedetails";
        }

        protected override int GetNumTopLevelNodes(BaseItem item)
        {
            var start = item.IndexNumber;
            var end = ((Episode)item).IndexNumberEnd;

            if (!start.HasValue || !end.HasValue || end.Value <= start.Value)
            {
                return base.GetNumTopLevelNodes(item);
            }

            return 1 + (end.Value - start.Value);
        }

        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            if (!item.IsFileProtocol)
            {
                return false;
            }

            return item is Episode && updateType >= MinimumUpdateType;
        }

        private string GetValueToSave(string value, int nodeIndex, int numNodes, string separator, bool allowNull)
        {
            if (value == null)
            {
                return string.Empty;
            }

            //Logger.Debug("GetValueToSave {0}---{1}-{2}", value, nodeIndex, numNodes);

            var parts = value.Split(new string[] { separator }, StringSplitOptions.None);

            var newValue = parts.ElementAtOrDefault(nodeIndex);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                return allowNull ? string.Empty : value;
            }

            // for overviews
            return newValue.Trim().Trim(new[] { '-', ' ' }).Trim();
        }

        protected override string GetNameToSave(string value, int nodeIndex, int numNodes)
        {
            if (numNodes > 1)
            {
                return GetValueToSave(value, nodeIndex, numNodes, ",  ", false);
            }
            return base.GetNameToSave(value, nodeIndex, numNodes);
        }

        protected override string GetOverviewToSave(string value, int nodeIndex, int numNodes)
        {
            if (numNodes > 1)
            {
                return GetValueToSave(value, nodeIndex, numNodes, "\n\n", true);
            }
            return base.GetOverviewToSave(value, nodeIndex, numNodes);
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer, int nodeIndex, int numNodes)
        {
            var episode = (Episode)item;

            if (episode.IndexNumber.HasValue)
            {
                writer.WriteElementString("episode", (episode.IndexNumber.Value + nodeIndex).ToString(CultureInfo.InvariantCulture));
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
                if (episode.SortIndexNumber.HasValue && episode.SortIndexNumber.Value != -1)
                {
                    writer.WriteElementString("displayepisode", (episode.SortIndexNumber.Value + nodeIndex).ToString(CultureInfo.InvariantCulture));
                }

                if (episode.SortParentIndexNumber.HasValue && episode.SortParentIndexNumber.Value != -1)
                {
                    writer.WriteElementString("displayseason", episode.SortParentIndexNumber.Value.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        protected override List<string> GetTagsUsed(BaseItem item, XbmcMetadataOptions options)
        {
            var list = base.GetTagsUsed(item, options);
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
    }
}