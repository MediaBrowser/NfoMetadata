using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

namespace NfoMetadata.Parsers
{
    public class GameNfoParser : BaseNfoParser<Game>
    {
        public GameNfoParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
        {
        }
    }
}
