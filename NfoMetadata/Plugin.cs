namespace NfoMetadata
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Emby.Web.GenericEdit.Common;

    using MediaBrowser.Common;
    using MediaBrowser.Common.Configuration;
    using MediaBrowser.Common.Plugins;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Controller.Plugins;
    using MediaBrowser.Model.Drawing;
    using MediaBrowser.Model.Entities;
    using MediaBrowser.Model.Querying;

    using NfoMetadata.Configuration;

    public class Plugin : BasePluginSimpleUI<NfoMetadataOptions>, IHasThumbImage
    {
        private readonly Guid id = new Guid("E610BA80-9750-47BC-979D-3F0FC86E0000");

        private readonly IUserManager userManager;
        private readonly IConfigurationManager configurationManager;

        public Plugin(IApplicationHost applicationHost)
            : base(applicationHost)
        {
            this.userManager = applicationHost.Resolve<IUserManager>();
            this.configurationManager = applicationHost.Resolve<IConfigurationManager>();
        }

        public override Guid Id => this.id;

        public override string Name => "Nfo Metadata";

        public override string Description => "Nfo metadata support";

        public Stream GetThumbImage()
        {
            var type = this.GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }

        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        protected override NfoMetadataOptions OnBeforeShowUI(NfoMetadataOptions options)
        {
            options.UserList = this.LoadUsers();

            var xmlConfig = configurationManager.GetNfoConfiguration();
            xmlConfig.CopyTo(options);

            return options;
        }

        protected override bool OnOptionsSaving(NfoMetadataOptions options)
        {
            var xmlConfig = configurationManager.GetNfoConfiguration();
            options.CopyTo(xmlConfig);

            this.configurationManager.SaveNfoConfiguration(xmlConfig);
            return false;
        }

        private List<EditorSelectOption> LoadUsers()
        {
            var userQuery = new UserQuery
            {
                OrderBy = new[] { new Tuple<string, SortOrder>(ItemSortBy.Name, SortOrder.Ascending) }
            };

            var result = this.userManager.GetUsers(userQuery);

            var options = result.Items
                .Select(e => new EditorSelectOption(e.Id.ToString("N"), e.Name))
                .ToList();

            options.Insert(0, new EditorSelectOption("", ""));
            return options;
        }
    }
}