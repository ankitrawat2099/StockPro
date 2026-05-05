public interface IPurchaseService
{
    Task<PurchaseOrder> CreatePO(PurchaseOrder po, List<CreatePOLineItemDto> items);
    Task<PurchaseOrder> GetPOById(int id);
    Task<List<PurchaseOrder>> GetPOsBySupplier(int supplierId);
    Task<List<PurchaseOrder>> GetPOsByStatus(string status);
    Task SubmitForApproval(int id);
    Task ApprovePO(int id);
    Task ReceiveGoods(int id, ReceiveGoodsDto dto);
    Task CancelPO(int id);
    Task UpdatePO(PurchaseOrder po);
    Task<List<PurchaseOrder>> GetPOsByWarehouse(int warehouseId);
    Task<List<PurchaseOrder>> GetPOsByDateRange(DateTime start, DateTime end);
}