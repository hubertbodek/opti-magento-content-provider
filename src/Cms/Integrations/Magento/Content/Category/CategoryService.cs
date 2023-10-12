namespace Cms.Integrations.Magento.Content.Category;

public class CategoryService
{
    public static IList<CategoryExternal> GetAll()
    {
        return new List<CategoryExternal>()
        {
            new() { Id = "1", Title = "Men", Description = "Men products" },
            new() { Id = "2", Title = "Women", Description = "Women products" },
            new() { Id = "3", Title = "Accessories", Description = "Accessories products" },
        };
    }

    public static CategoryExternal GetCategoryById(string id)
    {
        var categories = GetAll();

        return categories.FirstOrDefault(c => c.Id.Equals(id));
    }
}