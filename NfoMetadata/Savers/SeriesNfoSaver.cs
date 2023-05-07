using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Serialization;

namespace NfoMetadata.Savers
{
    public class SeriesNfoSaver : BaseNfoSaver
    {
        private readonly IJsonSerializer JsonSerializer;

        public SeriesNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger, IJsonSerializer jsonSerializer) : base(fileSystem, libraryMonitor, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
            JsonSerializer = jsonSerializer;
        }

        protected override string GetSavePath(BaseItem item, LibraryOptions libraryOptions)
        {
            return Path.Combine(item.Path, "tvshow.nfo");
        }

        protected override string GetRootElementName(BaseItem item)
        {
            return "tvshow";
        }

        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            if (!item.IsFileProtocol)
            {
                return false;
            }

            return item is Series && updateType >= MinimumUpdateType;
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer)
        {
            var series = (Series)item;

            if (item.ProviderIds.Count > 0)
            {
                writer.WriteElementString("episodeguide", JsonSerializer.SerializeToString(item.ProviderIds).ToLowerInvariant());
            }

            var tvdb = item.GetProviderId(MetadataProviders.Tvdb);

            // TODO: Deprecate once most other tools are no longer using this
            if (!string.IsNullOrEmpty(tvdb))
            {
                writer.WriteElementString("id", tvdb);
            }

            writer.WriteElementString("season", "-1");
            writer.WriteElementString("episode", "-1");

            writer.WriteElementString("displayorder", series.DisplayOrder.ToString().ToLowerInvariant());

            if (series.Status.HasValue)
            {
                writer.WriteElementString("status", series.Status.Value.ToString());
            }
        }

        protected override List<string> GetTagsUsed(BaseItem item)
        {
            var list = base.GetTagsUsed(item);
            list.AddRange(new string[]
            {
                "id",
                "episodeguide",
                "season",
                "episode",
                "status",
                "displayorder"
            });
            return list;
        }
    }
}
