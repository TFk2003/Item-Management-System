using MyAppMVC.Models;
using MyAppMVC.ViewModels;

namespace MyAppMVC.IServices
{
    public interface ItemIService
    {
        Task<List<Item>> getAllItem();
        Task<Item?> getItemById(int id);
        Task<Item?> getItemByIdWithCategoryAndSupplier(int id);
        Task<Item?> getItemByIdWithSCSI(int id);
        Task<Item> getItemByIdWithSCSIC(int id);
        Task<Item?> getItemByIdWithSCS(int id);
        Task addItem(Item item
            , string serialCode, string serialName,
            string warrantyInfo, DateTime? manufactureDate, DateTime? expiryDate);
        Task updateItem(int id, Item item,
            string serialCode, string serialName, string warrantyInfo, DateTime? manufactureDate, DateTime? expiryDate);
        Task deleteItem(int id);
        Task<List<Item>> lowStockItems(int? threshold);

        Task reorderItem(Item item, ReorderViewModel reorderViewModel);

        Task quickReorder(int id, int quantity = 10);

        bool itemExist(int id);
    }
}
