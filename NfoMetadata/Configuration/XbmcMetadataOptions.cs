
namespace NfoMetadata.Configuration
{
    public class XbmcMetadataOptions
    {
        public string UserIdForUserData { get; set; }

        public string ReleaseDateFormat { get; set; } = "yyyy-MM-dd";

        public bool SaveImagePathsInNfoFiles { get; set; }

        public bool PreferMovieNfo { get; set; }
    }
}
