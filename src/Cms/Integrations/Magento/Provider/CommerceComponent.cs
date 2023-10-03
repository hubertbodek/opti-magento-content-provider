using EPiServer.Shell;
using EPiServer.Shell.ViewComposition;

namespace Cms.Integrations.Magento.Provider;

[Component]
public sealed class CommerceComponent : ComponentDefinitionBase
{
    public CommerceComponent()
        : base("epi-cms/asset/HierarchicalList")
    {
        Categories = new string[] { "content" };
        Title = "Social";
        Description = "All the social for the given site";
        SortOrder = 1000;
        PlugInAreas = new[] { PlugInArea.Assets };
        Settings.Add(new Setting("repositoryKey", ProductsContentProvider.Key));
    }
}