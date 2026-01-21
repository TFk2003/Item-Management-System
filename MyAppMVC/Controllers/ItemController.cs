using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAppMVC.Data;
using MyAppMVC.Models;
using MyAppMVC.ViewModels;

namespace MyAppMVC.Controllers
{
    public class ItemController : Controller
    {
        private readonly DatabaseContext databaseContext;

        public ItemController(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task<IActionResult> Index()
        {
            var items = await databaseContext.Items
                .Include(s => s.SerialNumber)
                .Include(c => c.Category)
                .ToListAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(databaseContext.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name, Price, CategoryId")] Item item)
        {
            if (ModelState.IsValid)
            {
                databaseContext.Add(item);
                await databaseContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Categories = new SelectList(databaseContext.Categories, "Id", "Name");
            var items = await databaseContext.Items.FirstOrDefaultAsync(x => x.Id == id);
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Name, Price, CategoryId")] Item item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                databaseContext.Update(item);
                await databaseContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(item);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await databaseContext.Items.FirstOrDefaultAsync(x => x.Id == id);
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await databaseContext.Items.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return RedirectToAction("Index");
            }
            databaseContext.Items.Remove(item);
            await databaseContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public IActionResult Overview()
        {
            var item = new Item() { Name = "Keyboard" };
            return View(item);
        }

        public async Task<IActionResult> LowStock(int? threshold = null)
        {
            var query = databaseContext.Items
                .Include(i => i.Category)
                .Include(i => i.SerialNumber)
                .Where(i => i.StockQuantity < i.ReorderLevel);

            if (threshold.HasValue)
            {
                query = query.Where(i => i.StockQuantity < threshold.Value);
            }

            var lowStockItems = await query
                .OrderBy(i => i.StockQuantity)
                .ToListAsync();

            ViewBag.Threshold = threshold;
            ViewBag.TotalLowStockItems = lowStockItems.Count;

            return View(lowStockItems);
        }

        public async Task<IActionResult> Reorder(int id)
        {
            var item = await databaseContext.Items
                .Include(i => i.Category)
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            var viewModel = new ReorderViewModel
            {
                ItemId = item.Id,
                ItemName = item.Name,
                CurrentStock = item.StockQuantity,
                ReorderLevel = item.ReorderLevel,
                RecommendedReorder = Math.Max(item.ReorderLevel * 2 - item.StockQuantity, 10),
                SupplierId = item.SupplierId,
                Suppliers = await databaseContext.Suppliers.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Reorder(ReorderViewModel reorderViewModel)
        {
            if (ModelState.IsValid)
            {
                var item = await databaseContext.Items.FindAsync(reorderViewModel.ItemId);
                if (item != null)
                {
                    item.StockQuantity += reorderViewModel.QuantityToOrder;
                    item.LastUpdated = DateTime.UtcNow;

                    await databaseContext.SaveChangesAsync();

                    TempData["Success Message"] = $"{reorderViewModel.QuantityToOrder} units of {item.Name} have been ordered and stock updated.";

                    return RedirectToAction(nameof(LowStock));
                }
            }

            reorderViewModel.Suppliers = await databaseContext.Suppliers.ToListAsync();
            return View(reorderViewModel);
        }

        public async Task<IActionResult> QuickReorder(int id, int quantity = 10)
        {
            var item = await databaseContext.Items.FindAsync(id);
            if( item == null)
            {
                return NotFound();
            }
            item.StockQuantity += quantity;
            item.LastUpdated = DateTime.UtcNow;
            await databaseContext.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{quantity} units of item {item.Name} have been added to stock";
            return RedirectToAction(nameof(LowStock));
        }
    }
}
