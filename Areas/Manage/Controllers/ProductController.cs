using AllUp.Data;
using AllUp.Extensions;
using AllUp.Models;
using AllUp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Manage.Controllers;

[Area("Manage")]
public class ProductController : Controller
{
    private readonly AllUpDbContext _context;

    public ProductController(AllUpDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking().Include(p => p.Category).Include(p => p.Brand).Where(p => !p.IsDeleted);
        return View(await PaginationVM<Product>.CreateAsync(query, page));
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
        ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();
        ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();

        return View();
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
        ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();
        ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
        if (!ModelState.IsValid) return View(product);

        var mainFile = product.MainPhoto;
        var hoverFile = product.HoverPhoto;
        var files = product.Photos;

        if (mainFile == null)
        {
            ModelState.AddModelError(nameof(Product.MainPhoto), "Main image is required");
            return View(product);
        }
        if (hoverFile == null)
        {
            ModelState.AddModelError(nameof(Product.HoverPhoto), "Hover image is required");
            return View(product);
        }
        if (files == null)
        {
            ModelState.AddModelError(nameof(Product.Photos), "Images is required");
            return View(product);
        }

        if (!await _context.Brands.AnyAsync(b => !b.IsDeleted && b.Id == product.BrandId))
        {
            ModelState.AddModelError(nameof(Product.BrandId), "Invalid brand id");
            return View(product);
        }
        if (!await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Id == product.CategoryId))
        {
            ModelState.AddModelError(nameof(Product.CategoryId), "Invalid category id");
            return View(product);
        }
        foreach (int tagId in product.TagIds)
        {
            if (!await _context.Tags.AnyAsync(b => !b.IsDeleted && b.Id == tagId))
            {
                ModelState.AddModelError(nameof(Product.TagIds), "Invalid tag id");
                return View(product);
            }
        }

        if (!mainFile.IsImage())
        {
            ModelState.AddModelError(nameof(Product.MainPhoto), "Invalid format");
            return View(product);
        }
        if (mainFile.DoesSizeExceed(100))
        {
            ModelState.AddModelError(nameof(Product.MainPhoto), "File limit exceeded");
            return View(product);
        }

        product.MainImage = await mainFile.SaveFileAsync("product");

        if (!hoverFile.IsImage())
        {
            ModelState.AddModelError(nameof(Product.HoverPhoto), "Invalid format");
            return View(product);
        }
        if (hoverFile.DoesSizeExceed(100))
        {
            ModelState.AddModelError(nameof(Product.HoverPhoto), "File limit exceeded");
            return View(product);
        }
        product.HoverImage = await hoverFile.SaveFileAsync("product");


        List<ProductImage> images = new();
        foreach (IFormFile file in files)
        {
            if (!file.IsImage())
            {
                ModelState.AddModelError(nameof(Product.Photos), "Invalid format");
                return View(product);
            }
            if (file.DoesSizeExceed(100))
            {
                ModelState.AddModelError(nameof(Product.Photos), "File limit exceeded");
                return View(product);
            }
            string filename = await file.SaveFileAsync("product");
            ProductImage image = new() { CreateDate = DateTime.Now, Image = filename, ProductId = product.Id };
            images.Add(image);
        }
        product.ProductImages = images;


        List<ProductTag> tags = new();
        foreach (int tagId in product.TagIds)
        {
            ProductTag tag = new() { CreateDate = DateTime.Now, TagId = tagId, ProductId = product.Id };
            tags.Add(tag);
        }
        product.ProductTags = tags;

        product.CreateDate = DateTime.Now;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Detail(int? id)
    {
        if (id == null) return BadRequest();
        Product? product = await _context.Products.Include(p => p.ProductImages).Include(p => p.Brand).Include(p => p.Category).FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return BadRequest();
        Product? product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (product == null) return NotFound();
        product.IsDeleted = true;
        product.DeleteDate = DateTime.Now;
        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product", product.MainImage);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

        path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product", product.HoverImage);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);


        foreach (var productImage in product.ProductImages)
        {
            productImage.IsDeleted = true;
            productImage.DeleteDate = DateTime.Now;
            path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product", productImage.Image);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int? id)
    {
        if (id == null) return BadRequest();
        Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (product == null) return NotFound();
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
        ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();
        ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
        return View(product);
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Update(int? id, Product product)
    {
        if (id == null || id != product.Id) return BadRequest();
        Product? existingProduct = await _context.Products.Include(p => p.ProductTags).FirstOrDefaultAsync(p => !p.IsDeleted && p.Id == id);

        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
        ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();
        ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
        if (!ModelState.IsValid) return View(product);

        var mainFile = product.MainPhoto;
        var hoverFile = product.HoverPhoto;
        var files = product.Photos;

        if (!await _context.Brands.AnyAsync(b => !b.IsDeleted && b.Id == product.BrandId))
        {
            ModelState.AddModelError(nameof(Product.BrandId), "Invalid brand id");
            return View(product);
        }
        if (!await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Id == product.CategoryId))
        {
            ModelState.AddModelError(nameof(Product.CategoryId), "Invalid category id");
            return View(product);
        }
        foreach (int tagId in product.TagIds)
        {
            if (!await _context.Tags.AnyAsync(b => !b.IsDeleted && b.Id == tagId))
            {
                ModelState.AddModelError(nameof(Product.TagIds), "Invalid tag id");
                return View(product);
            }
        }

        if (mainFile != null)
        {

            if (!mainFile.IsImage())
            {
                ModelState.AddModelError(nameof(Product.MainPhoto), "Invalid format");
                return View(product);
            }
            if (mainFile.DoesSizeExceed(100))
            {
                ModelState.AddModelError(nameof(Product.MainPhoto), "File limit exceeded");
                return View(product);
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product", existingProduct.MainImage);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

            existingProduct.MainImage = await mainFile.SaveFileAsync("product");
        }

        if (hoverFile != null)
        {
            if (!hoverFile.IsImage())
            {
                ModelState.AddModelError(nameof(Product.HoverPhoto), "Invalid format");
                return View(product);
            }
            if (hoverFile.DoesSizeExceed(100))
            {
                ModelState.AddModelError(nameof(Product.HoverPhoto), "File limit exceeded");
                return View(product);
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product", existingProduct.HoverImage);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            existingProduct.HoverImage = await hoverFile.SaveFileAsync("product");
        }


        if (files != null)
        {

            List<ProductImage> images = existingProduct.ProductImages;
            foreach (IFormFile file in files)
            {
                if (!file.IsImage())
                {
                    ModelState.AddModelError(nameof(Product.Photos), "Invalid format");
                    return View(product);
                }
                if (file.DoesSizeExceed(100))
                {
                    ModelState.AddModelError(nameof(Product.Photos), "File limit exceeded");
                    return View(product);
                }
                string filename = await file.SaveFileAsync("product");
                ProductImage image = new() { CreateDate = DateTime.Now, Image = filename, ProductId = existingProduct.Id };
                images.Add(image);
            }
            existingProduct.ProductImages = images;
        }

        foreach (var productTag in existingProduct.ProductTags)
        {
            productTag.IsDeleted = true;
            productTag.DeleteDate = DateTime.Now;
        }
        List<ProductTag> tags = existingProduct.ProductTags;
        foreach (int tagId in product.TagIds)
        {
            ProductTag tag = new() { CreateDate = DateTime.Now, TagId = tagId, ProductId = existingProduct.Id };
            tags.Add(tag);
        }
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.Price = product.Price;
        existingProduct.ExTax = product.ExTax;
        existingProduct.DiscountPrice = product.DiscountPrice;
        existingProduct.BrandId = product.BrandId;
        existingProduct.Count = product.Count;
        existingProduct.IsBestSeller = product.IsBestSeller;
        existingProduct.IsFeatured = product.IsFeatured;
        existingProduct.IsNewArrival = product.IsNewArrival;
        existingProduct.Code = product.Code;
        existingProduct.Seria = product.Seria;
        existingProduct.TagIds = product.TagIds;


        existingProduct.UpdateDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> DeleteImage(int? id)
    {
        if (id == null) return BadRequest();
        ProductImage? productImage = await _context.ProductImages.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (productImage == null) return NotFound();
        productImage.IsDeleted = true;
        productImage.DeleteDate = DateTime.Now;

        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "product", productImage.Image);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Update));
    }
}
