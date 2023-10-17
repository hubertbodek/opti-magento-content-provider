using Cms.Integrations.Magento;
using Cms.Integrations.Magento.Content;
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
public class PopulateExternalContentJob : ScheduledJobBase
{
    private readonly Injected<IdentityMappingService> _identityMappingService;

    public override string Execute()
    {
        var categoriesProccessed = 0;
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        
        var entryPoint = MagentoProvider.GetEntryPoint(ProviderConstants.EntryPointName);

        if (!contentRepository.TryGet<ContentFolder>(entryPoint.ContentLink, out var magentoFolder))
        {
            return "Couldn't find the main Magento folder.";
        }
        
        var categoriesFolder = contentRepository.GetChildren<MagentoContentFolder>(magentoFolder.ContentLink).FirstOrDefault();

        if (categoriesFolder == null)
        {
            return "Couldn't find the main categories folder.";
        }

        var categories = contentRepository.GetChildren<MagentoContentFolder>(categoriesFolder.ContentLink);
        
        foreach (var magentoCategoryFolder in categories)
        {
            var mappedItem = _identityMappingService.Service.Get(magentoCategoryFolder.ContentLink);
            if (mappedItem == null) return null;

            CreateOrUpdateCategoryPage(magentoCategoryFolder);
            categoriesProccessed++;
        }
        
        return $"Created {categoriesProccessed} category pages.";
    }
    
    public void CreateOrUpdateCategoryPage(MagentoContentFolder category)
    {
        var siteDefinitionRepository = ServiceLocator.Current.GetInstance<ISiteDefinitionRepository>();
        var siteDefinition = siteDefinitionRepository.List().ToList().FirstOrDefault();
        // var loader = ServiceLocator.Current.GetInstance<IContentLoader>();
        // var pages = loader.GetChildren<ExternalCategoryPage>(ContentReference.StartPage);
        // var externalId = CreateExternalId(MagentoResourceType.Category, MagentoContentType.CategoryPage, category.Id.ToString());
        // var mappedContent = _identityMappingService.Service.Get(externalId, true);

        if (siteDefinition != null)
            CreatePage<ExternalCategoryPage>(siteDefinition.StartPage, (item) =>
            {
                item.Name = category.Name;
                item.CategoryReference = category.ContentLink;
            });
    }
    
    private static void CreatePage<T>(ContentReference parent, Action<T> applyProperties) where T : PageData
    {
        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
        var page = contentRepository.GetDefault<T>(parent);

        applyProperties(page);

        contentRepository.Save(page, EPiServer.DataAccess.SaveAction.Publish, EPiServer.Security.AccessLevel.NoAccess);
    }
    
    public Uri CreateExternalId(MagentoResourceType resourceType, MagentoContentType contentType, string id)
    {
        return MappedIdentity.ConstructExternalIdentifier(
            "magentokey",
            $"{contentType.ToString()}/{resourceType.ToString()}/{id}/");
    }
}