using System.Collections;
using Cms.Integrations.Magento.Content;
using Cms.Integrations.Magento.Content.Product;
using EPiServer.ServiceLocation;
using EPiServer.Shell;

namespace Cms.Integrations.Magento.UI;

[ServiceConfiguration(typeof(IContentRepositoryDescriptor))]
public class MagentoRepositoryDescriptor : ContentRepositoryDescriptorBase
{
    private Injected<IContentProviderManager> _contentProviderManager;

    private static string RepositoryKey => MagentoProvider.Key;
    public override string Key => RepositoryKey;
    public override string Name => "Magento Content";
    
    public override IEnumerable<ContentReference> Roots { get { return new[] { _contentProviderManager.Service.GetProvider(MagentoProvider.Key).EntryPoint }; } }
    public override IEnumerable<Type> ContainedTypes { get { return new[] { typeof(ProductContent) }; } }
    public override IEnumerable<Type> MainNavigationTypes { get { return new[] { typeof(ContentFolder) }; } }
    public override IEnumerable<Type> CreatableTypes { get { return new[] { typeof(ProductContent) }; } }
}