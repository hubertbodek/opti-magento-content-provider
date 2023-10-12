using System.ComponentModel.DataAnnotations;

namespace Cms.Integrations.Magento.Content.Category;

[ContentType(
    GUID = "f464b997-e421-448a-a398-f1cb8ee4b56f",
    DisplayName = "Magento Category",
    Description = "",
    GroupName = "Magento",
    AvailableInEditMode = false)]
public class CategoryContent : ContentBase
{
    [Editable(false)]
    public virtual string Id { get; set; }

    [Editable(false)]
    public virtual string Title { get; set; }
    
    [Editable(false)]
    public virtual string Description { get; set; }
}