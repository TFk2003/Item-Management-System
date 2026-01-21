using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAppMVC.Data;
using MyAppMVC.Models;

namespace MyAppMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger, DatabaseContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalItems= await _context.Items.CountAsync();
            ViewBag.TotalClients = await _context.Clients.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            
            var lowStockItems = await _context.Items
                .Where(i => i.StockQuantity < i.ReorderLevel)
                .Include(c => c.Category)
                .ToListAsync();
            ViewBag.LowStockItems = lowStockItems.Count;
            ViewBag.LowStockItemList = lowStockItems.Take(5).ToList();
            ViewBag.RecentItems = await _context.Items
                .Include(i => i.Category)
                .OrderByDescending(i => i.CreatedDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentClients = await _context.Clients
                .OrderByDescending(c => c.CreatedDate)
                .Take(5)
                .ToListAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
