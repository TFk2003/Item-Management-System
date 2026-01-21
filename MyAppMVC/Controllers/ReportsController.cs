using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppMVC.Data;
using MyAppMVC.ViewModels;

namespace MyAppMVC.Controllers
{
    public class ReportsController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(DatabaseContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Dashboard()
        {
            var dashboardStats = new ReportDashboardViewModel
            {
                TotalItems = _context.Items.Count(),
                TotalClients = _context.Clients.Count(),
                TotalCategories = _context.Categories.Count(),
                TotalSuppliers = _context.Suppliers.Count(),
                LowStockItems = _context.Items.Count(i => i.StockQuantity < i.ReorderLevel),
                ActiveItems = _context.Items.Count(i => i.IsActive),
                InactiveItems = _context.Items.Count(i => !i.IsActive)
            };

            dashboardStats.RecentTransactions = _context.ItemClients
                .Include(ic => ic.Item)
                .Include(ic => ic.Client)
                .OrderByDescending(ic => ic.PurchasedDate)
                .Take(5)
                .Select(ic => new TransactionViewModel
                {
                    ItemName = ic.Item.Name,
                    ClientName = $"{ic.Client.FirstName} {ic.Client.LastName}",
                    Quantity = ic.Quantity,
                    TotalPrice = ic.TotalPrice,
                    Date = ic.PurchasedDate
                })
                .ToList();

            dashboardStats.TopCategories = _context.Categories
                .Include(c => c.Items)
                .Select(c => new CategoryReportViewModel
                {
                    CategoryName = c.Name,
                    ItemCount = c.Items.Count,
                    TotalValue = c.Items.Sum(i => i.Price * i.StockQuantity)
                })
                .OrderByDescending(c => c.ItemCount)
                .Take(5)
                .ToList();

            return View(dashboardStats);
        }

        public async Task<IActionResult> Sales(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddDays(-30);
            endDate ??= DateTime.Now;

            var salesData = await _context.ItemClients
                .Include(ic => ic.Item)
                .Include(ic => ic.Client)
                .Where(ic => ic.PurchasedDate >= startDate && ic.PurchasedDate <= endDate)
                .ToListAsync();

            var viewModel = new SalesReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                TotalSales = salesData.Sum(s => s.TotalPrice),
                TotalItemsSold = salesData.Sum(s => s.Quantity),
                AverageSaleValue = salesData.Any() ? salesData.Average(s => s.TotalPrice) : 0,
                SalesData = salesData
            };

            viewModel.TopSellingItems = salesData
                .GroupBy(s => s.Item.Name)
                .Select(g => new ItemSalesViewModel
                {
                    ItemName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(i => i.TotalRevenue)
                .Take(10)
                .ToList();

            viewModel.TopClients = salesData
                .GroupBy(s => s.Client)
                .Select(g => new ClientSalesViewModel
                {
                    ClientId = g.Key.Id,
                    ClientName = $"{g.Key.FirstName} {g.Key.LastName}",
                    TotalPurchases = g.Sum(x => x.TotalPrice),
                    PurchaseCount = g.Count()
                })
                .OrderByDescending(c => c.TotalPurchases)
                .Take(10)
                .ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> Inventory(string categoryFilter = null, string statusFilter = null)
        {
            var query = _context.Items.Include(i => i.Category).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(categoryFilter) && int.TryParse(categoryFilter, out int categoryId))
            {
                query = query.Where(i => i.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                bool isActive = statusFilter == "active";
                query = query.Where(i => i.IsActive == isActive);
            }

            var items = await query.ToListAsync();

            var viewModel = new InventoryReportViewModel
            {
                Items = items,
                TotalStockValue = items.Sum(i => i.Price * i.StockQuantity),
                TotalItems = items.Count,
                LowStockItems = items.Count(i => i.StockQuantity < i.ReorderLevel),
                OutOfStockItems = items.Count(i => i.StockQuantity == 0),
                Categories = await _context.Categories.ToListAsync()
            };

            // Stock by category
            viewModel.StockByCategory = items
                .GroupBy(i => i.Category?.Name ?? "Uncategorized")
                .Select(g => new CategoryStockViewModel
                {
                    CategoryName = g.Key,
                    TotalItems = g.Count(),
                    TotalValue = g.Sum(i => i.Price * i.StockQuantity),
                    AverageStock = g.Average(i => i.StockQuantity)
                })
                .OrderByDescending(c => c.TotalValue)
                .ToList();

            return View(viewModel);
        }
        public async Task<IActionResult> Clients()
        {
            var clients = await _context.Clients
                .Include(c => c.ItemClients)
                .ThenInclude(ic => ic.Item)
                .ToListAsync();

            var viewModel = new ClientReportViewModel
            {
                TotalClients = clients.Count,
                ActiveClients = clients.Count(c => c.IsActive),
                TotalRevenue = clients.Sum(c => c.ItemClients?.Sum(ic => ic.TotalPrice) ?? 0),
                AveragePurchaseValue = clients.Any(c => c.ItemClients?.Any() == true)
                    ? clients.Where(c => c.ItemClients?.Any() == true)
                    .Average(c => c.ItemClients?.Sum(ic => ic.TotalPrice) ?? 0)
                    : 0,
                Clients = clients
            };

            // Top clients by purchase value
            viewModel.TopClientsByValue = clients
                .Select(c => new ClientSummaryViewModel
                {
                    ClientId = c.Id,
                    ClientName = $"{c.FirstName} {c.LastName}",
                    Email = c.Email,
                    TotalPurchases = c.ItemClients?.Sum(ic => ic.TotalPrice) ?? 0,
                    PurchaseCount = c.ItemClients?.Count ?? 0,
                    LastPurchaseDate = c.ItemClients?.Any() == true 
                    ? c.ItemClients.Max(ic => ic.PurchasedDate)
                    : (DateTime?)null
                })
                .OrderByDescending(c => c.TotalPurchases)
                .Take(10)
                .ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> Financial(int? year = null)
        {
            year ??= DateTime.Now.Year;

            var salesData = await _context.ItemClients
                .Where(ic => ic.PurchasedDate.Year == year)
                .ToListAsync();

            var monthlyData = new List<MonthlyFinancialViewModel>();

            for (int month = 1; month <= 12; month++)
            {
                var monthSales = salesData.Where(s => s.PurchasedDate.Month == month).ToList();

                monthlyData.Add(new MonthlyFinancialViewModel
                {
                    Month = new DateTime(year.Value, month, 1).ToString("MMM"),
                    Revenue = monthSales.Sum(s => s.TotalPrice),
                    ItemsSold = monthSales.Sum(s => s.Quantity),
                    TransactionCount = monthSales.Count
                });
            }

            var viewModel = new FinancialReportViewModel
            {
                Year = year.Value,
                TotalRevenue = monthlyData.Sum(m => m.Revenue),
                TotalItemsSold = monthlyData.Sum(m => m.ItemsSold),
                MonthlyData = monthlyData,
                YearlyComparison = await GetYearlyComparison(year.Value)
            };

            return View(viewModel);
        }

        public IActionResult Export(string reportType = null, string startDate = null,
                           string endDate = null, int? year = null, string format = "pdf")
        {
            ViewBag.ReportType = reportType;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Year = year;
            ViewBag.Format = format;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExportReport(ExportReportViewModel model)
        {
            // In a real application, this would generate and return a file
            // For now, we'll just show a success message

            try
            {
                // Based on the report type, generate the appropriate data
                var reportData = await GenerateReportData(model);

                // Based on format, generate file
                var fileBytes = model.Format.ToLower() switch
                {
                    "pdf" => GeneratePdfReport(reportData, model),
                    "excel" => GenerateExcelReport(reportData, model),
                    "csv" => GenerateCsvReport(reportData, model),
                    _ => GeneratePdfReport(reportData, model)
                };

                var contentType = model.Format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    _ => "application/pdf"
                };

                var fileName = $"{model.ReportType}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{model.Format}";

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                TempData["ErrorMessage"] = $"Error exporting report: {ex.Message}";
                return RedirectToAction("Export", new
                {
                    reportType = model.ReportType,
                    startDate = model.StartDate?.ToString("yyyy-MM-dd"),
                    endDate = model.EndDate?.ToString("yyyy-MM-dd")
                });
            }
        }

        private async Task<List<YearlyComparisonViewModel>> GetYearlyComparison(int currentYear)
        {
            var comparison = new List<YearlyComparisonViewModel>();

            for (int year = currentYear - 2; year <= currentYear; year++)
            {
                var yearSales = await _context.ItemClients
                    .Where(ic => ic.PurchasedDate.Year == year)
                    .ToListAsync();

                comparison.Add(new YearlyComparisonViewModel
                {
                    Year = year,
                    Revenue = yearSales.Sum(s => s.TotalPrice),
                    Growth = year == currentYear - 2 ? 0 : 0 // Calculate growth percentage
                });
            }

            return comparison;
        }

        private async Task<object> GenerateReportData(ExportReportViewModel model)
        {
            // This would generate data based on report type
            // For now, return a simple object
            return new
            {
                ReportType = model.ReportType,
                GeneratedDate = DateTime.Now,
                Parameters = model
            };
        }

        private byte[] GeneratePdfReport(object data, ExportReportViewModel model)
        {
            // In a real application, use a PDF library like iTextSharp or QuestPDF
            // For now, return a simple PDF placeholder
            string pdfContent = $@"
        <h1>{model.ReportType} Report</h1>
        <p>Generated: {DateTime.Now}</p>
        <p>Format: {model.Format}</p>
        <p>This is a placeholder PDF. In a real application, 
           you would use a PDF generation library.</p>
    ";

            // Convert HTML to PDF (requires a library in real implementation)
            // For demo purposes, return a simple text file
            return System.Text.Encoding.UTF8.GetBytes(pdfContent);
        }

        private byte[] GenerateExcelReport(object data, ExportReportViewModel model)
        {
            // In a real application, use EPPlus or ClosedXML
            // For now, return a simple CSV
            string csvContent = $"Report Type,Generated Date,Format\n";
            csvContent += $"{model.ReportType},{DateTime.Now},{model.Format}";

            return System.Text.Encoding.UTF8.GetBytes(csvContent);
        }

        private byte[] GenerateCsvReport(object data, ExportReportViewModel model)
        {
            string csvContent = $"Report Type,Generated Date,Format\n";
            csvContent += $"{model.ReportType},{DateTime.Now},{model.Format}";

            return System.Text.Encoding.UTF8.GetBytes(csvContent);
        }
    }
}
