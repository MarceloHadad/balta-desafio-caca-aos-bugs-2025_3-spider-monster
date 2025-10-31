using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Models;
using BugStore.Requests.Products;
using BugStore.Responses.Products;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Products;

public class CreateProductHandler : IHandler<CreateProductRequest, CreateProductResponse>
{
    private readonly AppDbContext _context;

    public CreateProductHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CreateProductResponse> HandleAsync(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Description is required");
        if (string.IsNullOrWhiteSpace(request.Slug))
            throw new ArgumentException("Slug is required");
        if (request.Price <= 0)
            throw new ArgumentException("Price must be greater than zero");

        var slugInUse = await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Slug == request.Slug);
        if (slugInUse)
            throw new InvalidOperationException("Slug already in use");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Slug = request.Slug,
            Price = request.Price
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var response = new CreateProductResponse
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
