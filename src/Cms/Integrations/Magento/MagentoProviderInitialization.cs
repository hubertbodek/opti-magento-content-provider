using System.Collections.Specialized;
using Cms.Integrations.Magento.Client;
using EPiServer.Core.Internal;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Cms.Integrations.Magento;

[InitializableModule]
[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
public class MagentoProviderInitialization : IInitializableModule
{
    private Injected<IMagentoClient> _magentoClient;
    public async void Initialize(InitializationEngine context)
    {
        var products = await _magentoClient.Service.GetProducts();
        var categories = await _magentoClient.Service.GetCategories();

        var magentoProvider = new MagentoProvider(products, categories);
        var providerValues = new NameValueCollection
        {
            { ContentProviderParameter.EntryPoint, MagentoProvider.GetEntryPoint("magento").ContentLink.ToString() },
            { ContentProviderParameter.Capabilities, "Create,Edit,Delete,Search" }
        };

        
        magentoProvider.Initialize(MagentoProvider.Key, providerValues);
        magentoProvider.ClearProviderPagesFromCache();
        var providerManager = context.Locate.Advanced.GetInstance<IContentProviderManager>();
        providerManager.ProviderMap.AddProvider(magentoProvider);
    }

    public void Uninitialize(InitializationEngine context)
    {
        throw new NotImplementedException();
    }
}