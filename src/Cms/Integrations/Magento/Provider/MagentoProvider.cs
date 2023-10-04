using Cms.Integrations.Magento.Content;
using EPiServer.Construction;
using EPiServer.Construction.Internal;
using EPiServer.Core.Internal;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace Cms.Integrations.Magento.Provider;

public class MagentoProvider : ContentProvider
{
    private readonly Injected<IdentityMappingService> _identityMappingService;
    private readonly Injected<IProductService> _productService;
    private readonly Injected<IContentTypeRepository> _contentTypeRepository;
    private readonly Injected<IContentRepository> _contentRepository;
    private readonly Injected<IContentFactory> _contentFactory;

    public const string Key = "magento";

    protected override IContent LoadContent(ContentReference contentLink, ILanguageSelector languageSelector)
    {
        var mappedItem = _identityMappingService.Service.Get(contentLink);
        if (mappedItem == null) return null;
        
        var sku = mappedItem.ExternalIdentifier.Segments[1];
        var product = _productService.Service.GetProductBySku(sku);

        return MapToContent(product);
    }

    protected override IList<GetChildrenReferenceResult> LoadChildrenReferencesAndTypes(ContentReference contentLink,
        string languageId, out bool languageSpecific)
    {
        languageSpecific = false;

        var products = _productService.Service.GetAll();

        return products.Select(p => new GetChildrenReferenceResult()
        {
            ContentLink = _identityMappingService.Service.Get(MappedIdentity.ConstructExternalIdentifier(ProviderKey, p.Sku))
                .ContentLink,
            ModelType = typeof(ProductContent)
        }).ToList();
    }
    
    public static ContentFolder GetEntryPoint(string name)
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

        if (contentRepository.GetBySegment(ContentReference.RootPage, name, LanguageSelector.AutoDetect()) is
            ContentFolder folder)
        {
            return folder;
        };
        
        folder = contentRepository.GetDefault<ContentFolder>(ContentReference.RootPage);
        folder.Name = name;
        contentRepository.Save(folder, SaveAction.Publish, AccessLevel.NoAccess);

        return folder;
    }
    
    public ProductContent MapToContent(ProductItem product)
    {
        var type = _contentTypeRepository.Service.Load(typeof(ProductContent));
        var productContent = _contentFactory.Service.CreateContent(type, new BuildingContext(type)
        {
            Parent = _contentRepository.Service.Get<ContentFolder>(EntryPoint),
        }) as ProductContent;

        productContent.Status = VersionStatus.Published;
        productContent.IsPendingPublish = false;
        productContent.StartPublish = DateTime.Now.Subtract(TimeSpan.FromDays(14));
        
        var externalId = MappedIdentity.ConstructExternalIdentifier(ProviderKey, product.Sku);
        
        var mappedContent = _identityMappingService.Service.Get(externalId, true);
        productContent.ContentLink = mappedContent.ContentLink;
        productContent.ContentGuid = mappedContent.ContentGuid;

        productContent.Sku = product.Sku;
        productContent.Title = product.Title;
        productContent.Description = product.Description;
        productContent.Price = product.Price;

        productContent.MakeReadOnly();
        
        return productContent;
    }
}