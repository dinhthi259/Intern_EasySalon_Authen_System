public interface ITaxService
{
    Task<List<Invoice>> GetUnreportedInvoicesAsync(string periodType,int? month,int? quarter,int year);
    Task<TaxDeclarationDto> GenerateDeclarationAsync(GenerateTaxDeclarationRequest request);
    Task<List<TaxDeclarationResponse>> GetDeclarationsAsync();
    Task<TaxDeclarationDetailResponse?> GetDeclarationDetailAsync(long id);
    Task<bool> ApproveDeclarationAsync(long id);
}