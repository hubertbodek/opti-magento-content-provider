using System.ComponentModel.DataAnnotations;
using Cms.Integrations.Magento.Content;
using Cms.Integrations.Magento.Content.Product;

namespace Cms.Models.Blocks;

[SiteContentType(GUID = "979e6598-2293-42a9-a64a-436704e94762")]
public class ProductsCollectionBlock : SiteBlockData
{
    [CultureSpecific]
    [AllowedTypes(typeof(ProductContent))]
    [Display(
        GroupName = SystemTabNames.Content,
        Order = 3)]
    public virtual IList<ContentReference> Products { get; set; }
}