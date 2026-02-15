namespace Backend;

public class ShoppingItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Quantity { get; set; }
    public bool IsChecked { get; set; }
    public string AddedBy { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
