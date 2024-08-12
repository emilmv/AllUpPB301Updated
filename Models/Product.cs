using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllUp.Models;

public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    [Column(TypeName = "money")]
    public decimal Price { get; set; }
    [Column(TypeName = "money")]
    public decimal DiscountPrice { get; set; }
    public int Count { get; set; }
    [Range(0, 9999)]
    public int Seria { get; set; }
    public string Code { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ExTax { get; set; }
    public string? MainImage { get; set; }
    public string? HoverImage { get; set; }
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<ProductImage>? ProductImages { get; set; }
    [NotMapped]
    public IFormFile? MainPhoto { get; set; }
    [NotMapped]
    public IFormFile? HoverPhoto { get; set; }
    [NotMapped]
    public IFormFile[]? Photos { get; set; }
    public List<ProductTag>? ProductTags { get; set; }
    public List<int>? TagIds { get; set; }
}
