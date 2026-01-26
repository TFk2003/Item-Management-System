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
                .Include(s => s.Supplier)
                .ToListAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(databaseContext.Categories, "Id", "Name");
            ViewBag.Suppliers = new SelectList(databaseContext.Suppliers, "Id", "CompanyName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,Description,Price,StockQuantity,ReorderLevel,SKU,Manufacturer,CategoryId,SupplierId")] Item item
            , string serialCode, string serialName,
            string warrantyInfo, DateTime? manufactureDate, DateTime? expiryDate)
        {
            if (ModelState.IsValid)
            {
                try 
                { 
                    // Ensure SerialNumber is associated with the Item
                    if (!string.IsNullOrEmpty(serialCode))
                    {
                        var serialNumber = new SerialNumber
                        {
                            SerialCode = serialCode,
                            Name = serialName,
                            WarrantyInfo = warrantyInfo,
                            ManufactureDate = manufactureDate,
                            ExpiryDate = expiryDate
                        };

                        // Add serial number to context first
                        databaseContext.SerialNumbers.Add(serialNumber);
                        await databaseContext.SaveChangesAsync(); // Save to get ID

                        // Assign the serial number to the item
                        item.SerailId = serialNumber.Id;
                    }

                    databaseContext.Add(item);
                    await databaseContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Item '{item.Name}' created successfully!";
                    return RedirectToAction("Index");

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating item: {ex.Message}");
                }
            }
            ViewBag.Categories = new SelectList(databaseContext.Categories, "Id", "Name");
            ViewBag.Suppliers = new SelectList(databaseContext.Suppliers, "Id", "CompanyName");

            return View(item);
}

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Categories = new SelectList(databaseContext.Categories, "Id", "Name");
            ViewBag.Suppliers = new SelectList(databaseContext.Suppliers, "Id", "CompanyName");

            var item = await databaseContext.Items
                .Include(s => s.SerialNumber)
                .Include(c => c.Category)
                .Include(s => s.Supplier)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,StockQuantity,ReorderLevel,SKU,Manufacturer,CategoryId,SupplierId,IsActive")] Item item,
            [Bind("Id,SerialCode,Name,ManufactureDate,ExpiryDate,WarrantyInfo")] SerialNumber serialNumber)
        {
            if (id != item.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve CreatedDate and add LastUpdated
                    var existingItem = await databaseContext.Items.FindAsync(id);
                    if (existingItem != null)
                    {
                        item.CreatedDate = existingItem.CreatedDate;
                    }
                    item.LastUpdated = DateTime.UtcNow;
                    // Update SerialNumber
                    serialNumber.ItemId = item.Id;
                    databaseContext.Update(serialNumber);

                    databaseContext.Update(item);
                    await databaseContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewBag.Categories = new SelectList(databaseContext.Categories, "Id", "Name");
            ViewBag.Suppliers = new SelectList(databaseContext.Suppliers, "Id", "CompanyName");

            return View(item);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await databaseContext.Items
                .Include(s => s.SerialNumber)
                .Include(c => c.Category)
                .Include(s => s.Supplier)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await databaseContext.Items
                .Include(s => s.SerialNumber)
                .Include(c => c.Category)
                .Include(s => s.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

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

        private bool ItemExists(int id)
        {
            return databaseContext.Items.Any(e => e.Id == id);
        }

    }
}
