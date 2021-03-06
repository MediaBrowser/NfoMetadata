﻿using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System;
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
                case "id":
                    {
                        string imdbId = reader.GetAttribute("IMDB");
                        string tmdbId = reader.GetAttribute("TMDB");
                        string tvdbId = reader.GetAttribute("TVDB");

                        if (string.IsNullOrWhiteSpace(tvdbId))
                        {
                            tvdbId = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        }
                        if (!string.IsNullOrWhiteSpace(imdbId))
                        {
                            item.SetProviderId(MetadataProviders.Imdb, imdbId);
                        }
                        if (!string.IsNullOrWhiteSpace(tmdbId))
                        {
                            item.SetProviderId(MetadataProviders.Tmdb, tmdbId);
                        }
                        if (!string.IsNullOrWhiteSpace(tvdbId))
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
