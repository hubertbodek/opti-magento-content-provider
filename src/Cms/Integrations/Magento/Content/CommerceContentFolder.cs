using static Cms.Integrations.Magento.Content.CommerceContent;

namespace Cms.Integrations.Magento.Content;

[ContentType(
    GUID = "DE8A22EF-EDD4-4F42-BD8B-4DEFFE3C8244", 
    DisplayName = "Products Folder", 
    AvailableInEditMode = false)]
public class CommerceContentFolder : ContentFolder
{
    [Ignore]
    public CommerceType CommerceType { get; set; }
}