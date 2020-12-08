using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using NfoMetadata.Parsers;
using System.IO;
using System.Threading;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.Entities;
using System;

namespace NfoMetadata.Providers
{
    public class SeasonNfoProvider : BaseNfoProvider<Season>
    {
        private readonly ILogger _logger;
        private readonly IConfigurationManager _config;
        private readonly IProviderManager _providerManager;

        public SeasonNfoProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager)
            : base(fileSystem)
        {
            _logger = logger;
            _config = config;
            _providerManager = providerManager;
        }

        protected override Task Fetch(MetadataResult<Season> result, string path, CancellationToken cancellationToken)
        {
            return new SeasonNfoParser(_logger, _config, _providerManager, FileSystem).Fetch(result, path, cancellationToken);
        }

        protected override FileSystemMetadata GetXmlFile(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService)
        {
            var path = info.Path;

            if (string.IsNullOrEmpty(path) || BaseItem.MediaSourceManager.GetPathProtocol(path.AsSpan()) != MediaBrowser.Model.MediaInfo.MediaProtocol.File)
            {
                return null;
            }

            return directoryService.GetFile(Path.Combine(path, "season.nfo"));
        }
    }
}

