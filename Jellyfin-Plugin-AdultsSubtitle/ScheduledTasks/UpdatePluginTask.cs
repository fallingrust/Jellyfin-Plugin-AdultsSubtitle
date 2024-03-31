using AngleSharp.Dom;
using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Model.Updates;
using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace Jellyfin_Plugin_AdultsSubtitle.ScheduledTasks
{
    public class UpdatePluginTask : IScheduledTask
    {
        public string Name => "Update Plugin";

        public string Key => $"{AdultsSubtitlePlugin.Instance.Name}UpdatePlugin";

        public string Description => $"Updates {AdultsSubtitlePlugin.Instance.Name} plugin to latest version.";

        public string Category => AdultsSubtitlePlugin.Instance.Name;

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UpdatePluginTask> _logger;
        private readonly IPluginManager _pluginManager;
        private readonly IApplicationPaths _applicationPaths;
        private readonly IApplicationHost _applicationHost;
        public UpdatePluginTask(IHttpClientFactory httpClientFactory, IPluginManager pluginManager, IApplicationPaths applicationPaths, IApplicationHost applicationHost, ILogger<UpdatePluginTask> logger)
        {
            _httpClientFactory = httpClientFactory;
            _pluginManager = pluginManager;
            _applicationPaths = applicationPaths;
            _applicationHost = applicationHost;
            _logger = logger;
        }
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            try
            {
                var curVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var lastestVersion = await GetLatestVersionAsync(cancellationToken);
                _logger.LogInformation($"Updates {AdultsSubtitlePlugin.Instance.Name} plugin to latest version:curVersion{curVersion} lastestVersion:{lastestVersion} ");
                if (curVersion != null && curVersion.CompareTo(lastestVersion.Value.Item1) < 0)
                {
                    using var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(lastestVersion.Value.Item2, cancellationToken).ConfigureAwait(false);
                    var zipStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var ms = new MemoryStream();
                    await zipStream.CopyToAsync(ms, cancellationToken);
                    ms.Position = 0;
                    using var archive = new ZipArchive(ms);
                    archive.ExtractToDirectory(Path.Combine(_applicationPaths.PluginsPath, $"AdultsSubtitle_{lastestVersion.Value.Item1}"), true);

                    var curVersionPlugin = _pluginManager.GetPlugin(AdultsSubtitlePlugin.Instance.Id, curVersion);
                    if (curVersionPlugin != null)
                        _pluginManager.RemovePlugin(curVersionPlugin);
                    _logger.LogInformation("remove cur version plugin");   
                    _applicationHost.NotifyPendingRestart();
                    _logger.LogInformation("update plugin complete");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield return new TaskTriggerInfo
            {
                Type = TaskTriggerInfo.TriggerDaily,
                TimeOfDayTicks = TimeSpan.FromHours(5).Ticks
            };
        }

      

        private async Task<(Version,string)?> GetLatestVersionAsync(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync("https://raw.githubusercontent.com/fallingrust/Jellyfin-Plugin-AdultsSubtitle/master/manifest.json", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var packInfo = JsonSerializer.Deserialize<List<PackageInfo>>(content)?.FirstOrDefault();
            if (packInfo != null)
            {
                var latestVersion = packInfo.Versions.OrderByDescending(p => p.VersionNumber).FirstOrDefault();
                if (latestVersion != null && latestVersion.SourceUrl != null)
                    return (latestVersion.VersionNumber, latestVersion.SourceUrl);
            }
            return null;
        }
    }
}
