using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using NfoMetadata.Savers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;

namespace NfoMetadata.Providers
{
    public abstract class BaseNfoProvider<T> : ILocalMetadataProvider<T>, IHasItemChangeMonitor
        where T : BaseItem, new()
    {
        protected IFileSystem FileSystem;

        public async Task<MetadataResult<T>> GetMetadata(ItemInfo info,
            IDirectoryService directoryService,
            CancellationToken cancellationToken)
        {
            var result = new MetadataResult<T>();

            var file = GetXmlFile(info, directoryService);

            if (file == null)
            {
                return result;
            }

            var path = file.FullName;

            try
            {
                result.Item = new T();

                await Fetch(result, path, cancellationToken).ConfigureAwait(false);
                result.HasMetadata = true;
            }
            catch (FileNotFoundException)
            {
                result.HasMetadata = false;
            }
            catch (IOException)
            {
                result.HasMetadata = false;
            }

            return result;
        }

        protected abstract Task Fetch(MetadataResult<T> result, string path, CancellationToken cancellationToken);

        protected BaseNfoProvider(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        protected abstract FileSystemMetadata GetXmlFile(ItemInfo info, IDirectoryService directoryService);

        public bool HasChanged(BaseItem item, IDirectoryService directoryService)
        {
            var file = GetXmlFile(new ItemInfo(item), directoryService);

            if (file == null)
            {
                return false;
            }

            return file.Exists && item.IsGreaterThanDateLastSaved(FileSystem.GetLastWriteTimeUtc(file));
        }

        public string Name
        {
            get
            {
                return BaseNfoSaver.SaverName;
            }
        }
    }
}
