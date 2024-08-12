using AllUp.Models;
using AllUp.ViewModels;

namespace AllUp.Interfaces
{
    public interface ILayoutService
    {
        Task<IDictionary<string, string>> GetSettingsAsync();
        Task<IEnumerable<Category>> GetCategoriesAsync();
        IEnumerable<CartVM> GetCart();
    }
}
