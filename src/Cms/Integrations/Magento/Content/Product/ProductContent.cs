using System.ComponentModel.DataAnnotations;

namespace Cms.Integrations.Magento.Content.Product;

[ContentType(
    GUID = "0D4A8F04-8337-4A59-882E-F39617E5D434",
    DisplayName = "Magento Product",
    GroupName = "Magento",
    AvailableInEditMode = false)]

public class ProductContent : ContentBase
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