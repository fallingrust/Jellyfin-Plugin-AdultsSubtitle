#if !__EMBY__
using Microsoft.Extensions.DependencyInjection;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Subtitles;

namespace Jellyfin_Plugin_AdultsSubtitle
{
    /// <inheritdoc />
    public class ServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {

            serviceCollection.AddSingleton<ISubtitleProvider, AdultsSubtitleProvider>();
        }
    }
}
#endif
