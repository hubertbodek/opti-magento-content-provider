namespace Cms.Integrations.Magento.Content;

[ContentType(
    GUID = "e8522b9a-daff-4726-bfb2-4c5f461dbbd2",
    DisplayName = "Magento Content Folder",
    AvailableInEditMode = false,
    GroupName = "Magento")]

public class MagentoContentFolder : ContentFolder
{
    [Ignore]
    public MagentoResourceType MagentoResourceType { get; set; }
} 