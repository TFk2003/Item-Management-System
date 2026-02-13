using MyAppMVC.Models;

namespace MyAppMVC.IServices
{
    public interface SupplierIService
    {
        Task<List<Supplier>> getAllSuppliers();
    }
}
