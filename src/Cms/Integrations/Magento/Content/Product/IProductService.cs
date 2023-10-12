namespace Cms.Integrations.Magento.Content.Product;

public interface IProductService
{
    public IList<ProductExternal> GetAll();
    public ProductExternal GetProductBySku(string sku);
}