using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

[Authorize(Roles = "ADMIN")]
[ApiController]
[Route("api/admin/invoices")]
public class InvoiceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public InvoiceController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] string? email,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var query = _context.Invoices.AsQueryable();

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(x => x.CustomerEmail.Contains(email));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Date >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Date <= toDate.Value.Date);
        }

        var invoices = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.InvoiceId,
                x.InvoiceCode,
                x.CustomerName,
                x.CustomerEmail,
                x.TotalAmount,
                x.FinalAmount,
                x.Status,
                x.PdfUrl,
                x.CreatedAt,
                x.SentAt
            })
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadInvoice(long id)
    {
        var invoice = await _context.Invoices.FindAsync(id);

        if (invoice == null)
            return NotFound("Không tìm thấy hóa đơn.");

        var filePath = Path.Combine(
            _env.WebRootPath,
            invoice.PdfUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
        );

        if (!System.IO.File.Exists(filePath))
            return NotFound("Không tìm thấy file PDF.");

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

        return File(bytes, "application/pdf", $"{invoice.InvoiceCode}.pdf");
    }

    [HttpPost("download-bulk")]
    public async Task<IActionResult> DownloadBulk([FromBody] List<long> invoiceIds)
    {
        var invoices = await _context.Invoices
            .Where(x => invoiceIds.Contains(x.InvoiceId))
            .ToListAsync();

        if (!invoices.Any())
            return BadRequest("Chưa chọn hóa đơn.");

        using var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var invoice in invoices)
            {
                var filePath = Path.Combine(
                    _env.WebRootPath,
                    invoice.PdfUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                if (!System.IO.File.Exists(filePath))
                    continue;

                var entry = archive.CreateEntry($"{invoice.InvoiceCode}.pdf");

                await using var entryStream = entry.Open();
                await using var fileStream = System.IO.File.OpenRead(filePath);

                await fileStream.CopyToAsync(entryStream);
            }
        }

        memoryStream.Position = 0;

        return File(memoryStream.ToArray(), "application/zip", "hoa-don.zip");
    }
}