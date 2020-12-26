using System;
using System.IO;
using System.Linq;
using System.Text;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System.Xml;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.Audio;

namespace NfoMetadata.Parsers
{
    class MovieNfoParser : BaseNfoParser<Video>
    {
        protected override bool SupportsUrlAfterClosingXmlTag
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Fetches the data from XML node.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="itemResult">The item result.</param>
        protected override async Task FetchDataFromXmlNode(XmlReader reader, MetadataResult<Video> itemResult)
        {
            var item = itemResult.Item;

            switch (reader.Name)
            {
                case "id":
                    {
                        string imdbId = reader.GetAttribute("IMDB");
                        string tmdbId = reader.GetAttribute("TMDB");

                        if (string.IsNullOrWhiteSpace(imdbId))
                        {
                            imdbId = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        }
                        if (!string.IsNullOrWhiteSpace(imdbId))
                        {
                            item.SetProviderId(MetadataProviders.Imdb, imdbId);
                        }
                        if (!string.IsNullOrWhiteSpace(tmdbId))
                        {
                            item.SetProviderId(MetadataProviders.Tmdb, tmdbId);
                        }
                        break;
                    }

                case "artist":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        var movie = item as MusicVideo;

                        if (!string.IsNullOrWhiteSpace(val) && movie != null)
                        {
                            movie.AddArtist(val);
                        }

                        break;
                    }

                case "album":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                        var movie = item as MusicVideo;

                        if (!string.IsNullOrWhiteSpace(val) && movie != null)
                        {
                            movie.Album = val;
                        }

                        break;
                    }

                default:
                    await base.FetchDataFromXmlNode(reader, itemResult).ConfigureAwait(false);
                    break;
            }
        }

        public MovieNfoParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
        {
        }
    }
}
