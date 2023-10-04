using Cms.Integrations.Magento.Provider;
using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;

namespace Cms.Integrations.Magento.UI;

[Component]
public sealed class ProductComponent : ComponentDefinitionBase
{
    public ProductComponent() : base("epi-cms/component/HierarchicalList")
    {
        Categories = new string[] { "content" };
        Title = "Epi persons";
        Description = "All persons";
        SortOrder = 1000;
        PlugInAreas = new[] { PlugInArea.Assets };
        Settings.Add(new Setting("repositoryKey", MagentoProvider.Key));
    }
}