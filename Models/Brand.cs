using System.ComponentModel.DataAnnotations;

namespace AllUp.Models;

public class Brand : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    public List<Product>? Products { get; set; }
}
