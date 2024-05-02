using Jellyfin_Plugin_AdultsSubtitle.Configuration;


#if __EMBY__
using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Logging;
using System.Reflection;
#else
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
#endif

namespace Jellyfin_Plugin_AdultsSubtitle
{
#if __EMBY__
    public class AdultsSubtitlePlugin : BasePluginSimpleUI<PluginConfiguration>
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly ILogger _logger;
        public AdultsSubtitlePlugin(IApplicationHost applicationHost, IApplicationPaths applicationPaths, ILogger logger) : base(applicationHost)
        {
            _applicationPaths = applicationPaths;
            _logger = logger;
            var path = Path.Combine(_applicationPaths.ProgramDataPath, "plugins", "AngleSharp.dll");
            _logger.Info($"Assembly Load {path}");
            try
            {
                Assembly.LoadFrom(path);
            }
            catch(Exception e)
            {
                _logger.Error(e.ToString());
            }
           
            Instance = this;
        }
#else

    public class AdultsSubtitlePlugin : BasePlugin<PluginConfiguration>
    {
        public AdultsSubtitlePlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }
#endif
        public static AdultsSubtitlePlugin? Instance { get; private set; }
        public override string Name => "Adults Subtitle";
        public override Guid Id
            => Guid.Parse("898269F2-F951-C3FF-B714-9E8F785BE3B2");
    }
}
