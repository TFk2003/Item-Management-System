using Microsoft.EntityFrameworkCore;
using MyAppMVC.Data;
using MyAppMVC.IServices;
using MyAppMVC.Models;

namespace MyAppMVC.Services
{
    public class SupplierService : SupplierIService
    {
        private readonly DatabaseContext _context;

        public SupplierService(DatabaseContext databaseContext)
        {
            _context = databaseContext;
        }
        public async Task<List<Supplier>> getAllSuppliers()
        {
            return await _context.Suppliers.ToListAsync();
        }
    }
}
