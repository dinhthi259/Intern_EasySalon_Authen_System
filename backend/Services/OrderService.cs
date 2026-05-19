using System.Text.RegularExpressions;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly AppDbContext _context;
    private readonly IInventoryDocumentService _service;
    private readonly PayOSClient _payOS;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IInvoiceService _invoiceService;
    public OrderService(IOrderRepository orderRepository, AppDbContext context, IInventoryDocumentService service, PayOSClient payOS, IConfiguration configuration, IHubContext<NotificationHub> hubContext, IInvoiceService invoiceService)
    {
        _orderRepository = orderRepository;
        _context = context;
        _service = service;
        _payOS = payOS;
        _configuration = configuration;
        _hubContext = hubContext;
        _invoiceService = invoiceService;
    }

    public async Task<CreateOrderResponseDto> CreateOrderAsync(CreateOrderRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (request.Items == null || !request.Items.Any())
                throw new Exception("Order must have items");

            var productIds = request.Items.Select(i => i.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var inventories = await _context.Inventories
                .Where(i => productIds.Contains(i.ProductId))
                .ToListAsync();

            var latestImportPrices = await _context.InventoryImportItems
                .Where(x => productIds.Contains(x.ProductId))
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    CostPrice = g
                        .OrderByDescending(x => x.Id)
                        .Select(x => x.Price)
                        .FirstOrDefault()
                }).ToListAsync();

            decimal? totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null)
                    throw new Exception($"Product {item.ProductId} not found");

                var inventory = inventories.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (inventory == null || inventory.Quantity < item.Quantity)
                    throw new Exception($"Sản phẩm này hiện tại không đủ hàng");

                var latestCostPrice = latestImportPrices
                    .FirstOrDefault(x => x.ProductId == item.ProductId)
                    ?.CostPrice ?? 0;

                var price = product.DiscountPrice;
                totalAmount += price * item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = price,
                    CostPrice = latestCostPrice
                });
            }

            var order = new Order
            {
                UserId = request.UserId,
                AddressId = request.AddressId,
                TotalAmount = totalAmount ?? 0,
                Note = request.Note,
                Status = "Chờ xác nhận",
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = "Chờ thanh toán",
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            orderItems.ForEach(i => i.OrderId = order.Id);
            _context.OrderItems.AddRange(orderItems);

            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethod = request.PaymentMethod,
                Amount = totalAmount ?? 0,
                Status = "Pending"
            };

            _context.Payments.Add(payment);
            string? checkoutUrl = null;

            if (request.PaymentMethod == "PAYOS")
            {
                var orderCode = long.Parse($"{order.Id}{DateTimeOffset.Now.ToUnixTimeSeconds()}");

                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = (int)(totalAmount ?? 0),
                    Description = $"DH{order.Id}",
                    CancelUrl = _configuration["PayOS:CancelUrl"],
                    ReturnUrl = _configuration["PayOS:ReturnUrl"]
                };

                var paymentLink = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

                checkoutUrl = paymentLink.CheckoutUrl;

                payment.OrderCode = orderCode;
                payment.CheckoutUrl = checkoutUrl;
                payment.Status = "Pending";
                payment.ExpiredAt = DateTime.Now.AddHours(24);

                order.PaymentStatus = "Chờ thanh toán";
                order.Status = "Chờ thanh toán";
            }

            if (request.Type == "cart")
            {
                var cartItems = await _context.CartItems
                    .Where(c => c.Cart.UserId == request.UserId
                        && productIds.Contains(c.ProductId))
                    .ToListAsync();

                foreach (var item in request.Items)
                {
                    var cartItem = cartItems.FirstOrDefault(c => c.ProductId == item.ProductId);

                    if (cartItem != null)
                    {
                        _context.CartItems.Remove(cartItem);
                    }
                }
            }

            await _context.SaveChangesAsync();
            var exportRequest = new CreateExportRequest
            {
                ExportType = "ORDER",
                ReferenceId = order.Id,
                Note = $"Xuất kho cho đơn hàng #{order.Id}",
                Status = "PENDING",
                Items = request.Items.Select(i => new ExportItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = products.FirstOrDefault(p => p.Id == i.ProductId)?.DiscountPrice ?? 0
                }).ToList()
            };

            var notification = new Notification
            {
                UserId = order.UserId,
                Title = "Đặt hàng thành công",
                Message = $"Đơn hàng #{order.Id} đã được đặt thành công",
                Type = "CREATE_ORDER",
                IsRead = false,
                CreatedAt = DateTime.Now,
                Link = $"/my-orders"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            await _hubContext.Clients
                .Group($"user_{order.UserId}")
                .SendAsync("ReceiveNotification", notification);

            await _service.CreateExportAsync(exportRequest, request.UserId);
            await transaction.CommitAsync();

            return new CreateOrderResponseDto
            {
                OrderId = order.Id,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                CheckoutUrl = checkoutUrl
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrderDto>> GetOrdersAsync(long userId, OrderQueryRequest? query)
    {
        var orders = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Address)
            .Where(o => o.UserId == userId);

        if (!string.IsNullOrEmpty(query.Status))
        {
            orders = orders.Where(o => o.Status == query.Status);
        }

        if (!string.IsNullOrEmpty(query.Keyword))
        {
            var keyword = Regex.Replace(query.Keyword.Trim(), @"\s+", " ");
            Console.WriteLine($"[GetOrdersAsync] Searching for keyword: '{keyword}'");
            orders = orders.Where(o =>
                o.OrderItems.Any(i =>
                    i.Product.Name.ToLower().Contains(keyword.ToLower())
                )
            );
        }

        var result = await orders
            .OrderByDescending(o => o.UpdateAt)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerName = o.Address.ReceiverName,
                Phone = o.Address.ReceiverPhone,
                TotalAmount = o.TotalAmount,
                PaymentStatus = o.PaymentStatus,
                PaymentMethod = o.PaymentMethod,
                ExpiredAt = o.Payments.OrderByDescending(p => p.CreateAt).Select(p => p.ExpiredAt).FirstOrDefault(),
                Status = o.Status,
                UpdateAt = o.UpdateAt,
                Items = o.OrderItems.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    Slug = i.Product.Slug,
                    ProductName = i.Product.Name,
                    Thumbnail = i.Product.Thumbnail,
                    Price = i.Price ?? 0,
                    Quantity = i.Quantity,
                    IsReview = i.IsReview,
                }).ToList()
            })
            .ToListAsync();

        return result;
    }

    public async Task CancelOrderAsync(long userId, long orderId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                throw new Exception("Đơn hàng không tồn tại");

            if (order.Status != OrderStatus.Pending)
                throw new Exception("Chỉ có thể hủy đơn hàng đang chờ xác nhận");

            order.Status = OrderStatus.Cancelled;
            order.UpdateAt = DateTime.Now;

            // 👉 tạo import trả kho
            var importRequest = new CreateImportRequest
            {
                SupplierId = null,
                Note = $"Nhập lại kho từ đơn hàng #{order.Id}",
                Items = order.OrderItems.Select(i => new ImportItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price ?? 0 // hoặc lấy cost chuẩn
                }).ToList()
            };

            await _service.CreateImportAsync(importRequest, userId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PayAgainResponseDto> PayAgainAsync(long orderId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId);

        if (order == null)
            throw new Exception("Không tìm thấy đơn hàng");

        if (order.PaymentMethod != "PAYOS")
            throw new Exception("Đơn hàng không dùng PayOS");

        if (order.PaymentStatus == "Đã thanh toán")
            throw new Exception("Đơn hàng đã thanh toán");

        var payment = await _context.Payments
            .Where(x => x.OrderId == orderId && x.Status == "Pending")
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (payment == null)
            throw new Exception("Không tìm thấy giao dịch thanh toán");

        if (payment.ExpiredAt == null || payment.ExpiredAt <= DateTime.Now)
            throw new Exception("Đơn hàng đã hết hạn thanh toán");

        var orderCode = long.Parse($"{order.Id}{DateTimeOffset.Now.ToUnixTimeSeconds()}");

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = (int)order.TotalAmount,
            Description = $"DH{order.Id}",
            CancelUrl = _configuration["PayOS:CancelUrl"],
            ReturnUrl = _configuration["PayOS:ReturnUrl"]
        };

        var paymentLink = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

        payment.OrderCode = orderCode;
        payment.CheckoutUrl = paymentLink.CheckoutUrl;
        payment.Status = "Pending";

        await _context.SaveChangesAsync();

        return new PayAgainResponseDto
        {
            CheckoutUrl = paymentLink.CheckoutUrl,
            ExpiredAt = payment.ExpiredAt
        };
    }

    public async Task UpdateAddressAsync(long userId, long orderId, long newAddressId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            throw new Exception("Đơn hàng không tồn tại");

        if (order.Status != OrderStatus.Pending)
            throw new Exception("Chỉ có thể cập nhật địa chỉ cho đơn hàng đang chờ xác nhận");

        order.AddressId = newAddressId;
        order.UpdateAt = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    //ADMIN
    public async Task<List<OrderDto>> GetAllOrdersAsync(AdminOrderQueryRequest query)
    {
        var orders = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .AsQueryable();

        // 🔥 STATUS
        if (!string.IsNullOrEmpty(query.Status))
        {
            orders = orders.Where(o => o.Status == query.Status);
        }

        // 🔥 ORDER ID
        if (!string.IsNullOrEmpty(query.OrderId))
        {
            if (long.TryParse(query.OrderId, out var orderId))
            {
                orders = orders.Where(o => o.Id == orderId);
            }
        }

        // 🔥 PHONE
        if (!string.IsNullOrEmpty(query.Phone))
        {
            orders = orders.Where(o => o.Address.ReceiverPhone.Contains(query.Phone));
        }

        // 🔥 CUSTOMER
        if (!string.IsNullOrEmpty(query.Customer))
        {
            orders = orders.Where(o => o.Address.ReceiverName.Contains(query.Customer));
        }

        // 🔥 PRODUCT NAME
        if (!string.IsNullOrEmpty(query.Product))
        {
            orders = orders.Where(o =>
                o.OrderItems.Any(i => i.Product.Name.Contains(query.Product)));
        }

        // 🔥 SHIPPING
        // if (!string.IsNullOrEmpty(query.Shipping))
        // {
        //     orders = orders.Where(o => o.ShippingMethod == query.Shipping);
        // }

        // 🔥 PAYMENT
        if (!string.IsNullOrEmpty(query.Payment))
        {
            orders = orders.Where(o => o.PaymentMethod == query.Payment);
        }

        // 🔥 DATE RANGE
        if (query.DateFrom.HasValue)
        {
            orders = orders.Where(o => o.CreateAt >= query.DateFrom.Value);
        }

        if (query.DateTo.HasValue)
        {
            orders = orders.Where(o => o.CreateAt <= query.DateTo.Value);
        }

        return await orders
            .OrderByDescending(o => o.CreateAt)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerName = o.Address.ReceiverName,
                Phone = o.Address.ReceiverPhone,
                Address = o.Address == null
                    ? ""
                    : o.Address.Street + ", " + o.Address.Ward + ", " + o.Address.District + ", " + o.Address.Province,

                TotalAmount = o.TotalAmount,
                Status = o.Status,
                UpdateAt = o.UpdateAt,
                PaymentStatus = o.PaymentStatus,
                PaymentMethod = o.PaymentMethod,
                ExpiredAt = o.Payments.OrderByDescending(p => p.CreateAt).Select(p => p.ExpiredAt).FirstOrDefault(),

                Items = o.OrderItems.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Thumbnail = i.Product.Thumbnail,
                    Price = i.Price ?? 0,
                    Quantity = i.Quantity
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<OrderDetailDto> GetOrderDetailAsync(long orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new Exception("Order not found");

        return new OrderDetailDto
        {
            Id = order.Id,
            CustomerName = order.Address.ReceiverName,
            Phone = order.Address.ReceiverPhone,
            Address = $"{order.Address.Street}, {order.Address.Ward}, {order.Address.District}, {order.Address.Province}",
            TotalAmount = order.TotalAmount,
            Status = order.Status,

            Items = order.OrderItems.Select(i => new OrderItemDto
            {
                ProductName = i.Product.Name,
                Thumbnail = i.Product.Thumbnail,
                Price = i.Price ?? 0,
                Quantity = i.Quantity
            }).ToList()
        };
    }

    public async Task UpdateOrderStatusAsync(long orderId, string newStatus)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var order = await _context.Orders
    .Include(o => o.Payments)
    .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
    .Include(o => o.User)
        .ThenInclude(u => u.Profile)
    .Include(o => o.Address)
    .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            throw new Exception("Order not found");

        if (order.PaymentMethod == "PAYOS" && order.PaymentStatus != "Đã thanh toán" && newStatus != "Đã hủy")
        {
            throw new Exception("Đơn hàng PayOS chưa thanh toán, không thể xác nhận đơn.");
        }

        var current = order.Status;

        // ❌ validate flow
        if (current == OrderStatus.Pending && newStatus == OrderStatus.Processing)
        {
            order.Status = newStatus;
            var notification = new Notification
            {
                UserId = order.UserId,
                Title = "Đơn hàng đang được xử lý",
                Message = $"Đơn hàng #{order.Id} của bạn đang được xử lý",
                Type = "UPDATE_ORDER",
                IsRead = false,
                CreatedAt = DateTime.Now,
                Link = $"/my-orders"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            Console.WriteLine($"SEND TO GROUP: user_{order.UserId}");
            await _hubContext.Clients
                .Group($"user_{order.UserId}")
                .SendAsync("ReceiveNotification", notification);
        }
        else if (current == OrderStatus.Processing && newStatus == OrderStatus.Shipping)
        {
            order.Status = newStatus;
            var notification = new Notification
            {
                UserId = order.UserId,
                Title = "Đơn hàng đang được giao",
                Message = $"Đơn hàng #{order.Id} của bạn đang được giao",
                Type = "UPDATE_ORDER",
                IsRead = false,
                CreatedAt = DateTime.Now,
                Link = $"/my-orders"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            await _hubContext.Clients
                .Group($"user_{order.UserId}")
                .SendAsync("ReceiveNotification", notification);
        }
        else if (current == OrderStatus.Shipping && newStatus == OrderStatus.Completed)
        {
            order.Status = newStatus;
            order.PaymentStatus = "Đã thanh toán";
            order.Payments.FirstOrDefault(p => p.Status == "Pending").Status = "Paid";
            order.CompletedAt = DateTime.Now;
            var invoice = await _invoiceService.CreateInvoiceAsync(order.Id);

            // 3. Gửi mail hóa đơn
            BackgroundJob.Enqueue<InvoiceEmailService>(
                x => x.SendInvoiceEmailAsync(invoice.InvoiceId)
            );
            var inventoryExport = await _context.InventoryExports
                .FirstOrDefaultAsync(x =>
                    x.ExportType == "ORDER" &&
                    x.ReferenceId == order.Id);

            if (inventoryExport == null)
                throw new Exception($"Không tìm thấy phiếu xuất kho cho đơn hàng #{order.Id}");

            inventoryExport.Status = "COMPLETED";
            inventoryExport.ApprovedAt = DateTime.Now;
            inventoryExport.ApprovedBy = order.UserId;
            var notification = new Notification
            {
                UserId = order.UserId,
                Title = "Đơn hàng đã hoàn thành",
                Message = $"Đơn hàng #{order.Id} của bạn đã hoàn thành",
                Type = "UPDATE_ORDER",
                IsRead = false,
                CreatedAt = DateTime.Now,
                Link = $"/my-orders"
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            await _hubContext.Clients
                .Group($"user_{order.UserId}")
                .SendAsync("ReceiveNotification", notification);
        }
        else
        {
            throw new Exception("Invalid status transition");
        }
        order.UpdateAt = DateTime.Now;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<List<OrderCountByStatusDto>> AdminCountOrdersByStatusAsync()
    {
        return await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new OrderCountByStatusDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<OrderCountByStatusDto>> CountOrdersByStatusAsync(long userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .GroupBy(o => o.Status)
            .Select(g => new OrderCountByStatusDto
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }
}