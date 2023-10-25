using System.Web;
using Cms.Integrations.Magento.Client;
using Cms.Integrations.Magento.Content;
using Cms.Integrations.Magento.Content.Category;
using Cms.Integrations.Magento.Content.Product;
using Cms.Integrations.Magento.Helpers;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using ContentFolder = EPiServer.Core.ContentFolder;

namespace Cms.Integrations.Magento;

public class MagentoProvider : ContentProvider
{
    private readonly Injected<IdentityMappingService> _identityMappingService;
    private List<ProductExternal> _products;
    private List<CategoryExternal> _categories;
    private Injected<IMagentoClient> _magentoClient;
    
    public const string Key = ProviderConstants.Key;

    protected override IContent LoadContent(ContentReference contentLink, ILanguageSelector languageSelector)
    {
        var mappedItem = _identityMappingService.Service.Get(contentLink);
        if (mappedItem == null) return null;

        if (!TryGetResourceAndContentTypes(mappedItem.ExternalIdentifier, out var contentType, out var resourceType))
        {
            return null;
        };
        
        var helper = new ProviderHelper(EntryPoint, ProviderKey);

        switch (contentType)
        {
            case MagentoContentType.ProductContent:
            {
                if (_products == null) return default;
                
                var sku = RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3]);
                var product = _products.FirstOrDefault(p => p.Sku.Equals(HttpUtility.UrlDecode(sku)));
                
                return helper.MapProductToContent(product);
            }
            case MagentoContentType.ContentFolder:
            {
                var name = HttpUtility.UrlDecode(RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[2]));
            
                return helper.CreateFolder(mappedItem, contentType, name);
            }
            case MagentoContentType.CategoryFolder:
            {
                var name = HttpUtility.UrlDecode(RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[4]));

                return helper.CreateFolder(mappedItem, contentType, name);
            }
            default:
                return default;
        }
    }

    protected override IList<GetChildrenReferenceResult> LoadChildrenReferencesAndTypes(ContentReference contentLink,
        string languageId, out bool languageSpecific)
    {
        languageSpecific = false;
        
        var helper = new ProviderHelper(EntryPoint, ProviderKey);

        if (EntryPoint.CompareToIgnoreWorkID(contentLink))
        {
            return CreateTopLevelFolders(contentLink, helper);
        }

        var mappedItem = _identityMappingService.Service.Get(contentLink);
        if (mappedItem == null)
            return default;
        
        if (!TryGetResourceAndContentTypes(mappedItem.ExternalIdentifier, out var contentType, out var resourceType))
        {
            return default;
        };

        if (!TryGetChildrenList(resourceType, mappedItem, out var childrenList))
        {
            return default;
        };
        
        return childrenList;
    }
    
    protected override void SetCacheSettings(IContent content, CacheSettings cacheSettings)
    {
        // TODO - figure out proper caching strategy
        cacheSettings.SlidingExpiration = TimeSpan.FromMinutes(1);
        cacheSettings.AbsoluteExpiration = DateTime.MaxValue;
    }
    
    public static ContentFolder GetEntryPoint(string name)
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

        var folder = contentRepository.GetBySegment(ContentReference.RootPage, name, LanguageSelector.AutoDetect()) as ContentFolder;
        if (folder != null) return folder;
        
        folder = contentRepository.GetDefault<ContentFolder>(ContentReference.RootPage);
        folder.Name = name;
        contentRepository.Save(folder, SaveAction.Publish, AccessLevel.NoAccess);
        
        return folder;
    }

    private IList<GetChildrenReferenceResult> CreateTopLevelFolders(ContentReference contentLink, ProviderHelper helper)
    {
        var children = new List<GetChildrenReferenceResult>();

        var nestedCategoryContentId = helper.CreateExternalId(MagentoResourceType.NestedCategory, MagentoContentType.ContentFolder,
            contentLink.ID.ToString());
 
        children.Add(new GetChildrenReferenceResult()
        {
            ContentLink = _identityMappingService.Service.Get(nestedCategoryContentId, true).ContentLink,
            IsLeafNode = false,
            ModelType = typeof(MagentoCategoryFolder)
        });

        return children;
    }
    
    private bool TryGetChildrenList(MagentoResourceType resourceType, MappedIdentity mappedItem, out List<GetChildrenReferenceResult> childrenList)
    {
        childrenList = new List<GetChildrenReferenceResult>();
        var helper = new ProviderHelper(EntryPoint, ProviderKey);

        switch (resourceType)
        {
            case MagentoResourceType.Category:
            {
                FetchCategories();
                FetchProducts();
                
                var id = RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3]);
                var categoryProducts = _products.FindAll(p =>
                {
                    var categoryLink =
                        p.ExtensionAttributes.CategoryLinks?.Find(category =>
                            category.CategoryId.Equals(id));
                
                    return categoryLink != null;
                });
                
                var childrenRefs = helper.CreateChildrenReferences(
                    categoryProducts,
                    p => helper.CreateExternalId(MagentoResourceType.Product,
                        MagentoContentType.ProductContent, p.Sku),
                    typeof(ProductContent));
                
                childrenList.AddRange(childrenRefs);

                break;
            }
            case MagentoResourceType.NestedCategory:
            {
                FetchCategories();
                
                var categories = helper.CreateChildrenReferences(
                    _categories, 
                    c => helper.CreateExternalId(MagentoResourceType.Category, MagentoContentType.CategoryFolder, $"{c.Id.ToString()}/{c.Name}"),
                    typeof(MagentoContentFolder),
                    false);
            
                childrenList.AddRange(categories);
                break;
            }
            default:
                return false;
        }

        return true;
    }

    private static bool TryGetResourceAndContentTypes(Uri externalId, out MagentoContentType contentType,
        out MagentoResourceType resourceType)
    {
        try
        {
            contentType = (MagentoContentType)Enum.Parse(typeof(MagentoContentType),
                RemoveEndingSlash(externalId.Segments[1]), true);
            resourceType = (MagentoResourceType)Enum.Parse(typeof(MagentoResourceType),
                RemoveEndingSlash(externalId.Segments[2]), true);

            return true;
        }
        catch
        {
            contentType = MagentoContentType.ContentFolder;
            resourceType = MagentoResourceType.NestedCategory;
            
            return false;
        }
    }

    private void FetchProducts()
    {
        if (_products != null) return;
        
        _products = Task.Run(() => _magentoClient.Service.GetProducts())
            .GetAwaiter()
            .GetResult();
    }
    
    private void FetchCategories()
    {
        if (_categories != null) return;
        
        _categories = Task.Run(() => _magentoClient.Service.GetCategories())
            .GetAwaiter()
            .GetResult();
    }
    
    private static string RemoveEndingSlash(string path)
    {
        return !string.IsNullOrEmpty(path) && path[^1] == '/' ? path[..^1] : path;
    }
}