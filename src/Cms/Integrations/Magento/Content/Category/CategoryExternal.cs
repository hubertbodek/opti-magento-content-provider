using Newtonsoft.Json;

namespace Cms.Integrations.Magento.Content.Category;

public class CategoryExternal : MagentoExternal
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("parent_id")]
    public int ParentId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("is_active")]
    public bool? IsActive { get; set; }

    [JsonProperty("position")]
    public int Position { get; set; }

    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("children")]
    public string Children { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("include_in_menu")]
    public bool IncludeInMenu { get; set; }

    [JsonProperty("custom_attributes")]
    public List<CustomAttribute> CustomAttributes { get; set; }
}

public class CustomAttribute
{
    [JsonProperty("attribute_code")]
    public string AttributeCode { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }
}