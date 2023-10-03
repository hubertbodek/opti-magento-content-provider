using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Cms.Integrations.Magento.Provider;

[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
public class ContentProviderConfig : IInitializableModule
{
    public void Initialize(InitializationEngine context)
    {
        /* Register provider to episerver this can be done through config as well */
        var providerManager = context.Locate.Advanced.GetInstance<IContentProviderManager>();
        providerManager.ProviderMap.AddProvider(ProductsContentProvider.Instance);
    }

    void IInitializableModule.Uninitialize(InitializationEngine context)
    {
        Uninitialize(context);
    }

    public void Uninitialize(InitializationEngine context) { }
}  