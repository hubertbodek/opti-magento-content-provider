using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;

namespace Cms.Integrations.Magento.UI;

[Component]
public sealed class ProductComponent : ComponentDefinitionBase
{
    public ProductComponent() : base("epi-cms/component/SharedBlocks")
    {
        Categories = new string[] { "content" };
        Title = "Magento content";
        Description = "Magento products and categories";
        SortOrder = 1000;
        PlugInAreas = new[] { PlugInArea.Assets };
        Settings.Add(new Setting("repositoryKey", MagentoProvider.Key));
    }
}