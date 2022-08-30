namespace NfoMetadata.Configuration
{
    using MediaBrowser.Common.Configuration;

    public static class ConfigurationExtensions
    {
        public static XbmcMetadataOptions GetNfoConfiguration(this IConfigurationManager manager)
        {
            return manager.GetConfiguration<XbmcMetadataOptions>(ConfigurationFactory.ConfigurationKey);
        }
        
        public static void SaveNfoConfiguration(this IConfigurationManager manager, XbmcMetadataOptions xmlOptions)
        {
            manager.SaveConfiguration(ConfigurationFactory.ConfigurationKey, xmlOptions);
        }
        
        public static void CopyTo(this XbmcMetadataOptions xmlOptions, NfoMetadataOptions options)
        {
            options.EnablePathSubstitution = xmlOptions.EnablePathSubstitution;
            options.ReleaseDateFormat = xmlOptions.ReleaseDateFormat;
            options.UserId = xmlOptions.UserId;
            options.SaveImagePathsInNfo = xmlOptions.SaveImagePathsInNfo;
        }

        public static void CopyTo(this NfoMetadataOptions options, XbmcMetadataOptions xmlOptions)
        {
            xmlOptions.EnablePathSubstitution = options.EnablePathSubstitution;
            xmlOptions.ReleaseDateFormat = options.ReleaseDateFormat;
            xmlOptions.UserId = options.UserId;
            xmlOptions.SaveImagePathsInNfo = options.SaveImagePathsInNfo;
        }
    }
}