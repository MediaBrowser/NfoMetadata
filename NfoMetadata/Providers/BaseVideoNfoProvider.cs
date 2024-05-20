using System.Linq;

using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Common.Configuration;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Configuration;

using NfoMetadata.Parsers;
using NfoMetadata.Configuration;

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

        protected override FileSystemMetadata GetXmlFile(ItemInfo info, LibraryOptions libraryOptions)
        {
            var options = _config.GetNfoConfiguration();
            var file = Helpers.GetMovieSavePaths(info, options)
                .Select(pathInfo => Helpers.GetFileInfo(FileSystem, pathInfo.Directory, pathInfo.FileName))
                .FirstOrDefault(f => f != null);

            return file;
        }
    }
}