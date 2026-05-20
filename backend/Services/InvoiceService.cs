
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend.Services;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public InvoiceService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<Invoice> CreateInvoiceAsync(long orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.User)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new Exception("Không tìm thấy đơn hàng.");

        var existedInvoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.OrderId == orderId);

        if (existedInvoice != null)
            return existedInvoice;

        var taxAmount = 0;
        var finalAmount = order.TotalAmount;

        var invoice = new Invoice
        {
            InvoiceCode = $"INV{DateTime.Now:yyyyMMddHHmmss}",
            OrderId = order.Id,
            CustomerName = order.User.Profile.FullName,
            CustomerEmail = order.User.Email,
            TotalAmount = order.TotalAmount,
            TaxAmount = taxAmount,
            FinalAmount = finalAmount,
            Status = "Created",
            CreatedAt = DateTime.Now,
            InvoiceItems = order.OrderItems.Select(x => new InvoiceItem
            {
                ProductName = x.Product.Name,
                Quantity = x.Quantity,
                UnitPrice = x.Price ?? 0,
                TotalPrice = (x.Price ?? 0) * x.Quantity
            }).ToList()
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        var pdfUrl = GenerateInvoicePdf(invoice, order);
        invoice.PdfUrl = pdfUrl;

        await _context.SaveChangesAsync();

        return invoice;
    }

    private string GenerateInvoicePdf(Invoice invoice, Order order)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var folderPath = Path.Combine(_environment.WebRootPath, "invoices");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileName = $"{invoice.InvoiceCode}.pdf";
        var filePath = Path.Combine(folderPath, fileName);

        Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(15);
        page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Blue.Darken3));

        page.Content()
            .Border(2)
            .BorderColor(Colors.Blue.Medium)
            .Padding(15)
            .Column(column =>
            {
                column.Item().AlignCenter().Text("HÓA ĐƠN BÁN HÀNG")
                    .Bold().FontSize(22);

                column.Item().AlignCenter().Text("SALES INVOICE")
                    .Italic().Bold().FontSize(16);

                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Text(
                        $"Ngày (day) {invoice.CreatedAt:dd} tháng (month) {invoice.CreatedAt:MM} năm (year) {invoice.CreatedAt:yyyy}"
                    );

                    row.RelativeItem().AlignRight().Text(
                        $"Số (invoice no.): {invoice.InvoiceCode}"
                    ).Bold();
                });

                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Blue.Medium);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                    });

                    AddInfoRow(table, "Đơn vị bán (Seller):", "Công ty cổ phần Tech AI Việt Nam");
                    AddInfoRow(table, "Chi nhánh (Brand):", "Tech AI Ngô Quyền Đà Nẵng");
                    AddInfoRow(table, "Mã số thuế (Tax code):", "011223344");
                    AddInfoRow(table, "Địa chỉ (Address):", "1123 Ngô Quyền, phường An Hải, thành phố Đà Nẵng");
                });

                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Blue.Medium);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(130);
                        columns.RelativeColumn();
                    });

                    AddInfoRow(table, "Người mua (Buyer):", invoice.CustomerName);
                    AddInfoRow(table, "Email:", invoice.CustomerEmail);
                    AddInfoRow(table, "Số điện thoại (Phone):", order.User.Profile.Phone);
                    AddInfoRow(table, "Địa chỉ (Address):",
                        $"{order.Address.Street}, {order.Address.Ward}, {order.Address.District}, {order.Address.Province}");
                });

                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(45);
                        columns.RelativeColumn(4);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(90);
                        columns.ConstantColumn(100);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("STT\n(No.)").AlignCenter();
                        header.Cell().Element(HeaderCell).Text("Tên hàng hóa, dịch vụ\n(Description)").AlignCenter();
                        header.Cell().Element(HeaderCell).Text("ĐVT\n(Unit)").AlignCenter();
                        header.Cell().Element(HeaderCell).Text("Số lượng\n(Quantity)").AlignCenter();
                        header.Cell().Element(HeaderCell).Text("Đơn giá\n(Unit price)").AlignCenter();
                        header.Cell().Element(HeaderCell).Text("Thành tiền\n(Amount)").AlignCenter();

                        header.Cell().Element(BodyCell).Text("1").AlignCenter();
                        header.Cell().Element(BodyCell).Text("2").AlignCenter();
                        header.Cell().Element(BodyCell).Text("3").AlignCenter();
                        header.Cell().Element(BodyCell).Text("4").AlignCenter();
                        header.Cell().Element(BodyCell).Text("5").AlignCenter();
                        header.Cell().Element(BodyCell).Text("6 = 4 x 5").AlignCenter();
                    });

                    int index = 1;
                    foreach (var item in invoice.InvoiceItems)
                    {
                        table.Cell().Element(BodyCell).Text(index.ToString()).AlignCenter();
                        table.Cell().Element(BodyCell).Text(item.ProductName);
                        table.Cell().Element(BodyCell).Text("Cái").AlignCenter();
                        table.Cell().Element(BodyCell).Text(item.Quantity.ToString()).AlignCenter();
                        table.Cell().Element(BodyCell).Text($"{item.UnitPrice:N0}").AlignRight();
                        table.Cell().Element(BodyCell).Text($"{item.TotalPrice:N0}").AlignRight();
                        index++;
                    }

                    int minRows = 7;
                    int emptyRows = Math.Max(0, minRows - invoice.InvoiceItems.Count);

                    for (int i = 0; i < emptyRows; i++)
                    {
                        table.Cell().Element(BodyCell).Text((index + i).ToString()).AlignCenter();
                        table.Cell().Element(BodyCell).Text("");
                        table.Cell().Element(BodyCell).Text("");
                        table.Cell().Element(BodyCell).Text("");
                        table.Cell().Element(BodyCell).Text("");
                        table.Cell().Element(BodyCell).Text("");
                    }

                    table.Cell().ColumnSpan(5).Element(BodyCell)
                        .AlignRight()
                        .Text("Tổng cộng tiền thanh toán (Total payment)")
                        .Bold();

                    table.Cell().Element(BodyCell)
                        .AlignRight()
                        .Text($"{invoice.TotalAmount:N0}")
                        .Bold();
                });

                column.Item().PaddingTop(25).Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(left =>
                    {
                        left.Item().Text("Người mua hàng (Buyer)").Bold().FontSize(12);
                        left.Item().PaddingTop(8).Text("(Ký và ghi rõ họ tên)").Italic();
                    });

                    row.RelativeItem().AlignCenter().Column(right =>
                    {
                        right.Item().Text("Người bán hàng (Seller)").Bold().FontSize(12);

                        right.Item().PaddingTop(15)
                            .Background(Colors.Green.Lighten5)
                            .Padding(10)
                            .Column(sign =>
                            {
                                sign.Item().AlignCenter().Text("Đã được ký điện tử bởi").Bold();
                                sign.Item().AlignCenter().Text("(Signed digitally by)").Italic();
                                sign.Item().PaddingTop(8).AlignCenter()
                                    .Text("CÔNG TY CỔ PHẦN TECH AI VIỆT NAM")
                                    .Bold();
                                sign.Item().AlignCenter()
                                    .Text($"Ngày: {invoice.CreatedAt:dd/MM/yyyy}");
                            });
                    });
                });

                column.Item().PaddingTop(30).AlignCenter().Text(
                    "(Cần kiểm tra đối chiếu khi lập, giao, nhận hóa đơn)"
                ).Italic();
            });
    });
}).GeneratePdf(filePath);

        return $"/invoices/{fileName}";
    }
    private string NumberToWords(long number)
    {
        if (number == 0)
            return "Không đồng";

        return $"{number:N0} đồng";
    }
    private static void AddInfoRow(TableDescriptor table, string label, string value)
    {
        table.Cell().PaddingVertical(3).Text(label).Italic();
        table.Cell().PaddingVertical(3).Text(value).Bold();
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Blue.Medium)
            .Padding(4)
            .AlignMiddle();
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .Border(1)
            .BorderColor(Colors.Blue.Medium)
            .Padding(4)
            .MinHeight(24)
            .AlignMiddle();
    }
}