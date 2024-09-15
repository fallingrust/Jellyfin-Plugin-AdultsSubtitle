using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Subtitles;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin_Plugin_AdultsSubtitle
{
    public class PluginRegistrator : IPluginServiceRegistrator
    { 
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {           
            serviceCollection.AddSingleton<ISubtitleProvider, AdultsSubtitleProvider>();
        }
    }
}
