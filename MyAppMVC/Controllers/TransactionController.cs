using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppMVC.Data;

namespace MyAppMVC.Controllers
{
    public class TransactionController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(DatabaseContext databaseContext, ILogger<TransactionController> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? clientFilter = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _databaseContext.ItemClients
                .Include(ic => ic.Item)
                .Include(ic => ic.Client)
                .AsQueryable();

            if (!string.IsNullOrEmpty(clientFilter) && int.TryParse(clientFilter, out var clientId))
            {
                query = query.Where(x => x.ClientId == clientId);
            }
            if (startDate.HasValue)
            {
                query = query.Where(ic => ic.PurchasedDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(ic => ic.PurchasedDate <= endDate.Value);
            }

            var transaction = await query.
                OrderByDescending(ic => ic.PurchasedDate)
                .ToListAsync();

            ViewBag.TotalRevenue = transaction.Sum(ic => ic.TotalPrice);
            ViewBag.TotalItemSold = transaction.Sum(ic => ic.Quantity);
            ViewBag.TotalTransactions = transaction.Count;
            
            return View(transaction);
        }
    }
}
