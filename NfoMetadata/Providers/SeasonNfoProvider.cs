using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using NfoMetadata.Parsers;
using System.IO;
using System.Threading;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;

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

        protected override FileSystemMetadata GetXmlFile(ItemInfo info, IDirectoryService directoryService)
        {
            return directoryService.GetFile(Path.Combine(info.Path, "season.nfo"));
        }
    }
}

