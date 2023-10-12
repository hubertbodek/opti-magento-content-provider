using Cms.Integrations.Magento.Client.Responses;
using Cms.Integrations.Magento.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cms.Integrations.Magento.Content.Product;

public class ProductExternal : MagentoExternal
{
    [JsonProperty("id")]
    public int Id { get; init; }
    
    [JsonProperty("sku")]
    public string Sku { get; init; }
    
    [JsonProperty("name")]
    public string Name { get; init; }

    [JsonProperty("price")]
    public int Price { get; init; }
    
    [JsonProperty("type_id")]
    public string TypeId { get; set; }

    [JsonProperty("extension_attributes")]
    public ExtensionAttributes ExtensionAttributes { get; set; }
    
    [JsonProperty("media_gallery_entries")]
    public List<MediaGalleryEntry> MediaGalleryEntries { get; set; }
}

public enum ProductType
{
    Simple,
    Configurable
}