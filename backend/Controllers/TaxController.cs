using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/tax")]
public class TaxController : ControllerBase
{
    private readonly ITaxService _taxService;

    public TaxController(ITaxService taxService)
    {
        _taxService = taxService;
    }

    [HttpGet("invoices-unreported")]
    public async Task<IActionResult> GetUnreportedInvoices(
        string periodType,
        int? month,
        int? quarter,
        int year)
    {
        var result = await _taxService.GetUnreportedInvoicesAsync(
            periodType, month, quarter, year);

        return Ok(result);
    }

    [HttpPost("declarations/generate")]
    public async Task<IActionResult> GenerateDeclaration(
        [FromBody] GenerateTaxDeclarationRequest request)
    {
        var result = await _taxService.GenerateDeclarationAsync(request);
        return Ok(result);
    }

    [HttpGet("declarations")]
    public async Task<IActionResult> GetDeclarations()
    {
        var result = await _taxService.GetDeclarationsAsync();
        return Ok(result);
    }

    [HttpGet("declarations/{id}")]
    public async Task<IActionResult> GetDeclarationDetail(long id)
    {
        var result = await _taxService.GetDeclarationDetailAsync(id);
        return Ok(result);
    }

    [HttpPut("declarations/{id}/approve")]
    public async Task<IActionResult> ApproveDeclaration(long id)
    {
        var result = await _taxService.ApproveDeclarationAsync(id);
        return Ok(new { success = result });
    }
}