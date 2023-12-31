namespace Cms.Integrations.Magento.Content;

[ContentType(
    GUID = "DE8A22EF-EDD4-4F42-BD8B-4DEFFE3C8244",
    DisplayName = "Magento Category Folder",
    AvailableInEditMode = false,
    GroupName = "Magento")]

public class MagentoCategoryFolder : MagentoContentFolder
{
    [Ignore]
    public string Title { get; set; }
}