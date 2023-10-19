using System.ComponentModel.DataAnnotations;
using Cms.Integrations.Magento.Content;
using Cms.Models.Pages;

namespace Cms.Pages.ExternalCategoryPage;

[ContentType(
    DisplayName = "External Category Page",
    GUID = "bb005e53-28a2-4274-994e-38b5faadcb56",
    Description = "Category page with external data provided by Content Provider")]
public class ExternalCategoryPage : SitePageData, IIncludedOnHomepage
{
    [Display(
        Name = "Id",
        GroupName = Globals.GroupNames.Content)
    ]
    [Editable(false)]
    public virtual string Id { get; set; }
    
    [Display(
        Name = "Title",
        GroupName = Globals.GroupNames.Content)
    ]
    [Editable(false)]
    public virtual string Title { get; set; }
    
    [Display(
        Name = "Category Reference",
        GroupName = Globals.GroupNames.Content)
    ]
    [Editable(false)]
    [AllowedTypes(typeof(MagentoContentFolder))]
    public virtual ContentReference CategoryReference { get; set; }
}