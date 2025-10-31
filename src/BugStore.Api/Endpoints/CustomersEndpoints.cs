using BugStore.Interfaces;
using BugStore.Requests.Customers;
using BugStore.Responses.Customers;

namespace BugStore.Endpoints;

public static class CustomersEndpoints
{
    public static void MapCustomersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/customers")
            .WithTags("Customers");

        group.MapGet("/", async (IHandler<GetCustomersRequest, GetCustomersResponse> handler) =>
        {
            var request = new GetCustomersRequest();
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapGet("/{id}", async (Guid id, IHandler<GetByIdCustomerRequest, GetByIdCustomerResponse> handler) =>
        {
            var request = new GetByIdCustomerRequest(id);
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/", async (IHandler<CreateCustomerRequest, CreateCustomerResponse> handler, CreateCustomerRequest request) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Created($"/v1/customers/{response.Id}", response);
        });

        group.MapPut("/{id}", async (Guid id, UpdateCustomerRequest request, IHandler<UpdateCustomerRequest, UpdateCustomerResponse> handler) =>
        {
            request.Id = id;
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapDelete("/{id}", async (Guid id, IHandler<DeleteCustomerRequest, DeleteCustomerResponse> handler) =>
        {
            var request = new DeleteCustomerRequest { Id = id };
            var response = await handler.HandleAsync(request);
            return Results.NoContent();
        });
    }
}