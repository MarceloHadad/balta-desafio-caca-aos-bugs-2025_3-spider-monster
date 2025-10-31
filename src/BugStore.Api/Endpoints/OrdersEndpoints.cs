using BugStore.Interfaces;
using BugStore.Requests.Orders;
using BugStore.Responses.Orders;

namespace BugStore.Endpoints;

public static class OrdersEndpoints
{
    public static void MapOrdersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/v1/orders")
            .WithTags("Orders");

        group.MapGet("/{id}", async (Guid id, IHandler<GetByIdOrderRequest, GetByIdOrderResponse> handler) =>
        {
            var request = new GetByIdOrderRequest(id);
            var response = await handler.HandleAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/", async (IHandler<CreateOrderRequest, CreateOrderResponse> handler, CreateOrderRequest request) =>
        {
            var response = await handler.HandleAsync(request);
            return Results.Created($"/v1/orders/{response.Id}", response);
        });
    }
}