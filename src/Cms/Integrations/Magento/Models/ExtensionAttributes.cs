using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Cms.Integrations.Magento.Models;

public class ExtensionAttributes
{
    [JsonProperty("category_links")]
    [CanBeNull] 
    public List<CategoryLink> CategoryLinks { get; set; }

    [JsonProperty("configurable_product_options")]
    [CanBeNull]
    public List<ConfigurableProductOption> ConfigurableProductOptions { get; set; }
    
    [JsonProperty("configurable_product_links")]
    [CanBeNull]
    public List<int> ConfigurableProductLinks { get; set; }
}

public class CategoryLink
{
    [JsonProperty("position")]
    public int Position { get; set; }

    [JsonProperty("category_id")]
    public string CategoryId { get; set; }
}

public class ConfigurableProductOption
{
    public int Id { get; set; }
    public string AttributeId { get; set; }
    public string Label { get; set; }
    public int Position { get; set; }
    public List<ValueItem> Values { get; set; }
    public int ProductId { get; set; }
}

public class ValueItem
{
    public int ValueIndex { get; set; }
}