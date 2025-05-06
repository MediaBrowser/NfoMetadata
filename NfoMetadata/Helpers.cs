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
    public static class Helpers
    {
        public static FileSystemMetadata GetFileInfo(IFileSystem fileSystem, string directory, string filename)
        {
            if (directory == null || filename == null)
                return null;

            try
            {
                var item = fileSystem
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
