using Microsoft.EntityFrameworkCore;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    public CartService(AppDbContext context)
    {
        _context = context;
    }
    public async Task AddToCartAsync(long userId, AddToCartRequest request)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive == true);
        if (product == null)
            throw new Exception("Product not found");
        var cart = await _context.Carts
            .Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
        if (cartItem != null)
        {
            cartItem.Quantity += request.Quantity;
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }
        await _context.SaveChangesAsync();
    }

    public async Task<CartDto> GetCartAsync(long userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }
        return new CartDto
        {
            CartId = cart.Id,
            Items = cart.CartItems.Select(ci => new CartItemDto
            {
                ProductId = ci.ProductId,
                Name = ci.Product.Name,
                Thumbnail = ci.Product.Thumbnail,
                Price = ci.Product.Price,
                DiscountPrice = ci.Product.DiscountPrice,
                Quantity = ci.Quantity,
                StockQuantity = _context.Inventories
                    .Where(i => i.ProductId == ci.ProductId)
                    .Select(i => i.Quantity)
                    .FirstOrDefault()
            }).ToList()
        };
    }

    public async Task RemoveFromCartAsync(long userId, long productId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) throw new Exception("Cart not found");

        var item = cart.CartItems
            .FirstOrDefault(x => x.ProductId == productId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateCartItemAsync(long userId, UpdateCartRequest request)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) throw new Exception("Cart not found");

        var item = cart.CartItems
            .FirstOrDefault(x => x.ProductId == request.ProductId);

        if (item == null) throw new Exception("Item not found");

        if (request.Quantity <= 0)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = request.Quantity;
        }

        await _context.SaveChangesAsync();
    }
}