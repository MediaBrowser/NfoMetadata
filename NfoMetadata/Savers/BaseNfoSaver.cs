using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using NfoMetadata.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using MediaBrowser.Model.IO;

namespace NfoMetadata.Savers
{
    public abstract class BaseNfoSaver : IMetadataSaver
    {
        public static readonly string YouTubeWatchUrl = "https://www.youtube.com/watch?v=";

        private static readonly Dictionary<string, string> CommonTags = new[] {

                    "plot",
                    "customrating",
                    "lockdata",
                    "dateadded",
                    "title",
                    "rating",
                    "year",
                    "sorttitle",
                    "mpaa",
                    "tmdbid",
                    "rottentomatoesid",
                    "language",
                    "tvcomid",
                    "tagline",
                    "studio",
                    "genre",
                    "tag",
                    "runtime",
                    "actor",
                    "criticrating",
                    "fileinfo",
                    "director",
                    "writer",
                    "trailer",
                    "premiered",
                    "releasedate",
                    "outline",
                    "id",
                    "credits",
                    "originaltitle",
                    "art",
                    "biography",
                    "formed",
                    "review",
                    "style",
                    "imdbid",
                    "imdb_id",
                    "country",
                    "audiodbalbumid",
                    "audiodbartistid",
                    "enddate",
                    "lockedfields",
                    "zap2itid",
                    "tvrageid",
                    "gamesdbid",

                    "musicbrainzartistid",
                    "musicbrainzalbumartistid",
                    "musicbrainzalbumid",
                    "musicbrainzreleasegroupid",
                    "tvdbid",

                    "countrycode",
                    "set",
                    "uniqueid",

                    "placeofbirth"

        }.ToDictionary(i => i, StringComparer.OrdinalIgnoreCase);

        protected BaseNfoSaver(IFileSystem fileSystem, ILibraryMonitor libraryMonitor, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger)
        {
            Logger = logger;
            UserDataManager = userDataManager;
            UserManager = userManager;
            LibraryManager = libraryManager;
            ConfigurationManager = configurationManager;
            FileSystem = fileSystem;
            LibraryMonitor = libraryMonitor;
        }

        protected IFileSystem FileSystem { get; private set; }
        protected IServerConfigurationManager ConfigurationManager { get; private set; }
        protected ILibraryManager LibraryManager { get; private set; }
        protected IUserManager UserManager { get; private set; }
        protected IUserDataManager UserDataManager { get; private set; }
        protected ILogger Logger { get; private set; }
        protected ILibraryMonitor LibraryMonitor { get; private set; }

        protected ItemUpdateType MinimumUpdateType
        {
            get
            {
                if (ConfigurationManager.GetNfoConfiguration().SaveImagePathsInNfoFiles)
                {
                    return ItemUpdateType.ImageUpdate;
                }

                return ItemUpdateType.MetadataDownload;
            }
        }

        public string Name
        {
            get
            {
                return SaverName;
            }
        }

        public static string SaverName
        {
            get
            {
                return "Nfo";
            }
        }

        /// <summary>
        /// Gets the save path.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>System.String.</returns>
        protected abstract string GetSavePath(BaseItem item, LibraryOptions libraryOptions);

        /// <summary>
        /// Gets the name of the root element.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>System.String.</returns>
        protected abstract string GetRootElementName(BaseItem item);

        /// <summary>
        /// Determines whether [is enabled for] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <returns><c>true</c> if [is enabled for] [the specified item]; otherwise, <c>false</c>.</returns>
        public abstract bool IsEnabledFor(BaseItem item, ItemUpdateType updateType);

        protected virtual List<string> GetTagsUsed(BaseItem item, XbmcMetadataOptions options)
        {
            var list = new List<string>();
            foreach (var providerKey in item.ProviderIds.Keys)
            {
                var providerIdTagName = GetTagForProviderKey(providerKey);
                if (!CommonTags.ContainsKey(providerIdTagName))
                {
                    list.Add(providerIdTagName);
                }
            }

            if (!string.IsNullOrEmpty(options.UserIdForUserData))
            {
                //list.Add("userrating");
                list.Add("isuserfavorite");
                list.Add("playcount");
                list.Add("watched");
                list.Add("lastplayed");
                list.Add("resume");
                list.Add("position");
                list.Add("total");
            }

            return list;
        }

        public async Task Save(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            var path = GetSavePath(item, libraryOptions);

            if (string.IsNullOrEmpty(path))
            {
                // Nothing to do
                return;
            }

            Logger.Debug("Saving nfo metadata for {0} to {1}.", item.Path ?? item.Name, path);

            LibraryMonitor.ReportFileSystemChangeBeginning(path);

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    Save(item, libraryOptions, memoryStream, path);

                    memoryStream.Position = 0;

                    cancellationToken.ThrowIfCancellationRequested();

                    await SaveToFile(memoryStream, path, libraryOptions, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                LibraryMonitor.ReportFileSystemChangeComplete(path, false);
            }
        }

        private async Task SaveToFile(Stream stream, string path, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            FileSystem.CreateDirectory(FileSystem.GetDirectoryName(path));
            // On Windows, saving the file will fail if the file is hidden or readonly
            FileSystem.SetAttributes(path, false, false);

            var fileCreated = false;

            try
            {
                using (var filestream = FileSystem.GetFileStream(path, FileOpenMode.Create, FileAccessMode.Write, FileShareMode.Read, true))
                {
                    fileCreated = true;
                    await stream.CopyToAsync(filestream, StreamDefaults.DefaultCopyToBufferSize, cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                if (fileCreated)
                {
                    TryDelete(path);
                }

                throw;
            }

            if (libraryOptions.SaveMetadataHidden)
            {
                SetHidden(path, true);
            }
        }

        private void TryDelete(string path)
        {
            try
            {
                FileSystem.DeleteFile(path);
            }
            catch
            {

            }
        }

        private void SetHidden(string path, bool hidden)
        {
            try
            {
                FileSystem.SetHidden(path, hidden);
            }
            catch (Exception ex)
            {
                Logger.Error("Error setting hidden attribute on {0} - {1}", path, ex.Message);
            }
        }

        protected virtual int GetNumTopLevelNodes(BaseItem item)
        {
            return 1;
        }

        protected virtual string GetNameToSave(string value, int nodeIndex, int numNodes)
        {
            return value;
        }

        protected virtual string GetOverviewToSave(string value, int nodeIndex, int numNodes)
        {
            return value;
        }

        private void Save(BaseItem item, LibraryOptions libraryOptions, Stream stream, string xmlPath)
        {
            var numNodes = GetNumTopLevelNodes(item);

            for (int i = 0; i < numNodes; i++)
            {
                using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = Encoding.UTF8,
                    CloseOutput = false,
                    ConformanceLevel = i == 0 ? ConformanceLevel.Document : ConformanceLevel.Fragment
                }))
                {
                    if (i == 0)
                    {
                        writer.WriteStartDocument(true);
                    }

                    writer.WriteStartElement(GetRootElementName(item));

                    var baseItem = item;

                    var options = ConfigurationManager.GetNfoConfiguration();

                    if (baseItem != null)
                    {
                        AddCommonNodes(baseItem, libraryOptions, writer, LibraryManager, UserManager, UserDataManager, FileSystem, options, i, numNodes);
                    }

                    WriteCustomElements(item, writer, i, numNodes);

                    var hasMediaSources = baseItem as IHasMediaSources;

                    if (hasMediaSources != null)
                    {
                        AddMediaInfo(hasMediaSources, writer);
                    }

                    if (numNodes < 2)
                    {
                        var tagsUsed = GetTagsUsed(item, options);

                        try
                        {
                            AddCustomTags(xmlPath, tagsUsed, writer, Logger, FileSystem);
                        }
                        catch (FileNotFoundException)
                        {

                        }
                        catch (IOException)
                        {

                        }
                        catch (XmlException ex)
                        {
                            Logger.ErrorException("Error reading existng nfo {0}", ex, xmlPath);
                        }
                    }

                    writer.WriteEndElement();

                    if (i == 0)
                    {
                        writer.WriteEndDocument();
                    }
                }
            }
        }

        protected abstract void WriteCustomElements(BaseItem item, XmlWriter writer, int nodeIndex, int numNodes);

        public static void AddMediaInfo<T>(T item, XmlWriter writer)
         where T : IHasMediaSources
        {
            writer.WriteStartElement("fileinfo");
            writer.WriteStartElement("streamdetails");

            var mediaStreams = item.GetMediaStreams();

            foreach (var stream in mediaStreams)
            {
                writer.WriteStartElement(stream.Type.ToString().ToLowerInvariant());

                if (!string.IsNullOrEmpty(stream.Codec))
                {
                    var codec = stream.Codec;

                    if ((stream.CodecTag ?? string.Empty).IndexOf("xvid", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        codec = "xvid";
                    }
                    else if ((stream.CodecTag ?? string.Empty).IndexOf("divx", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        codec = "divx";
                    }
                    else if (string.Equals(codec, "dca", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(stream.Profile, "DTS-HD MA", StringComparison.OrdinalIgnoreCase))
                        {
                            codec = "DTSHD_MA";
                        }
                        else
                        {
                            codec = "DTS";
                        }
                    }

                    writer.WriteElementString("codec", codec);
                    writer.WriteElementString("micodec", codec);
                }

                if (stream.BitRate.HasValue)
                {
                    writer.WriteElementString("bitrate", stream.BitRate.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (stream.Width.HasValue)
                {
                    writer.WriteElementString("width", stream.Width.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (stream.Height.HasValue)
                {
                    writer.WriteElementString("height", stream.Height.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (!string.IsNullOrEmpty(stream.AspectRatio))
                {
                    writer.WriteElementString("aspect", stream.AspectRatio);
                    writer.WriteElementString("aspectratio", stream.AspectRatio);
                }

                var framerate = stream.AverageFrameRate ?? stream.RealFrameRate;

                if (framerate.HasValue)
                {
                    writer.WriteElementString("framerate", framerate.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (!string.IsNullOrEmpty(stream.Language))
                {
                    // https://emby.media/community/index.php?/topic/49071-nfo-not-generated-on-actualize-or-rescan-or-identify
                    writer.WriteElementString("language", RemoveInvalidXMLChars(stream.Language));
                }

                var scanType = stream.IsInterlaced ? "interlaced" : "progressive";
                if (!string.IsNullOrEmpty(scanType))
                {
                    writer.WriteElementString("scantype", scanType);
                }

                if (stream.Channels.HasValue)
                {
                    writer.WriteElementString("channels", stream.Channels.Value.ToString(CultureInfo.InvariantCulture));
                }

                if (stream.SampleRate.HasValue)
                {
                    writer.WriteElementString("samplingrate", stream.SampleRate.Value.ToString(CultureInfo.InvariantCulture));
                }

                writer.WriteElementString("default", stream.IsDefault.ToString());
                writer.WriteElementString("forced", stream.IsForced.ToString());

                if (stream.Type == MediaStreamType.Video)
                {
                    var runtimeTicks = item.RunTimeTicks;
                    if (runtimeTicks.HasValue)
                    {
                        var timespan = TimeSpan.FromTicks(runtimeTicks.Value);

                        writer.WriteElementString("duration", Math.Floor(timespan.TotalMinutes).ToString(CultureInfo.InvariantCulture));
                        writer.WriteElementString("durationinseconds", Math.Floor(timespan.TotalSeconds).ToString(CultureInfo.InvariantCulture));
                    }

                    var video = item as Video;

                    if (video != null)
                    {
                        //AddChapters(video, builder, itemRepository);

                        if (video.Video3DFormat.HasValue)
                        {
                            switch (video.Video3DFormat.Value)
                            {
                                case Video3DFormat.FullSideBySide:
                                    writer.WriteElementString("format3d", "FSBS");
                                    break;
                                case Video3DFormat.FullTopAndBottom:
                                    writer.WriteElementString("format3d", "FTAB");
                                    break;
                                case Video3DFormat.HalfSideBySide:
                                    writer.WriteElementString("format3d", "HSBS");
                                    break;
                                case Video3DFormat.HalfTopAndBottom:
                                    writer.WriteElementString("format3d", "HTAB");
                                    break;
                                case Video3DFormat.MVC:
                                    writer.WriteElementString("format3d", "MVC");
                                    break;
                            }
                        }
                    }
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        // filters control characters but allows only properly-formed surrogate sequences
        private static Regex _invalidXMLChars = new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]");

        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        public static string RemoveInvalidXMLChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return _invalidXMLChars.Replace(text, string.Empty);
        }

        public const string DateAddedFormat = "yyyy-MM-dd HH:mm:ss";

        private void WriteOverviewNode(XmlWriter writer, string overview, string nodeName)
        {
            if (string.IsNullOrEmpty(overview))
            {
                writer.WriteElementString(nodeName, overview);
            }
            else
            {
                writer.WriteStartElement(nodeName);
                writer.WriteCData(overview);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Adds the common nodes.
        /// </summary>
        /// <returns>Task.</returns>
        private void AddCommonNodes(BaseItem item, LibraryOptions libraryOptions, XmlWriter writer, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataRepo, IFileSystem fileSystem, XbmcMetadataOptions options, int nodeIndex, int numNodes)
        {
            var writtenProviderIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var overview = GetOverviewToSave(item.Overview ?? string.Empty, nodeIndex, numNodes);

            if (item is MusicArtist)
            {
                WriteOverviewNode(writer, overview, "biography");
            }
            else if (item is MusicAlbum)
            {
                WriteOverviewNode(writer, overview, "review");
            }
            else
            {
                WriteOverviewNode(writer, overview, "plot");
            }

            if (item is Video)
            {
                var outline = (item.Tagline ?? string.Empty);

                WriteOverviewNode(writer, outline, "outline");
            }
            else
            {
                WriteOverviewNode(writer, overview, "outline");
            }

            if (!string.IsNullOrEmpty(item.CustomRating))
            {
                writer.WriteElementString("customrating", item.CustomRating);
            }

            writer.WriteElementString("lockdata", item.IsLocked.ToString().ToLowerInvariant());

            if (item.LockedFields.Length > 0)
            {
                writer.WriteElementString("lockedfields", string.Join("|", item.LockedFields));
            }

            writer.WriteElementString("dateadded", item.DateCreated.LocalDateTime.ToString(DateAddedFormat));

            writer.WriteElementString("title", GetNameToSave(item.Name ?? string.Empty, nodeIndex, numNodes));

            if (!string.IsNullOrEmpty(item.OriginalTitle))
            {
                writer.WriteElementString("originaltitle", item.OriginalTitle);
            }

            var people = item.SupportsPeople ? libraryManager.GetItemPeople(new InternalPeopleQuery
            {
                ItemIds = new[] { item.InternalId },
                EnableImages = options.SaveImagePathsInNfoFiles,
                EnableProviderIds = true

            }) : new List<PersonInfo>();

            AddActors(people, writer, libraryManager, fileSystem, libraryOptions, options.SaveImagePathsInNfoFiles);

            var directors = people
                .Where(i => IsPersonType(i, PersonType.Director))
                .ToList();

            foreach (var person in directors)
            {
                writer.WriteStartElement("director");
                foreach (var providerId in person.ProviderIds)
                {
                    writer.WriteAttributeString(providerId.Key.ToLowerInvariant() + "id", providerId.Value);
                }
                writer.WriteString(person.Name);
                writer.WriteEndElement();
            }

            var writers = people
                .Where(i => IsPersonType(i, PersonType.Writer))
                .ToList();

            foreach (var person in writers)
            {
                writer.WriteStartElement("writer");
                foreach (var providerId in person.ProviderIds)
                {
                    writer.WriteAttributeString(providerId.Key.ToLowerInvariant() + "id", providerId.Value);
                }
                writer.WriteString(person.Name);
                writer.WriteEndElement();
            }

            foreach (var person in writers)
            {
                writer.WriteStartElement("credits");
                foreach (var providerId in person.ProviderIds)
                {
                    writer.WriteAttributeString(providerId.Key.ToLowerInvariant() + "id", providerId.Value);
                }
                writer.WriteString(person.Name);
                writer.WriteEndElement();
            }

            foreach (var trailer in item.RemoteTrailers)
            {
                writer.WriteElementString("trailer", GetOutputTrailerUrl(trailer));
            }

            if (item.CommunityRating.HasValue)
            {
                writer.WriteElementString("rating", item.CommunityRating.Value.ToString(CultureInfo.InvariantCulture));
            }

            if (item.ProductionYear.HasValue)
            {
                writer.WriteElementString("year", item.ProductionYear.Value.ToString(CultureInfo.InvariantCulture));
            }

            var sortName = item.SortName;
            if (!string.IsNullOrEmpty(sortName))
            {
                writer.WriteElementString("sorttitle", sortName);
            }

            if (!string.IsNullOrEmpty(item.OfficialRating))
            {
                writer.WriteElementString("mpaa", item.OfficialRating);
            }

            if (nodeIndex == 0)
            {
                var imdb = item.GetProviderId(MetadataProviders.Imdb);
                if (!string.IsNullOrEmpty(imdb))
                {
                    if (item is Series)
                    {
                        writer.WriteElementString("imdb_id", imdb);
                    }
                    else
                    {
                        writer.WriteElementString("imdbid", imdb);
                    }
                    writtenProviderIds.Add(MetadataProviders.Imdb.ToString());
                }

                // Series xml saver already saves this
                if (!(item is Series))
                {
                    var tvdb = item.GetProviderId(MetadataProviders.Tvdb);
                    if (!string.IsNullOrEmpty(tvdb))
                    {
                        writer.WriteElementString("tvdbid", tvdb);
                        writtenProviderIds.Add(MetadataProviders.Tvdb.ToString());
                    }
                }

                var tmdb = item.GetProviderId(MetadataProviders.Tmdb);
                if (!string.IsNullOrEmpty(tmdb))
                {
                    writer.WriteElementString("tmdbid", tmdb);
                    writtenProviderIds.Add(MetadataProviders.Tmdb.ToString());
                }
            }

            if (!string.IsNullOrEmpty(item.PreferredMetadataLanguage))
            {
                writer.WriteElementString("language", item.PreferredMetadataLanguage);
            }
            if (!string.IsNullOrEmpty(item.PreferredMetadataCountryCode))
            {
                writer.WriteElementString("countrycode", item.PreferredMetadataCountryCode);
            }

            if (item.PremiereDate.HasValue && !(item is Episode))
            {
                var formatString = options.ReleaseDateFormat;

                if (item is MusicArtist)
                {
                    writer.WriteElementString("formed", item.PremiereDate.Value.LocalDateTime.ToString(formatString));
                }
                else
                {
                    writer.WriteElementString("premiered", item.PremiereDate.Value.LocalDateTime.ToString(formatString));
                    writer.WriteElementString("releasedate", item.PremiereDate.Value.LocalDateTime.ToString(formatString));
                }
            }

            if (item.EndDate.HasValue)
            {
                if (!(item is Episode))
                {
                    var formatString = options.ReleaseDateFormat;

                    writer.WriteElementString("enddate", item.EndDate.Value.LocalDateTime.ToString(formatString));
                }
            }

            if (item.CriticRating.HasValue)
            {
                writer.WriteElementString("criticrating", item.CriticRating.Value.ToString(CultureInfo.InvariantCulture));
            }

            // Use original runtime here, actual file runtime later in MediaInfo
            var runTimeTicks = item.RunTimeTicks;

            if (runTimeTicks.HasValue)
            {
                var timespan = TimeSpan.FromTicks(runTimeTicks.Value);

                writer.WriteElementString("runtime", Convert.ToInt64(timespan.TotalMinutes).ToString(CultureInfo.InvariantCulture));
            }

            if (!string.IsNullOrEmpty(item.Tagline))
            {
                writer.WriteElementString("tagline", item.Tagline);
            }

            if (item is Person)
            {
                foreach (var country in item.ProductionLocations)
                {
                    writer.WriteElementString("placeofbirth", country);
                }
            }
            else
            {
                foreach (var country in item.ProductionLocations)
                {
                    writer.WriteElementString("country", country);
                }
            }

            foreach (var genre in item.GenreItems)
            {
                writer.WriteElementString("genre", genre.Name);
            }

            foreach (var studio in item.StudioItems)
            {
                writer.WriteElementString("studio", studio.Name);
            }

            foreach (var tag in item.TagItems)
            {
                if (item is MusicAlbum || item is MusicArtist)
                {
                    writer.WriteElementString("style", tag.Name);
                }
                else
                {
                    writer.WriteElementString("tag", tag.Name);
                }
            }

            var collections = item.GetAllKnownCollections();
            foreach (var collection in collections)
            {
                writer.WriteStartElement("set");

                var providerIds = collection.ProviderIds ?? (collection.Id.Equals(0) ? new ProviderIdDictionary() : LibraryManager.GetProviderIds(collection.Id));

                foreach (var providerIdPair in providerIds)
                {
                    var providerIdValue = providerIdPair.Value;

                    if (string.IsNullOrEmpty(providerIdValue))
                    {
                        // safeguard
                        continue;
                    }

                    var providerIdName = providerIdPair.Key;
                    if (string.Equals(providerIdName, "Tmdb", StringComparison.OrdinalIgnoreCase))
                    {
                        providerIdName = "tmdbcolid";
                    }

                    writer.WriteAttributeString(providerIdName, providerIdValue);
                }

                writer.WriteElementString("name", collection.Name);
                writer.WriteEndElement();
            }

            if (nodeIndex == 0)
            {
                if (item.ProviderIds != null)
                {
                    foreach (var providerIdPair in item.ProviderIds)
                    {
                        var providerKey = providerIdPair.Key.ToLowerInvariant();
                        var providerId = providerIdPair.Value;

                        if (string.IsNullOrEmpty(providerId))
                        {
                            continue;
                        }

                        writer.WriteStartElement("uniqueid");
                        writer.WriteAttributeString("type", providerKey);
                        writer.WriteString(providerId);
                        writer.WriteEndElement();

                        if (!writtenProviderIds.Contains(providerKey))
                        {
                            try
                            {
                                var tagName = GetTagForProviderKey(providerKey);
                                //Logger.Debug("Verifying custom provider tagname {0}", tagName);
                                XmlConvert.VerifyName(tagName);
                                //Logger.Debug("Saving custom provider tagname {0}", tagName);

                                writer.WriteElementString(GetTagForProviderKey(providerKey), providerId);
                            }
                            catch (ArgumentException)
                            {
                                // catch invalid names without failing the entire operation
                            }
                            catch (XmlException)
                            {
                                // catch invalid names without failing the entire operation
                            }
                        }
                    }
                }
            }

            if (options.SaveImagePathsInNfoFiles)
            {
                AddImages(item, writer, libraryManager, libraryOptions);
            }

            AddUserData(item, writer, userManager, userDataRepo, options);
        }

        /// <summary>
        /// Gets the output trailer URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>System.String.</returns>
        private string GetOutputTrailerUrl(string url)
        {
            // This is what xbmc expects
            return url.Replace(YouTubeWatchUrl, "plugin://plugin.video.youtube/play/?video_id=", StringComparison.OrdinalIgnoreCase);
        }

        private void AddImages(BaseItem item, XmlWriter writer, ILibraryManager libraryManager, LibraryOptions libraryOptions)
        {
            writer.WriteStartElement("art");

            var image = item.GetImageInfo(ImageType.Primary, 0);

            if (image != null)
            {
                writer.WriteElementString("poster", GetImagePathToSave(image, libraryManager, libraryOptions));
            }

            foreach (var backdrop in item.GetImages(ImageType.Backdrop))
            {
                writer.WriteElementString("fanart", GetImagePathToSave(backdrop, libraryManager, libraryOptions));
            }

            writer.WriteEndElement();
        }

        private void AddUserData(BaseItem item, XmlWriter writer, IUserManager userManager, IUserDataManager userDataRepo, XbmcMetadataOptions options)
        {
            var userId = options.UserIdForUserData;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            if (item.IsFolder)
            {
                return;
            }

            var userdata = userDataRepo.GetUserData(userId, item);

            writer.WriteElementString("isuserfavorite", userdata.IsFavorite.ToString().ToLowerInvariant());

            //if (userdata.Rating.HasValue)
            //{
            //    writer.WriteElementString("userrating", userdata.Rating.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant());
            //}

            if (!item.IsFolder)
            {
                writer.WriteElementString("playcount", userdata.PlayCount.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("watched", userdata.Played.ToString().ToLowerInvariant());

                if (userdata.LastPlayedDate.HasValue)
                {
                    writer.WriteElementString("lastplayed", userdata.LastPlayedDate.Value.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss").ToLowerInvariant());
                }

                writer.WriteStartElement("resume");

                var runTimeTicks = item.RunTimeTicks ?? 0;

                writer.WriteElementString("position", TimeSpan.FromTicks(userdata.PlaybackPositionTicks).TotalSeconds.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("total", TimeSpan.FromTicks(runTimeTicks).TotalSeconds.ToString(CultureInfo.InvariantCulture));
            }

            writer.WriteEndElement();
        }

        private void AddActors(List<PersonInfo> people, XmlWriter writer, ILibraryManager libraryManager, IFileSystem fileSystem, LibraryOptions libraryOptions, bool saveImagePath)
        {
            var actors = people
                .Where(i => !IsPersonType(i, PersonType.Director) && !IsPersonType(i, PersonType.Writer))
                .ToList();

            foreach (var person in actors)
            {
                writer.WriteStartElement("actor");

                if (!string.IsNullOrEmpty(person.Name))
                {
                    writer.WriteElementString("name", person.Name);
                }

                if (!string.IsNullOrEmpty(person.Role))
                {
                    writer.WriteElementString("role", person.Role);
                }

                writer.WriteElementString("type", person.Type.ToString());

                foreach (var providerId in person.ProviderIds)
                {
                    var tagName = providerId.Key.ToLowerInvariant() + "id";
                    //Logger.Debug("Verifying custom provider tagname {0}", tagName);
                    XmlConvert.VerifyName(tagName);

                    writer.WriteElementString(tagName, providerId.Value);
                }

                if (saveImagePath)
                {
                    try
                    {
                        var image = person.ImageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);

                        if (image != null)
                        {
                            writer.WriteElementString("thumb", GetImagePathToSave(image, libraryManager, libraryOptions));
                        }
                    }
                    catch (Exception)
                    {
                        // Already logged in core
                    }
                }

                writer.WriteEndElement();
            }
        }

        private string GetImagePathToSave(ItemImageInfo image, ILibraryManager libraryManager, LibraryOptions libraryOptions)
        {
            if (!image.IsLocalFile)
            {
                return image.Path;
            }

            return libraryManager.GetPathAfterNetworkSubstitution(image.Path.AsSpan(), libraryOptions);
        }

        private bool IsPersonType(PersonInfo person, PersonType type)
        {
            return person.Type == type;
        }

        private XmlReaderSettings Create(bool enableValidation)
        {
            var settings = new XmlReaderSettings();

            if (!enableValidation)
            {
                settings.ValidationType = ValidationType.None;
            }

            return settings;
        }

        private void AddCustomTags(string path, List<string> xmlTagsUsed, XmlWriter writer, ILogger logger, IFileSystem fileSystem)
        {
            var settings = Create(false);

            settings.CheckCharacters = false;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreComments = true;

            using (var fileStream = fileSystem.OpenRead(path))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    // Use XmlReader for best performance
                    using (var reader = XmlReader.Create(streamReader, settings))
                    {
                        try
                        {
                            reader.MoveToContent();
                        }
                        catch (Exception ex)
                        {
                            //logger.ErrorException("Error reading existing xml tags from {0}.", ex, path);
                            return;
                        }

                        reader.Read();

                        // Loop through each element
                        while (!reader.EOF && reader.ReadState == ReadState.Interactive)
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                var name = reader.Name;

                                if (!CommonTags.ContainsKey(name) && !xmlTagsUsed.Contains(name, StringComparer.OrdinalIgnoreCase))
                                {
                                    writer.WriteNode(reader, false);
                                }
                                else
                                {
                                    reader.Skip();
                                }
                            }
                            else
                            {
                                reader.Read();
                            }
                        }
                    }
                }
            }
        }

        private string GetTagForProviderKey(string providerKey)
        {
            return providerKey.ToLowerInvariant() + "id";
        }
    }
}
