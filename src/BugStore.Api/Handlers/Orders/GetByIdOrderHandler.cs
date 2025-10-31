using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Requests.Orders;
using BugStore.Responses.Orders;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Orders;

public class GetByIdOrderHandler : IHandler<GetByIdOrderRequest, GetByIdOrderResponse>
{
    private readonly AppDbContext _context;

    public GetByIdOrderHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GetByIdOrderResponse> HandleAsync(GetByIdOrderRequest request)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .FirstOrDefaultAsync(o => o.Id == request.Id)
                ?? throw new KeyNotFoundException("Order not found");

        var response = new GetByIdOrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.Name,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            TotalAmount = order.Lines.Sum(l => l.Total),
            Lines = order.Lines.Select(l => new OrderLineResponse
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductTitle = l.Product.Title,
                Quantity = l.Quantity,
                UnitPrice = l.Product.Price,
                Total = l.Total
            }).ToList()
        };

        return response;
    }
}
