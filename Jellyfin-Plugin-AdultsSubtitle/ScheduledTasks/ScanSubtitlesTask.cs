using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin_Plugin_AdultsSubtitle.ScheduledTasks
{
    public class ScanSubtitlesTask : IScheduledTask
    {
        public string Name => "Scan Subtitles";

        public string Key => $"{AdultsSubtitlePlugin.Instance.Name}ScanSubtitles";

        public string Description => "Scan subtitles and download missing subtitles";

        public string Category => AdultsSubtitlePlugin.Instance.Name;
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<ScanSubtitlesTask> _logger;
        private readonly ISubtitleManager _subtitleManager;
        private readonly IHttpClientFactory _httpClientFactory;
        public ScanSubtitlesTask(ILibraryManager libraryManager, ISubtitleManager subtitleManager, IHttpClientFactory httpClientFactory, ILogger<ScanSubtitlesTask> logger)
        {
            _subtitleManager = subtitleManager;
            _libraryManager = libraryManager;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            var items = _libraryManager.GetItemList(new InternalItemsQuery()
            {
                IsVirtualItem = false,
                MediaTypes = new[] { MediaType.Video },
                SourceTypes = new[] { SourceType.Library },
            });
            progress.Report(0);
            double index = 1.0;
            foreach (var item in items)
            {
                if (item is Movie movie)
                {
                    var dirInfo = new DirectoryInfo(movie.ContainingFolderPath);
                   
                    if (!movie.HasSubtitles 
                        && !movie.FileNameWithoutExtension.ToLower().EndsWith("-C") 
                        && !dirInfo.GetFiles().Any(p => p.Name.Contains(movie.FileNameWithoutExtension) && p.Extension == ".srt"))
                    {
                        _logger.LogInformation($"{movie.FileNameWithoutExtension} has no subtitle");
                        var option = _libraryManager.GetLibraryOptions(item);
                        var language = "chi";
                        if (option.SubtitleDownloadLanguages != null && option.SubtitleDownloadLanguages.Length > 0)
                        {
                            language = option.SubtitleDownloadLanguages[0];
                        }
                       
                        if (option != null
                            && !option.DisabledSubtitleFetchers.Contains(AdultsSubtitlePlugin.Instance.Name)
                            && option.SubtitleFetcherOrder.Contains(AdultsSubtitlePlugin.Instance.Name) 
                            && Api.LanguagesMaps.TryGetValue(language, out var subCatLanguage))
                        {
                            using var client = _httpClientFactory.CreateClient();
                            try
                            {
                                var searchResult = await Api.SearchAsync(client, movie.FileNameWithoutExtension, cancellationToken);
                                _logger.LogInformation($"search {movie.FileNameWithoutExtension} {language} subtitle  result --->{searchResult} ");
                                if (!string.IsNullOrWhiteSpace(searchResult))
                                {
                                    var downloadUrl = await Api.SearchDownloadUrlAsync(client, subCatLanguage, searchResult, cancellationToken);
                                    _logger.LogInformation($"search{movie.FileNameWithoutExtension} {language} subtitle  download url --->{downloadUrl} ");
                                    if (!string.IsNullOrWhiteSpace(downloadUrl))
                                    {
                                        _logger.LogInformation($"start download subtitle {downloadUrl}");

                                        var response = await client.GetAsync(downloadUrl, cancellationToken);
                                        var ms = new MemoryStream();
                                        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                                        await stream.CopyToAsync(ms, cancellationToken);
                                        ms.Position = 0;
                                        _logger.LogInformation($"subtitle {downloadUrl} download comlete");


                                        await _subtitleManager.UploadSubtitle(movie, new SubtitleResponse()
                                        {
                                            Format = "srt",
                                            Language = language,
                                            Stream = ms,
                                        });
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError(ex.ToString());
                            }                          
                        }
                    }
                    progress.Report(index / items.Count);
                }
                index += 1;
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
    }
}
