using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using NfoMetadata.Parsers;
using NfoMetadata.Savers;
using System.Linq;
using System.Threading;

using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;

namespace NfoMetadata.Providers
{
    public class BaseVideoNfoProvider<T> : BaseNfoProvider<T>
        where T : Video, new ()
    {
        private readonly ILogger _logger;
        private readonly IConfigurationManager _config;
        private readonly IProviderManager _providerManager;

        public BaseVideoNfoProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager)
            : base(fileSystem)
        {
            _logger = logger;
            _config = config;
            _providerManager = providerManager;
        }

        protected override async Task Fetch(MetadataResult<T> result, string path, CancellationToken cancellationToken)
        {
            var tmpItem = new MetadataResult<Video>
            {
                Item = result.Item
            };
            await new MovieNfoParser(_logger, _config, _providerManager, FileSystem).Fetch(tmpItem, path, cancellationToken).ConfigureAwait(false);

            result.Item = (T)tmpItem.Item;
            result.People = tmpItem.People;

            if (tmpItem.UserDataList != null)
            {
                result.UserDataList = tmpItem.UserDataList;
            }
        }

        protected override FileSystemMetadata GetXmlFile(ItemInfo info, IDirectoryService directoryService)
        {
            return MovieNfoSaver.GetMovieSavePaths(info, FileSystem)
                .Select(directoryService.GetFile)
                .FirstOrDefault(i => i != null);
        }
    }
}