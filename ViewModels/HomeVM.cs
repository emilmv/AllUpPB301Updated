using AllUp.Models;

namespace AllUp.ViewModels;
public class HomeVM
{
    public IEnumerable<Slider> Sliders { get; set; }
    public IEnumerable<Category> Categories { get; set; }
    public IEnumerable<Product> NewArrivalProducts { get; set; }
    public IEnumerable<Product> BestSellerProducts { get; set; }
    public IEnumerable<Product> FeaturedProducts { get; set; }
}
