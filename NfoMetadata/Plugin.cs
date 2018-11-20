using System;
using MediaBrowser.Common.Plugins;
using System.IO;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Plugins;
using System.Collections.Generic;

namespace NfoMetadata
{
    public class Plugin : BasePlugin, IHasThumbImage, IHasWebPages
    {
        private Guid _id = new Guid("E610BA80-9750-47BC-979D-3F0FC86E0081");
        public override Guid Id
        {
            get { return _id; }
        }

        public override string Name
        {
            get { return StaticName; }
        }

        public static string StaticName
        {
            get { return "Nfo Metadata"; }
        }

        public override string Description
        {
            get
            {
                return "Nfo metadata support";
            }
        }

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
        }

        public ImageFormat ThumbImageFormat
        {
            get
            {
                return ImageFormat.Png;
            }
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "nfo",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.nfo.html",
                    EnableInMainMenu = true,
                    MenuSection = "server",
                    MenuIcon = "notes"
                },
                new PluginPageInfo
                {
                    Name = "nfojs",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.nfo.js"
                }
            };
        }
    }
}