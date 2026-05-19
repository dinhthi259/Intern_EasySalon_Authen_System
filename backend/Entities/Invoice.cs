public class Invoice
{
    public long InvoiceId { get; set; }

    public string InvoiceCode { get; set; } = string.Empty;

    public long OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public string PdfUrl { get; set; } = string.Empty;

    public string Status { get; set; } = "Created";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? SentAt { get; set; }

    public Order? Order { get; set; }

    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}