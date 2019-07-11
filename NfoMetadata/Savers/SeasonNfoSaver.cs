using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;

namespace NfoMetadata.Savers
{
    public class SeasonNfoSaver : BaseNfoSaver
    {
        public SeasonNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, libraryMonitor, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
        }

        protected override string GetLocalSavePath(BaseItem item)
        {
            return Path.Combine(item.Path, "season.nfo");
        }

        protected override string GetRootElementName(BaseItem item)
        {
            return "season";
        }

        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            if (!item.SupportsLocalMetadata)
            {
                return false;
            }

            if (!(item is Season))
            {
                return false;
            }

			return updateType >= MinimumUpdateType || (updateType >= ItemUpdateType.MetadataImport && FileSystem.FileExists(GetSavePath(item)));
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer)
        {
            var season = (Season)item;

            if (season.IndexNumber.HasValue)
            {
                writer.WriteElementString("seasonnumber", season.IndexNumber.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        protected override List<string> GetTagsUsed(BaseItem item)
        {
            var list = base.GetTagsUsed(item);
            list.AddRange(new string[]
            {
                "seasonnumber"
            });

            return list;
        }
    }
}
