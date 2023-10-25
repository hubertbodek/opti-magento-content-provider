using System.Web;
using Cms.Business;
using Cms.Integrations.Magento;
using Cms.Integrations.Magento.Content;
using Cms.Integrations.Magento.Content.Product;
using Cms.Pages;
using Cms.Pages.ExternalCategoryPage;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace Cms.Jobs;

[ScheduledPlugIn(
    DisplayName = "Populate Magento Pages",
    Description = "Create or update category and product pages from Magento",
    GUID = "5bc6fcf1-e13f-4fba-ae43-92888878f79f")
]
public class PopulateMagentoPagesJob : ScheduledJobBase
{
    private readonly Injected<IdentityMappingService> _identityMappingService;
    private readonly Injected<ContentLocator> _contentLocator;
    private static readonly IContentRepository ContentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
    private static readonly ContentFolder MagentoEntryPoint =
        MagentoProvider.GetEntryPoint(ProviderConstants.EntryPointName);
    
    public override string Execute()
    {
        var categoriesCreated = 0;
        var categoriesUpdated = 0;

        if (!ContentRepository.TryGet<ContentFolder>(MagentoEntryPoint.ContentLink, out var magentoFolder))
        {
            return "Couldn't find the main Magento folder.";
        }
        
        var categoriesFolder = ContentRepository.GetChildren<MagentoContentFolder>(magentoFolder.ContentLink).FirstOrDefault();

        if (categoriesFolder == null)
        {
            return "Couldn't find the main categories folder.";
        }

        var categories = ContentRepository.GetChildren<MagentoContentFolder>(categoriesFolder.ContentLink);

        var siteDefinitionRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
        var siteDefinition = siteDefinitionRepository.List().ToList().FirstOrDefault();

        if (siteDefinition == null)
        {
            return "Couldn't locate the start page.";
        }
        
        var parent = _contentLocator.Service.GetCategoryFolderPage(siteDefinition.StartPage);

        if (parent == null)
        {
            return "No category page root specified in site settings.";
        }

        var categoryPages = ContentRepository.GetChildren<ExternalCategoryPage>(parent.ContentLink).ToList();
        
        foreach (var magentoCategoryFolder in categories)
        {
            var mappedItem = _identityMappingService.Service.Get(magentoCategoryFolder.ContentLink);
            if (mappedItem == null) continue;
            
            var id = HttpUtility.UrlDecode(RemoveEndingSlash(mappedItem.ExternalIdentifier.Segments[3]));
            var page = categoryPages?.Find(page => page.Id.Equals(id));

            if (page != null)
            {
                UpdateCategoryTree(page, magentoCategoryFolder);
                categoriesUpdated++;
                continue;
            };
            
            CreateCategoryTree(parent.ContentLink, magentoCategoryFolder, id);
            categoriesCreated++;
        }
        
        return $"Created {categoriesCreated} and updated {categoriesUpdated} category pages.";
    }

    private void CreateCategoryTree(ContentReference parent, MagentoContentFolder category, string id)
    {
        var categoryPage = CreatePage<ExternalCategoryPage>(parent, (item) =>
        {
            item.Id = id;
            item.Name = category.Name;
            item.Title = category.Name;
            item.CategoryReference = category.ContentLink;
        });

        var products = ContentRepository.GetChildren<ProductContent>(category.ContentLink);

        foreach (var product in products)
        {
            CreateProductPage(categoryPage.ContentLink, product);
        }
    }

    private void UpdateCategoryTree(ExternalCategoryPage categoryPage, MagentoContentFolder category)
    {
        var updatedCategoryPage = UpdatePage(categoryPage, page =>
        {
            page.Name = category.Name;
            page.Title = category.Name;
            page.CategoryReference = category.ContentLink;
        });
        
        var products = ContentRepository.GetChildren<ProductContent>(category.ContentLink);
        var productPages = ContentRepository.GetChildren<ExternalProductPage>(updatedCategoryPage.ContentLink).ToList();            
        
        foreach (var product in products)
        {
            var page = productPages?.Find(page => page.Sku != null && page.Sku.Equals(product.Sku));
            
            if (page != null)
            {
                UpdateProductPage(page, product);
                continue;
            };
            
            CreateProductPage(updatedCategoryPage.ContentLink, product);
        }
    }

    private void CreateProductPage(ContentReference parent, ProductContent product)
    {
        CreatePage<ExternalProductPage>(parent, (productPage) =>
        {
            productPage.Sku = product.Sku;
            productPage.Name = product.Name;
            productPage.Title = product.Title;
            productPage.Description = product.Description;
            productPage.Price = product.Price;
        });
    }

    private void UpdateProductPage(ExternalProductPage page, ProductContent product)
    {
        UpdatePage(page, p =>
        {
            p.Name = product.Name;
            p.Title = product.Name;
            p.Description = product.Description;
            p.Price = product.Price;
        });
    }
    
    private static T CreatePage<T>(ContentReference parent, Action<T> applyProperties) where T : PageData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var page = contentRepository.GetDefault<T>(parent);

        applyProperties(page);

        ContentRepository.Save(page, EPiServer.DataAccess.SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);
        
        return page;
    }

    private static T UpdatePage<T>(T page, Action<T> applyProperties) where T : PageData
    {
        var updatedPage = page.CreateWritableClone() as T;
        
        applyProperties(updatedPage);
        ContentRepository.Save(updatedPage, EPiServer.DataAccess.SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);

        return updatedPage;
    }
    
    private static string RemoveEndingSlash(string path)
    {
        return !string.IsNullOrEmpty(path) && path[^1] == '/' ? path[..^1] : path;
    }
}