using Jellyfin_Plugin_AdultsSubtitle.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin_Plugin_AdultsSubtitle
{
    public class AdultsSubtitlePlugin : BasePlugin<PluginConfiguration>
    {
        public AdultsSubtitlePlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }
        public static AdultsSubtitlePlugin? Instance { get; private set; }
        public override string Name => "Adults Subtitle";
        public override Guid Id
            => Guid.Parse("898269F2-F951-C3FF-B714-9E8F785BE3B2");
    }
}
