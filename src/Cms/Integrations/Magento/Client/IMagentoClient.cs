using Cms.Integrations.Magento.Content.Product;

namespace Cms.Integrations.Magento.Client;

public interface IMagentoClient
{
    Task<List<ProductExternal>> GetProducts();
}