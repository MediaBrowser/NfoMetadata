using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml;
using MediaBrowser.Model.IO;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NfoMetadata.Parsers
{
    public class EpisodeNfoParser : BaseNfoParser<Episode>
    {
        public async Task FetchMultiple(List<MetadataResult<Episode>> items, string metadataFile, CancellationToken cancellationToken)
        {
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

            InitializeValidProviderIds(new Episode());

            await FetchMultiple(items, metadataFile, settings, cancellationToken).ConfigureAwait(false);
        }

        protected async Task FetchMultiple(List<MetadataResult<Episode>> items, string metadataFile, XmlReaderSettings settings, CancellationToken cancellationToken)
        {
            using (var fileStream = FileSystem.GetFileStream(metadataFile, FileOpenMode.Open, FileAccessMode.Read, FileShareMode.Read, true))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    var xml = await streamReader.ReadToEndAsync().ConfigureAwait(false);

                    var srch = "<episodedetails";
                    var index = xml.IndexOf(srch, StringComparison.OrdinalIgnoreCase);

                    if (index != -1)
                    {
                        var firstPart = xml.Substring(0, index);
                        var rest = xml.Substring(index);

                        xml = firstPart + "<episodes>" + rest;

                        xml += "</episodes>";
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
                                        switch (reader.Name)
                                        {
                                            case "episodedetails":
                                                {
                                                    if (!reader.IsEmptyElement)
                                                    {
                                                        using (var subtree = reader.ReadSubtree())
                                                        {
                                                            await FetchDataFromEpisodeDetailsNode(subtree, items, cancellationToken).ConfigureAwait(false);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        await reader.ReadAsync().ConfigureAwait(false);
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
                        }
                    }
                    catch (XmlException)
                    {

                    }
                }
            }
        }

        private async Task FetchDataFromEpisodeDetailsNode(XmlReader reader, List<MetadataResult<Episode>> items, CancellationToken cancellationToken)
        {
            await reader.MoveToContentAsync().ConfigureAwait(false);
            await reader.ReadAsync().ConfigureAwait(false);

            var newMetadataResult = new MetadataResult<Episode>() { Item = new Episode(), HasMetadata = true };

            // Loop through each element
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (reader.NodeType == XmlNodeType.Element)
                {
                    await FetchDataFromXmlNode(reader, newMetadataResult).ConfigureAwait(false);
                }
                else
                {
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }

            if (!newMetadataResult.HasMetadata)
            {
                return;
            }

            items.Add(newMetadataResult);
        }

        /// <summary>
        /// Fetches the data from XML node.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="itemResult">The item result.</param>
        protected override async Task FetchDataFromXmlNode(XmlReader reader, MetadataResult<Episode> itemResult)
        {
            var item = itemResult.Item;

            switch (reader.Name)
            {
                case "season":
                    {
                        var number = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(number))
                        {
                            int num;

                            if (int.TryParse(number, out num))
                            {
                                item.ParentIndexNumber = num;
                            }
                        }
                        break;
                    }

                case "episode":
                    {
                        var number = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(number))
                        {
                            int num;

                            if (int.TryParse(number, out num))
                            {
                                item.IndexNumber = num;
                            }
                        }
                        break;
                    }

                case "episodenumberend":
                    {
                        var number = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(number))
                        {
                            int num;

                            if (int.TryParse(number, out num))
                            {
                                item.IndexNumberEnd = num;
                            }
                        }
                        break;
                    }

                case "airsbefore_episode":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval) && rval > 0)
                            {
                                item.SortIndexNumber = rval;
                            }
                        }

                        break;
                    }

                case "airsafter_season":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval) && rval > 0)
                            {
                                item.SortParentIndexNumber = rval;
                            }
                        }

                        break;
                    }

                case "airsbefore_season":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval) && rval > 0)
                            {
                                item.SortParentIndexNumber = rval;
                            }
                        }

                        break;
                    }

                case "displayseason":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval) && rval > 0)
                            {
                                item.SortParentIndexNumber = rval;
                            }
                        }

                        break;
                    }

                case "displayepisode":
                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval) && rval > 0)
                            {
                                item.SortIndexNumber = rval;
                            }
                        }

                        break;
                    }


                default:
                    await base.FetchDataFromXmlNode(reader, itemResult).ConfigureAwait(false);
                    break;
            }
        }

        public EpisodeNfoParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
        {
        }
    }
}
