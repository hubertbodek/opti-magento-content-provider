using Cms.Integrations.Magento.Content;
using Cms.Integrations.Magento.Content.Category;
using Cms.Integrations.Magento.Content.Product;
using Cms.Pages.ExternalCategoryPage;
using EPiServer.Construction;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace Cms.Integrations.Magento.Helpers;

public class ProviderHelper
{
    private readonly Injected<IContentTypeRepository> _contentTypeRepository;
    private readonly Injected<IContentFactory> _contentFactory;
    private readonly Injected<IContentRepository> _contentRepository;
    private readonly Injected<IdentityMappingService> _identityMappingService;

    private readonly ContentReference _entryPoint;
    private readonly string _providerKey;
    
    public ProviderHelper(ContentReference entryPoint, string providerKey)
    {
        _entryPoint = entryPoint;
        _providerKey = providerKey;
    }

    public ProductContent MapProductToContent(ProductExternal product)
    {
        var productContent =
            CreateContent<ProductContent>(MagentoResourceType.Product, MagentoContentType.ProductContent, product.Sku);

        productContent.Sku = product.Sku;
        productContent.Name = product.Name;
        productContent.Title = product.Name;
        productContent.Price = product.Price;

        productContent.MakeReadOnly();
        
        return productContent;
    }
    
    public CategoryContent MapCategoryToContent(CategoryExternal category)
    {
        var categoryContent =
            CreateContent<CategoryContent>(MagentoResourceType.Category, MagentoContentType.CategoryContent, category.Id.ToString());
        
        categoryContent.Id = category.Id.ToString();
        categoryContent.Title = category.Name;
        categoryContent.Name = category.Name;
        categoryContent.Description = "Temporary test description";
        
        categoryContent.MakeReadOnly();
        
        return categoryContent;
    }

    private void CreateOrUpdateProductPage(ProductContent product)
    {
        CreatePage<ExternalCategoryPage>(ContentReference.StartPage, (item) =>
        {
            item.Name = product.Name;
        });
    }

    private static void CreatePage<T>(ContentReference parent, Action<T> applyProperties) where T : PageData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var page = contentRepository.GetDefault<T>(parent);

        applyProperties(page);

        contentRepository.Save(page, EPiServer.DataAccess.SaveAction.Publish);
    }

    public IContent CreateFolder(MappedIdentity mappedItem, MagentoContentType contentType, string name)
    {
        return CreateContent(
            mappedItem.ContentLink.ID,
            mappedItem.ContentGuid,
            int.Parse(RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3])),
            contentType,
            typeof(MagentoContentFolder),
            name);
    }

    private T CreateContent<T>(MagentoResourceType resourceType, MagentoContentType contentType, string id) where T : ContentBase
    {
        var type = _contentTypeRepository.Service.Load(typeof(T));
        var content = _contentFactory.Service.CreateContent(type, new BuildingContext(type)
        {
            Parent = _contentRepository.Service.Get<ContentFolder>(_entryPoint),
        }) as T;

        if (content == null)
        {
            return default;
        }
        
        content.Status = VersionStatus.Published;
        content.IsPendingPublish = false;
        content.StartPublish = DateTime.Now;
        
        var externalId = CreateExternalId(resourceType, contentType, id);

        var mappedContent = _identityMappingService.Service.Get(externalId, true);
        content.ContentLink = mappedContent.ContentLink;
        content.ContentGuid = mappedContent.ContentGuid;
        
        return content;
    }
    
    public IContent CreateContent(int contentId, Guid contentGuid, int parentContentId, MagentoContentType contentType, Type modelType, string name)
    {
        /* Find parent */
        var parentLink = _entryPoint;
        /* Getting parent id */
        if (contentType != MagentoContentType.ContentFolder && contentType != MagentoContentType.CategoryFolder)
            parentLink = new ContentReference(parentContentId, _providerKey);

        var epiContentType = _contentTypeRepository.Service.Load(modelType);
        var content = _contentFactory.Service.CreateContent(epiContentType);

        content.ContentTypeID = epiContentType.ID;
        content.ParentLink = parentLink;
        content.ContentGuid = contentGuid;
        content.ContentLink = new ContentReference(contentId, _providerKey);
        content.Name = name;

        var securable = content as IContentSecurable;
        securable.GetContentSecurityDescriptor().AddEntry(new AccessControlEntry(EveryoneRole.RoleName, AccessLevel.Read));

        var versionable = content as IVersionable;
        if (versionable != null)
        {
            versionable.Status = VersionStatus.Published;
            versionable.IsPendingPublish = false;
            versionable.StartPublish = DateTime.Now.Subtract(TimeSpan.FromDays(14));;
        }

        var changeTrackable = content as IChangeTrackable;
        if (changeTrackable != null)
        {
            changeTrackable.Created = DateTime.Now;
            changeTrackable.Changed = DateTime.Now;
            changeTrackable.Saved = DateTime.Now;
        }

        return content;
    }
    
    public Uri CreateExternalId(MagentoResourceType resourceType, MagentoContentType contentType, string id)
    {
        return MappedIdentity.ConstructExternalIdentifier(
            _providerKey,
            $"{contentType.ToString()}/{resourceType.ToString()}/{id}/");
    }
    
    public IEnumerable<GetChildrenReferenceResult> CreateChildrenReferences<T>(
        IEnumerable<T> items,
        Func<T, Uri> createExternalId,
        Type modelType,
        bool isLeafNode = true) 
    {
        var children = items.Select(i =>
        {
            var itemContentId = createExternalId(i);
            var itemIdentity = _identityMappingService.Service.Get(itemContentId, true);

            return new GetChildrenReferenceResult()
            {
                ContentLink = itemIdentity.ContentLink,
                IsLeafNode = isLeafNode,
                ModelType = modelType
            };
        }).ToList();

        return children;
    }
    
    private static string RemoveEndingSlash(string path)
    {
        return !string.IsNullOrEmpty(path) && path[^1] == '/' ? path[..^1] : path;
    }
}