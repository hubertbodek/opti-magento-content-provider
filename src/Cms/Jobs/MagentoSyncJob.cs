using EPiServer.PlugIn;
using EPiServer.Scheduler;

namespace Cms.Jobs;

[ScheduledPlugIn(
    DisplayName = "Magento Sync Job",
    Description = "Import products and categories as blocks from Magento",
    GUID = "d6619008-3e76-4886-b3c7-9a025a0c2603")
]
public class MagentoSyncJob : ScheduledJobBase
{
    public override string Execute()
    {
        // get site definition
        // get folder to save categories and products to
        // call magento api
        //
        return "Change to message that describes outcome of execution";
    }
}