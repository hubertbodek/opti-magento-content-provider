using Cms.Integrations.Magento.Content.Product;
using Newtonsoft.Json;

namespace Cms.Integrations.Magento.Client.Responses;

public class ProductsResponse
{
    [JsonProperty("items")]
    public List<ProductExternal> Items;

    [JsonProperty("total_count")]
    public int TotalCount;
}
