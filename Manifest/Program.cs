using System.Security.Cryptography;
using System.Text.Json;

namespace Manifest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var version = args[0];
            var md5 = GetMD5(args[1]);
            var path = args[2];
            using var sr = new StreamReader(path);
            var json = sr.ReadToEnd();
            var packet = JsonSerializer.Deserialize(json, PackageInfoJsonContext.Default.IEnumerablePackageInfo)?.FirstOrDefault();

            var versionInfo = packet!.Versions.FirstOrDefault(p => p.Version == version);
            if (versionInfo == null)
            {
                packet!.Versions.Add(new VersionInfo()
                {
                    Checksum = md5,
                    TargetAbi = "10.8.0.0",
                    Changelog = $"update version {version}",
                    Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Version = version,
                    SourceUrl = $"https://github.com/fallingrust/Jellyfin-Plugin-AdultsSubtitle/releases/download/v{version}/AdultsSubtitle.zip"
                });
            }
            else
            {
                versionInfo.Checksum = md5;
                versionInfo.Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
            sr.Dispose();
            using var sw = new StreamWriter(path);
            sw.Write(JsonSerializer.Serialize(new PackageInfo[] { packet }, PackageInfoJsonContext.Default.IEnumerablePackageInfo));
        }

        private static string GetMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var fs = File.OpenRead(filePath);
            var buffer = md5.ComputeHash(fs);
            return BitConverter.ToString(buffer).Replace("-", "").ToLower();
        }
    }
}
