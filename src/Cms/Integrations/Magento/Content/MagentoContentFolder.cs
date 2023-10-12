namespace Cms.Integrations.Magento.Content;

[ContentType(
    GUID = "DE8A22EF-EDD4-4F42-BD8B-4DEFFE3C8244",
    DisplayName = "Magento Content Folder",
    AvailableInEditMode = false,
    GroupName = "Magento")]

public class MagentoContentFolder : ContentFolder
{
    [Ignore]
    public MagentoResourceType MagentoResourceType { get; set; }
} 