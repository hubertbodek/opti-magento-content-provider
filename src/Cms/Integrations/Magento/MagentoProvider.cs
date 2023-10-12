using System.Web;
using Cms.Integrations.Magento.Client;
using Cms.Integrations.Magento.Content;
using Cms.Integrations.Magento.Content.Category;
using Cms.Integrations.Magento.Content.Product;
using Cms.Integrations.Magento.Helpers;
using EPiServer.Construction;
using EPiServer.DataAccess;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using ContentFolder = EPiServer.Core.ContentFolder;

namespace Cms.Integrations.Magento;

public sealed class MagentoProvider : ContentProvider
{
    private readonly Injected<IdentityMappingService> _identityMappingService;
    private readonly List<ProductExternal> _products;
    
    public const string Key = "magento-external-2";
    public MagentoProvider(List<ProductExternal> products)
    {
        _products = products;
    }

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
                var id = RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3]);
                var category = CategoryService.GetCategoryById(id);
                return helper.MapCategoryToContent(category);
            }
            case MagentoContentType.ProductContent:
            {
                var sku = RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3]);
                var product = _products.FirstOrDefault(p => p.Sku.Equals(HttpUtility.UrlDecode(sku)));
                return helper.MapProductToContent(product);
            }
            case MagentoContentType.ContentFolder:
            {
                var folder = RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[2]);
            
                return helper.CreateContent(
                    mappedItem.ContentLink.ID,
                    mappedItem.ContentGuid,
                    int.Parse(RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3])),
                    contentType,
                    typeof(ContentFolder),
                    folder);
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
            var children = new List<GetChildrenReferenceResult>();

            var productsContentId =
                helper.CreateExternalId(MagentoResourceType.Product, MagentoContentType.ContentFolder, contentLink.ID.ToString());
            var categoryContentId = helper.CreateExternalId(MagentoResourceType.Category, MagentoContentType.ContentFolder,
                contentLink.ID.ToString());
            
            children.Add(new GetChildrenReferenceResult()
            {
                ContentLink = _identityMappingService.Service.Get(productsContentId, true).ContentLink,
                IsLeafNode = false,
                ModelType = typeof(MagentoContentFolder)
            });
            
            children.Add(new GetChildrenReferenceResult()
            {
                ContentLink = _identityMappingService.Service.Get(categoryContentId, true).ContentLink,
                IsLeafNode = false,
                ModelType = typeof(MagentoContentFolder)
            });

            return children;
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
            case MagentoResourceType.Product:
            {
                var products = _products.Select(p =>
                {
                    var productContentId = helper.CreateExternalId(MagentoResourceType.Product,
                        MagentoContentType.ProductContent, p.Sku
                    );

                    return new GetChildrenReferenceResult()
                    {
                        ContentLink = _identityMappingService.Service.Get(productContentId, true).ContentLink,
                        IsLeafNode = true,
                        ModelType = typeof(ProductContent)
                    };
                }).ToList();

                childrenList.AddRange(products);
                break;
            }
            case MagentoResourceType.Category:
            {
                var categories = CategoryService.GetAll().Select(c =>
                {
                    var categoryContentId = helper.CreateExternalId(MagentoResourceType.Category,
                        MagentoContentType.CategoryContent, c.Id
                    );

                    var categoryIdentity = _identityMappingService.Service.Get(categoryContentId, true);

                    return new GetChildrenReferenceResult()
                    {
                        ContentLink = categoryIdentity.ContentLink,
                        IsLeafNode = true,
                        ModelType = typeof(CategoryContent)
                    };
                }).ToList();

                childrenList.AddRange(categories);
                break;
            }
            default:
                return childrenList;
        }

        return childrenList;
    }
    
    public static ContentFolder GetEntryPoint(string name)
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

        var folder = contentRepository.GetBySegment(ContentReference.RootPage, name, LanguageSelector.AutoDetect()) as ContentFolder;
        if (folder == null)
        {
            folder = contentRepository.GetDefault<ContentFolder>(ContentReference.RootPage);
            folder.Name = name;
            contentRepository.Save(folder, SaveAction.Publish, AccessLevel.NoAccess);
        }
        return folder;
    }
    
    public bool TryGetResourceAndContentTypes(Uri externalId, out MagentoContentType contentType,
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
            resourceType = MagentoResourceType.Product;
            
            return false;
        }
    }

    private static string RemoveEndingSlash(string path)
    {
        return !string.IsNullOrEmpty(path) && path[^1] == '/' ? path[..^1] : path;
    }
}