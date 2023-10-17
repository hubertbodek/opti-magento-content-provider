using Cms.Integrations.Magento.Content.Category;
using Cms.Integrations.Magento.Content.Product;
using Cms.Pages.ExternalCategoryPage;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Cms;

[InitializableModule]
[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
public class CmsPublishEventInit : IInitializableModule
{
    private readonly Injected<IContentEvents> _contentEvents;
    
    public void Initialize(InitializationEngine context)
    {
        Console.WriteLine(">>>>>> initialized cms publish event init");
        _contentEvents.Service.SavedContent += OnPublishedContent;
    }

    public void Uninitialize(InitializationEngine context)
    {
        _contentEvents.Service.PublishedContent -= OnPublishedContent;
    }

    private void OnPublishedContent(object sender, ContentEventArgs e)
    {
        if (e.Content is CategoryContent category)
        {
            // CreateOrUpdateCategoryPage(category);
        }

        if (e.Content is ProductContent product)
        {
            // CreateOrUpdateProductPage(product);
        }
    }
}