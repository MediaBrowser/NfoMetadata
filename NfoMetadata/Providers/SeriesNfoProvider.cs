using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using NfoMetadata.Parsers;
using System.IO;
using System.Threading;
using MediaBrowser.Model.IO;

namespace NfoMetadata.Providers
{
    public class SeriesNfoProvider : BaseNfoProvider<Series>
    {
        private readonly ILogger _logger;
        private readonly IConfigurationManager _config;
        private readonly IProviderManager _providerManager;
        private readonly IFileSystem _fileSystem;

        public SeriesNfoProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem1)
            : base(fileSystem)
        {
            _logger = logger;
            _config = config;
            _providerManager = providerManager;
            _fileSystem = fileSystem1;
        }

        protected override void Fetch(MetadataResult<Series> result, string path, CancellationToken cancellationToken)
        {
            new SeriesNfoParser(_logger, _config, _providerManager, _fileSystem).Fetch(result, path, cancellationToken);
        }

        protected override FileSystemMetadata GetXmlFile(ItemInfo info, IDirectoryService directoryService)
        {
            return directoryService.GetFile(Path.Combine(info.Path, "tvshow.nfo"));
        }
    }
}
