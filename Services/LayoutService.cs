using AllUp.Data;
using Microsoft.EntityFrameworkCore;
using AllUp.Interfaces;
using AllUp.Models;
using System.Text.Json;
using AllUp.ViewModels;
using Microsoft.IdentityModel.Tokens;

namespace AllUp.Services;

public class LayoutService : ILayoutService
{
    private readonly AllUpDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LayoutService(AllUpDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public IEnumerable<CartVM> GetCart()
    {
        var cartCookie = _httpContextAccessor.HttpContext.Request.Cookies["cart"];
        if (cartCookie.IsNullOrEmpty())
        {
            return new List<CartVM>();
        }
        IEnumerable<CartVM> cart = JsonSerializer.Deserialize<IEnumerable<CartVM>>(cartCookie);
        foreach (CartVM cartVM in cart)
        {
            Product product = _context.Products.FirstOrDefault(p => p.Id == cartVM.Id);
            cartVM.Name = product.Name;
            cartVM.Image = product.MainImage;
            cartVM.Price = product.DiscountPrice > 0 ? product.DiscountPrice : product.Price;
            cartVM.ExTax = product.ExTax;
        }
        return cart;
    }

    public async Task<IDictionary<string, string>> GetSettingsAsync() => await _context.Settings.AsNoTracking().Where(s => !s.IsDeleted).ToDictionaryAsync(s => s.Key, s => s.Value);

    public async Task<IEnumerable<Category>> GetCategoriesAsync() => await _context.Categories.AsNoTracking().Where(c => !c.IsDeleted && c.IsMain).Include(c => c.Children).ToListAsync();
}
