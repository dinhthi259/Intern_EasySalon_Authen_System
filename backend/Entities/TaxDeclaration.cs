public class TaxDeclaration
{
    public long TaxDeclarationId { get; set; }
    public string DeclarationCode { get; set; } = "";

    public string PeriodType { get; set; } = "";
    public int? Month { get; set; }
    public int? Quarter { get; set; }
    public int Year { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalFinalAmount { get; set; }
    public int TotalInvoice { get; set; }

    public string Status { get; set; } = "Draft";
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public decimal PurchaseAmount { get; set; }
    public decimal PurchaseTaxAmount { get; set; }
    public decimal DeductibleTaxAmount { get; set; }

    public decimal PreviousDeductibleTax { get; set; }
    public decimal VatPayable { get; set; }
    public decimal VatCarriedForward { get; set; }

    public List<TaxDeclarationDetail> TaxDeclarationDetails { get; set; } = new();
    public virtual ICollection<InventoryImport> InventoryImports { get; set; }
        = new List<InventoryImport>();
    public virtual ICollection<Invoice> Invoices { get; set; }
        = new List<Invoice>();

}