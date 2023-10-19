using System.ComponentModel.DataAnnotations;
using Cms.Models.Pages;

namespace Cms.Pages;

[ContentType(
    DisplayName = "External Product Details Page",
    GUID = "77e77bc1-922b-4250-b0e4-52929b22ef64",
    Description = "Product page with external data provided by Content Provider")]
public class ExternalProductPage : SitePageData
{
    [Required]
    [Editable(false)]
    public virtual string Sku { get; set; }
    
    [Editable(false)]
    public virtual string Title { get; set; }
    
    [Editable(false)]
    public virtual string Description { get; set; }
    
    [Editable(false)]
    public virtual int Price { get; set; }
    
    [Editable(false)]
    public virtual string ImageUrl { get; set; }
    
    [Editable(false)]
    public virtual bool IsConfigurable  { get; set; }
}