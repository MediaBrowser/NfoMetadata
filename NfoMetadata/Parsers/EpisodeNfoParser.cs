﻿using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Xml;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Xml;
using System.IO;
using System.Text;

namespace NfoMetadata.Parsers
{
    public class EpisodeNfoParser : BaseNfoParser<Episode>
    {
        public void Fetch(MetadataResult<Episode> item,
            List<LocalImageInfo> images,
            string metadataFile, 
            CancellationToken cancellationToken)
        {
            Fetch(item, metadataFile, cancellationToken);
        }

        protected override void Fetch(MetadataResult<Episode> item, string metadataFile, XmlReaderSettings settings, CancellationToken cancellationToken)
        {
            using (var fileStream = FileSystem.OpenRead(metadataFile))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                {
                    item.ResetPeople();

                    var xml = streamReader.ReadToEnd();

                    var srch = "</episodedetails>";
                    var index = xml.IndexOf(srch, StringComparison.OrdinalIgnoreCase);

                    if (index != -1)
                    {
                        xml = xml.Substring(0, index + srch.Length);
                    }

                    // These are not going to be valid xml so no sense in causing the provider to fail and spamming the log with exceptions
                    try
                    {
                        using (var stringReader = new StringReader(xml))
                        {
                            // Use XmlReader for best performance
                            using (var reader = XmlReader.Create(stringReader, settings))
                            {
                                reader.MoveToContent();
                                reader.Read();

                                // Loop through each element
                                while (!reader.EOF && reader.ReadState == ReadState.Interactive)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();

                                    if (reader.NodeType == XmlNodeType.Element)
                                    {
                                        FetchDataFromXmlNode(reader, item);
                                    }
                                    else
                                    {
                                        reader.Read();
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

        /// <summary>
        /// Fetches the data from XML node.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="itemResult">The item result.</param>
        protected override void FetchDataFromXmlNode(XmlReader reader, MetadataResult<Episode> itemResult)
        {
            var item = itemResult.Item;

            switch (reader.Name)
            {
                case "season":
                    {
                        var number = reader.ReadElementContentAsString();

                        if (!string.IsNullOrWhiteSpace(number))
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
                        var number = reader.ReadElementContentAsString();

                        if (!string.IsNullOrWhiteSpace(number))
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
                        var number = reader.ReadElementContentAsString();

                        if (!string.IsNullOrWhiteSpace(number))
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
                        var val = reader.ReadElementContentAsString();

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval))
                            {
                                item.AirsBeforeEpisodeNumber = rval;
                            }
                        }

                        break;
                    }

                case "airsafter_season":
                    {
                        var val = reader.ReadElementContentAsString();

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval))
                            {
                                item.AirsAfterSeasonNumber = rval;
                            }
                        }

                        break;
                    }

                case "airsbefore_season":
                    {
                        var val = reader.ReadElementContentAsString();

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval))
                            {
                                item.AirsBeforeSeasonNumber = rval;
                            }
                        }

                        break;
                    }

                case "displayseason":
                    {
                        var val = reader.ReadElementContentAsString();

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval))
                            {
                                item.AirsBeforeSeasonNumber = rval;
                            }
                        }

                        break;
                    }

                case "displayepisode":
                    {
                        var val = reader.ReadElementContentAsString();

                        if (!string.IsNullOrWhiteSpace(val))
                        {
                            int rval;

                            // int.TryParse is local aware, so it can be probamatic, force us culture
                            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out rval))
                            {
                                item.AirsBeforeEpisodeNumber = rval;
                            }
                        }

                        break;
                    }


                default:
                    base.FetchDataFromXmlNode(reader, itemResult);
                    break;
            }
        }

        public EpisodeNfoParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem, IXmlReaderSettingsFactory xmlReaderSettingsFactory) : base(logger, config, providerManager, fileSystem, xmlReaderSettingsFactory)
        {
        }
    }
}
