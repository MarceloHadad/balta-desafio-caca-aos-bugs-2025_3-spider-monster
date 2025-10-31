using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Requests.Products;
using BugStore.Responses.Products;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Products;

public class GetProductsHandler : IHandler<GetProductsRequest, GetProductsResponse>
{
    private readonly AppDbContext _context;

    public GetProductsHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductsResponse> HandleAsync(GetProductsRequest request)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Select(p => new GetByIdProductResponse
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Slug = p.Slug,
                Price = p.Price
            })
            .ToListAsync();

        return new GetProductsResponse
        {
            Products = products
        };
    }
}
