using System.Collections;
using System.Linq;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

using NUnit.Framework;

namespace NfoMetadata.Tests
{
    public class HelperTests
    {
        private class TestItem : Video
        {
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            BaseItem.MediaSourceManager = new TestMediaSourceManager();
            BaseItem.FileSystem = new TestFileSystem();
        }

        [SetUp]
        public void Setup()
        {
        }

        public static IEnumerable SavePathTestCases_MKV
        {
            get
            {
                #region MKV (not mix folder)

                yield return
                    new TestCaseData(
                        MediaContainer.Mkv.ToString(),
                        @"C:\Video\Movies\9 (2009)\9 (2009) - BluRay 1080p DTS x264-Group.mkv",
                        false,
                        false
                    )
                    .Returns(new[]
                    {
                        (@"C:\Video\Movies\9 (2009)", "9 (2009) - BluRay 1080p DTS x264-Group.nfo"),
                        (@"C:\Video\Movies\9 (2009)", "movie.nfo")
                    });

                yield return
                    new TestCaseData(
                        MediaContainer.Mkv.ToString(),
                        @"C:\Video\Movies\9 (2009)\9 (2009) - BluRay 1080p DTS x264-Group.mkv",
                        false,
                        true
                    )
                    .Returns(new[]
                    {
                        (@"C:\Video\Movies\9 (2009)", "movie.nfo"),
                        (@"C:\Video\Movies\9 (2009)", "9 (2009) - BluRay 1080p DTS x264-Group.nfo")
                    });

                #endregion

                #region MKV (Mixed Folder)

                yield return
                    new TestCaseData(
                        MediaContainer.Mkv.ToString(),
                        @"C:\Video\Movies\9 (2009)\9 (2009) - BluRay 1080p DTS x264-Group.mkv",
                        true,
                        false
                    )
                    .Returns(new[]
                    {
                        (@"C:\Video\Movies\9 (2009)", "9 (2009) - BluRay 1080p DTS x264-Group.nfo")
                    });

                yield return
                    new TestCaseData(
                        MediaContainer.Mkv.ToString(),
                        @"C:\Video\Movies\9 (2009)\9 (2009) - BluRay 1080p DTS x264-Group.mkv",
                        true,
                        true
                    )
                    .Returns(new[]
                    {
                        (@"C:\Video\Movies\9 (2009)", "9 (2009) - BluRay 1080p DTS x264-Group.nfo")
                    });

                #endregion
            }
        }

        public static IEnumerable SavePathTestCases_DVD
        {
            get
            {
                #region Dvd

                yield return
                    new TestCaseData(
                        MediaContainer.Dvd.ToString(),
                        @"C:\Video\Movies\Léon (1994)",
                        false,
                        false
                    )
                    .Returns(new[]
                    {
                        (@"C:\Video\Movies\Léon (1994)\VIDEO_TS", "VIDEO_TS.nfo"),
                        (@"C:\Video\Movies\Léon (1994)", "Léon (1994).nfo")
                    });

                #endregion
            }
        }

        public static IEnumerable SavePathTestCases_BluRay
        {
            get
            {
                #region BluRay

                yield return
                    new TestCaseData(
                        MediaContainer.Bluray.ToString(),
                        @"E:\Movies\Movies\Alien (1979)",
                        false,
                        false
                    )
                    .Returns(new[]
                    {
                        (@"E:\Movies\Movies\Alien (1979)\BDMV", "index.nfo"),
                        (@"E:\Movies\Movies\Alien (1979)", "Alien (1979).nfo")
                    });

                #endregion
            }
        }

        [TestCaseSource(nameof(SavePathTestCases_MKV))]
        [TestCaseSource(nameof(SavePathTestCases_DVD))]
        [TestCaseSource(nameof(SavePathTestCases_BluRay))]
        public (string, string)[] Validate_File_SavePaths(string container, string path, bool isInMixedFolder, bool preferMovieNfo)
        {
            var itemInfo = new ItemInfo(new TestItem
            {
                Container = container,
                Path = path,
                IsInMixedFolder = isInMixedFolder,
            });

            var actual = Helpers
                .GetMovieSavePaths(itemInfo, new Configuration.XbmcMetadataOptions { PreferMovieNfo = preferMovieNfo })
                .ToArray();

            return actual;
        }
    }
}
