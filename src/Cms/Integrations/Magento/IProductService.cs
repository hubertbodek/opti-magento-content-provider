using Cms.Integrations.Magento.Content;

namespace Cms.Integrations.Magento;

public interface IProductService
{
    public IList<ProductItem> GetAll();
    public ProductItem GetProductBySku(string sku);
}