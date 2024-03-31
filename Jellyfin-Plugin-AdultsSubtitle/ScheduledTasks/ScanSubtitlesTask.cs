using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin_Plugin_AdultsSubtitle.ScheduledTasks
{
    //public class ScanSubtitlesTask : IScheduledTask
    //{
    //    public string Name => "Scan Subtitles";

    //    public string Key => $"{AdultsSubtitlePlugin.Instance.Name}ScanSubtitles";

    //    public string Description => "Scan subtitles and download missing subtitles";

    //    public string Category => AdultsSubtitlePlugin.Instance.Name;
    //    private readonly ILibraryManager _libraryManager;
    //    private readonly ILogger<ScanSubtitlesTask> _logger;
    //    private readonly IProviderManager _providerManager;
    //    public ScanSubtitlesTask(ILibraryManager libraryManager, IProviderManager providerManager, ILogger<ScanSubtitlesTask> logger)
    //    {
    //        _libraryManager = libraryManager;
    //        _providerManager = providerManager;
    //        _logger = logger;
    //    }
    //    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    //    {
    //        var items = _libraryManager.GetItemList(new InternalItemsQuery()
    //        {
    //            MediaTypes = new[] { MediaType.Video },
    //            SourceTypes = new[] { SourceType.Library },
    //        });
    //        foreach (var item in items)
    //        {
    //            var option = _libraryManager.GetLibraryOptions(item);
    //        }
           
    //        _providerManager.GetAllMetadataPlugins();
    //        return Task.CompletedTask;
    //    }

    //    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    //    {
    //        yield return new TaskTriggerInfo
    //        {
    //            Type = TaskTriggerInfo.TriggerDaily,
    //            TimeOfDayTicks = TimeSpan.FromHours(5).Ticks
    //        };
    //    }
    //}
}
