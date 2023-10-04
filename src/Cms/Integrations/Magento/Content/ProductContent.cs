using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Cms.Integrations.Magento.Content;

[ContentType(GUID = "0D4A8F04-8337-4A59-882E-F39617E5D434", DisplayName = "Magento Product", AvailableInEditMode = false)]

public class ProductContent : ContentBase
{
    [EmailAddress]
    [Required]
    public virtual string Sku { get; set; }
    
    public virtual string Title { get; set; }
    
    public virtual string Description { get; set; }
    
    public virtual int Price { get; set; }
}