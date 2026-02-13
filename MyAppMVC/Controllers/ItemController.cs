using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAppMVC.Data;
using MyAppMVC.IServices;
using MyAppMVC.Models;
using MyAppMVC.ViewModels;

namespace MyAppMVC.Controllers
{
    public class ItemController(DatabaseContext databaseContext, ItemIService itemService, SupplierIService supplierService) : Controller
    {

        public async Task<IActionResult> Index()
        {
            var items = await itemService.getAllItem();
            return View(items);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(databaseContext.Categories, "Id", "Name");
            ViewBag.Suppliers = new SelectList(databaseContext.Suppliers, "Id", "CompanyName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,Description,Price,StockQuantity,ReorderLevel,SKU,Manufacturer,CategoryId,SupplierId")] Item item
            , string serialCode, string serialName,
            string warrantyInfo, DateTime? manufactureDate, DateTime? expiryDate)
        {
            if (ModelState.IsValid)
            {
                try 
                {
                    await itemService.addItem(item, serialCode, serialName, warrantyInfo, manufactureDate, expiryDate);

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

            var item = await itemService.getItemByIdWithSCS(id);
            
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,StockQuantity,ReorderLevel,SKU,Manufacturer,CategoryId,SupplierId,IsActive")] Item item,
            string serialCode, string serialName, string warrantyInfo, DateTime? manufactureDate, DateTime? expiryDate)
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
                    await itemService.updateItem(id, item, serialCode, serialName, warrantyInfo, manufactureDate,
                        expiryDate);

                    TempData["SuccessMessage"] = $"Item '{item.Name}' updated successfully!";
                    return RedirectToAction("Index");
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
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating item: {ex.Message}");
                }
            }
            
            ViewBag.Categories = new SelectList(databaseContext.Categories, "Id", "Name");
            ViewBag.Suppliers = new SelectList(databaseContext.Suppliers, "Id", "CompanyName");

            return View(item);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await itemService.getItemByIdWithSCSI(id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await itemService.deleteItem(id);

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(int id)
        {
            var item = await itemService.getItemByIdWithSCSIC(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        public async Task<IActionResult> LowStock(int? threshold = null)
        {
            var lowStockItems = await itemService.lowStockItems(threshold);

            ViewBag.Threshold = threshold;
            ViewBag.TotalLowStockItems = lowStockItems.Count;

            return View(lowStockItems);
        }

        public async Task<IActionResult> Reorder(int id)
        {
            var item = await itemService.getItemByIdWithCategoryAndSupplier(id);

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
                Suppliers = await supplierService.getAllSuppliers()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Reorder(ReorderViewModel reorderViewModel)
        {
            if (ModelState.IsValid)
            {
                var item = await itemService.getItemById(reorderViewModel.ItemId);
                if (item != null)
                {
                    await itemService.reorderItem(item, reorderViewModel);

                    TempData["Success Message"] = $"{reorderViewModel.QuantityToOrder} units of {item.Name} have been ordered and stock updated.";

                    return RedirectToAction(nameof(LowStock));
                }
            }

            reorderViewModel.Suppliers = await supplierService.getAllSuppliers();
            return View(reorderViewModel);
        }

        public async Task<IActionResult> QuickReorder(int id, int quantity = 10)
        {
            await itemService.quickReorder(id, quantity);

            TempData["SuccessMessage"] = $"{quantity} units of item {id} have been added to stock";
            return RedirectToAction(nameof(LowStock));
        }

        private bool ItemExists(int id)
        {
            return itemService.itemExist(id);
        }

    }
}
