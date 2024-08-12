namespace AllUp.Models;

public class Tag : BaseEntity
{
    public required string Name { get; set; }
    public List<ProductTag> ProductTags { get; set; }
}
