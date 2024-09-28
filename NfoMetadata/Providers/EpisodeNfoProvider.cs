using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using NfoMetadata.Parsers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.Entities;
using System;

namespace NfoMetadata.Providers
{
    public class EpisodeNfoProvider : BaseNfoProvider<Episode>, IMultipleLocalMetadataProvider<Episode>
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

        protected override Task FetchMultiple(List<MetadataResult<Episode>> result, string path, CancellationToken cancellationToken)
        {
            return new EpisodeNfoParser(_logger, _config, _providerManager, FileSystem).FetchMultiple(result, path, cancellationToken);
        }

        protected override Task Fetch(MetadataResult<Episode> result, string path, CancellationToken cancellationToken)
        {
            return new EpisodeNfoParser(_logger, _config, _providerManager, FileSystem).Fetch(result, path, cancellationToken);
        }

        protected override FileSystemMetadata GetXmlFile(ItemInfo info, LibraryOptions libraryOptions)
        {
            var path = info.Path;

            if (string.IsNullOrEmpty(path) || BaseItem.MediaSourceManager.GetPathProtocol(path.AsSpan()) != MediaBrowser.Model.MediaInfo.MediaProtocol.File)
            {
                return null;
            }

            path = Path.ChangeExtension(path, ".nfo");

            return FileSystem.GetFileInfo(path);
        }
    }
}
