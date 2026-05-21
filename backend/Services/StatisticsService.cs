using backend.DTOs.Statistics;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IStatisticsRepository _statisticsRepository;
    private readonly AppDbContext _context;

    public StatisticsService(IStatisticsRepository statisticsRepository, AppDbContext context)
    {
        _statisticsRepository = statisticsRepository;
        _context = context;
    }

    public async Task<StatisticsResponseDto<BusinessStatisticsDto>> GetBusinessStatisticsAsync(
        string type,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var normalizedType = type.ToLower();

        if (!fromDate.HasValue || !toDate.HasValue)
        {
            switch (normalizedType)
            {
                case "daily":
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date;
                    break;

                case "monthly":
                    fromDate = DateTime.Now.AddMonths(-11).Date;
                    toDate = DateTime.Now.Date;
                    break;

                case "quarterly":
                    fromDate = DateTime.Now.AddYears(-1).Date;
                    toDate = DateTime.Now.Date;
                    break;

                case "yearly":
                    fromDate = DateTime.Now.AddYears(-4).Date;
                    toDate = DateTime.Now.Date;
                    break;

                default:
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date;
                    break;
            }
        }

        var data = normalizedType switch
        {
            "daily" => await _statisticsRepository.GetStatisticsByDayAsync(fromDate.Value, toDate.Value),
            "monthly" => await _statisticsRepository.GetStatisticsByMonthAsync(fromDate.Value, toDate.Value),
            "quarterly" => await _statisticsRepository.GetStatisticsByQuarterAsync(fromDate.Value, toDate.Value),
            "yearly" => await _statisticsRepository.GetStatisticsByYearAsync(fromDate.Value, toDate.Value),
            _ => await _statisticsRepository.GetStatisticsByDayAsync(fromDate.Value, toDate.Value)
        };

        return new StatisticsResponseDto<BusinessStatisticsDto>
        {
            Data = data,
            TotalRevenue = data.Sum(x => x.Revenue),
            TotalCost = data.Sum(x => x.Cost),
            TotalProfit = data.Sum(x => x.Profit),

            AverageRevenue = data.Count > 0 ? data.Average(x => x.Revenue) : 0,
            AverageProfit = data.Count > 0 ? data.Average(x => x.Profit) : 0,

            Count = data.Count
        };
    }

    public async Task<PaginatedStatisticsDto<ProductAnalyticsDto>> GetProductAnalyticsAsync(
        int pageNumber = 1, int pageSize = 50)
    {
        var products = await _statisticsRepository.GetProductAnalyticsAsync(pageNumber, pageSize);

        var totalCount = products.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedStatisticsDto<ProductAnalyticsDto>
        {
            Items = products,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = pageNumber < totalPages,
            HasPreviousPage = pageNumber > 1
        };
    }

    public async Task<ProductAnalyticsDto> GetProductAnalyticsByIdAsync(long productId)
    {
        return await _statisticsRepository.GetProductAnalyticsByIdAsync(productId);
    }

    public async Task<List<TopProductDto>> GetTopSellingProductsAsync(int topCount = 10)
    {
        return await _statisticsRepository.GetTopSellingProductsAsync(topCount);
    }

    public async Task<List<TopProductDto>> GetTopProfitProductsAsync(int topCount = 10)
    {
        return await _statisticsRepository.GetTopProfitProductsAsync(topCount);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        return await _statisticsRepository.GetDashboardSummaryAsync();
    }

    public async Task<List<CategoricalRevenueDto>> GetRevenueByCategoryAsync(
    string type,
    DateTime? fromDate = null,
    DateTime? toDate = null)
    {
        var normalizedType = string.IsNullOrWhiteSpace(type)
            ? "daily"
            : type.ToLower();

        if (!fromDate.HasValue && !toDate.HasValue)
        {
            switch (normalizedType)
            {
                case "daily":
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date;
                    break;

                case "monthly":
                    fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-11);
                    toDate = DateTime.Now.Date;
                    break;

                case "quarterly":
                    fromDate = DateTime.Now.AddYears(-1).Date;
                    toDate = DateTime.Now.Date;
                    break;

                case "yearly":
                    fromDate = new DateTime(DateTime.Now.Year - 4, 1, 1);
                    toDate = DateTime.Now.Date;
                    break;

                default:
                    fromDate = DateTime.Now.Date;
                    toDate = DateTime.Now.Date;
                    break;
            }
        }

        fromDate ??= DateTime.Now.Date;
        toDate ??= DateTime.Now.Date;

        return await _statisticsRepository.GetRevenueByCategoryAsync(
            normalizedType,
            fromDate.Value,
            toDate.Value
        );
    }

    public async Task<ComparisonStatisticsDto> CompareCurrentVsPreviousMonthAsync()
    {
        return await _statisticsRepository.CompareCurrentVsPreviousMonthAsync();
    }

    public async Task<List<ComparisonStatisticsDto>> CompareCurrentVsPreviousYearAsync()
    {
        return await _statisticsRepository.CompareCurrentVsPreviousYearAsync();
    }

    public async Task<List<KpiCardDto>> GetKpiCardsAsync()
    {
        var dashboard = await GetDashboardSummaryAsync();
        var comparisonMonth = await CompareCurrentVsPreviousMonthAsync();

        var kpiCards = new List<KpiCardDto>
        {
            new KpiCardDto
            {
                Title = "Today Revenue",
                Value = dashboard.TodayRevenue,
                Unit = "VND",
                PreviousPeriodValue = dashboard.TodayRevenuePreviousDay,
                ChangePercentage = dashboard.TodayRevenuePreviousDay > 0
                    ? ((dashboard.TodayRevenue - dashboard.TodayRevenuePreviousDay) / dashboard.TodayRevenuePreviousDay) * 100
                    : 0,
                Trend = dashboard.TodayRevenue >= dashboard.TodayRevenuePreviousDay ? "UP" : "DOWN",
                Color = "success"
            },
            new KpiCardDto
            {
                Title = "Today Profit",
                Value = dashboard.TodayProfit,
                Unit = "VND",
                PreviousPeriodValue = dashboard.TodayProfitPreviousDay,
                ChangePercentage = dashboard.TodayProfitPreviousDay > 0
                    ? ((dashboard.TodayProfit - dashboard.TodayProfitPreviousDay) / dashboard.TodayProfitPreviousDay) * 100
                    : 0,
                Trend = dashboard.TodayProfit >= dashboard.TodayProfitPreviousDay ? "UP" : "DOWN",
                Color = "info"
            },
            new KpiCardDto
            {
                Title = "Monthly Revenue Growth",
                Value = dashboard.RevenueGrowthPercentage,
                Unit = "%",
                PreviousPeriodValue = 0,
                ChangePercentage = dashboard.RevenueGrowthPercentage,
                Trend = dashboard.RevenueGrowthPercentage >= 0 ? "UP" : "DOWN",
                Color = dashboard.RevenueGrowthPercentage >= 0 ? "success" : "danger"
            },
            new KpiCardDto
            {
                Title = "Monthly Profit Growth",
                Value = dashboard.ProfitGrowthPercentage,
                Unit = "%",
                PreviousPeriodValue = 0,
                ChangePercentage = dashboard.ProfitGrowthPercentage,
                Trend = dashboard.ProfitGrowthPercentage >= 0 ? "UP" : "DOWN",
                Color = dashboard.ProfitGrowthPercentage >= 0 ? "success" : "danger"
            },
            new KpiCardDto
            {
                Title = "Completed Orders",
                Value = dashboard.CompletedOrders,
                Unit = "orders",
                PreviousPeriodValue = 0,
                ChangePercentage = 0,
                Trend = "STABLE",
                Color = "primary"
            },
            new KpiCardDto
            {
                Title = "Low Stock Products",
                Value = dashboard.TotalLowStockProducts,
                Unit = "products",
                PreviousPeriodValue = 0,
                ChangePercentage = 0,
                Trend = dashboard.TotalLowStockProducts > 0 ? "DOWN" : "UP",
                Color = dashboard.TotalLowStockProducts > 0 ? "warning" : "success"
            }
        };

        return kpiCards;
    }

    public async Task<List<BusinessAlertDto>> GetBusinessAlertsAsync()
    {
        var alerts = new List<BusinessAlertDto>();
        var id = 1;

        var now = DateTime.Now;

        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = currentMonthStart;

        var currentRevenue = await _context.Orders
            .Where(o =>
                o.Status == "Đã giao" &&
                o.CompletedAt >= currentMonthStart &&
                o.CompletedAt <= now)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        var previousRevenue = await _context.Orders
            .Where(o =>
                o.Status == "Đã giao" &&
                o.CompletedAt >= previousMonthStart &&
                o.CompletedAt < previousMonthEnd)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        if (previousRevenue > 0)
        {
            var change = ((currentRevenue - previousRevenue) / previousRevenue) * 100;

            if (change < -10)
            {
                alerts.Add(new BusinessAlertDto
                {
                    Id = id++,
                    Type = "danger",
                    Title = "Doanh thu giảm mạnh",
                    Message = $"Doanh thu tháng này giảm {Math.Abs(change):0.0}% so với tháng trước.",
                    Time = "Vừa cập nhật"
                });
            }
            else if (change > 10)
            {
                alerts.Add(new BusinessAlertDto
                {
                    Id = id++,
                    Type = "success",
                    Title = "Doanh thu tăng trưởng tốt",
                    Message = $"Doanh thu tháng này tăng {change:0.0}% so với tháng trước.",
                    Time = "Vừa cập nhật"
                });
            }
        }

        var outStockProducts = await _context.Products
            .CountAsync(p =>
            p.Inventory == null ||
            p.Inventory.Quantity <= 0);

        if (outStockProducts > 0)
        {
            alerts.Add(new BusinessAlertDto
            {
                Id = id++,
                Type = "danger",
                Title = "Sản phẩm hết hàng",
                Message = $"Có {outStockProducts} sản phẩm đã hết hàng. Hãy nhập thêm hàng ngay.",
                Time = "Hôm nay"
            });
        }

        var lowStockProducts = await _context.Products
            .Where(p => p.Inventory.Quantity <= 5)
            .CountAsync();

        if (lowStockProducts > 0)
        {
            alerts.Add(new BusinessAlertDto
            {
                Id = id++,
                Type = "warning",
                Title = "Sản phẩm sắp hết hàng",
                Message = $"Có {lowStockProducts} sản phẩm có tồn kho thấp, cần nhập thêm hàng.",
                Time = "Hôm nay"
            });
        }

        var pendingOrders = await _context.Orders
            .Where(o =>
                o.Status == "Chờ xác nhận" &&
                o.CreateAt <= now.AddHours(-12))
            .CountAsync();

        if (pendingOrders > 0)
        {
            alerts.Add(new BusinessAlertDto
            {
                Id = id++,
                Type = "info",
                Title = "Đơn hàng chờ xử lý",
                Message = $"Có {pendingOrders} đơn hàng chờ xác nhận quá 12 giờ.",
                Time = "Hôm nay"
            });
        }
        var refundOrders = await _context.Orders
            .Where(o =>
                o.Status == "Chờ hoàn tiền")
            .CountAsync();

        if (refundOrders > 0)
        {
            alerts.Add(new BusinessAlertDto
            {
                Id = id++,
                Type = "info",
                Title = "Đơn hàng chờ hoàn tiền",
                Message = $"Có {refundOrders} đơn hàng chờ hoàn tiền. Hãy kiểm tra và xử lý ngay",
                Time = "Hôm nay"
            });
        }

        return alerts;
    }

    public async Task<byte[]> ExportStatisticsToExcelAsync(ExportStatisticsRequestDto request)
    {
        // This requires a library like EPPlus or ClosedXML
        // For now, returning empty bytes - implement actual Excel export
        throw new NotImplementedException("Excel export requires EPPlus or ClosedXML NuGet package");
    }

    public async Task<byte[]> ExportStatisticsToPdfAsync(ExportStatisticsRequestDto request)
    {
        // This requires a library like iText or SelectPdf
        // For now, returning empty bytes - implement actual PDF export
        throw new NotImplementedException("PDF export requires iTextSharp or SelectPdf NuGet package");
    }

    #region Helper Methods

    private decimal CalculateGrowthPercentage<T>(
    List<T> data,
    Func<T, decimal> selector)
    {
        if (data.Count < 2)
            return 0;

        var firstValue = selector(data.First());
        var lastValue = selector(data.Last());

        if (firstValue == 0)
            return lastValue > 0 ? 100 : 0;

        return ((lastValue - firstValue) / firstValue) * 100;
    }

    #endregion
}
