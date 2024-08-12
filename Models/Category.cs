using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllUp.Models;

public class Category : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    public string? Image { get; set; }
    public int? ParentId { get; set; }
    public bool IsMain { get; set; }
    public Category? Parent { get; set; }
    public List<Category>? Children { get; set; }
    public List<Product>? Products { get; set; }
    [NotMapped]
    public IFormFile? Photo { get; set; }
}
