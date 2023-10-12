namespace Cms.Integrations.Magento.Content.Category;

public interface ICategoryService
{
    public IList<CategoryExternal> GetAll();
    public CategoryExternal GetCategoryById(string id); 
}