public class CartItemDto
{
    public long ProductId { get; set; }
    public string Name { get; set; }
    public string Thumbnail { get; set; }

    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }

    public int Quantity { get; set; }
    public int StockQuantity { get; set; }

    public decimal FinalPrice => (DiscountPrice ?? Price) * Quantity;
}