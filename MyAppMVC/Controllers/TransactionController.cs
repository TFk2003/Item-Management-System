using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAppMVC.Data;
using MyAppMVC.Models;

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
            ViewBag.Clients = new SelectList(await _databaseContext.Clients
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, Display = $"{c.FirstName} {c.LastName} - {c.Company}" })
                .ToListAsync(), "Id", "Display");

            return View(transaction);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemClient itemClient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var item = await _databaseContext.Items.FindAsync(itemClient.ItemId);
                    if (item == null)
                    {
                        ModelState.AddModelError("", "Selected item does not exist.");
                        await PopulateDropdowns();
                        return View(itemClient);
                    }
                    if (item.StockQuantity < itemClient.Quantity)
                    {
                        ModelState.AddModelError("Quantity", $"Insufficient Stock. Only {item.StockQuantity} units available.");
                        await PopulateDropdowns();
                        return View(itemClient);
                    }

                    itemClient.UnitPrice = item.Price;
                    item.StockQuantity -= itemClient.Quantity;
                    item.LastUpdated = DateTime.UtcNow;

                    _databaseContext.Add(itemClient);
                    await _databaseContext.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Transaction created successfully. {itemClient.Quantity} x {item.Name} is sold to client.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating transaction");
                    ModelState.AddModelError("", "An error occurred while creating the transaction. Please try again.");
                }
            }
            await _databaseContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int itemId, int clientId)
        {
            var itemClient = await _databaseContext.ItemClients
                .Include(ic => ic.Item)
                .Include(ic => ic.Client)
                .FirstOrDefaultAsync(ic => ic.ItemId == itemId && ic.ClientId == clientId);

            if (itemClient == null)
            {
                return NotFound();
            }

            await PopulateDropdowns();
            return View(itemClient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemClient itemClient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var originalTransactiion = await _databaseContext.ItemClients
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ic => ic.ItemId == itemClient.ItemId && ic.ClientId == itemClient.ClientId);
                    
                    if (originalTransactiion == null)
                    {
                        return NotFound();
                    }

                    var item = await _databaseContext.Items.FindAsync(itemClient.ItemId);

                    if (item == null)
                    {
                        ModelState.AddModelError("", "Selected item does not exist.");
                        await PopulateDropdowns();
                        return View(itemClient);
                    }

                    int quantityDifference = itemClient.Quantity - originalTransactiion.Quantity;

                    if (quantityDifference > 0 && item.StockQuantity < quantityDifference)
                    {
                        ModelState.AddModelError("Quantity", $"Insufficient Stock. Only {item.StockQuantity} additional units available.");
                        await PopulateDropdowns();
                        return View(itemClient);
                    }

                    item.StockQuantity -= quantityDifference;
                    item.LastUpdated = DateTime.UtcNow;

                    _databaseContext.Update(itemClient);
                    await _databaseContext.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Transaction updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch(DbUpdateConcurrencyException)
                {
                    if (!ItemClientExists(itemClient.ItemId,itemClient.ClientId))
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
                    _logger.LogError(ex, "Error updating transaction");
                    ModelState.AddModelError("", "An error occurred while updating the transaction. Please try again.");
                }
            }
            await PopulateDropdowns();
            return View(itemClient);
        }

        public async Task<IActionResult> Delete(int itemId, int clientId)
        {
            var itemClient = await _databaseContext.ItemClients
                .Include(ic => ic.Item)
                .Include(ic => ic.Client)
                .FirstOrDefaultAsync(ic => ic.ItemId == itemId && ic.ClientId == clientId);

            if (itemClient == null)
            {
                return NotFound();
            }
            return View(itemClient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int itemId, int clientId)
        {
            try
            {
                var itemClient = await _databaseContext.ItemClients
                    .FirstOrDefaultAsync(ic => ic.ItemId == itemId && ic.ClientId == clientId);

                if (itemClient == null)
                {
                    return NotFound();
                }

                var item = await _databaseContext.Items.FindAsync(itemClient.ItemId);
                
                if (item != null)
                {
                    item.StockQuantity += itemClient.Quantity;
                    item.LastUpdated = DateTime.UtcNow;
                }

                _databaseContext.ItemClients.Remove(itemClient);
                await _databaseContext.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Transaction deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction");
                ModelState.AddModelError("", "An error occurred while deleting the transaction. Please try again.");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ClientHistory(int id)
        {
            var client = await _databaseContext.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var transactions = await _databaseContext.ItemClients
                .Include(ic => ic.Item)
                .Where(ic => ic.ClientId == id)
                .OrderByDescending(ic => ic.PurchasedDate)
                .ToListAsync();

            ViewBag.Client = client;
            ViewBag.TotalSpent = transactions.Sum(ic => ic.TotalPrice);
            ViewBag.TotalItems = transactions.Sum(ic => ic.Quantity);

            return View(transactions);
        }

        // GET: Transaction/ItemHistory/5
        public async Task<IActionResult> ItemHistory(int id)
        {
            var item = await _databaseContext.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var transactions = await _databaseContext.ItemClients
                .Include(ic => ic.Client)
                .Where(ic => ic.ItemId == id)
                .OrderByDescending(ic => ic.PurchasedDate)
                .ToListAsync();

            ViewBag.Item = item;
            ViewBag.TotalSold = transactions.Sum(ic => ic.Quantity);
            ViewBag.TotalRevenue = transactions.Sum(ic => ic.TotalPrice);

            return View(transactions);
        }

        // Quick Sale Action
        public async Task<IActionResult> QuickSale(int itemId)
        {
            var item = await _databaseContext.Items
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                return NotFound();
            }

            var viewModel = new ItemClient
            {
                ItemId = itemId,
                Item = item,
                UnitPrice = item.Price,
                Quantity = 1,
                PurchasedDate = DateTime.UtcNow
            };

            await PopulateDropdowns();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickSale(ItemClient itemClient)
        {
            if (ModelState.IsValid)
            {
                return await Create(itemClient);
            }

            await PopulateDropdowns();
            return View(itemClient);
        }

        private bool ItemClientExists(int itemId, int clientId)
        {
            return _databaseContext.ItemClients.Any(ic => ic.ItemId == itemId && ic.ClientId == clientId);
        }

        private async Task PopulateDropdowns()
        {
            var items = await _databaseContext.Items
                .Where(i => i.IsActive && i.StockQuantity > 0)
                .Select(i => new { i.Id, Display = $"{i.Name} (Stock: {i.StockQuantity}, Price: ${i.Price})" })
                .ToListAsync();

            var clients = await _databaseContext.Clients
                .Where(c => c.IsActive)
                .Select(c => new { c.Id, Display = $"{c.FirstName} {c.LastName} - {c.Company}" })
                .ToListAsync();

            ViewBag.Items = new SelectList(items, "Id", "Display");
            ViewBag.Clients = new SelectList(clients, "Id", "Display");
        }
    }
}
