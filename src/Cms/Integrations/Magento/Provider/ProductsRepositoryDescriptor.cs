using Cms.Integrations.Magento.Content;
using EPiServer.ServiceLocation;
using EPiServer.Shell;

namespace Cms.Integrations.Magento.Provider;

[ServiceConfiguration(typeof(IContentRepositoryDescriptor))]
public class ProductsRepositoryDescriptor : ContentRepositoryDescriptorBase
{
    private Injected<IContentProviderManager> ContentProviderManager { get; set; }

    public override string Key => ProductsContentProvider.Key;

    public override string Name => ProductsContentProvider.Key;

    public override IEnumerable<ContentReference> Roots => new List<ContentReference> { ContentProviderManager.Service.GetProvider(ProductsContentProvider.Key).EntryPoint };

    public override IEnumerable<Type> ContainedTypes { get { return new[] { typeof(ProductContent) }; } }

    public override IEnumerable<Type> MainNavigationTypes { get { return new[] { typeof(ContentFolder), typeof(CommerceContentFolder) }; } }

    public override IEnumerable<Type> CreatableTypes { get { return new[] { typeof(ProductContent) }; } }

    public bool ChangeContextOnItemSelection => true;
}