using System.Text.Json.Serialization;

namespace Manifest
{
    public class PackageInfo
    {
        public PackageInfo()
        {
            Versions = Array.Empty<VersionInfo>();
            Category = string.Empty;
            Name = string.Empty;
            Overview = string.Empty;
            Owner = string.Empty;
            Description = string.Empty;
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }


        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; }

        [JsonPropertyName("owner")]
        public string Owner { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("guid")]
        public Guid Id { get; set; }

        [JsonPropertyName("versions")]

        public IList<VersionInfo> Versions { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }
    }

    [JsonSerializable(typeof(IEnumerable<PackageInfo>))]
    [JsonSerializable(typeof(PackageInfo))]
    public partial class PackageInfoJsonContext : JsonSerializerContext
    {

    }
}
