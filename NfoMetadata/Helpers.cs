using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using NfoMetadata.Savers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Configuration;
using System;

namespace NfoMetadata
{
    public static class Helpers
    {
        public static FileSystemMetadata GetFileInfo(IDirectoryService directoryService, string directory, string filename)
        {
            var entries = directoryService.GetFileSystemEntries(directory);

            foreach (var file in entries)
            {
                if (!file.IsDirectory)
                {
                    if (string.Equals(filename, file.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return file;
                    }
                }
            }

            return null;
        }
    }
}
