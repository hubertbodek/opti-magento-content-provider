using Cms.Integrations.Magento.Content;
using Cms.Integrations.Magento.Content.Product;
using EPiServer.Web;
using Microsoft.AspNetCore.Mvc;

namespace Cms.Controllers;

public class ProductContentController : Controller, IRenderTemplate<ProductContent>
{
    public ActionResult Index(ProductContent currentContent)
    {
        return View(currentContent);
    }
}