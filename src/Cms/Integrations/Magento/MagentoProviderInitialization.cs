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
    public void Initialize(InitializationEngine context)
    {
        var magentoProvider = new MagentoProvider();
        var providerValues = new NameValueCollection
        {
            { ContentProviderParameter.EntryPoint, MagentoProvider.GetEntryPoint(ProviderConstants.EntryPointName).ContentLink.ToString() },
            { ContentProviderParameter.Capabilities, "Create,Edit,Delete,Search" }
        };

        magentoProvider.Initialize(MagentoProvider.Key, providerValues);
        var providerManager = context.Locate.Advanced.GetInstance<IContentProviderManager>();
        providerManager.ProviderMap.AddProvider(magentoProvider);
        magentoProvider.ClearProviderPagesFromCache();
    }

    public void Uninitialize(InitializationEngine context)
    {
    }
}