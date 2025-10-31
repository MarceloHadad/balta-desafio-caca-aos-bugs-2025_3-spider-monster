using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Requests.Products;
using BugStore.Responses.Products;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Products;

public class GetByIdProductHandler : IHandler<GetByIdProductRequest, GetByIdProductResponse>
{
    private readonly AppDbContext _context;

    public GetByIdProductHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GetByIdProductResponse> HandleAsync(GetByIdProductRequest request)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id)
                ?? throw new KeyNotFoundException("Product not found");

        var response = new GetByIdProductResponse
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price
        };

        return response;
    }
}
