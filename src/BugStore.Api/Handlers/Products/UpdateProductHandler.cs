using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Requests.Products;
using BugStore.Responses.Products;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Products;

public class UpdateProductHandler : IHandler<UpdateProductRequest, UpdateProductResponse>
{
    private readonly AppDbContext _context;

    public UpdateProductHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateProductResponse> HandleAsync(UpdateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Description is required");
        if (string.IsNullOrWhiteSpace(request.Slug))
            throw new ArgumentException("Slug is required");
        if (request.Price <= 0)
            throw new ArgumentException("Price must be greater than zero");

        var existingProduct = await _context.Products.FindAsync(request.Id)
            ?? throw new KeyNotFoundException("Product not found");

        var slugInUse = await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Slug == request.Slug && p.Id != request.Id);
        if (slugInUse)
            throw new InvalidOperationException("Slug already in use");

        existingProduct.Title = request.Title;
        existingProduct.Description = request.Description;
        existingProduct.Slug = request.Slug;
        existingProduct.Price = request.Price;

        await _context.SaveChangesAsync();

        var response = new UpdateProductResponse
        {
            Id = existingProduct.Id,
            Title = existingProduct.Title,
            Description = existingProduct.Description,
            Slug = existingProduct.Slug,
            Price = existingProduct.Price
        };

        return response;
    }
}
