using AllUp.Data;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Controllers
{
    public class ProductController : Controller
    {
        private readonly AllUpDbContext _context;

        public ProductController(AllUpDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Modal(int? id)
        {
            if (id == null) return BadRequest();
            Product? product = _context.Products.Include(p => p.ProductImages).FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (product == null) return NotFound();
            return PartialView("_ProductModal", product);
        }

        public IActionResult Search(int? categoryId, string query)
        {
            if (categoryId != null && !_context.Categories.Any(c => c.Id == categoryId))
            {
                return BadRequest();
            }

            IEnumerable<Product> products = _context.Products
                .Include(p => p.Brand)
                .AsNoTracking()
                .Where(p => !p.IsDeleted &&
                categoryId != null ? p.CategoryId == categoryId : true &&
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Brand.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).AsEnumerable();

            return PartialView("_SearchResult", products);
        }

        public IActionResult Detail(int? id)
        {
            if (id is null) return BadRequest();

            Product? product = _context.Products.Include(p => p.ProductTags).ThenInclude(pt => pt.Tag).Include(p => p.ProductImages).Include(p => p.Brand).FirstOrDefault(p => !p.IsDeleted && p.Id == id);
            if (product == null) return NotFound();

            return View(product);

        }
    }
}
