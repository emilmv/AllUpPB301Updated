using AllUp.Data;
using AllUp.Extensions;
using AllUp.Models;
using AllUp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Manage.Controllers;

[Area("Manage")]
public class CategoryController : Controller
{
    private readonly AllUpDbContext _context;

    public CategoryController(AllUpDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        IQueryable<Category> query = _context.Categories.Include(c => c.Products).Where(c => !c.IsDeleted && c.IsMain);
        return View(await PaginationVM<Category>.CreateAsync(query, page, 2));
    }

    public async Task<IActionResult> SubCategoryIndex()
    {
        IEnumerable<Category> categories = await _context.Categories.Include(c => c.Products).Where(c => !c.IsDeleted && !c.IsMain).ToListAsync();
        return View(categories);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted && c.IsMain).ToListAsync();
        return View();
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted && c.IsMain).ToListAsync();
        if (await _context.Categories.AnyAsync(c => !c.IsDeleted && c.Name.ToLower() == category.Name.ToLower()))
        {
            ModelState.AddModelError("Name", "Already exists");
            return View(category);
        }
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        if (category.IsMain)
        {
            if (category.Photo == null)
            {
                ModelState.AddModelError("Photo", "Photo is required");
                return View(category);
            }
            if (!category.Photo.IsImage())
            {
                ModelState.AddModelError("Photo", "Invalid file type");
                return View(category);
            }
            if (category.Photo.DoesSizeExceed(150))
            {
                ModelState.AddModelError("Photo", "File size limit exceeded");
                return View(category);
            }
            category.Image = await category.Photo.SaveFileAsync();
            category.ParentId = null;
        }
        else
        {
            if (category.ParentId == null || !await _context.Categories.AnyAsync(c => !c.IsDeleted && c.IsMain))
            {
                ModelState.AddModelError("ParentId", "Invalid parent id");
                return View(category);
            }
            category.Photo = null;
        }
        category.CreateDate = DateTime.Now;
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(int? id)
    {
        if (id == null) return BadRequest();
        Category? existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && c.IsMain);
        if (existingCategory == null) return NotFound();
        return View(existingCategory);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> Update(int? id, Category category)
    {
        if (id == null || id != category.Id) return BadRequest();
        Category? existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && c.IsMain);
        if (existingCategory == null) return NotFound();
        if (!ModelState.IsValid) return View(existingCategory);
        var file = category.Photo;
        if (file != null)
        {
            if (!file.IsImage())
            {
                ModelState.AddModelError("Photo", "Invalid format");
                return View(existingCategory);
            }
            if (file.DoesSizeExceed(100))
            {
                ModelState.AddModelError("Photo", "File size exceeded");
                return View(existingCategory);
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images", existingCategory.Image);
            existingCategory.Image = await file.SaveFileAsync();
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        existingCategory.Name = category.Name.Trim();
        existingCategory.ParentId = null;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> UpdateSub(int? id)
    {
        if (id == null) return BadRequest();
        Category? existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && !c.IsMain);
        if (existingCategory == null) return NotFound();
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted && c.IsMain).ToListAsync();
        return View(existingCategory);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    public async Task<IActionResult> UpdateSub(int? id, Category category)
    {
        ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted && c.IsMain).ToListAsync();
        if (id == null || id != category.Id) return BadRequest();
        Category? existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (existingCategory == null) return NotFound();
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        if (await _context.Categories.AnyAsync(c => c.Id != id && !c.IsDeleted && c.Name.ToLower() == category.Name.ToLower()))
        {
            ModelState.AddModelError("Name", "Already exists");
            return View(category);
        }
        if (category.IsMain)
        {
            if (category.Photo == null)
            {
                ModelState.AddModelError("Photo", "Photo is required");
                return View(category);
            }
            if (!category.Photo.IsImage())
            {
                ModelState.AddModelError("Photo", "Invalid file type");
                return View(category);
            }
            if (category.Photo.DoesSizeExceed(150))
            {
                ModelState.AddModelError("Photo", "File size limit exceeded");
                return View(category);
            }
            existingCategory.Image = await category.Photo.SaveFileAsync();
            existingCategory.ParentId = null;
            existingCategory.IsMain = true;
        }
        else
        {
            if (category.ParentId == null || !await _context.Categories.AnyAsync(c => !c.IsDeleted && c.IsMain))
            {
                ModelState.AddModelError("ParentId", "Invalid parent id");
                return View(category);
            }
            existingCategory.Photo = null;
            existingCategory.IsMain = false;
            existingCategory.ParentId = category.ParentId;
        }
        existingCategory.Name = category.Name.Trim();
        existingCategory.UpdateDate = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(SubCategoryIndex));
    }

    public async Task<IActionResult> Detail(int? id)
    {
        if (id == null) return BadRequest();
        Category? existingCategory = await _context.Categories.AsNoTracking().Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (existingCategory == null) return NotFound();
        return View(existingCategory);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return BadRequest();
        Category? category = await _context.Categories.Include(c => c.Children).Include(c => c.Products.Where(p => !p.IsDeleted)).ThenInclude(p => p.ProductImages).FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);
        if (category == null) return NotFound();
        category.DeleteDate = DateTime.Now;
        category.IsDeleted = true;

        foreach (Product product in category.Products)
        {
            product.DeleteDate = DateTime.Now;
            product.IsDeleted = true;
            foreach (var image in product.ProductImages)
            {
                image.IsDeleted = true;
                image.DeleteDate = DateTime.Now;
            }
        }
        if (category.IsMain)
        {
            foreach (Category childCategory in category.Children)
            {
                childCategory.DeleteDate = DateTime.Now;
                childCategory.IsDeleted = true;
            }
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
