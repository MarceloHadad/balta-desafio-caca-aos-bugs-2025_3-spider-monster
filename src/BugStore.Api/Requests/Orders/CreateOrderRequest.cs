namespace BugStore.Requests.Orders;

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public List<OrderLineRequest> Lines { get; set; } = [];
}