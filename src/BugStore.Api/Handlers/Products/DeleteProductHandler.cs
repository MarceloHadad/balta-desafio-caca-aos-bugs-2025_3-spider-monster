using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Requests.Products;
using BugStore.Responses.Products;

namespace BugStore.Handlers.Products;

public class DeleteProductHandler : IHandler<DeleteProductRequest, DeleteProductResponse>
{
    private readonly AppDbContext _context;

    public DeleteProductHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteProductResponse> HandleAsync(DeleteProductRequest request)
    {
        var product = await _context.Products.FindAsync(request.Id)
            ?? throw new KeyNotFoundException("Product not found");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return new DeleteProductResponse();
    }
}
