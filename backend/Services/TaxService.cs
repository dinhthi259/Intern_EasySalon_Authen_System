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
            .Where(x => !x.TaxDeclared)
            .Where(x => x.Status == "Created" || x.Status == "Sent")
            .Where(x => x.CreatedAt.Year == year);

        if (periodType == "MONTH")
        {
            query = query.Where(x => x.CreatedAt.Month == month);
        }

        if (periodType == "QUARTER")
        {
            int startMonth = ((quarter!.Value - 1) * 3) + 1;
            int endMonth = startMonth + 2;

            query = query.Where(x =>
                x.CreatedAt.Month >= startMonth &&
                x.CreatedAt.Month <= endMonth);
        }

        return await query.ToListAsync();
    }

    public async Task<List<InventoryImport>> GetUnreportedImportsAsync(
        string periodType,
        int? month,
        int? quarter,
        int year)
    {
        var query = _context.InventoryImports
            .Where(x => !x.TaxDeclared)
            .Where(x => x.Status == "COMPLETED")
            .Where(x => x.ApprovedAt != null)
            .Where(x => x.ApprovedAt!.Value.Year == year);

        if (periodType == "MONTH")
        {
            query = query.Where(x => x.ApprovedAt!.Value.Month == month);
        }

        if (periodType == "QUARTER")
        {
            int startMonth = ((quarter!.Value - 1) * 3) + 1;
            int endMonth = startMonth + 2;

            query = query.Where(x =>
                x.ApprovedAt!.Value.Month >= startMonth &&
                x.ApprovedAt!.Value.Month <= endMonth);
        }

        return await query.ToListAsync();
    }

    public async Task<TaxDeclarationDto> GenerateDeclarationAsync(
        GenerateTaxDeclarationRequest request)
    {
        ValidateRequest(request);

        var invoices = await GetUnreportedInvoicesAsync(
            request.PeriodType,
            request.Month,
            request.Quarter,
            request.Year
        );

        var imports = await GetUnreportedImportsAsync(
            request.PeriodType,
            request.Month,
            request.Quarter,
            request.Year
        );

        if (!invoices.Any() && !imports.Any())
        {
            throw new Exception("Không có dữ liệu mua vào hoặc bán ra trong kỳ này.");
        }

        bool existedDraft = await _context.TaxDeclarations.AnyAsync(x =>
            x.PeriodType == request.PeriodType &&
            x.Month == request.Month &&
            x.Quarter == request.Quarter &&
            x.Year == request.Year &&
            x.Status == "Draft");

        if (existedDraft)
        {
            throw new Exception("Kỳ này đã có tờ khai nháp. Vui lòng duyệt hoặc hủy tờ khai cũ trước.");
        }

        decimal previousDeductibleTax = await GetPreviousDeductibleTaxAsync(
            request.PeriodType,
            request.Month,
            request.Quarter,
            request.Year
        );

        // Bán ra: giá đã gồm VAT
        decimal saleFinalAmount = invoices.Sum(x => x.FinalAmount);
        decimal saleRevenue = invoices.Sum(x => Math.Round(x.FinalAmount / 1.1m, 2));
        decimal saleVat = saleFinalAmount - saleRevenue;

        // Mua vào: giá nhập đã gồm VAT
        decimal purchaseFinalAmount = imports.Sum(x => x.TotalAmount);
        decimal purchaseAmount = imports.Sum(x => Math.Round(x.TotalAmount / 1.1m, 2));
        decimal purchaseTaxAmount = purchaseFinalAmount - purchaseAmount;
        decimal deductibleTaxAmount = purchaseTaxAmount;

        // [36] = [33] - [25]
        decimal vatGenerated = saleVat - deductibleTaxAmount;

        // Số quyết toán = [36] - [22]
        decimal taxBalance = vatGenerated - previousDeductibleTax;

        // [40]
        decimal vatPayable = taxBalance > 0 ? taxBalance : 0;

        // [43]
        decimal vatCarriedForward = taxBalance < 0 ? Math.Abs(taxBalance) : 0;

        var declaration = new TaxDeclaration
        {
            DeclarationCode = GenerateDeclarationCode(request),
            PeriodType = request.PeriodType,
            Month = request.Month,
            Quarter = request.Quarter,
            Year = request.Year,

            // [22]
            PreviousDeductibleTax = previousDeductibleTax,

            // Mua vào: [23], [24], [25]
            PurchaseAmount = purchaseAmount,
            PurchaseTaxAmount = purchaseTaxAmount,
            DeductibleTaxAmount = deductibleTaxAmount,

            // Bán ra: [32], [33]
            TotalRevenue = saleRevenue,
            TotalTaxAmount = saleVat,
            TotalFinalAmount = saleFinalAmount,
            TotalInvoice = invoices.Count,

            // [40], [43]
            VatPayable = vatPayable,
            VatCarriedForward = vatCarriedForward,

            Status = "Draft",
            Note = request.Note,
            CreatedAt = DateTime.Now
        };

        foreach (var invoice in invoices)
        {
            decimal revenueWithoutVat = Math.Round(invoice.FinalAmount / 1.1m, 2);
            decimal vatAmount = invoice.FinalAmount - revenueWithoutVat;

            Console.WriteLine(invoice.InvoiceId);
            declaration.TaxDeclarationDetails.Add(new TaxDeclarationDetail
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceCode = invoice.InvoiceCode,
                CustomerName = invoice.CustomerName,

                RevenueAmount = revenueWithoutVat,
                TaxAmount = vatAmount,
                FinalAmount = invoice.FinalAmount,

                InvoiceCreatedAt = invoice.CreatedAt
            });
        }

        foreach (var import in imports)
        {
            decimal importAmountWithoutVat = Math.Round(import.TotalAmount / 1.1m, 2);
            decimal importVatAmount = import.TotalAmount - importAmountWithoutVat;

            declaration.TaxDeclarationDetails.Add(new TaxDeclarationDetail
            {
                ImportId = import.Id,
                ImportCode = import.Code,

                PurchaseAmount = importAmountWithoutVat,
                PurchaseTaxAmount = importVatAmount,
                PurchaseFinalAmount = import.TotalAmount,

                ImportCreatedAt = import.ApprovedAt ?? import.CreatedAt
            });
        }

        _context.TaxDeclarations.Add(declaration);
        await _context.SaveChangesAsync();

        return MapToDto(declaration);
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

                PreviousDeductibleTax = x.PreviousDeductibleTax,

                PurchaseAmount = x.PurchaseAmount,
                PurchaseTaxAmount = x.PurchaseTaxAmount,
                DeductibleTaxAmount = x.DeductibleTaxAmount,

                TotalRevenue = x.TotalRevenue,
                TotalTaxAmount = x.TotalTaxAmount,
                TotalFinalAmount = x.TotalFinalAmount,
                TotalInvoice = x.TotalInvoice,

                VatPayable = x.VatPayable,
                VatCarriedForward = x.VatCarriedForward,

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

                PreviousDeductibleTax = x.PreviousDeductibleTax,

                PurchaseAmount = x.PurchaseAmount,
                PurchaseTaxAmount = x.PurchaseTaxAmount,
                DeductibleTaxAmount = x.DeductibleTaxAmount,

                TotalRevenue = x.TotalRevenue,
                TotalTaxAmount = x.TotalTaxAmount,
                TotalFinalAmount = x.TotalFinalAmount,
                TotalInvoice = x.TotalInvoice,

                VatPayable = x.VatPayable,
                VatCarriedForward = x.VatCarriedForward,

                Status = x.Status,
                Note = x.Note,
                CreatedAt = x.CreatedAt,
                ApprovedAt = x.ApprovedAt,

                Details = x.TaxDeclarationDetails
                    .Select(d => new TaxDeclarationInvoiceResponse
                    {
                        TaxDeclarationDetailId = d.TaxDeclarationDetailId,
                        TaxDeclarationId = d.TaxDeclarationId,

                        InvoiceId = d.InvoiceId ?? 0,
                        InvoiceCode = d.InvoiceCode,
                        CustomerName = d.CustomerName,
                        RevenueAmount = d.RevenueAmount,
                        TaxAmount = d.TaxAmount,
                        FinalAmount = d.FinalAmount,
                        InvoiceCreatedAt = d.InvoiceCreatedAt,

                        ImportId = d.ImportId ?? 0,
                        ImportCode = d.ImportCode,
                        PurchaseAmount = d.PurchaseAmount,
                        PurchaseTaxAmount = d.PurchaseTaxAmount,
                        PurchaseFinalAmount = d.PurchaseFinalAmount,
                        ImportCreatedAt = d.ImportCreatedAt
                    })
                    .ToList()
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

        var invoiceIds = declaration.TaxDeclarationDetails
            .Where(x => x.InvoiceId.HasValue)
            .Select(x => x.InvoiceId.Value)
            .ToList();

        var invoices = await _context.Invoices
            .Where(x => invoiceIds.Contains(x.InvoiceId))
            .ToListAsync();

        foreach (var invoice in invoices)
        {
            invoice.TaxDeclared = true;
            invoice.TaxDeclarationId = declaration.TaxDeclarationId;
        }

        var importIds = declaration.TaxDeclarationDetails
            .Where(x => x.ImportId.HasValue)
            .Select(x => x.ImportId.Value)
            .ToList();

        var imports = await _context.InventoryImports
            .Where(x => importIds.Contains(x.Id))
            .ToListAsync();

        foreach (var import in imports)
        {
            import.TaxDeclared = true;
            import.TaxDeclarationId = declaration.TaxDeclarationId;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelDeclarationAsync(long id)
    {
        var declaration = await _context.TaxDeclarations
            .FirstOrDefaultAsync(x => x.TaxDeclarationId == id);

        if (declaration == null)
            throw new Exception("Không tìm thấy tờ khai.");

        if (declaration.Status == "Approved")
            throw new Exception("Không thể hủy tờ khai đã duyệt.");

        declaration.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteDeclarationAsync(long id)
    {
        var declaration = await _context.TaxDeclarations
            .Include(x => x.TaxDeclarationDetails)
            .FirstOrDefaultAsync(x => x.TaxDeclarationId == id);

        if (declaration == null)
        {
            throw new Exception("Không tìm thấy tờ khai.");
        }

        if (declaration.Status != "Draft")
        {
            throw new Exception("Chỉ được xóa tờ khai ở trạng thái Draft.");
        }

        // Reset invoices
        var invoiceIds = declaration.TaxDeclarationDetails
            .Where(x => x.InvoiceId != null)
            .Select(x => x.InvoiceId!.Value)
            .ToList();

        if (invoiceIds.Any())
        {
            var invoices = await _context.Invoices
                .Where(x => invoiceIds.Contains(x.InvoiceId))
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                invoice.TaxDeclared = false;
                invoice.TaxDeclarationId = null;
            }
        }

        // Reset imports
        var importIds = declaration.TaxDeclarationDetails
            .Where(x => x.ImportId != null)
            .Select(x => x.ImportId!.Value)
            .ToList();

        if (importIds.Any())
        {
            var imports = await _context.InventoryImports
                .Where(x => importIds.Contains(x.Id))
                .ToListAsync();

            foreach (var import in imports)
            {
                import.TaxDeclared = false;
                import.TaxDeclarationId = null;
            }
        }

        // Xóa detail
        _context.TaxDeclarationDetails.RemoveRange(
            declaration.TaxDeclarationDetails
        );

        // Xóa declaration
        _context.TaxDeclarations.Remove(declaration);

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<decimal> GetPreviousDeductibleTaxAsync(
        string periodType,
        int? month,
        int? quarter,
        int year)
    {
        TaxDeclaration? previousDeclaration = null;

        if (periodType == "MONTH")
        {
            int prevMonth = month!.Value - 1;
            int prevYear = year;

            if (prevMonth == 0)
            {
                prevMonth = 12;
                prevYear--;
            }

            previousDeclaration = await _context.TaxDeclarations
                .Where(x => x.PeriodType == "MONTH")
                .Where(x => x.Month == prevMonth)
                .Where(x => x.Year == prevYear)
                .Where(x => x.Status == "Approved")
                .OrderByDescending(x => x.ApprovedAt)
                .FirstOrDefaultAsync();
        }

        if (periodType == "QUARTER")
        {
            int prevQuarter = quarter!.Value - 1;
            int prevYear = year;

            if (prevQuarter == 0)
            {
                prevQuarter = 4;
                prevYear--;
            }

            previousDeclaration = await _context.TaxDeclarations
                .Where(x => x.PeriodType == "QUARTER")
                .Where(x => x.Quarter == prevQuarter)
                .Where(x => x.Year == prevYear)
                .Where(x => x.Status == "Approved")
                .OrderByDescending(x => x.ApprovedAt)
                .FirstOrDefaultAsync();
        }

        return previousDeclaration?.VatCarriedForward ?? 0;
    }

    private void ValidateRequest(GenerateTaxDeclarationRequest request)
    {
        if (request.PeriodType != "MONTH" && request.PeriodType != "QUARTER")
            throw new Exception("PeriodType không hợp lệ. Chỉ nhận MONTH hoặc QUARTER.");

        if (request.PeriodType == "MONTH")
        {
            if (request.Month == null || request.Month < 1 || request.Month > 12)
                throw new Exception("Tháng kê khai không hợp lệ.");
        }

        if (request.PeriodType == "QUARTER")
        {
            if (request.Quarter == null || request.Quarter < 1 || request.Quarter > 4)
                throw new Exception("Quý kê khai không hợp lệ.");
        }

        if (request.Year < 2000)
            throw new Exception("Năm kê khai không hợp lệ.");
    }

    private string GenerateDeclarationCode(GenerateTaxDeclarationRequest request)
    {
        if (request.PeriodType == "MONTH")
        {
            return $"TAX-{request.Year}-M{request.Month}-{DateTime.Now:yyyyMMddHHmmss}";
        }

        return $"TAX-{request.Year}-Q{request.Quarter}-{DateTime.Now:yyyyMMddHHmmss}";
    }

    private TaxDeclarationDto MapToDto(TaxDeclaration declaration)
    {
        return new TaxDeclarationDto
        {
            TaxDeclarationId = declaration.TaxDeclarationId,
            DeclarationCode = declaration.DeclarationCode,
            PeriodType = declaration.PeriodType,
            Month = declaration.Month,
            Quarter = declaration.Quarter,
            Year = declaration.Year,

            PreviousDeductibleTax = declaration.PreviousDeductibleTax,

            PurchaseAmount = declaration.PurchaseAmount,
            PurchaseTaxAmount = declaration.PurchaseTaxAmount,
            DeductibleTaxAmount = declaration.DeductibleTaxAmount,

            TotalRevenue = declaration.TotalRevenue,
            TotalTaxAmount = declaration.TotalTaxAmount,
            TotalFinalAmount = declaration.TotalFinalAmount,
            TotalInvoice = declaration.TotalInvoice,

            VatPayable = declaration.VatPayable,
            VatCarriedForward = declaration.VatCarriedForward,

            Status = declaration.Status,
            Note = declaration.Note,
            CreatedAt = declaration.CreatedAt,
            ApprovedAt = declaration.ApprovedAt,

            Details = declaration.TaxDeclarationDetails
                .Select(d => new TaxDeclarationInvoiceResponse
                {
                    TaxDeclarationDetailId = d.TaxDeclarationDetailId,
                    TaxDeclarationId = d.TaxDeclarationId,

                    InvoiceId = d.InvoiceId ?? 0,
                    InvoiceCode = d.InvoiceCode,
                    CustomerName = d.CustomerName,
                    RevenueAmount = d.RevenueAmount,
                    TaxAmount = d.TaxAmount,
                    FinalAmount = d.FinalAmount,
                    InvoiceCreatedAt = d.InvoiceCreatedAt,

                    ImportId = d.ImportId ?? 0,
                    ImportCode = d.ImportCode,
                    PurchaseAmount = d.PurchaseAmount,
                    PurchaseTaxAmount = d.PurchaseTaxAmount,
                    PurchaseFinalAmount = d.PurchaseFinalAmount,
                    ImportCreatedAt = d.ImportCreatedAt
                })
                .ToList()
        };
    }
}