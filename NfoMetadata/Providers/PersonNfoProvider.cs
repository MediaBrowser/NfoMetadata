using MediaBrowser.Common.Configuration;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Configuration;
using System.IO;
using NfoMetadata.Parsers;

namespace NfoMetadata.Providers
{
    public class PersonNfoProvider : BaseNfoProvider<Person>
    {
        private readonly ILogger _logger;
        private readonly IConfigurationManager _config;
        private readonly IProviderManager _providerManager;

        public PersonNfoProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager)
            : base(fileSystem)
        {
            _logger = logger;
            _config = config;
            _providerManager = providerManager;
        }

        protected override Task Fetch(MetadataResult<Person> result, string path, CancellationToken cancellationToken)
        {
            return new BaseNfoParser<Person>(_logger, _config, _providerManager, FileSystem).Fetch(result, path, cancellationToken);
        }

        protected override FileSystemMetadata GetXmlFile(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService)
        {
            return directoryService.GetFile(Path.Combine(info.GetInternalMetadataPath(), "person.nfo"));
        }
    }
}
