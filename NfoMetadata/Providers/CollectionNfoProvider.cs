using MediaBrowser.Common.Configuration;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Configuration;
using NfoMetadata.Parsers;
using System;

namespace NfoMetadata.Providers
{
    public class CollectionNfoProvider : BaseNfoProvider<BoxSet>
    {
        private readonly ILogger _logger;
        private readonly IConfigurationManager _config;
        private readonly IProviderManager _providerManager;

        public override MetadataFeatures[] Features => Array.Empty<MetadataFeatures>();

        public CollectionNfoProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager)
            : base(fileSystem)
        {
            _logger = logger;
            _config = config;
            _providerManager = providerManager;
        }

        protected override Task Fetch(MetadataResult<BoxSet> result, string path, CancellationToken cancellationToken)
        {
            return new CollectionNfoParser(_logger, _config, _providerManager, FileSystem).Fetch(result, path, cancellationToken);
        }

        protected override FileSystemMetadata GetXmlFile(ItemInfo info, LibraryOptions libraryOptions)
        {
            return Helpers.GetFileInfo(FileSystem, info.GetInternalMetadataPath(), "collection.nfo");
        }
    }
}
