using BugStore.Data;
using BugStore.Interfaces;
using BugStore.Requests.Customers;
using BugStore.Responses.Customers;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Handlers.Customers;

public class GetCustomersHandler : IHandler<GetCustomersRequest, GetCustomersResponse>
{
    private readonly AppDbContext _context;

    public GetCustomersHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GetCustomersResponse> HandleAsync(GetCustomersRequest request)
    {
        var customers = await _context.Customers
            .AsNoTracking()
            .Select(c => new GetByIdCustomerResponse
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                BirthDate = c.BirthDate
            })
            .ToListAsync();

        return new GetCustomersResponse
        {
            Customers = customers
        };
    }
}