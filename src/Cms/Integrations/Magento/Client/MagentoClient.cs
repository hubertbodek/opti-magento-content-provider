using Cms.Integrations.Magento.Client.Responses;
using Cms.Integrations.Magento.Content.Category;
using Cms.Integrations.Magento.Content.Product;
using Newtonsoft.Json;

namespace Cms.Integrations.Magento.Client;

public class MagentoClient : IMagentoClient
{
    private readonly HttpClient _httpClient;
    
    public MagentoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetToken()
    {
        var response = _httpClient.GetAsync(ApiUrlConstants.Token).Result;
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadAsStringAsync();

        return token;
    }

    public async Task<List<ProductExternal>> GetProducts()
    {
        var response = _httpClient.GetAsync(ApiUrlConstants.Products).Result;
        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadAsStringAsync();
        var productsResult = JsonConvert.DeserializeObject<ProductsResponse>(products);
        
        return productsResult?.Items;
    }

    public async Task<List<CategoryExternal>> GetCategories()
    {
        var response = _httpClient.GetAsync(ApiUrlConstants.Categories).Result;
        response.EnsureSuccessStatusCode();

        var categories = await response.Content.ReadAsStringAsync();
        var categoriesResult = JsonConvert.DeserializeObject<CategoriesResponse>(categories);
        
        return categoriesResult?.Items;
    }
}