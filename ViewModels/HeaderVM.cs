using AllUp.Models;

namespace AllUp.ViewModels;
public class HeaderVM
{
    public IDictionary<string, string> Settings { get; set; }
    public IEnumerable<Category> Categories { get; set; }
    public IEnumerable<CartVM> Cart { get; set; }
}
