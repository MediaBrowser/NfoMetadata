﻿using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using NfoMetadata.Savers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;

namespace NfoMetadata.Providers
{
    using MediaBrowser.Model.MediaInfo;

    public abstract class BaseNfoProvider<T> : ILocalMetadataProvider<T>, IHasItemChangeMonitor, IHasMetadataFeatures
        where T : BaseItem, new()
    {
        protected IFileSystem FileSystem;

        public async Task<MetadataResult<T>> GetMetadata(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<T>();

            if (string.IsNullOrEmpty(info.Path))
            {
                return result;
            }

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

        public MetadataFeatures[] Features => new[] { MetadataFeatures.Collections };

        protected abstract Task Fetch(MetadataResult<T> result, string path, CancellationToken cancellationToken);

        protected BaseNfoProvider(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        protected abstract FileSystemMetadata GetXmlFile(ItemInfo info, LibraryOptions libraryOptions, IDirectoryService directoryService);

        public bool HasChanged(BaseItem item, LibraryOptions libraryOptions, IDirectoryService directoryService)
        {
            if (string.IsNullOrEmpty(item.Path) || item.PathProtocol.HasValue && item.PathProtocol != MediaProtocol.File)
            {
                return false;
            }

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
