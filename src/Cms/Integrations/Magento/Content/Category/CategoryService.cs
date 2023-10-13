namespace Cms.Integrations.Magento.Content.Category;

public class CategoryService
{
    public static IList<CategoryExternal> GetAll()
    {
        return new List<CategoryExternal>();
    }

    public static CategoryExternal GetCategoryById(string id)
    {
        var categories = GetAll();

        return categories.FirstOrDefault(c => c.Id.Equals(id));
    }
}