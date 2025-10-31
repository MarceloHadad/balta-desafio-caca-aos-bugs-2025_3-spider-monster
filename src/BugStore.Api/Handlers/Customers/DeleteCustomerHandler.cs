using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Requests.Customers;
using BugStore.Responses.Customers;

namespace BugStore.Handlers.Customers;

public class DeleteCustomerHandler : IHandler<DeleteCustomerRequest, DeleteCustomerResponse>
{
    private readonly AppDbContext _context;

    public DeleteCustomerHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteCustomerResponse> HandleAsync(DeleteCustomerRequest request)
    {
        var customer = await _context.Customers.FindAsync(request.Id)
            ?? throw new KeyNotFoundException("Customer not found");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return new DeleteCustomerResponse();
    }
}