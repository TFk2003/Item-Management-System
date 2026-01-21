using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyAppMVC.Data;
using MyAppMVC.Models;

namespace MyAppMVC.Controllers
{
    public class PurchaseOrderItemsController : Controller
    {
        private readonly DatabaseContext _context;

        public PurchaseOrderItemsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: PurchaseOrderItems
        public async Task<IActionResult> Index()
        {
            var databaseContext = _context.PurcahseOrderItems.Include(p => p.Item).Include(p => p.PurchaseOrder);
            return View(await databaseContext.ToListAsync());
        }

        // GET: PurchaseOrderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrderItem = await _context.PurcahseOrderItems
                .Include(p => p.Item)
                .Include(p => p.PurchaseOrder)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrderItem == null)
            {
                return NotFound();
            }

            return View(purchaseOrderItem);
        }

        // GET: PurchaseOrderItems/Create
        public IActionResult Create()
        {
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name");
            ViewData["PurchaseOrderId"] = new SelectList(_context.PurchaseOrders, "Id", "OrderNumber");
            return View();
        }

        // POST: PurchaseOrderItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PurchaseOrderId,ItemId,Quantity,UnitPrice")] PurchaseOrderItem purchaseOrderItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(purchaseOrderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name", purchaseOrderItem.ItemId);
            ViewData["PurchaseOrderId"] = new SelectList(_context.PurchaseOrders, "Id", "OrderNumber", purchaseOrderItem.PurchaseOrderId);
            return View(purchaseOrderItem);
        }

        // GET: PurchaseOrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrderItem = await _context.PurcahseOrderItems.FindAsync(id);
            if (purchaseOrderItem == null)
            {
                return NotFound();
            }
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name", purchaseOrderItem.ItemId);
            ViewData["PurchaseOrderId"] = new SelectList(_context.PurchaseOrders, "Id", "OrderNumber", purchaseOrderItem.PurchaseOrderId);
            return View(purchaseOrderItem);
        }

        // POST: PurchaseOrderItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PurchaseOrderId,ItemId,Quantity,UnitPrice")] PurchaseOrderItem purchaseOrderItem)
        {
            if (id != purchaseOrderItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchaseOrderItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderItemExists(purchaseOrderItem.Id))
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
            ViewData["ItemId"] = new SelectList(_context.Items, "Id", "Name", purchaseOrderItem.ItemId);
            ViewData["PurchaseOrderId"] = new SelectList(_context.PurchaseOrders, "Id", "OrderNumber", purchaseOrderItem.PurchaseOrderId);
            return View(purchaseOrderItem);
        }

        // GET: PurchaseOrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseOrderItem = await _context.PurcahseOrderItems
                .Include(p => p.Item)
                .Include(p => p.PurchaseOrder)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseOrderItem == null)
            {
                return NotFound();
            }

            return View(purchaseOrderItem);
        }

        // POST: PurchaseOrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchaseOrderItem = await _context.PurcahseOrderItems.FindAsync(id);
            if (purchaseOrderItem != null)
            {
                _context.PurcahseOrderItems.Remove(purchaseOrderItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseOrderItemExists(int id)
        {
            return _context.PurcahseOrderItems.Any(e => e.Id == id);
        }
    }
}
