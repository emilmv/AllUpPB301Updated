namespace AllUp.ViewModels;
public class CartVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public int Count { get; set; }
    public decimal ExTax { get; set; }
    public decimal Price { get; set; }
}
