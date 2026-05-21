using Microsoft.EntityFrameworkCore;

public class ProductService : IProductService
{
    private readonly ProductRepository _repo;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductService(ProductRepository repo, AppDbContext context, IWebHostEnvironment env)
    {
        _repo = repo;
        _context = context;
        _env = env;
    }

    public async Task<List<ProductDto>> GetAllAsync(ProductFilterRequest filter)
    {
        var query = _repo.GetQuery();

        // SEARCH
        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{filter.Search}%"));
        }

        // FILTER CATEGORY
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId);
        }
        else if (filter.ParentCategoryId.HasValue)
        {
            query = query.Where(p =>
                p.Category.ParentId == filter.ParentCategoryId);
        }

        // FILTER STATUS
        if (!string.IsNullOrEmpty(filter.Status))
        {
            if (filter.Status == "active")
                query = query.Where(p => p.IsActive);
            else if (filter.Status == "disabled")
                query = query.Where(p => !p.IsActive);
        }

        // FILTER PRICE
        if (filter.MinPrice.HasValue)
        {
            var minPrice = filter.MinPrice.Value;
            query = query.Where(p => p.DiscountPrice.HasValue
                ? p.DiscountPrice >= minPrice
                : p.Price >= minPrice);
        }

        if (filter.MaxPrice.HasValue)
        {
            var maxPrice = filter.MaxPrice.Value;
            query = query.Where(p => p.DiscountPrice.HasValue
                ? p.DiscountPrice <= maxPrice
                : p.Price <= maxPrice);
        }

        // FILTER BRANDS
        if (!string.IsNullOrEmpty(filter.Brands))
        {
            var brandList = filter.Brands.Split(",").Select(b => b.Trim()).ToList();
            query = query.Where(p => brandList.Contains(p.Brand));
        }

        // SORT
        if (!string.IsNullOrEmpty(filter.Sort))
        {
            switch (filter.Sort)
            {
                case "price-asc":
                    query = query.OrderBy(p => p.DiscountPrice ?? p.Price);
                    break;
                case "price-desc":
                    query = query.OrderByDescending(p => p.DiscountPrice ?? p.Price);
                    break;
                case "discount":
                    query = query.OrderByDescending(p =>
                        p.DiscountPrice.HasValue
                            ? (p.Price - p.DiscountPrice.Value) * 100 / p.Price
                            : 0);
                    break;
                default:
                    query = query.OrderByDescending(p => p.CreateAt);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(p => p.CreateAt);
        }

        return await query.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Price = p.Price,
            DiscountPrice = p.DiscountPrice,
            CategoryName = p.Category.Name,
            IsActive = p.IsActive,
        }
        ).ToListAsync();
    }

    public async Task<object> GetByIdAsync(long id)
    {
        var product = await _context.Products
            .Include(p => p.Category) // để lấy tên category
            .Where(p => p.Id == id && p.IsActive)
            .Select(p => new
            {
                p.Id,
                p.Name,
                Slug = p.Slug ?? "",
                Brand = p.Brand ?? "",
                p.Price,
                p.DiscountPrice,
                Description = p.Description ?? "",
                p.RatingAvg,
                p.RatingCount,
                Thumbnail = p.Thumbnail ?? "",
                p.CategoryId,
                CategoryName = p.Category.Name,
                ParentcategoryId = p.Category.ParentId,
                StockQuantity = _context.Inventories
                    .Where(i => i.ProductId == p.Id)
                    .Select(i => (int?)i.Quantity)
                    .FirstOrDefault() ?? 0,

                ParentCategoryName = _context.Categories
                    .Where(c => c.Id == p.Category.ParentId)
                    .Select(c => c.Name)
                    .FirstOrDefault(),

                DiscountPercent = p.DiscountPrice != null
                    ? (int)((p.Price - p.DiscountPrice) * 100 / p.Price)
                    : 0,

                SaleMoney = p.DiscountPrice != null
                    ? p.Price - p.DiscountPrice
                    : 0,

                Images = _context.productImages
                    .Where(i => i.ProductId == p.Id)
                    .OrderByDescending(i => i.IsMain)
                    .ThenBy(i => i.SortOrder)
                    .Select(i => new
                    {
                        i.Id,
                        i.ImageUrl,
                        i.IsMain
                    }).ToList(),

                Specifications = _context.productSpecifications
                    .Where(s => s.ProductId == p.Id)
                    .Select(s => new
                    {
                        s.Id,
                        s.SpecName,
                        s.SpecValue
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        return product; // null nếu không có
    }

    public async Task<long> CreateAsync(ProductCreateUpdateDto dto, string baseUrl)
    {
        var product = new Product
        {
            Name = dto.Name,
            Slug = GenerateSlug(dto.Name),
            CategoryId = dto.CategoryId,
            Brand = dto.Brand,
            Description = dto.Description,
            Price = dto.Price,
            DiscountPrice = dto.DiscountPrice,
            CreateAt = DateTime.Now,
            UpdateAt = DateTime.Now
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // ====== IMAGES ======
        if (dto.NewImages != null && dto.NewImages.Any())
        {
            var images = new List<ProductImage>();

            int index = 0;
            foreach (var file in dto.NewImages)
            {
                var url = await SaveFile(file, baseUrl);

                images.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    IsMain = index == 0,
                    SortOrder = index++
                });
            }

            _context.productImages.AddRange(images);

            // set thumbnail
            product.Thumbnail = images.First().ImageUrl;
        }

        // ====== SPECS ======
        if (dto.Specifications != null)
        {
            var specs = dto.Specifications.Select(s => new ProductSpecification
            {
                ProductId = product.Id,
                SpecName = s.SpecName,
                SpecValue = s.SpecValue
            }).ToList();

            _context.productSpecifications.AddRange(specs);
        }

        await _context.SaveChangesAsync();
        return product.Id;
    }

    public async Task<bool> UpdateAsync(long id, ProductCreateUpdateDto dto, string baseUrl)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return false;

        // ====== UPDATE BASIC ======
        product.Name = dto.Name;
        product.Slug = GenerateSlug(dto.Name);
        product.CategoryId = dto.CategoryId;
        product.Brand = dto.Brand;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.DiscountPrice = dto.DiscountPrice;
        product.UpdateAt = DateTime.Now;
        product.CategoryId = dto.CategoryId;

        // ====== DELETE IMAGES ======
        if (dto.DeletedImageIds != null && dto.DeletedImageIds.Any())
        {
            var deleteImgs = product.Images
                .Where(i => dto.DeletedImageIds.Contains(i.Id))
                .ToList();

            _context.productImages.RemoveRange(deleteImgs);
        }

        // ====== ADD NEW IMAGES ======
        if (dto.NewImages != null && dto.NewImages.Any())
        {
            int index = product.Images.Count;

            foreach (var file in dto.NewImages)
            {
                var url = await SaveFile(file, baseUrl);

                _context.productImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    SortOrder = index++
                });
            }
        }

        // ====== UPDATE SPECS ======
        _context.productSpecifications.RemoveRange(product.Specifications);

        if (dto.Specifications != null)
        {
            var specs = dto.Specifications.Select(s => new ProductSpecification
            {
                ProductId = product.Id,
                SpecName = s.SpecName,
                SpecValue = s.SpecValue
            });

            _context.productSpecifications.AddRange(specs);
        }

        // update thumbnail
        var firstImg = await _context.productImages
            .Where(i => i.ProductId == product.Id)
            .OrderBy(i => i.SortOrder)
            .FirstOrDefaultAsync();

        product.Thumbnail = firstImg?.ImageUrl;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ToggleActiveAsync(long id)
    {
        var product = await _repo.GetByIdAsync(id);

        if (product == null)
            throw new Exception("Product not found");

        product.IsActive = !product.IsActive;
        product.UpdateAt = DateTime.Now;

        _repo.Update(product);
        await _repo.SaveAsync();
    }

    // 🔍 SEARCH FOR HEADER - Tối ưu cho realtime search
    public async Task<List<ProductSearchDto>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<ProductSearchDto>();

        keyword = keyword.Trim().ToLower();

        var products = await _context.Products
            .Where(p => p.IsActive && EF.Functions.Like(p.Name, $"%{keyword}%"))
            .AsNoTracking()
            .OrderByDescending(p => p.Name.StartsWith(keyword)) // Ưu tiên sản phẩm bắt đầu với từ khóa
            .ThenByDescending(p => p.RatingAvg) // Sau đó sắp xếp theo rating
            .ThenByDescending(p => p.CreateAt) // Cuối cùng theo ngày tạo
            .Select(p => new ProductSearchDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Thumbnail = p.Thumbnail
            })
            .ToListAsync();

        return products;
    }

    private string GenerateSlug(string name)
    {
        return name.ToLower().Replace(" ", "-");
    }

    // ================= HELPER =================
    private async Task<string> SaveFile(IFormFile file, string baseUrl)
    {
        if (file == null || file.Length == 0)
            throw new Exception("File is empty");

        // fallback nếu null
        var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var folder = Path.Combine(rootPath, "uploads");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(folder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"{baseUrl}/uploads/{fileName}";
    }
}