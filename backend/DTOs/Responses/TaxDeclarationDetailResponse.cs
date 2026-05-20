public class TaxDeclarationDetailResponse : TaxDeclarationResponse
{
    public string? Note { get; set; }

    public List<TaxDeclarationInvoiceResponse> Details { get; set; } = new();
}