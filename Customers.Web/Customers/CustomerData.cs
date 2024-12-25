namespace Customers.Web.Customers;

public class CustomerData
{
    private readonly Guid _customerId1 = Guid.Parse("ea30ae59-a0f4-4234-af2f-840fbd442ae0");
    private readonly Guid _customerId2 = Guid.Parse("0da62277-d30a-4c99-a277-14f38c142c7f");

    private readonly List<Customer> _customers;

    public CustomerData()
    {
        _customers = new List<Customer>()
        {
            new Customer(_customerId1, "Acme", new List<Project>()
            {
                new Project(Guid.NewGuid(), "Project1", _customerId1),
                new Project(Guid.NewGuid(), "Project2", _customerId1),
            }),
            new Customer(_customerId2, "Contoso", new List<Project>()
            {
                new Project(Guid.NewGuid(), "Project1", _customerId2),
                new Project(Guid.NewGuid(), "Project2", _customerId2),
            })
        };
    }

    public Task<List<Customer>> ListAsync()
    {
        return Task.FromResult(_customers);
    }

    public Task<Customer?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
    }

    public Task AddAsync(Customer customer)
    {
        _customers.Add(customer);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Customer customer)
    {
        if (_customers.Any(c => c.Id == customer.Id))
        {
            var index = _customers.FindIndex(c => c.Id == customer.Id);
            _customers[index] = customer;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        if (_customers.Any(c => c.Id == id))
        {
            var index = _customers.FindIndex(c => c.Id == id);
            _customers.RemoveAt(index);
        }

        return Task.CompletedTask;
    }
}