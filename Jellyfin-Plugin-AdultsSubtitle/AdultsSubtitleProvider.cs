using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin_Plugin_AdultsSubtitle
{
    public class AdultsSubtitleProvider : ISubtitleProvider
    {
       
        public string Name => "Adults Subtitle";
        private readonly ILogger<AdultsSubtitleProvider> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public IEnumerable<VideoContentType> SupportedMediaTypes => new[] { VideoContentType.Movie };


        public AdultsSubtitleProvider(ILogger<AdultsSubtitleProvider> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<SubtitleResponse> GetSubtitles(string id, CancellationToken cancellationToken)
        {
            if (Api.DownloadUrls.TryGetValue(id, out var targetSub))
            {
                _logger.LogInformation($"start download subtitle {targetSub}");
                using var client = new HttpClient();
                var response = await client.GetAsync(targetSub.Item1, cancellationToken);
                var ms = new MemoryStream();
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await stream.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;
                _logger.LogInformation($"subtitle {targetSub} download comlete");
                return new SubtitleResponse()
                {
                    Format = "srt",
                    Language = targetSub.Item2,
                    Stream = ms,
                };
            }
            throw new FileNotFoundException();
        }

        public async Task<IEnumerable<RemoteSubtitleInfo>> Search(SubtitleSearchRequest request, CancellationToken cancellationToken)
        {
            var fileInfo = new FileInfo(request.MediaPath);
            var searchName = fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
            _logger.LogInformation($"start search {searchName} {request.Language} subtitle ");
            if (!Api.LanguagesMaps.TryGetValue(request.Language,out var subCatLanguage))
            {
                _logger.LogInformation($"language({request.Language}) not support~");
                return Enumerable.Empty<RemoteSubtitleInfo>();
            }
            Api.DownloadUrls.Clear();

            var results = new List<RemoteSubtitleInfo>();
            using var client = _httpClientFactory.CreateClient();
            var searchResult = await Api.SearchAsync(client, searchName, cancellationToken);
            _logger.LogInformation($"search {searchName} {request.Language} subtitle  result --->{searchResult} ");
            if (!string.IsNullOrWhiteSpace(searchResult))
            {
                var downloadUrl = await Api.SearchDownloadUrlAsync(client, subCatLanguage, searchResult, cancellationToken);
                _logger.LogInformation($"search {searchName} {request.Language} subtitle  download url --->{downloadUrl} ");
                if (!string.IsNullOrWhiteSpace(downloadUrl))
                {
                    var id = Guid.NewGuid().ToString("N");
                    results.Add(new RemoteSubtitleInfo()
                    {
                        Format = "srt",
                        Name = downloadUrl[(downloadUrl.LastIndexOf("/") + 1)..],
                        ProviderName = Name,
                        Id = id
                    });
                    Api.DownloadUrls.TryAdd(id, (downloadUrl, request.Language));
                }
            }
            return results;
        }
    }
}
