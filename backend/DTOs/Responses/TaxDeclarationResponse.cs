public class TaxDeclarationResponse
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

    public decimal PurchaseAmount { get; set; }
    public decimal PurchaseTaxAmount { get; set; }
    public decimal DeductibleTaxAmount { get; set; }

    public decimal PreviousDeductibleTax { get; set; }
    public decimal VatPayable { get; set; }
    public decimal VatCarriedForward { get; set; }
    public int TotalInvoice { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}