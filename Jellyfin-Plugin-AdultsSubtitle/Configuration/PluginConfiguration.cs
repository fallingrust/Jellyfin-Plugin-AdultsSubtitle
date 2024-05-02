#if __EMBY__
using Emby.Web.GenericEdit;
#else
using MediaBrowser.Model.Plugins;
#endif


namespace Jellyfin_Plugin_AdultsSubtitle.Configuration
{
#if __EMBY__
    public class PluginConfiguration : EditableOptionsBase
    {
        public override string EditorTitle => AdultsSubtitlePlugin.Instance.Name;
#else
    public class PluginConfiguration : BasePluginConfiguration
    {
#endif
    }
}
