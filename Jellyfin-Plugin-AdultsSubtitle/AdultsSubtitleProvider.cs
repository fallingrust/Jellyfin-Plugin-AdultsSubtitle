using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Jellyfin_Plugin_AdultsSubtitle
{
    public class AdultsSubtitleProvider : ISubtitleProvider
    {
        private static readonly HtmlParser _parser = new();
        private static readonly ConcurrentDictionary<string, (string,string)> _urls = new();
        public static readonly Dictionary<string, string> LanguagesMaps = new()
        {
            {"chi","zh-CN"},
            {"eng","en"},
        };
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
            if(_urls.TryGetValue(id, out var targetSub))
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(targetSub.Item1, cancellationToken);
                var ms = new MemoryStream();
                var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await stream.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;
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
            if (!LanguagesMaps.TryGetValue(request.Language,out var subCatLanguage))
            {
                _logger.LogInformation($"language({request.Language}) not support~");
                return Enumerable.Empty<RemoteSubtitleInfo>();
            }
            _urls.Clear();

            var results = new List<RemoteSubtitleInfo>();

            var searchResult = await SearchAsync(request.Name, cancellationToken);
            if (!string.IsNullOrWhiteSpace(searchResult))
            {
                var downloadUrl = await SearchDownloadUrlAsync(subCatLanguage, searchResult, cancellationToken);
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
                    _urls.TryAdd(id, (downloadUrl, request.Language));
                }
            }
            return results;
        }


        private async Task<string?> SearchDownloadUrlAsync(string language, string url, CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://www.subtitlecat.com{url}", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var document = _parser.ParseDocument(content);
            var element = document.All.FirstOrDefault(p => p.Id == $"download_{language}");
            if (element is IHtmlAnchorElement anchorElement)
            {
                return $"https://www.subtitlecat.com{anchorElement.Href.Replace("about://", "")}";
            }
            return null;
        }

        private async Task<string?> SearchAsync(string name, CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://www.subtitlecat.com/index.php?search={name}", cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = _parser.ParseDocument(content);
            var element = document.All.FirstOrDefault(p => p is IHtmlAnchorElement anchorElement && anchorElement.Href.ToLower().Contains(name.ToLower()));
            if (element is IHtmlAnchorElement anchorElement)
            {
                return anchorElement.Href.Replace("about://", "");
            }
            return null;
        }
    }
}
