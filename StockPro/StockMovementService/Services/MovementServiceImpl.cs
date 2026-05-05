using System.Security.Claims;

public class MovementServiceImpl : IMovementService
{
    private readonly IMovementRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MovementServiceImpl(IMovementRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task RecordMovementAsync(CreateMovementDto dto)
    {
        if (dto == null)
            throw new ArgumentException("Invalid request");

        if (dto.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        if (string.IsNullOrWhiteSpace(dto.MovementType))
            throw new ArgumentException("MovementType is required");

        var movementType = dto.MovementType.ToUpper();
        var validTypes = new[] { "STOCK_IN", "STOCK_OUT", "TRANSFER_IN", "TRANSFER_OUT" };
        if (!validTypes.Contains(movementType))
        {

            throw new ArgumentException("Invalid movement type");
        }

        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        Guid performedBy;

        if (!Guid.TryParse(userId, out performedBy))
            throw new UnauthorizedAccessException("Invalid user token");

        var movement = new StockMovement
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            MovementType = movementType,
            Quantity = dto.Quantity,
            ReferenceType = dto.ReferenceType,
            ReferenceId = dto.ReferenceId,
            UnitCost = dto.UnitCost,
            Notes = dto.Notes,
            MovementDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
            PerformedBy = performedBy,
            BalanceAfter = dto.BalanceAfter
        };

        await _repository.AddAsync(movement);
    }

    public async Task<List<StockMovement>> GetByProductAsync(Guid productId)
    {
        return await _repository.FindByProductIdAsync(productId);
    }
    public async Task<List<StockMovement>> GetByWarehouseAsync(int warehouseId)
    {
        return await _repository.FindByWarehouseIdAsync(warehouseId);
    }

    public async Task<List<StockMovement>> GetByTypeAsync(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Movement type is required");

        return await _repository.FindByMovementTypeAsync(type.ToUpper());
    }

    public async Task<List<StockMovement>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        if (start > end)
            throw new ArgumentException("Invalid date range");

        return await _repository.FindByMovementDateBetweenAsync(start, end);
    }

    public async Task<List<StockMovement>> GetByReferenceAsync(int referenceId)
    {
        return await _repository.FindByReferenceIdAsync(referenceId);
    }

    public async Task<List<StockMovement>> GetMovementHistoryAsync(Guid productId, int warehouseId)
    {
        var list = await _repository.FindByProductIdAsync(productId);

        return list.Where(x => x.WarehouseId == warehouseId).OrderByDescending(x => x.MovementDate).ToList();
    }

    public async Task<int> GetStockInAsync(Guid productId)
    {
        return await _repository.CountByProductIdAndTypeAsync(productId, "STOCK_IN");
    }

    public async Task<int> GetStockOutAsync(Guid productId)
    {
        return await _repository.CountByProductIdAndTypeAsync(productId, "STOCK_OUT");
    }

    public async Task<List<StockMovement>> GetAllMovementsAsync()
    {
        return await _repository.FindByMovementDateBetweenAsync(DateTime.MinValue, DateTime.MaxValue);
    }

    public async Task<List<StockMovement>> GetByPerformedByAsync(Guid userId)
    {
        return await _repository.FindByPerformedByAsync(userId);
    }
}