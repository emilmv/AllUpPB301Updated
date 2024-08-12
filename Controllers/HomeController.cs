using AllUp.Data;
using AllUp.Models;
using AllUp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AllUp.Controllers;

public class HomeController : Controller
{
    private readonly AllUpDbContext _context;

    public HomeController(AllUpDbContext allUpDbContext)
    {
        _context = allUpDbContext;
    }

    public IActionResult Index()
    {
        IEnumerable<Slider> sliders = _context.Sliders.Where(s => !s.IsDeleted).AsEnumerable();
        IEnumerable<Category> categories = _context.Categories.Where(c => c.IsMain && !c.IsDeleted).AsEnumerable();
        IEnumerable<Product> newArrivalProducts = _context.Products.Where(p => p.IsNewArrival && !p.IsDeleted).AsEnumerable();
        IEnumerable<Product> bestSellerProducts = _context.Products.Where(p => p.IsBestSeller && !p.IsDeleted).AsEnumerable();
        IEnumerable<Product> featuredProducts = _context.Products.Where(p => p.IsFeatured && !p.IsDeleted).AsEnumerable();

        HomeVM vm = new();
        vm.Sliders = sliders;
        vm.Categories = categories;
        vm.FeaturedProducts = featuredProducts;
        vm.NewArrivalProducts = newArrivalProducts;
        vm.BestSellerProducts = bestSellerProducts;

        return View(vm);
    }
}
