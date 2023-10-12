namespace Cms.Integrations.Magento.Content.Product;

public class ProductService
{
    public static IList<ProductExternal> GetAll()
    {
        return new List<ProductExternal>
        {
            new()
            {
                Sku = "1234",
                Name = "Test title 1",
                Price = 20,
            },
            new()
            {
                Sku = "345",
                Name = "Test title 2",
                Price = 20,
            },
            new()
            {
                Sku = "456",
                Name = "Test title 3",
                Price = 20,
            },
            new()
            {
                Sku = "567",
                Name = "Test title 4",
                Price = 20,
            },
            new()
            {
                Sku = "8910",
                Name = "Test title 4",
                Price = 20,
            },
        };
    }

    public static ProductExternal GetProductBySku(string sku)
    {
        var productList = GetAll();
        
        return productList.FirstOrDefault(item => item.Sku.Equals(sku));
    }
}