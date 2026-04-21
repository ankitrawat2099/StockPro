public interface IPurchaseRepository
{
    Task<PurchaseOrder> FindByPoId(int poId);
    Task<List<PurchaseOrder>> FindBySupplierId(int supplierId);
    Task<List<PurchaseOrder>> FindByWarehouseId(int warehouseId);
    Task<List<PurchaseOrder>> FindByStatus(string status);
    Task<List<PurchaseOrder>> FindByOrderDateBetween(DateTime start, DateTime end);
    Task<List<PurchaseOrder>> FindByCreatedById(Guid userId);
    Task<int> CountByStatus(string status);
}