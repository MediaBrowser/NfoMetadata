using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using NfoMetadata.Parsers;
using System.IO;
using System.Threading;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;
using MediaBrowser.Model.Configuration;

namespace NfoMetadata.Providers
{
    //public class ArtistNfoProvider : BaseNfoProvider<MusicArtist>
    //{
    //    private readonly ILogger _logger;
    //    private readonly IConfigurationManager _config;
    //    private readonly IProviderManager _providerManager;

    //    public ArtistNfoProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager)
    //        : base(fileSystem)
    //    {
    //        _logger = logger;
    //        _config = config;
    //        _providerManager = providerManager;
    //    }

    //    protected override Task Fetch(MetadataResult<MusicArtist> result, string path, CancellationToken cancellationToken)
    //    {
    //        return new BaseNfoParser<MusicArtist>(_logger, _config, _providerManager, FileSystem).Fetch(result, path, cancellationToken);
    //    }

    //    protected override FileSystemMetadata GetXmlFile(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService)
    //    {
    //        var path = new MusicArtist()
    //        {
    //            InternalId = info.Id,
    //            Name = info.Name

    //        }.GetMediaContainingFolderPath(libraryOptions);

    //        if (string.IsNullOrEmpty(path))
    //        {
    //            return null;
    //        }

    //        return directoryService.GetFile(Path.Combine(path, "artist.nfo"));
    //    }
    //}
}
