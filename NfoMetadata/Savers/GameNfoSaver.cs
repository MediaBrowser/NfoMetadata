using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using NfoMetadata.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace NfoMetadata.Savers
{
    public class GameNfoSaver : BaseNfoSaver
    {
        public GameNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, libraryMonitor, configurationManager, libraryManager, userManager, userDataManager, logger)
        {
        }

        protected override string GetSavePath(BaseItem item, LibraryOptions libraryOptions)
        {
            return Path.ChangeExtension(item.Path, ".nfo");
        }

        protected override string GetRootElementName(BaseItem item)
        {
            return "game";
        }

        public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            if (!item.IsFileProtocol)
            {
                return false;
            }

            return item is Game && updateType >= MinimumUpdateType;
        }

        protected override void WriteCustomElements(BaseItem item, XmlWriter writer, int nodeIndex, int numNodes)
        {
        }

        protected override List<string> GetTagsUsed(BaseItem item, XbmcMetadataOptions options)
        {
            var list = base.GetTagsUsed(item, options);
            list.AddRange(new string[]
            {
            });
            return list;
        }
    }
}