using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System;
using System.Linq;
using System.Xml;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;

namespace NfoMetadata.Parsers
{
    public class SeriesNfoParser : BaseNfoParser<Series>
    {
        protected override bool SupportsUrlAfterClosingXmlTag
        {
            get
            {
                return true;
            }
        }

        protected override string MovieDbParserSearchString
        {
            get { return "themoviedb.org/tv/"; }
        }

        /// <summary>
        /// Fetches the data from XML node.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="itemResult">The item result.</param>
        protected override async Task FetchDataFromXmlNode(XmlReader reader, MetadataResult<Series> itemResult)
        {
            var item = itemResult.Item;

            switch (reader.Name)
            {
                // TODO: Deprecate once most other tools are no longer using this
                case "id":
                    {
                        string imdbId = reader.GetAttribute("IMDB");
                        string tmdbId = reader.GetAttribute("TMDB");
                        string tvdbId = reader.GetAttribute("TVDB");

                        var content = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (string.IsNullOrWhiteSpace(tvdbId))
                        {
                            tvdbId = content;

                            // only use this if the tvdb id is null, since the <id> node is not very explicit about what id it represents
                            // also check it against other provider ids and avoid incorrectly assigning it
                            if (!string.IsNullOrEmpty(tvdbId))
                            {
                                if (item.HasProviderId(MetadataProviders.Tvdb) || item.ProviderIds.Values.Contains(tvdbId, StringComparer.OrdinalIgnoreCase))
                                {
                                    tvdbId = null;
                                }
                            }
                        }
                        if (IsValidProviderId(imdbId))
                        {
                            item.SetProviderId(MetadataProviders.Imdb, imdbId);
                        }
                        if (IsValidProviderId(tmdbId))
                        {
                            item.SetProviderId(MetadataProviders.Tmdb, tmdbId);
                        }
                        if (IsValidProviderId(tvdbId))
                        {
                            item.SetProviderId(MetadataProviders.Tvdb, tvdbId);
                        }
                        break;
                    }
                case "airs_dayofweek":
                    {
                        item.AirDays = GetAirDays(await reader.ReadElementContentAsStringAsync().ConfigureAwait(false));
                        break;
                    }

                case "airs_time":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            item.AirTime = val;
                        }
                        break;
                    }

                case "displayorder":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (Enum.TryParse(val, true, out SeriesDisplayOrder result))
                        {
                            item.DisplayOrder = result;
                        }
                        break;
                    }

                case "status":
                    {
                        var status = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(status))
                        {
                            SeriesStatus seriesStatus;
                            if (Enum.TryParse(status, true, out seriesStatus))
                            {
                                item.Status = seriesStatus;
                            }
                            else
                            {
                                Logger.Info("Unrecognized series status: " + status);
                            }
                        }

                        break;
                    }

                default:
                    await base.FetchDataFromXmlNode(reader, itemResult).ConfigureAwait(false);
                    break;
            }
        }

        public static DayOfWeek[] GetAirDays(string day)
        {
            if (!string.IsNullOrEmpty(day))
            {
                if (string.Equals(day, "Daily", StringComparison.OrdinalIgnoreCase))
                {
                    return new DayOfWeek[]
                               {
                                   DayOfWeek.Sunday,
                                   DayOfWeek.Monday,
                                   DayOfWeek.Tuesday,
                                   DayOfWeek.Wednesday,
                                   DayOfWeek.Thursday,
                                   DayOfWeek.Friday,
                                   DayOfWeek.Saturday
                               };
                }

                DayOfWeek value;

                if (Enum.TryParse(day, true, out value))
                {
                    return new DayOfWeek[]
                               {
                                   value
                               };
                }

                return new DayOfWeek[] { };
            }
            return null;
        }

        public SeriesNfoParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
        {
        }
    }
}
