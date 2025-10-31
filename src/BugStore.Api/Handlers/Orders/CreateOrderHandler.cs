using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Models;
using BugStore.Requests.Orders;
using BugStore.Responses.Orders;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Orders;

public class CreateOrderHandler : IHandler<CreateOrderRequest, CreateOrderResponse>
{
    private readonly AppDbContext _context;

    public CreateOrderHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CreateOrderResponse> HandleAsync(CreateOrderRequest request)
    {
        if (request.CustomerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required");

        if (request.Lines == null || request.Lines.Count == 0)
            throw new ArgumentException("Order must have at least one line");

        var customerExists = await _context.Customers
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.CustomerId);
        if (!customerExists)
            throw new KeyNotFoundException("Customer not found");

        var productIds = request.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => new { p.Title, p.Price });

        if (products.Count != productIds.Count)
            throw new KeyNotFoundException("One or more products not found");

        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            CreatedAt = now,
            UpdatedAt = now,
            Lines = []
        };

        foreach (var lineRequest in request.Lines)
        {
            var product = products[lineRequest.ProductId];
            var total = product.Price * lineRequest.Quantity;

            var orderLine = new OrderLine
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = lineRequest.ProductId,
                Quantity = lineRequest.Quantity,
                Total = total
            };

            order.Lines.Add(orderLine);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var response = new CreateOrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Lines = order.Lines.Select(l => new OrderLineResponse
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductTitle = products[l.ProductId].Title,
                Quantity = l.Quantity,
                UnitPrice = products[l.ProductId].Price,
                Total = l.Total
            }).ToList()
        };

        return response;
    }
}
