﻿using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using NfoMetadata.Configuration;
using NfoMetadata.Savers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;

namespace NfoMetadata.Parsers
{
    public class BaseNfoParser<T>
        where T : BaseItem, new ()
    {
        /// <summary>
        /// The logger
        /// </summary>
        protected ILogger Logger { get; private set; }
        protected IFileSystem FileSystem { get; private set; }
        protected IProviderManager ProviderManager { get; private set; }

        private readonly IConfigurationManager _config;
        private Dictionary<string, string> _validProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseNfoParser{T}" /> class.
        /// </summary>
        public BaseNfoParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem)
        {
            Logger = logger;
            _config = config;
            ProviderManager = providerManager;
            FileSystem = fileSystem;
        }

        protected void InitializeValidProviderIds(T item)
        {
            var idInfos = ProviderManager.GetExternalIdInfos(item);

            foreach (var info in idInfos)
            {
                var id = info.Key + "Id";
                if (!_validProviderIds.ContainsKey(id))
                {
                    _validProviderIds.Add(id, info.Key);
                }
            }

            //Additional Mappings
            _validProviderIds.Add("imdb_id", "Imdb");
        }

        /// <summary>
        /// Fetches metadata for an item from one xml file
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="metadataFile">The metadata file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public Task Fetch(MetadataResult<T> item, string metadataFile, CancellationToken cancellationToken)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }

            if (string.IsNullOrEmpty(metadataFile))
            {
                throw new ArgumentNullException();
            }

            var settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.None;
            settings.Async = true;
            settings.CheckCharacters = false;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreComments = true;

            InitializeValidProviderIds(item.Item);

            return Fetch(item, metadataFile, settings, cancellationToken);
        }

        protected virtual bool SupportsUrlAfterClosingXmlTag
        {
            get { return false; }
        }

        /// <summary>
        /// Fetches the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="metadataFile">The metadata file.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected virtual async Task Fetch(MetadataResult<T> item, string metadataFile, XmlReaderSettings settings, CancellationToken cancellationToken)
        {
            if (!SupportsUrlAfterClosingXmlTag)
            {
                using (var fileStream = FileSystem.GetFileStream(metadataFile, FileOpenMode.Open, FileAccessMode.Read, FileShareMode.Read, true))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        // Use XmlReader for best performance
                        using (var reader = XmlReader.Create(streamReader, settings))
                        {
                            item.ResetPeople();

                            await reader.MoveToContentAsync().ConfigureAwait(false);
                            await reader.ReadAsync().ConfigureAwait(false);

                            // Loop through each element
                            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                if (reader.NodeType == XmlNodeType.Element)
                                {
                                    await FetchDataFromXmlNode(reader, item).ConfigureAwait(false);
                                }
                                else
                                {
                                    await reader.ReadAsync().ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
                return;
            }

            using (var fileStream = FileSystem.GetFileStream(metadataFile, FileOpenMode.Open, FileAccessMode.Read, FileShareMode.Read, true))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    item.ResetPeople();

                    // Need to handle a url after the xml data
                    // http://kodi.wiki/view/NFO_files/movies

                    var xml = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                    // Find last closing Tag
                    // Need to do this in two steps to account for random > characters after the closing xml
                    var index = xml.LastIndexOf(@"</", StringComparison.Ordinal);

                    // If closing tag exists, move to end of Tag
                    if (index != -1)
                    {
                        index = xml.IndexOf('>', index);
                    }

                    if (index != -1)
                    {
                        var endingXml = xml.Substring(index);

                        ParseProviderLinks(item.Item, endingXml);

                        // If the file is just an imdb url, don't go any further
                        if (index == 0)
                        {
                            return;
                        }

                        xml = xml.Substring(0, index + 1);
                    }
                    else
                    {
                        // If the file is just an Imdb url, handle that

                        ParseProviderLinks(item.Item, xml);

                        return;
                    }

                    // These are not going to be valid xml so no sense in causing the provider to fail and spamming the log with exceptions
                    try
                    {
                        using (var stringReader = new StringReader(xml))
                        {
                            // Use XmlReader for best performance
                            using (var reader = XmlReader.Create(stringReader, settings))
                            {
                                await reader.MoveToContentAsync().ConfigureAwait(false);
                                await reader.ReadAsync().ConfigureAwait(false);

                                // Loop through each element
                                while (!reader.EOF && reader.ReadState == ReadState.Interactive)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    if (reader.NodeType == XmlNodeType.Element)
                                    {
                                        await FetchDataFromXmlNode(reader, item).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await reader.ReadAsync().ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                    catch (XmlException)
                    {

                    }
                }
            }
        }

        protected virtual string MovieDbParserSearchString
        {
            get { return "themoviedb.org/movie/"; }
        }

        protected void ParseProviderLinks(T item, string xml)
        {
            //Look for a match for the Regex pattern "tt" followed by 7 digits
            Match m = Regex.Match(xml, @"tt([0-9]{7,})", RegexOptions.IgnoreCase);
            if (m.Success && IsValidProviderId(m.Value))
            {
                item.SetProviderId(MetadataProviders.Imdb, m.Value);
            }

            // Support Tmdb
            // https://www.themoviedb.org/movie/30287-fallo
            var srch = MovieDbParserSearchString;
            var index = xml.IndexOf(srch, StringComparison.OrdinalIgnoreCase);

            if (index != -1)
            {
                var tmdbId = xml.Substring(index + srch.Length).TrimEnd('/').Split('-')[0];
                int value;
                if (!string.IsNullOrEmpty(tmdbId) && int.TryParse(tmdbId, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value > 0)
                {
                    item.SetProviderId(MetadataProviders.Tmdb, value.ToString(CultureInfo.InvariantCulture));
                }
            }

            if (item is Series)
            {
                srch = "thetvdb.com/?tab=series&id=";

                index = xml.IndexOf(srch, StringComparison.OrdinalIgnoreCase);

                if (index != -1)
                {
                    var tvdbId = xml.Substring(index + srch.Length).TrimEnd('/');
                    int value;
                    if (!string.IsNullOrEmpty(tvdbId) && int.TryParse(tvdbId, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value > 0)
                    {
                        item.SetProviderId(MetadataProviders.Tvdb, value.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        private async Task ParseCreditsNode(XmlReader reader, PersonType personType, MetadataResult<T> itemResult)
        {
            var providerIds = new ProviderIdDictionary();

            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    ParseProviderIdIfValid(reader.Name, reader.Value, providerIds);
                }

                // Move the reader back to the element node.
                reader.MoveToElement();
            }

            var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

            var personInfos = val.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(v => new PersonInfo { Name = v.Trim(), Type = personType })
                .ToList();

            if (personInfos.Count == 1)
            {
                personInfos[0].ProviderIds = providerIds;
            }

            foreach (var p in personInfos)
            {
                itemResult.AddPerson(p);
            }
        }

        private async Task ParsePersonNode(XmlReader reader, PersonType personType, MetadataResult<T> itemResult)
        {
            var providerIds = new ProviderIdDictionary();

            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    ParseProviderIdIfValid(reader.Name, reader.Value, providerIds);
                }

                // Move the reader back to the element node.
                reader.MoveToElement();
            }

            var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

            var personInfos = SplitNames(val)
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(v => new PersonInfo { Name = v.Trim(), Type = personType })
                .ToList();

            if (personInfos.Count == 1)
            {
                personInfos[0].ProviderIds = providerIds;
            }

            foreach (var p in personInfos)
            {
                itemResult.AddPerson(p);
            }
        }

        protected virtual async Task FetchDataFromXmlNode(XmlReader reader, MetadataResult<T> itemResult)
        {
            var item = itemResult.Item;

            switch (reader.Name)
            {
                // DateCreated
                case "dateadded":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            DateTimeOffset added;
                            if (DateTimeOffset.TryParseExact(val, BaseNfoSaver.DateAddedFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out added))
                            {
                                item.DateCreated = added.ToUniversalTime();
                            }
                            else if (DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out added))
                            {
                                item.DateCreated = added.ToUniversalTime();
                            }
                            else
                            {
                                Logger.Warn("Invalid Added value found: " + val);
                            }
                        }
                        break;
                    }

                case "uniqueid":
                    {
                        // ID's from multiple scraper sites eg IMDB, TVDB, TMDB-TV Shows
                        var type = reader.GetAttribute("type");
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(type) && !string.IsNullOrWhiteSpace(val) && IsProviderIdValid(val))
                        {
                            if (string.Equals(type, "TMDB-TV", StringComparison.OrdinalIgnoreCase))
                            {
                                type = MetadataProviders.Tmdb.ToString();
                            }
                            else
                            {
                                foreach (var enumType in Enum.GetNames(typeof(MetadataProviders)))
                                {
                                    if (string.Equals(type, enumType.ToString(), StringComparison.OrdinalIgnoreCase))
                                    {
                                        type = enumType.ToString();
                                    }
                                }
                            }

                            item.SetProviderId(type, val);
                        }
                        break;
                    }

                case "originaltitle":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            item.OriginalTitle = val;
                        }
                        break;
                    }

                case "title":
                case "localtitle":
                    item.Name = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    break;

                case "criticrating":
                    {
                        var text = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(text))
                        {
                            float value;
                            if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                            {
                                item.CriticRating = value;
                            }
                        }

                        break;
                    }

                case "sorttitle":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            item.SortName = val;
                        }
                        break;
                    }

                case "biography":
                case "plot":
                case "review":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            item.Overview = val;
                        }

                        break;
                    }

                case "language":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            item.PreferredMetadataLanguage = val;
                        }

                        break;
                    }

                case "countrycode":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            item.PreferredMetadataCountryCode = val;
                        }

                        break;
                    }

                case "lockedfields":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            item.LockedFields = val.Split('|').Select(i =>
                            {
                                MetadataFields field;

                                if (Enum.TryParse<MetadataFields>(i, true, out field))
                                {
                                    return (MetadataFields?)field;
                                }

                                return null;

                            }).Where(i => i.HasValue).Select(i => i.Value).ToArray();
                        }

                        break;
                    }

                case "tagline":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            item.Tagline = val;
                        }
                        break;
                    }

                case "placeofbirth":
                case "country":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            var productionLocations = val.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(i => i.Trim())
                                .Where(i => !string.IsNullOrWhiteSpace(i))
                                .ToArray();

                            AddProductionLocations(item, productionLocations);
                        }
                        break;
                    }

                case "mpaa":
                    {
                        var rating = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(rating))
                        {
                            item.OfficialRating = rating;
                        }
                        break;
                    }

                case "customrating":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            item.CustomRating = val;
                        }
                        break;
                    }

                case "runtime":
                    {
                        var text = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(text))
                        {
                            int runtime;
                            if (int.TryParse(text.Split(' ')[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out runtime))
                            {
                                item.RunTimeTicks = TimeSpan.FromMinutes(runtime).Ticks;
                            }
                        }
                        break;
                    }

                case "lockdata":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        item.IsLocked = string.Equals("true", val, StringComparison.OrdinalIgnoreCase);
                        break;
                    }

                case "studio":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            //var parts = val.Split('/')
                            //    .Select(i => i.Trim())
                            //    .Where(i => !string.IsNullOrWhiteSpace(i));

                            //foreach (var p in parts)
                            //{
                            //    item.AddStudio(p);
                            //}
                            item.AddStudio(val);
                        }
                        break;
                    }

                case "director":
                    {
                        await ParsePersonNode(reader, PersonType.Director, itemResult).ConfigureAwait(false);
                        break;
                    }
                case "credits":
                    {
                        await ParseCreditsNode(reader, PersonType.Writer, itemResult).ConfigureAwait(false);
                        break;
                    }

                case "writer":
                    {
                        await ParsePersonNode(reader, PersonType.Writer, itemResult).ConfigureAwait(false);
                        break;
                    }

                case "actor":
                    {
                        if (!reader.IsEmptyElement)
                        {
                            using (var subtree = reader.ReadSubtree())
                            {
                                var person = await GetPersonFromXmlNode(subtree).ConfigureAwait(false);

                                if (!string.IsNullOrWhiteSpace(person.Name))
                                {
                                    itemResult.AddPerson(person);
                                }
                            }
                        }
                        else
                        {
                            await reader.ReadAsync().ConfigureAwait(false);
                        }
                        break;
                    }

                case "trailer":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            val = val.Replace("plugin://plugin.video.youtube/?action=play_video&videoid=", BaseNfoSaver.YouTubeWatchUrl, StringComparison.OrdinalIgnoreCase)
                                .Replace("plugin://plugin.video.youtube/play/?video_id=", BaseNfoSaver.YouTubeWatchUrl, StringComparison.OrdinalIgnoreCase);

                            item.AddTrailerUrl(val);
                        }
                        break;
                    }

                case "year":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            int productionYear;
                            if (int.TryParse(val, out productionYear) && productionYear > 1850)
                            {
                                item.ProductionYear = productionYear;
                            }
                        }

                        break;
                    }

                case "rating":
                    {

                        var rating = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(rating))
                        {
                            float val;
                            // All external meta is saving this as '.' for decimal I believe...but just to be sure
                            if (float.TryParse(rating.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out val))
                            {
                                item.CommunityRating = val;
                            }
                        }
                        break;
                    }

                case "aired":
                case "formed":
                case "premiered":
                case "releasedate":
                    {
                        var formatString = _config.GetNfoConfiguration().ReleaseDateFormat;

                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            DateTime date;

                            if (DateTime.TryParseExact(val, formatString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date) && date.Year > 1850)
                            {
                                item.PremiereDate = date.ToUniversalTime();
                                item.ProductionYear = date.Year;
                            }
                        }

                        break;
                    }

                case "enddate":
                    {
                        var formatString = _config.GetNfoConfiguration().ReleaseDateFormat;

                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            DateTime date;

                            if (DateTime.TryParseExact(val, formatString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date) && date.Year > 1850)
                            {
                                item.EndDate = date.ToUniversalTime();
                            }
                        }

                        break;
                    }

                case "genre":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            var parts = val.Split('/')
                                .Select(i => i.Trim())
                                .Where(i => !string.IsNullOrWhiteSpace(i));

                            foreach (var p in parts)
                            {
                                item.AddGenre(p);
                            }
                        }
                        break;
                    }

                case "style":
                case "tag":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            item.AddTag(val);
                        }
                        break;
                    }

                case "fileinfo":
                    {
                        if (!reader.IsEmptyElement)
                        {
                            using (var subtree = reader.ReadSubtree())
                            {
                                await FetchFromFileInfoNode(subtree, item).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await reader.ReadAsync().ConfigureAwait(false);
                        }
                        break;
                    }

                case "ratings":
                    {
                        if (!reader.IsEmptyElement)
                        {
                            using (var subtree = reader.ReadSubtree())
                            {
                                await FetchFromRatingsNode(subtree, item).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            await reader.ReadAsync().ConfigureAwait(false);
                        }
                        break;
                    }

                case "set":
                    {
                        var tmdbcolid = reader.GetAttribute("tmdbcolid");
                        var linkedItemInfo = new LinkedItemInfo();

                        if (IsValidProviderId(tmdbcolid))
                        {
                            linkedItemInfo.SetProviderId(MetadataProviders.Tmdb, tmdbcolid);
                        }

                        var val = await reader.ReadInnerXmlAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            // TODO Handle this better later
                            if (val.IndexOf('<') == -1)
                            {
                                linkedItemInfo.Name = val;
                            }
                            else
                            {
                                try
                                {
                                    linkedItemInfo.Name = await ParseSetXml(val).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    Logger.ErrorException("Error parsing set node", ex);
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(linkedItemInfo.Name))
                            {
                                item.AddCollection(linkedItemInfo);
                            }
                        }

                        break;
                    }

                default:
                    string readerName = reader.Name;
                    if (_validProviderIds.TryGetValue(readerName, out var providerIdValue))
                    {
                        var id = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        if (!string.IsNullOrWhiteSpace(id) && IsProviderIdValid(id))
                        {
                            item.SetProviderId(providerIdValue, id);
                        }
                    }
                    else
                    {
                        await reader.SkipAsync().ConfigureAwait(false);
                    }
                    break;
            }
        }

        protected bool IsValidProviderId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static bool IsProviderIdValid(string value)
        {
            if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static bool IsValidHttpUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (Uri.TryCreate(value, UriKind.Absolute, out var uriResult))
            {
                return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
            }

            return false;
        }

        private void AddProductionLocations(BaseItem item, string[] locations)
        {
            foreach (var name in locations)
            {
                AddProductionLocation(item, name);
            }
        }

        private void AddProductionLocation(BaseItem item, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            var genres = item.ProductionLocations;
            if (!genres.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                var list = genres.ToList();
                list.Add(name);
                item.ProductionLocations = list.ToArray();
            }
        }

        private async Task FetchFromRatingsNode(XmlReader reader, T item)
        {
            await reader.MoveToContentAsync().ConfigureAwait(false);
            await reader.ReadAsync().ConfigureAwait(false);

            var ratings = new List<Tuple<double, string, bool>>();

            // Loop through each element
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "rating":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    await reader.ReadAsync().ConfigureAwait(false);
                                    continue;
                                }
                                var name = reader.GetAttribute("name");
                                var max = reader.GetAttribute("max");
                                var isDefault = reader.GetAttribute("default");

                                using (var subtree = reader.ReadSubtree())
                                {
                                    var rating = await GetRatingFromNode(subtree, name, max, isDefault).ConfigureAwait(false);

                                    if (rating != null)
                                    {
                                        ratings.Add(rating);
                                    }
                                }
                                break;
                            }

                        default:
                            await reader.SkipAsync().ConfigureAwait(false);
                            break;
                    }
                }
                else
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }

            foreach (var rating in ratings)
            {
                //Logger.Info("Found rating in nfo {0} {1} {2}", rating.Item2, rating.Item1, rating.Item3);

                if (string.Equals(rating.Item2, "tomatometerallcritics", StringComparison.OrdinalIgnoreCase))
                {
                    item.CriticRating = (float)(rating.Item1 * 10);
                }
                else if (rating.Item3 || !item.CommunityRating.HasValue)
                {
                    item.CommunityRating = (float)rating.Item1;
                }
            }
        }

        private async Task<Tuple<double, string, bool>> GetRatingFromNode(XmlReader reader, string ratingName, string maxAttributeValue, string isDefaultAttributeValue)
        {
            // use double to avoid loss of precision: https://emby.media/community/index.php?/topic/84266-rating-imported-from-nfo-with-a-rating-of-61-have-8-decimal-places/#entry863512

            double value = 0;
            var isDefault = string.Equals(isDefaultAttributeValue, "true", StringComparison.OrdinalIgnoreCase);
            double ratingMax = 10;

            if (double.TryParse(maxAttributeValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double max) && max > 0)
            {
                ratingMax = max;
            }
            //Logger.Info("Rating {0} max {1} default {2}", ratingName, ratingMax, isDefault);

            await reader.MoveToContentAsync().ConfigureAwait(false);
            await reader.ReadAsync().ConfigureAwait(false);

            // Loop through each element
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "value":
                            {
                                double.TryParse(await reader.ReadElementContentAsStringAsync().ConfigureAwait(false), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                                break;
                            }

                        default:
                            await reader.SkipAsync().ConfigureAwait(false);
                            break;
                    }
                }
                else
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }

            if (string.IsNullOrEmpty(ratingName) || value <= 0)
            {
                return null;
            }

            value /= ratingMax;
            value *= 10;

            return new Tuple<double, string, bool>(value, ratingName, isDefault);
        }

        private async Task FetchFromFileInfoNode(XmlReader reader, T item)
        {
            await reader.MoveToContentAsync().ConfigureAwait(false);
            await reader.ReadAsync().ConfigureAwait(false);

            // Loop through each element
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "streamdetails":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    await reader.ReadAsync().ConfigureAwait(false);
                                    continue;
                                }
                                using (var subtree = reader.ReadSubtree())
                                {
                                    await FetchFromStreamDetailsNode(subtree, item).ConfigureAwait(false);
                                }
                                break;
                            }

                        default:
                            await reader.SkipAsync().ConfigureAwait(false);
                            break;
                    }
                }
                else
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task FetchFromStreamDetailsNode(XmlReader reader, T item)
        {
            await reader.MoveToContentAsync().ConfigureAwait(false);
            await reader.ReadAsync().ConfigureAwait(false);

            // Loop through each element
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "video":
                            {
                                if (reader.IsEmptyElement)
                                {
                                    await reader.ReadAsync().ConfigureAwait(false);
                                    continue;
                                }
                                using (var subtree = reader.ReadSubtree())
                                {
                                    await FetchFromVideoNode(subtree, item).ConfigureAwait(false);
                                }
                                break;
                            }

                        default:
                            await reader.SkipAsync().ConfigureAwait(false);
                            break;
                    }
                }
                else
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }
        }

        private async Task FetchFromVideoNode(XmlReader reader, T item)
        {
            await reader.MoveToContentAsync().ConfigureAwait(false);
            await reader.ReadAsync().ConfigureAwait(false);

            // Loop through each element
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "format3d":
                            {
                                var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                                var video = item as Video;

                                if (video != null)
                                {
                                    if (string.Equals("HSBS", val, StringComparison.OrdinalIgnoreCase))
                                    {
                                        video.Video3DFormat = Video3DFormat.HalfSideBySide;
                                    }
                                    else if (string.Equals("HTAB", val, StringComparison.OrdinalIgnoreCase))
                                    {
                                        video.Video3DFormat = Video3DFormat.HalfTopAndBottom;
                                    }
                                    else if (string.Equals("FTAB", val, StringComparison.OrdinalIgnoreCase))
                                    {
                                        video.Video3DFormat = Video3DFormat.FullTopAndBottom;
                                    }
                                    else if (string.Equals("FSBS", val, StringComparison.OrdinalIgnoreCase))
                                    {
                                        video.Video3DFormat = Video3DFormat.FullSideBySide;
                                    }
                                    else if (string.Equals("MVC", val, StringComparison.OrdinalIgnoreCase))
                                    {
                                        video.Video3DFormat = Video3DFormat.MVC;
                                    }
                                }
                                break;
                            }

                        default:
                            await reader.SkipAsync().ConfigureAwait(false);
                            break;
                    }
                }
                else
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }
        }

        private void ParseProviderIdIfValid(string key, string value, ProviderIdDictionary providerIds)
        {
            if (key.EndsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                key = key.Substring(0, key.Length - 2);
                if (key.Length > 0)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        providerIds[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the persons from XML node.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>IEnumerable{PersonInfo}.</returns>
        private async Task<PersonInfo> GetPersonFromXmlNode(XmlReader reader)
        {
            var name = string.Empty;
            var type = PersonType.Actor;  // If type is not specified assume actor
            var role = string.Empty;
            var thumb = string.Empty;
            int? sortOrder = null;

            var providerIds = new ProviderIdDictionary();

            await reader.MoveToContentAsync().ConfigureAwait(false);
            await reader.ReadAsync().ConfigureAwait(false);

            // Loop through each element
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    var readerName = reader.Name ?? string.Empty;

                    switch (readerName)
                    {
                        case "name":
                            name = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false) ?? string.Empty;
                            break;

                        case "type":
                            if (Enum.TryParse(await reader.ReadElementContentAsStringAsync().ConfigureAwait(false) ?? string.Empty, true, out PersonType personType))
                            {
                                type = personType;
                            }
                            break;

                        case "role":
                            {
                                var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    role = val;
                                }
                                break;
                            }
                        case "thumb":
                            {
                                var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                                if (IsValidHttpUrl(val))
                                {
                                    thumb = val;
                                }
                                break;
                            }
                        case "sortorder":
                            {
                                var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                                if (!string.IsNullOrEmpty(val))
                                {
                                    int intVal;
                                    if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out intVal))
                                    {
                                        sortOrder = intVal;
                                    }
                                }
                                break;
                            }

                        default:

                            if (readerName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                            {
                                var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                                ParseProviderIdIfValid(readerName, val, providerIds);
                            }
                            else
                            {
                                await reader.SkipAsync().ConfigureAwait(false);
                            }
                            break;
                    }
                }
                else
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }

            return new PersonInfo
            {
                Name = name.Trim(),
                Role = role,
                Type = type,
                ImageUrl = thumb,
                ProviderIds = providerIds
            };
        }

        /// <summary>
        /// Used to split names of comma or pipe delimeted genres and people
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>IEnumerable{System.String}.</returns>
        private string[] SplitNames(string value)
        {
            value = value ?? string.Empty;

            // Only split by comma if there is no pipe in the string
            // We have to be careful to not split names like Matthew, Jr.
            var separator = value.IndexOf('|') == -1 && value.IndexOf(';') == -1 ? new[] { ',' } : new[] { '|', ';' };

            value = value.Trim().Trim(separator);

            return string.IsNullOrWhiteSpace(value) ? Array.Empty<string>() : value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }

        private async Task<string> ParseSetXml(string xml)
        {
            //xml = xml.Substring(xml.IndexOf('<'));
            //xml = xml.Substring(0, xml.LastIndexOf('>'));

            using (var stringReader = new StringReader("<set>" + xml + "</set>"))
            {
                // These are not going to be valid xml so no sense in causing the provider to fail and spamming the log with exceptions
                try
                {
                    var settings = new XmlReaderSettings();
                    settings.Async = true;
                    settings.ValidationType = ValidationType.None;
                    settings.CheckCharacters = false;
                    settings.IgnoreProcessingInstructions = true;
                    settings.IgnoreComments = true;

                    // Use XmlReader for best performance
                    using (var reader = XmlReader.Create(stringReader, settings))
                    {
                        await reader.MoveToContentAsync().ConfigureAwait(false);
                        await reader.ReadAsync().ConfigureAwait(false);

                        // Loop through each element
                        while (!reader.EOF && reader.ReadState == ReadState.Interactive)
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "name":
                                        return await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                                    default:
                                        await reader.SkipAsync().ConfigureAwait(false);
                                        break;
                                }
                            }
                            else
                            {
                                await reader.ReadAsync().ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch (XmlException)
                {

                }
            }

            return null;
        }
    }
}