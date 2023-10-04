using System.Collections.Specialized;
using Cms.Integrations.Magento.Content;
using EPiServer.Core.Internal;
using EPiServer.DataAccess;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace Cms.Integrations.Magento.Provider;

[InitializableModule]
[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
public class MagentoProviderInitialization : IInitializableModule
{
    public void Initialize(InitializationEngine context)
    {
        var magentoProvider = new MagentoProvider();
        var providerValues = new NameValueCollection
        {
            { ContentProviderParameter.EntryPoint, MagentoProvider.GetEntryPoint("magento").ContentLink.ToString() },
            { ContentProviderParameter.Capabilities, "Create,Edit,Delete,Search" }
        };
        
        magentoProvider.Initialize(MagentoProvider.Key, providerValues);
        var providerManager = context.Locate.Advanced.GetInstance<IContentProviderManager>();
        providerManager.ProviderMap.AddProvider(magentoProvider);
    }

    public void Uninitialize(InitializationEngine context)
    {
        throw new NotImplementedException();
    }
}