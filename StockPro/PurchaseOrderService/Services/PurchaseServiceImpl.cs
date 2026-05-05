using Microsoft.EntityFrameworkCore;

public class PurchaseServiceImpl : IPurchaseService
{
    private readonly IPurchaseRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly PurchaseDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PurchaseServiceImpl(IPurchaseRepository repository, IHttpClientFactory httpClientFactory, PurchaseDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    //create 
    public async Task<PurchaseOrder> CreatePO(PurchaseOrder po, List<CreatePOLineItemDto> items)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            po.Status = "DRAFT";
            po.OrderDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

            await _context.PurchaseOrders.AddAsync(po);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                var lineItem = new POLineItem
                {
                    PoId = po.PoId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    TotalCost = item.Quantity * item.UnitCost,
                    ReceivedQty = 0
                };

                await _context.POLineItems.AddAsync(lineItem);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return po;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    //get by id
    public async Task<PurchaseOrder> GetPOById(int id)
    {
        return await _context.PurchaseOrders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.PoId == id);
    }

    public async Task<List<PurchaseOrder>> GetPOsBySupplier(int supplierId)
    {
        return await _context.PurchaseOrders
            .Include(x => x.Items)
            .Where(x => x.SupplierId == supplierId).ToListAsync();
    }

    public async Task<List<PurchaseOrder>> GetPOsByStatus(string status)
    {
        return await _context.PurchaseOrders
            .Include(x => x.Items)
            .Where(x => x.Status == status).ToListAsync();
    }

    public async Task<List<PurchaseOrder>> GetPOsByWarehouse(int warehouseId)
    {
        return await _context.PurchaseOrders
            .Include(x => x.Items)
            .Where(x => x.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<List<PurchaseOrder>> GetPOsByDateRange(DateTime start, DateTime end)
    {
        return await _context.PurchaseOrders
            .Include(x => x.Items)
            .Where(x => x.OrderDate >= start && x.OrderDate <= end).ToListAsync();
    }
    public async Task SubmitForApproval(int id)
    {
        var po = await _context.PurchaseOrders.FindAsync(id);

        if (po == null)
            throw new Exception("PO not found");

        if (po.Status != "DRAFT")
            throw new Exception("PO must be in DRAFT state to be submitted");

        po.Status = "PENDING";

        await _context.SaveChangesAsync();
    }

    public async Task ApprovePO(int id)
    {
        var po = await _context.PurchaseOrders.FindAsync(id);

        if (po == null)
            throw new Exception("PO not found");

        if (po.Status != "PENDING")
            throw new Exception("PO must be in PENDING state to be approved");

        po.Status = "APPROVED";

        await _context.SaveChangesAsync();
    }
    //recienve goods
    public async Task ReceiveGoods(int poId, ReceiveGoodsDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var po = await _context.PurchaseOrders.FindAsync(poId);

            if (po == null)
                throw new Exception("PO not found");

            if (po.Status != "APPROVED" && po.Status != "PARTIALLY_RECEIVED")
                throw new Exception("PO must be approved");

            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();

            var client = _httpClientFactory.CreateClient("warehouse");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    token.Replace("Bearer ", "")
                );

            var lineItems = await _context.POLineItems.Where(x => x.PoId == poId).ToListAsync();

            foreach (var itemDto in dto.Items)
            {
                var lineItem = lineItems.FirstOrDefault(x => x.LineItemId == itemDto.LineItemId);

                if (lineItem == null)
                    throw new Exception("Line item not found");

                var remaining = lineItem.Quantity - lineItem.ReceivedQty;

                if (itemDto.ReceivedQty > remaining)
                    throw new Exception("Invalid quantity");

                var response = await client.PostAsJsonAsync("/api/stock/update", new
                {
                    warehouseId = po.WarehouseId,
                    productId = lineItem.ProductId,
                    quantity = itemDto.ReceivedQty,
                    referenceType = "PURCHASE",
                    referenceId = po.PoId,
                    unitCost = lineItem.UnitCost,
                    notes = "Goods received from PO"
                });

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Warehouse update failed");

                lineItem.ReceivedQty += itemDto.ReceivedQty;
            }

            bool isFull = lineItems.All(x => x.ReceivedQty == x.Quantity);

            if (isFull)
            {
                po.Status = "RECEIVED";
                po.ReceivedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
            }
            else
            {
                po.Status = "PARTIALLY_RECEIVED";
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    //cancel
    public async Task CancelPO(int id)
    {
        var po = await _context.PurchaseOrders.FindAsync(id);

        if (po == null)
            throw new Exception("PO not found");

        if (po.Status == "RECEIVED")
            throw new Exception("Cannot cancel received PO");

        po.Status = "CANCELLED";

        await _context.SaveChangesAsync();
    }

    //update
    public async Task UpdatePO(PurchaseOrder po)
    {
        var existing = await _context.PurchaseOrders.FindAsync(po.PoId);

        if (existing == null)
            throw new Exception("PO not found");

        if (existing.Status != "DRAFT")
            throw new Exception("Only DRAFT PO can be updated");

        existing.SupplierId = po.SupplierId;
        existing.WarehouseId = po.WarehouseId;
        existing.ExpectedDate = po.ExpectedDate;
        existing.Notes = po.Notes;
        existing.ReferenceNumber = po.ReferenceNumber;

        await _context.SaveChangesAsync();
    }
}