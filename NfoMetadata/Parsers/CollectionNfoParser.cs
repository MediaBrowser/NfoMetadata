using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System;
using System.Xml;
using MediaBrowser.Model.IO;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;

namespace NfoMetadata.Parsers
{
    public class CollectionNfoParser : BaseNfoParser<BoxSet>
    {
        /// <summary>
        /// Fetches the data from XML node.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="itemResult">The item result.</param>
        protected override async Task FetchDataFromXmlNode(XmlReader reader, MetadataResult<BoxSet> itemResult)
        {
            var item = itemResult.Item;

            switch (reader.Name)
            {
                case "displayorder":

                    {
                        var val = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

                        if (Enum.TryParse<CollectionDisplayOrder>(val, true, out CollectionDisplayOrder result))
                        {
                            itemResult.Item.DisplayOrder = result;
                        }
                    }
                    break;

                default:
                    await base.FetchDataFromXmlNode(reader, itemResult).ConfigureAwait(false);
                    break;
            }
        }

        public CollectionNfoParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
        {
        }
    }
}
