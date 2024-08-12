using AllUp.Data;
using AllUp.Models;
using AllUp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Manage.Controllers;

[Area("Manage")]
public class BrandController : Controller
{
    private readonly AllUpDbContext _context;

    public BrandController(AllUpDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        IQueryable<Brand> query = _context.Brands.AsNoTracking().Include(b => b.Products.Where(p => !p.IsDeleted)).Where(b => !b.IsDeleted);
        return View(await PaginationVM<Brand>.CreateAsync(query, page, 2));
    }

    public async Task<IActionResult> Detail(int? id)
    {
        if (id == null) return BadRequest();
        Brand? brand = await _context.Brands.AsNoTracking().Include(b => b.Products.Where(p => !p.IsDeleted)).FirstOrDefaultAsync(b => !b.IsDeleted && b.Id == id);
        if (brand is null) return NotFound();

        return View(brand);
    }

    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Create(Brand brand)
    {
        if (!ModelState.IsValid)
        {
            return View(brand);
        }
        if (await _context.Brands.AnyAsync(b => brand.Name.ToLower() == b.Name.ToLower() && !b.IsDeleted))
        {
            ModelState.AddModelError("Name", "Brand already exists");
            return View(brand);
        }
        Brand newBrand = new() { Name = brand.Name, CreateDate = DateTime.Now };
        await _context.Brands.AddAsync(newBrand);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return BadRequest();
        Brand? brand = await _context.Brands.Include(b => b.Products.Where(p => !p.IsDeleted)).FirstOrDefaultAsync(b => !b.IsDeleted && b.Id == id);
        if (brand == null) return NotFound();
        brand.DeleteDate = DateTime.Now;
        brand.IsDeleted = true;
        foreach (Product product in brand.Products)
        {
            product.IsDeleted = true;
            product.DeleteDate = DateTime.Now;
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int? id)
    {
        if (id == null) return BadRequest();
        Brand? brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        if (brand == null) return NotFound();
        return View(brand);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Update(int? id, Brand brand)
    {
        if (id == null) return BadRequest();
        if (brand.Id != id) return BadRequest();
        if (!ModelState.IsValid) return View(brand);
        if (await _context.Brands.AnyAsync(b => b.Id != id && b.Name == brand.Name))
        {
            ModelState.AddModelError("Name", "Brand with that name exists");
            return View(brand);
        };
        Brand? existBrand = await _context.Brands.FirstOrDefaultAsync(b => !b.IsDeleted && b.Id == brand.Id);
        if (existBrand == null) return NotFound();
        existBrand.UpdateDate = DateTime.Now;
        existBrand.Name = brand.Name;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
