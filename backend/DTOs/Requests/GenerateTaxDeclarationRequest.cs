public class GenerateTaxDeclarationRequest
{
    public string PeriodType { get; set; } = ""; // MONTH / QUARTER
    public int? Month { get; set; }
    public int? Quarter { get; set; }
    public int Year { get; set; }
    public string? Note { get; set; }
}