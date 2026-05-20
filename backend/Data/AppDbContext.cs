using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ExternalLogin> ExternalLogins { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<ResetPasswordToken> ResetPasswordTokens { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Passkey> Passkeys { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> productImages { get; set; }
    public DbSet<ProductSpecification> productSpecifications { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryLog> InventoryLogs { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<InventoryImport> InventoryImports { get; set; }
    public DbSet<InventoryImportItem> InventoryImportItems { get; set; }
    public DbSet<InventoryExport> InventoryExports { get; set; }
    public DbSet<InventoryExportItem> InventoryExportItems { get; set; }
    public DbSet<AiChatSession> AiChatSessions { get; set; }
    public DbSet<AiChatMessage> AiChatMessages { get; set; }
    public DbSet<ProductEmbedding> ProductEmbeddings { get; set; }
    public DbSet<AiRecommendation> AiRecommendations { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewImage> ReviewImages { get; set; }
    public DbSet<ReviewReply> ReviewReplies { get; set; }
    public DbSet<ProductView> ProductViews { get; set; }
    public DbSet<InventoryBatch> InventoryBatches { get; set; }
    public DbSet<InventoryExportItemBatch> InventoryExportItemBatches { get; set; }
    public DbSet<UserBankAccount> UserBankAccounts { get; set; }
    public DbSet<RefundRequest> RefundRequests { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<TaxDeclaration> TaxDeclarations { get; set; }
    public DbSet<TaxDeclarationDetail> TaxDeclarationDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureUser(modelBuilder);
    }
    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.PasswordHash).HasColumnName("password_hash");
            entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(x => x.EmailVerified).HasColumnName("email_verified").HasDefaultValue(false);
            entity.Property(x => x.EmailVerifiedAt).HasColumnName("email_verified_at");
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("update_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.HasMany(x => x.UserRoles).WithOne(ur => ur.User).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Orders).WithOne(o => o.User).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.UserAddresses).WithOne(ua => ua.User).HasForeignKey(ua => ua.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Reviews).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.UserBankAccounts).WithOne(uba => uba.User).HasForeignKey(uba => uba.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.RefundRequests).WithOne(rr => rr.User).HasForeignKey(rr => rr.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.ToTable("email_verification_tokens");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            entity.Property(x => x.Token).HasColumnName("token").HasMaxLength(255);
            entity.Property(x => x.IsUsed).HasColumnName("used").HasDefaultValue(false);
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_token");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            entity.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(255);
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false);
            entity.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            entity.Property(x => x.DeviceInfo).HasColumnName("device_info").HasMaxLength(255);
            entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(255);
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ExternalLogin>(entity =>
        {
            entity.ToTable("external_logins");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            entity.Property(x => x.Provider).HasColumnName("provider").HasMaxLength(255);
            entity.Property(x => x.ProviderUserId).HasColumnName("provider_user_id").HasMaxLength(255);
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("user_profiles");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.User).WithOne().HasForeignKey<UserProfile>(x => x.UserId);
            entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(255);
            entity.Property(x => x.Avatar).HasColumnName("avatar_url").HasMaxLength(255);
            entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(x => x.BirthDate).HasColumnName("date_of_birth");
            entity.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(10);
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("timestamp").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("update_at").HasColumnType("timestamp").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(x => x.User).WithOne(u => u.Profile).HasForeignKey<UserProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ResetPasswordToken>(entity =>
        {
            entity.ToTable("password_reset_tokens");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            entity.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(255);
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.IsUsed).HasColumnName("is_used").HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("timestamp").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(x => x.Id);

            entity.HasOne(s => s.User)
                  .WithMany(u => u.Sessions)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.RefreshToken)
                  .WithOne(r => r.Session)
                  .HasForeignKey<Session>(s => s.RefreshTokenId);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.RefreshTokenId).HasColumnName("refresh_token_id");
            entity.Property(x => x.DeviceInfo).HasColumnName("device_info").HasMaxLength(255);
            entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(255);
            entity.Property(x => x.LastActiveAt).HasColumnName("last_active_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.CreateAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(255);
            entity.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(255);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(x => x.ParentId).HasColumnName("parent_id");
            entity.Property(x => x.CreateAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(x => x.Parent).WithMany(c => c.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Passkey>(entity =>
        {
            entity.ToTable("passkeys");
            entity.HasKey(x => x.Id);
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            entity.Property(x => x.CredentialId).HasColumnName("credential_id").HasMaxLength(255);
            entity.Property(x => x.PublicKey).HasColumnName("public_key").HasMaxLength(1000);
            entity.Property(x => x.SignCount).HasColumnName("sign_count");
            entity.Property(x => x.DeviceName).HasColumnName("device_name").HasMaxLength(255);
            entity.Property(x => x.CreateAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(255);
            entity.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
            entity.Property(x => x.Brand).HasColumnName("brand").HasMaxLength(100);
            entity.Property(x => x.Description).HasColumnName("description");
            entity.Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(x => x.DiscountPrice).HasColumnName("discount_price").HasColumnType("decimal(12,2)");
            entity.Property(x => x.Thumbnail).HasColumnName("thumbnail").HasMaxLength(500);
            entity.Property(x => x.RatingAvg).HasColumnName("rating_avg").HasColumnType("decimal(3,2)").HasDefaultValue(0);
            entity.Property(x => x.RatingCount).HasColumnName("rating_count").HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(x => x.CreateAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            // 🔗 Relation Category
            entity.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Images).WithOne(i => i.Product).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Specifications).WithOne(s => s.Product).HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.CartItems).WithOne(ci => ci.Product).HasForeignKey(ci => ci.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Inventory).WithOne(i => i.Product).HasForeignKey<Inventory>(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.InventoryLogs).WithOne(il => il.Product).HasForeignKey(il => il.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.OrderItems).WithOne(oi => oi.Product).HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.Reviews).WithOne(r => r.Product).HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(500).IsRequired();
            entity.Property(x => x.IsMain).HasColumnName("is_main").HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
            // 🔗 Relation Product
            entity.HasOne(x => x.Product).WithMany(p => p.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductSpecification>(entity =>
        {
            entity.ToTable("product_specifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(x => x.SpecName).HasColumnName("spec_name").HasMaxLength(255);
            entity.Property(x => x.SpecValue).HasColumnName("spec_value").HasMaxLength(255);
            // 🔗 Relation Product
            entity.HasOne(x => x.Product).WithMany(p => p.Specifications).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("carts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            // 🔗 Relation User
            entity.HasOne(x => x.User).WithMany(u => u.Carts).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.CartId).HasColumnName("cart_id").IsRequired();
            entity.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
            // 🔗 Relation Cart
            entity.HasOne(x => x.Cart).WithMany(c => c.CartItems).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
            // 🔗 Relation Product
            entity.HasOne(x => x.Product).WithMany(p => p.CartItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.ToTable("user_addresses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.ReceiverName).HasColumnName("receiver_name").HasMaxLength(100);
            entity.Property(x => x.ReceiverPhone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(x => x.Province).HasColumnName("province").HasMaxLength(100);
            entity.Property(x => x.District).HasColumnName("district").HasMaxLength(100);
            entity.Property(x => x.Ward).HasColumnName("ward").HasMaxLength(100);
            entity.Property(x => x.Street).HasColumnName("address_detail").HasMaxLength(255);
            entity.Property(x => x.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            // 🔗 Relation User
            entity.HasOne(x => x.User).WithMany(u => u.UserAddresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Orders).WithOne(o => o.Address).HasForeignKey(o => o.AddressId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(x => x.CreateAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserRole>(entity =>
       {
           entity.ToTable("user_roles");
           entity.HasKey(x => new { x.UserId, x.RoleId });
           entity.HasOne(x => x.User).WithMany(u => u.UserRoles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
           entity.HasOne(x => x.Role).WithMany(r => r.UserRoles).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
       });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.ToTable("inventory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
            entity.HasIndex(x => x.ProductId).IsUnique();
            entity.Property(x => x.Quantity).HasColumnName("quantity").IsRequired().HasDefaultValue(0);
            entity.Property(x => x.LastUpdated).HasColumnName("last_updated").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            // 🔗 Relation Product
            entity.HasOne(x => x.Product).WithOne(p => p.Inventory).HasForeignKey<Inventory>(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InventoryLog>(entity =>
        {
            entity.ToTable("inventory_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(x => x.QuantityChanged).HasColumnName("quantity_change").IsRequired();
            entity.Property(x => x.QuantityBefore).HasColumnName("quantity_before").IsRequired();
            entity.Property(x => x.QuantityAfter).HasColumnName("quantity_after").IsRequired();
            entity.Property(x => x.ChangeType).HasColumnName("change_type").HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReferenceId).HasColumnName("reference_id").HasMaxLength(255);
            entity.Property(x => x.Note).HasColumnName("note").HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(12,2)").HasDefaultValue(0);
            entity.Property(e => e.Total).HasColumnName("total").HasColumnType("decimal(12,2)").ValueGeneratedOnAddOrUpdate(); // vì là generated column
            entity.Property(x => x.CreateAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(x => x.ProductId);
            entity.HasIndex(x => x.CreateAt);

            // 🔗 Relation Product
            entity.HasOne(x => x.Product).WithMany(p => p.InventoryLogs).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(x => x.AddressId).HasColumnName("address_id").IsRequired();
            entity.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(x => x.Note).HasColumnName("note").HasMaxLength(255);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
            entity.Property(x => x.PaymentStatus).HasColumnName("payment_status").HasMaxLength(50);
            entity.Property(x => x.CreateAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateAt).HasColumnName("update_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔗 Relation Orders
            entity.HasOne(x => x.User).WithMany(u => u.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Address).WithMany(ua => ua.Orders).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(x => x.OrderItems).WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Payments).WithOne(p => p.Order).HasForeignKey(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.Reviews).WithOne(oi => oi.Order).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(x => x.RefundRequests).WithOne(rr => rr.Order).HasForeignKey(rr => rr.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(x => x.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(x => x.Price).HasColumnName("price").HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(x => x.CostPrice).HasColumnName("cost_price").HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
            entity.Property(x => x.IsReview)
            .HasColumnName("is_review").HasDefaultValue(false);

            // 🔗 Relation Order
            entity.HasOne(x => x.Order).WithMany(o => o.OrderItems).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            // 🔗 Relation Product
            entity.HasOne(x => x.Product).WithMany(p => p.OrderItems).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OrderCode).HasColumnName("order_code");
            entity.Property(x => x.OrderId).HasColumnName("order_id").IsRequired();
            entity.Property(x => x.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)").IsRequired();
            entity.Property(x => x.TransactionId).HasColumnName("transaction_id").HasMaxLength(255);
            entity.Property(x => x.CheckoutUrl).HasColumnName("checkout_url").HasColumnType("text");
            entity.Property(x => x.ExpiredAt).HasColumnName("expired_at").HasColumnType("datetime");

            entity.Property(x => x.PaidAt).HasColumnName("paid_at").HasColumnType("datetime");

            entity.Property(x => x.CreateAt).HasColumnName("create_at").HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔗 Relation Order
            entity.HasOne(x => x.Order).WithMany(o => o.Payments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<UserBankAccount>(entity =>
        {
            entity.ToTable("user_bank_account");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(x => x.BankName)
                .HasColumnName("bank_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.BankAccountNumber)
                .HasColumnName("bank_account_number")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.BankAccountName)
                .HasColumnName("bank_account_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.BankLogo)
                .HasColumnName("bank_logo")
                .HasMaxLength(255);

            entity.Property(x => x.IsDefault)
                .HasColumnName("is_default")
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔗 Relation User
            entity.HasOne(x => x.User)
                .WithMany(u => u.UserBankAccounts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefundRequest>(entity =>
        {
            entity.ToTable("refund_request");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            entity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .IsRequired();

            entity.Property(x => x.Reason)
                .HasColumnName("reason")
                .HasColumnType("text");

            entity.Property(x => x.BankName)
                .HasColumnName("bank_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.BankAccountNumber)
                .HasColumnName("bank_account_number")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.BankAccountName)
                .HasColumnName("bank_account_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.BankLogo)
                .HasColumnName("bank_logo")
                .HasMaxLength(255);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.RefundedAt)
                .HasColumnName("refunded_at")
                .HasColumnType("datetime");

            // 🔗 Relation Order
            entity.HasOne(x => x.Order)
                .WithMany(o => o.RefundRequests)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔗 Relation User
            entity.HasOne(x => x.User)
                .WithMany(u => u.RefundRequests)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Message)
                .HasColumnName("message")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(x => x.Type)
                .HasColumnName("type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.IsRead)
                .HasColumnName("is_read")
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.Link)
                .HasColumnName("link")
                .HasMaxLength(500);

            // 🔗 Relation User
            entity.HasOne(x => x.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ActorUserId).HasColumnName("actor_user_id");
            entity.Property(e => e.TargetUserId).HasColumnName("target_user_id");
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Details).HasColumnName("details").HasColumnType("text");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.ActorUserId);
            entity.HasIndex(e => e.TargetUserId);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();

            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();

            entity.Property(x => x.ContactPerson).HasColumnName("contact_person");
            entity.Property(x => x.Phone).HasColumnName("phone");
            entity.Property(x => x.Email).HasColumnName("email");

            entity.Property(x => x.Address).HasColumnName("address");
            entity.Property(x => x.Province).HasColumnName("province");
            entity.Property(x => x.District).HasColumnName("district");

            entity.Property(x => x.TaxCode).HasColumnName("tax_code");

            entity.Property(x => x.BankName).HasColumnName("bank_name");
            entity.Property(x => x.BankAccount).HasColumnName("bank_account");

            entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);

            entity.Property(x => x.Note).HasColumnName("note");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.CreatedBy).HasColumnName("created_by");

            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InventoryImport>(entity =>
        {
            entity.ToTable("inventory_imports");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();

            entity.Property(x => x.SupplierId).HasColumnName("supplier_id");

            entity.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(12,2)");

            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);

            entity.Property(x => x.Note).HasColumnName("note");

            entity.Property(x => x.CreatedBy).HasColumnName("created_by");
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.TaxDeclarationId).HasColumnName("tax_declaration_id");
            entity.Property(x => x.TaxDeclared).HasColumnName("tax_declared").HasDefaultValue(false);

            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.Imports)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.TaxDeclaration)
                .WithMany(x => x.InventoryImports)
                .HasForeignKey(x => x.TaxDeclarationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InventoryImportItem>(entity =>
        {
            entity.ToTable("inventory_import_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ImportId).HasColumnName("import_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");

            entity.Property(x => x.Quantity).HasColumnName("quantity");
            entity.Property(x => x.Price).HasColumnName("cost_price").HasColumnType("decimal(12,2)");
            entity.Property(x => x.TotalCost).HasColumnName("total_cost").HasColumnType("decimal(12,2)");

            entity.HasOne(x => x.Import)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ImportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId);
        });

        modelBuilder.Entity<InventoryExport>(entity =>
        {
            entity.ToTable("inventory_exports");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.Code).IsUnique();

            entity.Property(x => x.ExportType).HasColumnName("export_type").HasMaxLength(50);

            entity.Property(x => x.ReferenceId).HasColumnName("reference_id");

            entity.Property(x => x.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(12,2)");

            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(50);

            entity.Property(x => x.Note).HasColumnName("note");

            entity.Property(x => x.CreatedBy).HasColumnName("created_by");
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by");

            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.HasOne(x => x.CreatedByUser)
         .WithMany()
         .HasForeignKey(x => x.CreatedBy);

            entity.HasOne(x => x.ApprovedByUser)
                  .WithMany()
                  .HasForeignKey(x => x.ApprovedBy);
        });

        modelBuilder.Entity<InventoryExportItem>(entity =>
        {
            entity.ToTable("inventory_export_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExportId).HasColumnName("export_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");

            entity.Property(x => x.Quantity).HasColumnName("quantity");

            entity.Property(x => x.Price)
                .HasColumnName("price")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            entity.Property(x => x.CostPrice)
                .HasColumnName("cost_price")
                .HasColumnType("decimal(12,2)");

            entity.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasColumnType("decimal(12,2)");

            entity.HasOne(x => x.Export)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ExportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId);

            entity.HasMany(x => x.ExportItemBatches)
    .WithOne(x => x.ExportItem)
    .HasForeignKey(x => x.ExportItemId);
        });

        modelBuilder.Entity<AiChatSession>(entity =>
        {
            entity.ToTable("ai_chat_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔗 Relation User
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // 🔗 Relation AiChatMessage
            entity.HasMany(x => x.Messages)
                .WithOne(m => m.Session)
                .HasForeignKey(m => m.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiChatMessage>(entity =>
        {
            entity.ToTable("ai_chat_messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SessionId).HasColumnName("session_id").IsRequired();
            entity.Property(x => x.Role).HasColumnName("role").HasMaxLength(20);
            entity.Property(x => x.Message).HasColumnName("message").HasColumnType("text");
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔗 Relation AiChatSession
            entity.HasOne(x => x.Session)
                .WithMany(s => s.Messages)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductEmbedding>(entity =>
        {
            entity.ToTable("product_embeddings");
            entity.HasKey(x => x.ProductId);
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.Embedding).HasColumnName("embedding").HasColumnType("json");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔗 Relation Product
            entity.HasOne(x => x.Product)
                .WithOne()
                .HasForeignKey<ProductEmbedding>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AiRecommendation>(entity =>
        {
            entity.ToTable("ai_recommendations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.Score).HasColumnName("score").HasColumnType("float");
            entity.Property(x => x.CreatedAt).HasColumnName("create_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔗 Relation User
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // 🔗 Relation Product
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");

            // Primary Key
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            entity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(x => x.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            entity.Property(x => x.Rating)
                .HasColumnName("rating")
                .IsRequired();

            entity.Property(x => x.Comment)
                .HasColumnName("comment")
                .HasColumnType("text");

            entity.Property(x => x.CreateAt)
                .HasColumnName("create_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.VerifyPurchase)
            .HasColumnName("verified_purchase").HasDefaultValue(false);

            // 🔗 Relation Product
            entity.HasOne(x => x.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔗 Relation User
            entity.HasOne(x => x.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Order)
                .WithMany(p => p.Reviews)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔗 Relation ReviewImage
            entity.HasMany(x => x.Images)
                .WithOne(i => i.Review)
                .HasForeignKey(i => i.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewImage>(entity =>
        {
            entity.ToTable("review_images");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.ReviewId)
                .HasColumnName("review_id")
                .IsRequired();

            entity.Property(x => x.ImageUrl)
                .HasColumnName("image_url")
                .HasMaxLength(500)
                .IsRequired();

            // 🔗 Review
            entity.HasOne(x => x.Review)
                .WithMany(r => r.Images)
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewReply>(entity =>
        {
            entity.ToTable("review_replies");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("id");

            entity.Property(x => x.ReviewId)
                .HasColumnName("review_id")
                .IsRequired();

            entity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            entity.Property(x => x.Reply)
                .HasColumnName("reply")
                .HasColumnType("text");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(x => x.ReviewId)
                .HasDatabaseName("idx_reviewreply_review");

            entity.HasIndex(x => x.UserId)
                .HasDatabaseName("idx_reviewreply_user");

            entity.HasOne(x => x.Review)
                .WithOne(x => x.Reply)
                .HasForeignKey<ReviewReply>(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany(x => x.ReviewReplies)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductView>(entity =>
        {
            entity.ToTable("product_views");

            entity.HasKey(x => x.Id);

            // FK mapping
            entity.Property(x => x.ProductId)
                .HasColumnName("product_id");

            entity.Property(x => x.UserId)
                .HasColumnName("user_id");

            entity.Property(x => x.ViewTime)
                .HasColumnName("view_time")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // relationships (CHỈ VIẾT 1 LẦN)
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InventoryBatch>(entity =>
        {
            entity.ToTable("inventory_batches");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProductId)
                .HasColumnName("product_id");

            entity.Property(x => x.ImportItemId)
                .HasColumnName("import_item_id");

            entity.Property(x => x.OriginalQuantity)
                .HasColumnName("original_quantity");

            entity.Property(x => x.RemainingQuantity)
                .HasColumnName("remaining_quantity");

            entity.Property(x => x.CostPrice)
                .HasColumnName("cost_price")
                .HasColumnType("decimal(12,2)");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId);

            entity.HasOne(x => x.ImportItem)
                .WithMany()
                .HasForeignKey(x => x.ImportItemId);
        });

        modelBuilder.Entity<InventoryExportItemBatch>(entity =>
        {
            entity.ToTable("inventory_export_item_batches");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExportItemId)
                .HasColumnName("export_item_id");

            entity.Property(x => x.BatchId)
                .HasColumnName("batch_id");

            entity.Property(x => x.Quantity)
                .HasColumnName("quantity");

            entity.Property(x => x.CostPrice)
                .HasColumnName("cost_price")
                .HasColumnType("decimal(12,2)");

            entity.HasOne(x => x.ExportItem)
                .WithMany(x => x.ExportItemBatches)
                .HasForeignKey(x => x.ExportItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Batch)
                .WithMany(x => x.ExportItemBatches)
                .HasForeignKey(x => x.BatchId);
        });
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");

            entity.HasKey(x => x.InvoiceId);

            entity.Property(x => x.InvoiceId)
                .HasColumnName("invoice_id");

            entity.Property(x => x.InvoiceCode)
                .HasColumnName("invoice_code")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.InvoiceCode)
                .IsUnique();

            entity.Property(x => x.OrderId)
                .HasColumnName("order_id");

            entity.Property(x => x.CustomerName)
                .HasColumnName("customer_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.CustomerEmail)
                .HasColumnName("customer_email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.TaxAmount)
                .HasColumnName("tax_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.FinalAmount)
                .HasColumnName("final_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.PdfUrl)
                .HasColumnName("pdf_url")
                .HasColumnType("text");

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .HasDefaultValue("Created");

            entity.Property(x => x.TaxDeclarationId)
                .HasColumnName("tax_declaration_id");

            entity.Property(x => x.TaxDeclared)
                .HasColumnName("tax_declared")
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.SentAt)
                .HasColumnName("sent_at")
                .HasColumnType("datetime");

            entity.HasOne(x => x.Order)
                .WithMany(o => o.Invoices)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.InvoiceItems)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TaxDeclaration)
                .WithMany(t => t.Invoices)
                .HasForeignKey(x => x.TaxDeclarationId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("invoice_items");

            entity.HasKey(x => x.InvoiceItemId);

            entity.Property(x => x.InvoiceItemId)
                .HasColumnName("invoice_item_id");

            entity.Property(x => x.InvoiceId)
                .HasColumnName("invoice_id");

            entity.Property(x => x.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .HasDefaultValue(1);

            entity.Property(x => x.UnitPrice)
                .HasColumnName("unit_price")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(x => x.TotalPrice)
                .HasColumnName("total_price")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.InvoiceItems)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<TaxDeclaration>(entity =>
        {
            entity.ToTable("tax_declarations");

            entity.HasKey(x => x.TaxDeclarationId);

            entity.Property(x => x.TaxDeclarationId)
                .HasColumnName("tax_declaration_id");

            entity.Property(x => x.DeclarationCode)
                .HasColumnName("declaration_code")
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.DeclarationCode)
                .IsUnique();

            entity.Property(x => x.PeriodType)
                .HasColumnName("period_type")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Month)
                .HasColumnName("month");

            entity.Property(x => x.Quarter)
                .HasColumnName("quarter");

            entity.Property(x => x.Year)
                .HasColumnName("year")
                .IsRequired();

            entity.Property(x => x.TotalRevenue)
                .HasColumnName("total_revenue")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.TotalTaxAmount)
                .HasColumnName("total_tax_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.TotalFinalAmount)
                .HasColumnName("total_final_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.TotalInvoice)
                .HasColumnName("total_invoice")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(x => x.PurchaseTaxAmount)
                .HasColumnName("purchase_tax_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(x => x.DeductibleTaxAmount)
                .HasColumnName("deductible_tax_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(x => x.PurchaseAmount)
                .HasColumnName("purchase_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(x => x.PreviousDeductibleTax)
                .HasColumnName("previous_deductible_tax")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(x => x.VatPayable)
                .HasColumnName("vat_payable")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(x => x.VatCarriedForward)
                .HasColumnName("vat_carried_forward")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .HasDefaultValue("Draft")
                .IsRequired();

            entity.Property(x => x.Note)
                .HasColumnName("note")
                .HasColumnType("text");

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.ApprovedAt)
                .HasColumnName("approved_at");
        });
        modelBuilder.Entity<TaxDeclarationDetail>(entity =>
        {
            entity.ToTable("tax_declaration_details");

            entity.HasKey(x => x.TaxDeclarationDetailId);

            entity.Property(x => x.TaxDeclarationDetailId)
                .HasColumnName("tax_declaration_detail_id");

            entity.Property(x => x.TaxDeclarationId)
                .HasColumnName("tax_declaration_id")
                .IsRequired();

            entity.Property(x => x.InvoiceId)
                .HasColumnName("invoice_id")
                .IsRequired(false);

            entity.Property(x => x.InvoiceCode)
                .HasColumnName("invoice_code")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.CustomerName)
                .HasColumnName("customer_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.RevenueAmount)
                .HasColumnName("revenue_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.TaxAmount)
                .HasColumnName("tax_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.FinalAmount)
                .HasColumnName("final_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(x => x.InvoiceCreatedAt)
                .HasColumnName("invoice_created_at")
                .IsRequired();

            entity.Property(x => x.ImportId)
                .HasColumnName("import_id")
                .IsRequired(false);

            entity.Property(x => x.ImportCode)
                .HasColumnName("import_code")
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(x => x.PurchaseAmount)
                .HasColumnName("purchase_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(x => x.PurchaseTaxAmount)
                .HasColumnName("purchase_tax_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(x => x.PurchaseFinalAmount)
                .HasColumnName("purchase_final_amount")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();
            entity.Property(x => x.ImportCreatedAt)
                .HasColumnName("import_created_at")
                .IsRequired();
        
            // 🔗 Relation TaxDeclaration
            entity.HasOne(x => x.TaxDeclaration)
                .WithMany(t => t.TaxDeclarationDetails)
                .HasForeignKey(x => x.TaxDeclarationId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔗 Relation Invoice
            entity.HasOne(x => x.Invoice)
                .WithMany(x => x.TaxDeclarationDetails)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.InventoryImports)
                .WithMany(x => x.TaxDeclarationDetails)
                .HasForeignKey(x => x.ImportId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
