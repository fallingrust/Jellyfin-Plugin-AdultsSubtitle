using System.Security.Cryptography;
using System.Text.Json;

namespace Manifest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "manifest.json");
            var version = args[0];
            var sourceUrl = $"https://github.com/fallingrust/Jellyfin-Plugin-AdultsSubtitle/releases/download/{version}/AdultsSubtitle.zip";
            using var client = new HttpClient();
            var response = await client.GetAsync(sourceUrl);
            var stream = await response.Content.ReadAsStreamAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;


            var md5 = GetMD5(ms);
            
            using var sr = new StreamReader(localPath);
            var json = sr.ReadToEnd();
            var packet = JsonSerializer.Deserialize(json, PackageInfoJsonContext.Default.IEnumerablePackageInfo)?.FirstOrDefault();

            var versionInfo = packet!.Versions.FirstOrDefault(p => p.Version == version);
            if (versionInfo == null)
            {
                packet!.Versions.Add(new VersionInfo()
                {
                    Checksum = md5,
                    TargetAbi = "10.8.13",
                    Changelog = $"update version {version}",
                    Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Version = version.Replace("v",""),
                    SourceUrl = sourceUrl
                });
            }
            else
            {
                versionInfo.Checksum = md5;
                versionInfo.Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            sr.Dispose();
            using var sw = new StreamWriter(localPath);
            sw.Write(JsonSerializer.Serialize(new PackageInfo[] { packet }, PackageInfoJsonContext.Default.IEnumerablePackageInfo));
        }

        private static string GetMD5(Stream ms)
        {
            using var md5 = MD5.Create();
           
            var buffer = md5.ComputeHash(ms);
            return BitConverter.ToString(buffer).Replace("-", "").ToLower();
        }
    }
}
