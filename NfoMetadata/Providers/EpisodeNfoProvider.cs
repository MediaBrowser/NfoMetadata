using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using NfoMetadata.Parsers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediaBrowser.Model.IO;

namespace NfoMetadata.Providers
{
    public class EpisodeNfoProvider : BaseNfoProvider<Episode>
    {
        private readonly ILogger _logger;
        private readonly IConfigurationManager _config;
        private readonly IProviderManager _providerManager;

        public EpisodeNfoProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager)
            : base(fileSystem)
        {
            _logger = logger;
            _config = config;
            _providerManager = providerManager;
        }

        protected override void Fetch(MetadataResult<Episode> result, string path, CancellationToken cancellationToken)
        {
            var images = new List<LocalImageInfo>();

            new EpisodeNfoParser(_logger, _config, _providerManager, FileSystem).Fetch(result, images, path, cancellationToken);

            result.Images = images;
        }

        protected override FileSystemMetadata GetXmlFile(ItemInfo info, IDirectoryService directoryService)
        {
            var path = Path.ChangeExtension(info.Path, ".nfo");

            return directoryService.GetFile(path);
        }
    }
}
