using Microsoft.EntityFrameworkCore;

public class TaxService : ITaxService
{
    private readonly AppDbContext _context;
    public TaxService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<Invoice>> GetUnreportedInvoicesAsync(
    string periodType,
    int? month,
    int? quarter,
    int year)
    {
        var query = _context.Invoices
            .Where(x => x.TaxDeclared == false)
            .Where(x => x.Status == "Created" || x.Status == "Sent")
            .Where(x => x.CreatedAt.Year == year);

        if (periodType == "MONTH")
        {
            query = query.Where(x => x.CreatedAt.Month == month);
        }

        if (periodType == "QUARTER")
        {
            int startMonth = ((quarter.Value - 1) * 3) + 1;
            int endMonth = startMonth + 2;

            query = query.Where(x =>
                x.CreatedAt.Month >= startMonth &&
                x.CreatedAt.Month <= endMonth);
        }

        return await query.ToListAsync();
    }
    public async Task<TaxDeclarationDto> GenerateDeclarationAsync(
    GenerateTaxDeclarationRequest request)
    {
        var invoices = await GetUnreportedInvoicesAsync(
            request.PeriodType,
            request.Month,
            request.Quarter,
            request.Year
        );

        if (!invoices.Any())
        {
            throw new Exception("Không có hóa đơn chưa kê khai trong kỳ này.");
        }

        var declaration = new TaxDeclaration
        {
            DeclarationCode = GenerateDeclarationCode(request),
            PeriodType = request.PeriodType,
            Month = request.Month,
            Quarter = request.Quarter,
            Year = request.Year,

            TotalRevenue = invoices.Sum(x => x.TotalAmount),
            TotalTaxAmount = invoices.Sum(x => x.TaxAmount),
            TotalFinalAmount = invoices.Sum(x => x.FinalAmount),
            TotalInvoice = invoices.Count,

            Status = "Draft",
            Note = request.Note,
            CreatedAt = DateTime.Now
        };

        foreach (var invoice in invoices)
        {
            declaration.TaxDeclarationDetails.Add(new TaxDeclarationDetail
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceCode = invoice.InvoiceCode,
                CustomerName = invoice.CustomerName,
                RevenueAmount = invoice.TotalAmount,
                TaxAmount = invoice.TaxAmount,
                FinalAmount = invoice.FinalAmount,
                InvoiceCreatedAt = invoice.CreatedAt
            });
        }

        _context.TaxDeclarations.Add(declaration);
        await _context.SaveChangesAsync();

        return new TaxDeclarationDto
        {
            TaxDeclarationId = declaration.TaxDeclarationId,
            DeclarationCode = declaration.DeclarationCode,
            PeriodType = declaration.PeriodType,
            Month = declaration.Month,
            Quarter = declaration.Quarter,
            Year = declaration.Year,
            TotalRevenue = declaration.TotalRevenue,
            TotalTaxAmount = declaration.TotalTaxAmount,
            TotalFinalAmount = declaration.TotalFinalAmount,
            TotalInvoice = declaration.TotalInvoice,
            Status = declaration.Status,
            Note = declaration.Note,
            CreatedAt = declaration.CreatedAt,
            ApprovedAt = declaration.ApprovedAt,

            Details = declaration.TaxDeclarationDetails.Select(d => new TaxDeclarationInvoiceResponse
            {
                TaxDeclarationDetailId = d.TaxDeclarationDetailId,
                TaxDeclarationId = d.TaxDeclarationId,
                InvoiceId = d.InvoiceId,
                InvoiceCode = d.InvoiceCode,
                CustomerName = d.CustomerName,
                RevenueAmount = d.RevenueAmount,
                TaxAmount = d.TaxAmount,
                FinalAmount = d.FinalAmount,
                InvoiceCreatedAt = d.InvoiceCreatedAt
            }).ToList()
        };
    }
    public async Task<List<TaxDeclarationResponse>> GetDeclarationsAsync()
    {
        return await _context.TaxDeclarations
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new TaxDeclarationResponse
            {
                TaxDeclarationId = x.TaxDeclarationId,
                DeclarationCode = x.DeclarationCode,
                PeriodType = x.PeriodType,
                Month = x.Month,
                Quarter = x.Quarter,
                Year = x.Year,
                TotalRevenue = x.TotalRevenue,
                TotalTaxAmount = x.TotalTaxAmount,
                TotalFinalAmount = x.TotalFinalAmount,
                TotalInvoice = x.TotalInvoice,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                ApprovedAt = x.ApprovedAt
            })
            .ToListAsync();
    }
    public async Task<TaxDeclarationDetailResponse?> GetDeclarationDetailAsync(long id)
    {
        return await _context.TaxDeclarations
            .Where(x => x.TaxDeclarationId == id)
            .Select(x => new TaxDeclarationDetailResponse
            {
                TaxDeclarationId = x.TaxDeclarationId,
                DeclarationCode = x.DeclarationCode,
                PeriodType = x.PeriodType,
                Month = x.Month,
                Quarter = x.Quarter,
                Year = x.Year,
                TotalRevenue = x.TotalRevenue,
                TotalTaxAmount = x.TotalTaxAmount,
                TotalFinalAmount = x.TotalFinalAmount,
                TotalInvoice = x.TotalInvoice,
                Status = x.Status,
                Note = x.Note,
                CreatedAt = x.CreatedAt,
                ApprovedAt = x.ApprovedAt,

                Details = x.TaxDeclarationDetails.Select(d => new TaxDeclarationInvoiceResponse
                {
                    TaxDeclarationDetailId = d.TaxDeclarationDetailId,
                    InvoiceId = d.InvoiceId,
                    InvoiceCode = d.InvoiceCode,
                    CustomerName = d.CustomerName,
                    RevenueAmount = d.RevenueAmount,
                    TaxAmount = d.TaxAmount,
                    FinalAmount = d.FinalAmount,
                    InvoiceCreatedAt = d.InvoiceCreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }
    public async Task<bool> ApproveDeclarationAsync(long id)
    {
        var declaration = await _context.TaxDeclarations
            .Include(x => x.TaxDeclarationDetails)
            .FirstOrDefaultAsync(x => x.TaxDeclarationId == id);

        if (declaration == null)
            throw new Exception("Không tìm thấy tờ khai.");

        if (declaration.Status != "Draft")
            throw new Exception("Chỉ có thể duyệt tờ khai ở trạng thái Draft.");

        declaration.Status = "Approved";
        declaration.ApprovedAt = DateTime.Now;

        var invoiceIds = declaration.TaxDeclarationDetails.Select(x => x.InvoiceId).ToList();

        var invoices = await _context.Invoices
            .Where(x => invoiceIds.Contains(x.InvoiceId))
            .ToListAsync();

        foreach (var invoice in invoices)
        {
            invoice.TaxDeclared = true;
            invoice.TaxDeclarationId = declaration.TaxDeclarationId;
        }

        await _context.SaveChangesAsync();
        return true;
    }
    private string GenerateDeclarationCode(GenerateTaxDeclarationRequest request)
    {
        if (request.PeriodType == "MONTH")
        {
            return $"TAX-{request.Year}-M{request.Month}-{DateTime.Now:yyyyMMddHHmmss}";
        }

        return $"TAX-{request.Year}-Q{request.Quarter}-{DateTime.Now:yyyyMMddHHmmss}";
    }
}