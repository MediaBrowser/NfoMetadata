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
    public class CollectionNfoSaver : BaseNfoSaver
    {
        public CollectionNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, libraryMonitor, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
        }

        protected override string GetSavePath(BaseItem item, LibraryOptions libraryOptions)
        {
            return Path.Combine(item.GetInternalMetadataPath(), "collection.nfo");
        }

        protected override string GetRootElementName(BaseItem item)
        {
            return "collection";
        }

        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            return item is BoxSet && updateType >= MinimumUpdateType;
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer)
        {
            var collection = (BoxSet)item;

            writer.WriteElementString("displayorder", collection.DisplayOrder.ToString());
        }

        protected override List<string> GetTagsUsed(BaseItem item)
        {
            var list = base.GetTagsUsed(item);
            list.AddRange(new string[]
            {
                "displayorder"
            });
            return list;
        }
    }
}
