﻿using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using System.Globalization;
using System.Xml;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;

namespace NfoMetadata.Parsers
{
    public class SeasonNfoParser : BaseNfoParser<Season>
    {
        /// <summary>
        /// Fetches the data from XML node.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="itemResult">The item result.</param>
        protected override async Task FetchDataFromXmlNode(XmlReader reader, MetadataResult<Season> itemResult)
        {
            var item = itemResult.Item;

            switch (reader.Name)
            {
                case "seasonnumber":
                    {
                        var number = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(number))
                        {
                            int num;

                            if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                            {
                                item.IndexNumber = num;
                            }
                        }
                        break;
                    }

                default:
                    await base.FetchDataFromXmlNode(reader, itemResult).ConfigureAwait(false);
                    break;
            }
        }

        public SeasonNfoParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
        {
        }
    }
}
