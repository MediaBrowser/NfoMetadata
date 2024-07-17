using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MediaBrowser.Model.IO;

namespace NfoMetadata.Tests
{
    public class TestFileSystem : IFileSystem
    {
        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        #region Not Implemented

        public string DefaultDirectory => throw new NotImplementedException();

        public IEnumerable<FileSystemMetadata> CommonFolders => throw new NotImplementedException();

        public char DirectorySeparatorChar => throw new NotImplementedException();

        public bool AreEqual(ReadOnlySpan<char> path1, ReadOnlySpan<char> path2)
        {
            throw new NotImplementedException();
        }

        public bool AreEqual(string path1, string path2)
        {
            throw new NotImplementedException();
        }

        public bool ContainsSubPath(ReadOnlySpan<char> parentPath, ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public bool ContainsSubPath(string parentPath, string path)
        {
            throw new NotImplementedException();
        }

        public void CopyFile(string source, string target, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectory(string path, bool recursive, bool sendToRecycleBin)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path, bool sendToRecycleBin)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path, FileSystemCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path, FileSystemCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetCreationTimeUtc(FileSystemMetadata info)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetCreationTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileSystemMetadata> GetDirectories(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public FileSystemMetadata GetDirectoryInfo(string path)
        {
            throw new NotImplementedException();
        }

        public FileSystemMetadata GetDirectoryInfo(string path, FileSystemCredentials credentials)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirectoryPaths(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public DriveInfo GetDriveInfo(string path)
        {
            throw new NotImplementedException();
        }

        public List<FileSystemMetadata> GetDrives()
        {
            throw new NotImplementedException();
        }

        public FileSystemMetadata GetFileInfo(string path)
        {
            throw new NotImplementedException();
        }

        public string GetFileNameWithoutExtension(FileSystemMetadata info)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public string GetFileNameWithoutExtension(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFilePaths(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFilePaths(string path, string[] extensions, bool enableCaseSensitiveExtensions, bool recursive)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileSystemMetadata> GetFiles(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileSystemMetadata> GetFiles(string path, string[] extensions, bool enableCaseSensitiveExtensions, bool recursive)
        {
            throw new NotImplementedException();
        }

        public Stream GetFileStream(string path, FileOpenMode mode, FileAccessMode access, FileShareMode share, bool isAsync = false)
        {
            throw new NotImplementedException();
        }

        public Stream GetFileStream(string path, FileOpenMode mode, FileAccessMode access, bool isAsync = false)
        {
            throw new NotImplementedException();
        }

        public Stream GetFileStream(string path, FileOpenMode mode, FileAccessMode access, FileOpenOptions fileOpenOptions)
        {
            throw new NotImplementedException();
        }

        public Stream GetFileStream(string path, FileOpenMode mode, FileAccessMode access, FileOpenOptions fileOpenOptions, long preAllocationSize)
        {
            throw new NotImplementedException();
        }

        public Stream GetFileStream(string path, FileOpenMode mode, FileAccessMode access, FileShareMode share, FileOpenOptions fileOpenOptions)
        {
            throw new NotImplementedException();
        }

        public Stream GetFileStream(string path, FileOpenMode mode, FileAccessMode access, FileShareMode share, FileOpenOptions fileOpenOptions, long preAllocationSize)
        {
            throw new NotImplementedException();
        }

        public Stream GetFileStream(string path, FileOpenMode mode, FileAccessMode access, FileShareMode share, int bufferSize, FileOpenOptions fileOpenOptions)
        {
            throw new NotImplementedException();
        }

        public Stream GetFileStream(string path, FileOpenMode mode, FileAccessMode access, FileShareMode share, int bufferSize, FileOpenOptions fileOpenOptions, long preAllocationSize)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FileSystemMetadata> GetFileSystemEntries(string path, bool recursive = false, FileSystemCredentials credentials = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFileSystemEntryPaths(string path, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public FileSystemMetadata GetFileSystemInfo(string path)
        {
            throw new NotImplementedException();
        }

        public string GetFullPath(string path)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetLastWriteTimeUtc(FileSystemMetadata info)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetLastWriteTimeUtc(string path, bool fileExists)
        {
            throw new NotImplementedException();
        }

        public string GetValidFilename(string filename)
        {
            throw new NotImplementedException();
        }

        public bool IsPathFile(ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public bool IsRootPath(ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<char> MakeAbsolutePath(ReadOnlySpan<char> folderPath, ReadOnlySpan<char> filePath)
        {
            throw new NotImplementedException();
        }

        public void MoveDirectory(string source, string target)
        {
            throw new NotImplementedException();
        }

        public void MoveFile(string source, string target, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public void MoveFile(string source, string target)
        {
            throw new NotImplementedException();
        }

        public List<FileSystemMetadata> NormalizeDuplicates(FileSystemMetadata[] paths, bool checkSubPaths)
        {
            throw new NotImplementedException();
        }

        public ReadOnlySpan<char> NormalizePath(ReadOnlySpan<char> path)
        {
            throw new NotImplementedException();
        }

        public string NormalizePath(string path)
        {
            throw new NotImplementedException();
        }

        public Stream OpenRead(string path)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public string[] ReadAllLines(string path)
        {
            throw new NotImplementedException();
        }

        public Task<string[]> ReadAllLinesAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public string ReadAllText(string path)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void SetAttributes(string path, bool isHidden, bool readOnly)
        {
            throw new NotImplementedException();
        }

        public void SetExecutable(string path)
        {
            throw new NotImplementedException();
        }

        public void SetHidden(string path, bool isHidden)
        {
            throw new NotImplementedException();
        }

        public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }

        public void SetReadOnly(string path, bool readOnly)
        {
            throw new NotImplementedException();
        }

        public bool SupportsPathNatively(string path)
        {
            throw new NotImplementedException();
        }

        public void SwapFiles(string file1, string file2)
        {
            throw new NotImplementedException();
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void WriteAllLines(string path, IEnumerable<string> lines)
        {
            throw new NotImplementedException();
        }

        public void WriteAllText(string path, string text)
        {
            throw new NotImplementedException();
        }

        public void WriteAllText(string path, string text, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public Task WriteAllTextAsync(string path, string text, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task WriteAllTextAsync(string path, string text, Encoding encoding, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
