namespace BugStore.Requests.Customers;

public class GetByIdCustomerRequest
{
    public Guid Id { get; set; }

    public GetByIdCustomerRequest(Guid id)
    {
        Id = id;
    }
}