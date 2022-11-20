namespace NfoMetadata.Configuration
{
    using System.Collections.Generic;

    using MediaBrowser.Common.Configuration;

    public class ConfigurationFactory : IConfigurationFactory
    {
        public const string ConfigurationKey = @"xbmcmetadata";

        public IEnumerable<ConfigurationStore> GetConfigurations()
        {
            return new[]
                       {
                           new ConfigurationStore
                               {
                                   ConfigurationType = typeof(XbmcMetadataOptions),
                                   Key = ConfigurationKey
                               }
                       };
        }
    }
}
