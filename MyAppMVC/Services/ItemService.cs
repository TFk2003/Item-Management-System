using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MyAppMVC.Data;
using MyAppMVC.IServices;
using MyAppMVC.Models;
using MyAppMVC.ViewModels;

namespace MyAppMVC.Services
{
    public class ItemService : ItemIService
    {
        private readonly DatabaseContext _context;
        public ItemService(DatabaseContext databaseContext) 
        {
            _context = databaseContext;
        }
        public async Task addItem(Item item
            , string serialCode, string serialName,
            string warrantyInfo, DateTime? manufactureDate, DateTime? expiryDate)
        {
            Debug.WriteLine(item.Name);
            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(serialCode))
            {
                var serialNumber = new SerialNumber
                {
                    SerialCode = serialCode,
                    Name = serialName,
                    WarrantyInfo = warrantyInfo,
                    ManufactureDate = manufactureDate,
                    ExpiryDate = expiryDate,
                    ItemId = item.Id  // ✅ Now item.Id has a valid value
                };

                _context.SerialNumbers.Add(serialNumber);
                await _context.SaveChangesAsync();

                // Update the item with the serial number reference
                item.SerailId = serialNumber.Id;
                _context.Update(item);
                await _context.SaveChangesAsync();
            }

        }

        public async Task deleteItem(int id)
        {
            var item = await _context.Items
                .Include(i => i.SerialNumber) // Include the one SerialNumber via SerailId
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return;
            }

            // Break the circular reference by nulling out the Item's SerailId
            item.SerailId = null;
            item.SerialNumber.ItemId = null;
            await _context.SaveChangesAsync();

            // Delete all SerialNumbers that reference this Item via ItemId
            var relatedSerialNumbers = await _context.SerialNumbers
                .Where(s => s.ItemId == id)
                .ToListAsync();

            _context.SerialNumbers.RemoveRange(relatedSerialNumbers);

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Item>> getAllItem()
        {
             List<Item> items = await _context.Items
                .Include(s => s.SerialNumber)
                .Include(c => c.Category)
                .Include(i => i.ItemClients)
                .Include(s => s.Supplier)
                .ToListAsync();

            return items;
        }

        public async Task<Item?> getItemById(int id)
        {
            var item = await _context.Items.FindAsync(id);

            if (item == null)
            {
                return null;
            }

            return item;
        }

        public async Task<Item?> getItemByIdWithCategoryAndSupplier(int id)
        {
            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return null;
            }
            return item;
        }

        public async Task<Item?> getItemByIdWithSCS(int id)
        {
            var item = await _context.Items
                .Include(s => s.SerialNumber)
                .Include(c => c.Category)
                .Include(s => s.Supplier)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return null;
            }

            return item;
        }

        public async Task<Item?> getItemByIdWithSCSI(int id)
        {
            var item = await _context.Items
                .Include(s => s.SerialNumber)
                .Include(c => c.Category)
                .Include(s => s.Supplier)
                .Include(i => i.ItemClients)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return null;
            }

            return item;
        }

        public async Task<Item?> getItemByIdWithSCSIC(int id)
        {
            var item = await _context.Items
                .Include(s => s.SerialNumber)
                .Include(c => c.Category)
                .Include(s => s.Supplier)
                .Include(i => i.ItemClients).ThenInclude(ic => ic.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return null;
            }

            return item;
        }

        public async Task<List<Item>> lowStockItems(int? threshold)
        {
            var query = _context.Items
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

            return lowStockItems;
        }

        public async Task updateItem(int id, Item item,
            string serialCode, string serialName, string warrantyInfo, DateTime? manufactureDate, DateTime? expiryDate)
        {
            var existingItem = await _context.Items
                        .Include(i => i.SerialNumber)
                        .FirstOrDefaultAsync(i => i.Id == id);

            if (existingItem == null)
            {
                return;
            }

            existingItem.Name = item.Name;
            existingItem.Description = item.Description;
            existingItem.Price = item.Price;
            existingItem.StockQuantity = item.StockQuantity;
            existingItem.ReorderLevel = item.ReorderLevel;
            existingItem.SKU = item.SKU;
            existingItem.Manufacturer = item.Manufacturer;
            existingItem.CategoryId = item.CategoryId;
            existingItem.SupplierId = item.SupplierId;
            existingItem.IsActive = item.IsActive;
            existingItem.LastUpdated = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(serialCode))
            {
                if (existingItem.SerialNumber != null)
                {
                    // Update existing serial number
                    existingItem.SerialNumber.SerialCode = serialCode;
                    existingItem.SerialNumber.Name = serialName;
                    existingItem.SerialNumber.WarrantyInfo = warrantyInfo;
                    existingItem.SerialNumber.ManufactureDate = manufactureDate;
                    existingItem.SerialNumber.ExpiryDate = expiryDate;
                    existingItem.SerialNumber.ItemId = id;
                }
                else
                {
                    // Create new serial number
                    var serialNumber = new SerialNumber
                    {
                        SerialCode = serialCode,
                        Name = serialName,
                        WarrantyInfo = warrantyInfo,
                        ManufactureDate = manufactureDate,
                        ExpiryDate = expiryDate,
                        ItemId = id
                    };

                    _context.SerialNumbers.Add(serialNumber);
                    await _context.SaveChangesAsync(); // Save to get ID

                    existingItem.SerailId = serialNumber.Id;
                }
            }
            else
            {
                // Remove serial number if serialCode is empty
                if (existingItem.SerialNumber != null)
                {
                    _context.SerialNumbers.Remove(existingItem.SerialNumber);
                    existingItem.SerailId = null;
                }
            }
            _context.Update(existingItem);
            await _context.SaveChangesAsync();
        }

        public async Task reorderItem(Item item, ReorderViewModel reorderViewModel)
        {
            item.StockQuantity += reorderViewModel.QuantityToOrder;
            item.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task quickReorder(int id, int quantity = 10)
        {
            var item = await getItemById(id);
            if (item == null)
            {
                return;
            }
            item.StockQuantity += quantity;
            item.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public bool itemExist(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}
