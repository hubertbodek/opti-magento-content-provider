using Cms.Integrations.Magento.Content.Category;
using Cms.Integrations.Magento.Content.Product;

namespace Cms.Integrations.Magento.Client;

public interface IMagentoClient
{
    Task<string> GetToken();
    
    Task<List<ProductExternal>> GetProducts();
    
    Task<List<CategoryExternal>> GetCategories();
}