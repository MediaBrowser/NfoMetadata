using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using NfoMetadata.Parsers;
using System.IO;
using System.Threading;

using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;
using MediaBrowser.Model.Configuration;

namespace NfoMetadata.Providers
{
    //public class AlbumNfoProvider : BaseNfoProvider<MusicAlbum>
    //{
    //    private readonly ILogger _logger;
    //    private readonly IConfigurationManager _config;
    //    private readonly IProviderManager _providerManager;

    //    public AlbumNfoProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager)
    //        : base(fileSystem)
    //    {
    //        _logger = logger;
    //        _config = config;
    //        _providerManager = providerManager;
    //    }

    //    protected override Task Fetch(MetadataResult<MusicAlbum> result, string path, CancellationToken cancellationToken)
    //    {
    //        return new BaseNfoParser<MusicAlbum>(_logger, _config, _providerManager, FileSystem).Fetch(result, path, cancellationToken);
    //    }

    //    protected override FileSystemMetadata GetXmlFile(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService)
    //    {
    //        string path = new MusicAlbum() { InternalId = info.Id }.GetMediaContainingFolderPath(libraryOptions);

    //        if (string.IsNullOrEmpty(path))
    //        {
    //            return null;
    //        }

    //        return directoryService.GetFile(Path.Combine(path, "album.nfo"));
    //    }
    //}
}
