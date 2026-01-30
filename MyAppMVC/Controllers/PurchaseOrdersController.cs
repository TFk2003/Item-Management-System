using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAppMVC.Data;
using MyAppMVC.Models;
using MyAppMVC.ViewModels;

namespace MyAppMVC.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private readonly DatabaseContext _context;

        public PurchaseOrdersController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: PurchaseOrders
        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(pi => pi.PurchaseOrderItems);
            return View(await databaseContext.ToListAsync());
        }

        // GET: PurchaseOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(pi => pi.PurchaseOrderItems).ThenInclude(i => i.Item)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            return View(purchaseOrder);
        }

        // GET: PurchaseOrders/Create
        public IActionResult Create()
        {
            ViewData["Items"] = _context.Items
                .Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    sku = i.SKU,
                    price = i.Price
                }).ToList();
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "CompanyName");
            return View(new PurchaseOrderViewModel());
        }

        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderViewModel purchaseOrderViewModel)
        {
            if (ModelState.IsValid)
            {
                // Set the order date
                var purchaseOrder = new PurchaseOrder
                {
                    OrderNumber = purchaseOrderViewModel.OrderNumber,
                    OrderDate = DateTime.UtcNow,
                    SupplierId = purchaseOrderViewModel.SupplierId,
                    ExpectedDeliveryDate = purchaseOrderViewModel.ExpectedDeliveryDate,
                    TotalAmount = purchaseOrderViewModel.TotalAmount,
                    Status = "Pending"
                };

                // Add the purchase order
                _context.Add(purchaseOrder);
                await _context.SaveChangesAsync();

                // Add purchase order items
                if (purchaseOrderViewModel.Items != null && purchaseOrderViewModel.Items.Any())
                {
                    foreach (var itemDto in purchaseOrderViewModel.Items.Where(i => i.ItemId > 0))
                    {
                        var purchaseOrderItem = new PurchaseOrderItem
                        {
                            PurchaseOrderId = purchaseOrder.Id,
                            ItemId = itemDto.ItemId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = itemDto.UnitPrice
                        };
                        _context.PurcahseOrderItems.Add(purchaseOrderItem);
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }


            ViewData["Items"] = _context.Items
                .Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    sku = i.SKU,
                    price = i.Price
                }).ToList();
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "CompanyName", purchaseOrderViewModel.SupplierId);
            return View(purchaseOrderViewModel);
        }

        // GET: PurchaseOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(pc => pc.PurchaseOrderItems).ThenInclude(i => i.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (purchaseOrder == null)
            {
                return NotFound();
            }

            // Prevent editing of delivered orders
            if (purchaseOrder.Status == "Delivered")
            {
                TempData["ErrorMessage"] = "Cannot edit a delivered purchase order.";
                return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
            }

            var ViewModel = new PurchaseOrderViewModel
            {
                Id = purchaseOrder.Id,
                OrderNumber = purchaseOrder.OrderNumber,
                OrderDate = purchaseOrder.OrderDate,
                SupplierId = purchaseOrder.SupplierId,
                ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
                ActualDeliveryDate = purchaseOrder.ActualDeliveryDate,
                TotalAmount = purchaseOrder.TotalAmount,
                Status = purchaseOrder.Status,
                Items = purchaseOrder.PurchaseOrderItems.Select(poi => new PurchaseOrderItemDto
                {
                    ItemId = poi.ItemId,
                    Quantity = poi.Quantity,
                    UnitPrice = poi.UnitPrice
                }).ToList()
            };

            ViewData["Items"] = _context.Items
                .Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    sku = i.SKU,
                    price = i.Price
                }).ToList();

            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "CompanyName", purchaseOrder.SupplierId);
            return View(ViewModel);
        }

        // POST: PurchaseOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseOrderViewModel purchaseOrderViewModel)
        {
            if (id != purchaseOrderViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var purchaseOrder = await _context.PurchaseOrders
                        .Include(po => po.PurchaseOrderItems)
                        .FirstOrDefaultAsync(po => po.Id == id);

                    if (purchaseOrder == null)
                    {
                        return NotFound();
                    }

                    // Prevent editing of already delivered orders
                    if (purchaseOrder.Status == "Delivered")
                    {
                        TempData["ErrorMessage"] = "Cannot edit a delivered purchase order.";
                        return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
                    }

                    // Check if status is changing to "Delivered"
                    bool statusChangedToDelivered = purchaseOrder.Status != "Delivered" && purchaseOrderViewModel.Status == "Delivered";

                    purchaseOrder.OrderNumber = purchaseOrderViewModel.OrderNumber;
                    purchaseOrder.SupplierId = purchaseOrderViewModel.SupplierId;
                    purchaseOrder.ExpectedDeliveryDate = purchaseOrderViewModel.ExpectedDeliveryDate;
                    purchaseOrder.Status = purchaseOrderViewModel.Status;
                    purchaseOrder.TotalAmount = purchaseOrderViewModel.TotalAmount;

                    // Set ActualDeliveryDate when status changes to "Delivered"
                    if (statusChangedToDelivered)
                    {
                        purchaseOrder.ActualDeliveryDate = DateTime.UtcNow;
                    }
                    else if (purchaseOrderViewModel.Status != "Delivered")
                    {
                        // Clear ActualDeliveryDate if status is changed from Delivered to something else
                        purchaseOrder.ActualDeliveryDate = purchaseOrderViewModel.ActualDeliveryDate;
                    }

                    _context.PurcahseOrderItems.RemoveRange(purchaseOrder.PurchaseOrderItems);

                    if(purchaseOrderViewModel.Items != null)
                    {
                        foreach (var itemDto in purchaseOrderViewModel.Items.Where(i => i.ItemId > 0))
                        {
                            var purchaseOrderItem = new PurchaseOrderItem
                            {
                                PurchaseOrderId = purchaseOrder.Id,
                                ItemId = itemDto.ItemId,
                                Quantity = itemDto.Quantity,
                                UnitPrice = itemDto.UnitPrice
                            };
                            _context.PurcahseOrderItems.Add(purchaseOrderItem);
                        }
                    }
                    
                    await _context.SaveChangesAsync();

                    // Update inventory if status changed to "Delivered"
                    if (statusChangedToDelivered)
                    {
                        await UpdateInventoryOnDelivery(id);
                        TempData["SuccessMessage"] = "Purchase order marked as delivered and inventory has been updated.";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderExists(purchaseOrderViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Items"] = _context.Items
                .Select(i => new
                {
                    id = i.Id,
                    name = i.Name,
                    sku = i.SKU,
                    price = i.Price
                }).ToList();
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "Id", "CompanyName", purchaseOrderViewModel.SupplierId);
            return View(purchaseOrderViewModel);
        }

        // GET: PurchaseOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrder = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrder == null)
            {
                return NotFound();
            }

            // Prevent deletion of delivered orders
            if (purchaseOrder.Status == "Delivered")
            {
                TempData["ErrorMessage"] = "Cannot delete a delivered purchase order.";
                return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
            }

            return View(purchaseOrder);
        }

        // POST: PurchaseOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder != null)
            {
                // Prevent deletion of delivered orders
                if (purchaseOrder.Status == "Delivered")
                {
                    TempData["ErrorMessage"] = "Cannot delete a delivered purchase order.";
                    return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
                }

                _context.PurchaseOrders.Remove(purchaseOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseOrderExists(int? id)
        {
            return _context.PurchaseOrders.Any(e => e.Id == id);
        }

        /// <summary>
        /// Updates inventory when a purchase order is marked as delivered
        /// </summary>
        private async Task UpdateInventoryOnDelivery(int purchaseOrderId)
        {
            var purchaseOrderItems = await _context.PurcahseOrderItems
                .Where(poi => poi.PurchaseOrderId == purchaseOrderId)
                .ToListAsync();

            foreach (var orderItem in purchaseOrderItems)
            {
                var item = await _context.Items.FindAsync(orderItem.ItemId);
                if (item != null)
                {
                    // Increase the stock quantity by the purchased quantity
                    item.StockQuantity += orderItem.Quantity;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
