public class Order
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long AddressId { get; set; }

    public decimal TotalAmount { get; set; }
    public string Note { get; set; }

    public string Status { get; set; }
    public string PaymentMethod { get; set; }
    public string PaymentStatus { get; set; }

    public DateTime CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    public DateTime? CompletedAt { get; set; }


    public User User { get; set; }
    public UserAddress Address { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<Review> Reviews { get; set; }
    public ICollection<Payment> Payments { get; set; }
    public virtual ICollection<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}