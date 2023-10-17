using System.Web;
// using System.Web.Caching;
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

public sealed class MagentoProvider : ContentProvider
{
    private readonly Injected<IdentityMappingService> _identityMappingService;
    private Injected<IMagentoClient> _magentoClient;

    private List<ProductExternal> _products;
    private List<CategoryExternal> _categories;
    
    public const string Key = "magentokey";

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
            case MagentoContentType.CategoryContent:
            {
                if (_categories == null) return default;
                
                var id = RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3]);
                var category = _categories.FirstOrDefault(c => c.Id.ToString().Equals(HttpUtility.UrlDecode(id)));
                
                return helper.MapCategoryToContent(category);
            }
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
            
                return helper.CreateContent(
                    mappedItem.ContentLink.ID,
                    mappedItem.ContentGuid,
                    int.Parse(RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3])),
                    contentType,
                    typeof(MagentoContentFolder),
                    name);
            }
            case MagentoContentType.NestedContentFolder:
            {
                var name = HttpUtility.UrlDecode(RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[4]));
                
                return helper.CreateContent(
                    mappedItem.ContentLink.ID,
                    mappedItem.ContentGuid,
                    int.Parse(RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3])),
                    contentType,
                    typeof(MagentoContentFolder),
                    name);
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
        
        var childrenList = new List<GetChildrenReferenceResult>();

        if (!TryGetResourceAndContentTypes(mappedItem.ExternalIdentifier, out var contentType, out var resourceType))
        {
            return default;
        };

        switch (resourceType)
        {
            // case MagentoResourceType.Product:
            // {
            //     FetchProducts();
            //     
            //     var products = helper.CreateChildrenReferences(
            //         _products,
            //         p =>
            //         {
            //             AddContentToCache(helper.MapProductToContent(p));
            //             return helper.CreateExternalId(MagentoResourceType.Product,
            //                 MagentoContentType.ProductContent, p.Sku);
            //         },
            //         typeof(ProductContent));
            //     
            //     childrenList.AddRange(products);
            //     break;
            // }
            case MagentoResourceType.Category:
            {
                FetchCategories();
                
                // var categories = helper.CreateChildrenReferences(
                //     _categories,
                //     c => helper.CreateExternalId(MagentoResourceType.Category,
                //         MagentoContentType.CategoryContent, c.Id.ToString()),
                //     typeof(CategoryContent));
                //
                // childrenList.AddRange(categories);
                if (contentType == MagentoContentType.NestedContentFolder)
                {
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
                }

                break;
            }
            case MagentoResourceType.NestedCategory:
            {
                FetchCategories();
                
                var categories = helper.CreateChildrenReferences(
                    _categories, 
                    c => helper.CreateExternalId(MagentoResourceType.Category, MagentoContentType.NestedContentFolder, $"{c.Id.ToString()}/{c.Name}"),
                    typeof(MagentoContentFolder),
                    false);
            
                childrenList.AddRange(categories);
                break;
            }
        }

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

    private IList<GetChildrenReferenceResult> CreateTopLevelFolders(ContentReference contentLink, ProviderHelper helper)
    {
        var children = new List<GetChildrenReferenceResult>();

        // var productsContentId =
        //     helper.CreateExternalId(MagentoResourceType.Product, MagentoContentType.ContentFolder, contentLink.ID.ToString());
        // var categoryContentId = helper.CreateExternalId(MagentoResourceType.Category, MagentoContentType.ContentFolder,
        //     contentLink.ID.ToString());
        var nestedCategoryContentId = helper.CreateExternalId(MagentoResourceType.NestedCategory, MagentoContentType.ContentFolder,
            contentLink.ID.ToString());
            
        // children.Add(new GetChildrenReferenceResult()
        // {
        //     ContentLink = _identityMappingService.Service.Get(productsContentId, true).ContentLink,
        //     IsLeafNode = false,
        //     ModelType = typeof(MagentoContentFolder)
        // });
        // children.Add(new GetChildrenReferenceResult()
        // {
        //     ContentLink = _identityMappingService.Service.Get(categoryContentId, true).ContentLink,
        //     IsLeafNode = false,
        //     ModelType = typeof(MagentoContentFolder)
        // });  
        children.Add(new GetChildrenReferenceResult()
        {
            ContentLink = _identityMappingService.Service.Get(nestedCategoryContentId, true).ContentLink,
            IsLeafNode = false,
            ModelType = typeof(MagentoCategoryFolder)
        });

        return children;
    }

    // private void CreateCategoryPages(List<CategoryExternal> categories, ProviderHelper helper)
    // {
    //     if()
    //     
    // }
    
    private static string RemoveEndingSlash(string path)
    {
        return !string.IsNullOrEmpty(path) && path[^1] == '/' ? path[..^1] : path;
    }
}