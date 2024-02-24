using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using System.Collections.Generic;
using NfoMetadata.Savers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;

namespace NfoMetadata.Providers
{
    public abstract class BaseNfoProvider<T> : ILocalMetadataProvider<T>, IHasItemChangeMonitor, IHasMetadataFeatures
        where T : BaseItem, new()
    {
        protected IFileSystem FileSystem;

        public async Task<MetadataResult<T>> GetMetadata(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<T>();

            var file = GetXmlFile(info, libraryOptions, directoryService);

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

        public async Task<List<MetadataResult<T>>> GetMultipleMetadata(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            var result = new List<MetadataResult<T>>();

            var file = GetXmlFile(info, libraryOptions, directoryService);

            if (file == null)
            {
                return result;
            }

            var path = file.FullName;

            try
            {
                await FetchMultiple(result, path, cancellationToken).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
            }
            catch (IOException)
            {
            }

            return result;
        }

        protected virtual Task FetchMultiple(List<MetadataResult<T>> result, string path, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public MetadataFeatures[] Features => new[] { MetadataFeatures.Collections };

        protected abstract Task Fetch(MetadataResult<T> result, string path, CancellationToken cancellationToken);

        protected BaseNfoProvider(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        protected abstract FileSystemMetadata GetXmlFile(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService);

        public bool HasChanged(BaseItem item, LibraryOptions libraryOptions, IDirectoryService directoryService)
        {
            var file = GetXmlFile(new ItemInfo(item), libraryOptions, directoryService);

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
