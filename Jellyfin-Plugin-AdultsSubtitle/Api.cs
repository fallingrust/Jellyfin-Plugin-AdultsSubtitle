using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Collections.Concurrent;

namespace Jellyfin_Plugin_AdultsSubtitle
{
    public class Api
    {
        public static readonly ConcurrentDictionary<string, (string, string)> DownloadUrls = new();
        public static readonly Dictionary<string, string> LanguagesMaps = new()
        {
            {"chi","zh-CN"},
            {"eng","en"},
        };
        private static readonly HtmlParser _parser = new();
        public static async Task<string?> SearchDownloadUrlAsync(HttpClient client, string language, string url, CancellationToken cancellationToken)
        {
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

        public static async Task<string?> SearchAsync(HttpClient client, string name, CancellationToken cancellationToken)
        {
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
