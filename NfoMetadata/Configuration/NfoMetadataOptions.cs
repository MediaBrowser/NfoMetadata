namespace NfoMetadata.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;

    using Emby.Web.GenericEdit;
    using Emby.Web.GenericEdit.Common;

    using MediaBrowser.Model.Attributes;
    using MediaBrowser.Model.LocalizationAttributes;
    using NfoMetadata.Properties;

    public class NfoMetadataOptions : EditableOptionsBase
    {
        public NfoMetadataOptions()
        {
            this.DateFormatList = new List<EditorSelectOption>
                { new EditorSelectOption("yyyy-MM-dd", "yyyy-MM-dd") };

            this.UserList = new List<EditorSelectOption>
                                { new EditorSelectOption("", "") };
        }

        /// <summary>Gets the editor title.</summary>
        /// <value>The editor title.</value>
        public override string EditorTitle => "Nfo Metadata Settings";

        /// <summary>Gets the editor description.</summary>
        /// <value>The editor description.</value>
        public override string EditorDescription => Resources.HeaderKodiMetadataHelp;

        [Browsable(false)]
        public List<EditorSelectOption> DateFormatList { get; set; }

        [Browsable(false)]
        public List<EditorSelectOption> UserList { get; set; }

        [LocalizedDisplayName("LabelKodiMetadataUser", typeof(Resources))]
        [LocalizedDescription("LabelKodiMetadataUserHelp", typeof(Resources))]
        [SelectItemsSource(nameof(UserList))]
        public string UserId { get; set; }

        [LocalizedDisplayName("LabelKodiMetadataDateFormat", typeof(Resources))]
        [LocalizedDescription("LabelKodiMetadataDateFormatHelp", typeof(Resources))]
        [SelectItemsSource(nameof(DateFormatList))]
        public string ReleaseDateFormat { get; set; } = @"yyyy-MM-dd";

        [LocalizedDisplayName("LabelKodiMetadataSaveImagePaths", typeof(Resources))]
        [LocalizedDescription("LabelKodiMetadataSaveImagePathsHelp", typeof(Resources))]
        public bool SaveImagePathsInNfo { get; set; }

        [LocalizedDisplayName("LabelKodiMetadataEnablePathSubstitution", typeof(Resources))]
        [LocalizedDescription("LabelKodiMetadataEnablePathSubstitutionHelp", typeof(Resources))]
        public bool EnablePathSubstitution { get; set; } = true;

        [LocalizedDisplayName("LabelKodiMetadataEnableExtraThumbs", typeof(Resources))]
        [LocalizedDescription("LabelKodiMetadataEnableExtraThumbsHelp", typeof(Resources))]
        public bool EnableExtraThumbs { get; set; } = true;
    }
}
