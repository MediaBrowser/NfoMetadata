using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;

using NfoMetadata.Configuration;

namespace NfoMetadata
{
    // This is a thin wrapper around ItemInfo/BaseItem to support unit testing since those items
    // cannot be mocked. My preference would be to change ItemInfo (since itself seems to be a wrapper)
    // to add an empty constructor
    public struct InfoPathArgs
    {
        public string Path { get; set; }

        public string ContainingFolderPath { get; set; }

        public string Container { get; set; }

        public bool IsInMixedFolder { get; set; }

        public static InfoPathArgs Create(ItemInfo item)
        {
            return new InfoPathArgs
            {
                Path = item.Path,
                ContainingFolderPath = item.ContainingFolderPath,
                Container = item.Container,
                IsInMixedFolder = item.IsInMixedFolder
            };
        }
    }

    public static class Helpers
    {
        public static FileSystemMetadata GetFileInfo(IDirectoryService directoryService, string directory, string filename)
        {
            if (directory == null || filename == null)
                return null;

            try
            {
                var item = directoryService
                    .GetFileSystemEntries(directory)
                    .FirstOrDefault(file => !file.IsDirectory && string.Equals(filename, file.Name, StringComparison.OrdinalIgnoreCase));

                return item;
            }
            catch (DirectoryNotFoundException)
            {

            }

            return null;
        }

        public static IEnumerable<(string Directory, string FileName)> GetMovieSavePaths(ItemInfo item, XbmcMetadataOptions options)
        {
            var container = item.Container.AsSpan();
            var path = item.ContainingFolderPath;

            if (container.Equals(MediaContainer.Dvd.Span, StringComparison.OrdinalIgnoreCase))
            {
                yield return (Path.Combine(path, "VIDEO_TS"), "VIDEO_TS.nfo");
                yield return (path, Path.GetFileName(path) + ".nfo");
                yield break;
            }

            if (container.Equals(MediaContainer.Bluray.Span, StringComparison.OrdinalIgnoreCase))
            {
                yield return (Path.Combine(path, "BDMV"), "index.nfo");
                yield return (path, Path.GetFileName(path) + ".nfo");
                yield break;
            }

            path = item.Path;

            if (string.IsNullOrEmpty(path) || BaseItem.MediaSourceManager.GetPathProtocol(path.AsSpan()) != MediaBrowser.Model.MediaInfo.MediaProtocol.File)
                yield break;

            if (options.PreferMovieNfo && !item.IsInMixedFolder)
                yield return (item.ContainingFolderPath, "movie.nfo");

            yield return (item.ContainingFolderPath, Path.GetFileNameWithoutExtension(path) + ".nfo");

            if (!options.PreferMovieNfo && !item.IsInMixedFolder)
                yield return (item.ContainingFolderPath, "movie.nfo");
        }
    }
}
