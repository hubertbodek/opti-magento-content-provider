using Cms.Integrations.Magento.Content;
using Cms.Integrations.Magento.Provider;
using EPiServer.Construction;
using Newtonsoft.Json;

namespace Cms.Integrations.Magento;

public class ProductService : IProductService
{
    private readonly IContentTypeRepository _contentTypeRepository;
    private readonly IContentFactory _contentFactory;
    private readonly IdentityMappingService _mappingService;

    private readonly List<ProductItem> _productList = new List<ProductItem>()
    {
        new ProductItem()
        {
            Sku = "123",
            Title = "Test title 1",
            Description = "Test description 1",
            Price = 20,
        },
        new ProductItem()
        {
            Sku = "345",
            Title = "Test title 2",
            Description = "Test description 2",
            Price = 20,
        },
        new ProductItem()
        {
            Sku = "456",
            Title = "Test title 3",
            Description = "Test description 3",
            Price = 20,
        },
    };
    
    
    public ProductService(
        IContentTypeRepository contentTypeRepository, 
        IContentFactory contentFactory, 
        IdentityMappingService mappingService)
    {
        _contentTypeRepository = contentTypeRepository;
        _contentFactory = contentFactory;
        _mappingService = mappingService;
    }

    public IList<ProductItem> GetAll()
    {
        return _productList;
    }

    public ProductItem GetProductBySku(string sku)
    {
        return _productList.FirstOrDefault(item => item.Sku.Equals(sku));
    }
}