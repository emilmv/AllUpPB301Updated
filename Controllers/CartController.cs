using AllUp.Data;
using AllUp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace AllUp.Controllers;

public class CartController : Controller
{
    private readonly AllUpDbContext _context;

    public CartController(AllUpDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> AddCartAsync(int? id)
    {
        if (id == null) return BadRequest();
        var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);
        if (product == null) return BadRequest();
        List<CartVM> cart;
        string basket = HttpContext.Request.Cookies["cart"];
        if (basket.IsNullOrEmpty())
        {
            cart = new();
        }
        else
        {
            cart = JsonSerializer.Deserialize<List<CartVM>>(basket);
            if (cart.Exists(p => p.Id == id))
            {
                cart.Find(p => p.Id == id).Count++;
            }
            else
            {
                cart.Add(new()
                {
                    Id = product.Id,
                    Count = 1,
                    ExTax = product.ExTax,
                    Image = product.MainImage,
                    Name = product.Name,
                    Price = product.DiscountPrice > 0 ? product.DiscountPrice : product.Price
                });
            }
        }
        HttpContext.Response.Cookies.Append("cart", JsonSerializer.Serialize(cart));

        return PartialView("_CartPartial", cart);
    }

    public async Task<IActionResult> RemoveProduct(int? id)
    {
        if (id == null) return BadRequest();
        if (HttpContext.Request.Cookies["cart"] is null) return BadRequest();

        List<CartVM> cart = JsonSerializer.Deserialize<List<CartVM>>(HttpContext.Request.Cookies["cart"]);
        CartVM? existProduct = cart.FirstOrDefault(p => p.Id == id);

        if (existProduct is null) return BadRequest();
        cart.Remove(existProduct);

        HttpContext.Response.Cookies.Append("cart", JsonSerializer.Serialize(cart));
        return Json(cart);
    }

    public IActionResult Increase(int? id)
    {
        if (id is null) return BadRequest();
        if (HttpContext.Request.Cookies["cart"] is null) return BadRequest();

        List<CartVM> cart = JsonSerializer.Deserialize<List<CartVM>>(HttpContext.Request.Cookies["cart"]);
        CartVM? existProduct = cart.FirstOrDefault(p => p.Id == id);

        if (existProduct is null) return BadRequest();
        existProduct.Count++;

        HttpContext.Response.Cookies.Append("cart", JsonSerializer.Serialize(cart));
        return Json(cart);
    }

    public IActionResult Decrease(int? id)
    {
        if (id is null) return BadRequest();
        if (HttpContext.Request.Cookies["cart"] is null) return BadRequest();

        List<CartVM> cart = JsonSerializer.Deserialize<List<CartVM>>(HttpContext.Request.Cookies["cart"]);
        CartVM? existProduct = cart.FirstOrDefault(p => p.Id == id);

        if (existProduct is null) return BadRequest();
        if (existProduct.Count > 1)
            existProduct.Count--;
        else
        {
            cart.Remove(existProduct);
        }

        HttpContext.Response.Cookies.Append("cart", JsonSerializer.Serialize(cart));
        return Json(cart);
    }
}
