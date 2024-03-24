using System.Text.Json.Serialization;

namespace Manifest
{
    public class VersionInfo
    {
        
        [JsonPropertyName("version")]
        public string? Version { get; set; }
      
        /// <summary>
        /// Gets or sets the changelog for this version.
        /// </summary>
        /// <value>The changelog.</value>
        [JsonPropertyName("changelog")]
        public string? Changelog { get; set; }

        /// <summary>
        /// Gets or sets the ABI that this version was built against.
        /// </summary>
        /// <value>The target ABI version.</value>
        [JsonPropertyName("targetAbi")]
        public string? TargetAbi { get; set; }

        /// <summary>
        /// Gets or sets the source URL.
        /// </summary>
        /// <value>The source URL.</value>
        [JsonPropertyName("sourceUrl")]
        public string? SourceUrl { get; set; }

        /// <summary>
        /// Gets or sets a checksum for the binary.
        /// </summary>
        /// <value>The checksum.</value>
        [JsonPropertyName("checksum")]
        public string? Checksum { get; set; }

        /// <summary>
        /// Gets or sets a timestamp of when the binary was built.
        /// </summary>
        /// <value>The timestamp.</value>
        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }
    }

    [JsonSerializable(typeof(IEnumerable<VersionInfo>))]
    [JsonSerializable(typeof(VersionInfo))]
    public partial class VersionInfoJsonContext : JsonSerializerContext
    {

    }
}
