using Newtonsoft.Json;

namespace Cms.Integrations.Magento.Models;

public class MediaGalleryEntry
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("media_type")]
    public string MediaType { get; set; }

    [JsonProperty("label")]
    public string Label { get; set; }

    [JsonProperty("position")]
    public int Position { get; set; }

    [JsonProperty("disabled")]
    public bool Disabled { get; set; }

    [JsonProperty("types")]
    public List<string> Types { get; set; }

    [JsonProperty("file")]
    public string File { get; set; }
}