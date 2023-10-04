using Cms.Integrations.Magento.Content;
using Cms.Integrations.Magento.Provider;
using EPiServer.ServiceLocation;
using EPiServer.Shell;

namespace Cms.Integrations.Magento.UI;

[ServiceConfiguration(typeof(IContentRepositoryDescriptor))]
public class MagentoRepositoryDescriptor : ContentRepositoryDescriptorBase
{
    private Injected<IContentProviderManager> _contentProviderManager;

    public override string Key => MagentoProvider.Key;
    public override string Name => "Magento";
    public override IEnumerable<ContentReference> Roots { get { return new[] { _contentProviderManager.Service.GetProvider(MagentoProvider.Key).EntryPoint }; } }
    public override IEnumerable<Type> ContainedTypes { get { return new[] { typeof(ProductContent) }; } }
    public override IEnumerable<Type> MainNavigationTypes { get { return new[] { typeof(ContentFolder) }; } }
    public override IEnumerable<Type> CreatableTypes { get { return new[] { typeof(ProductContent) }; } }
}