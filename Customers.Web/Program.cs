using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<CustomerData>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/customers", async (CustomerData data) =>
    {
        var customers = await data.ListAsync();
        return customers;
    })
    .WithName("ListCustomers")
    .WithOpenApi();

app.MapGet("/customers/{id:guid}", async (Guid id, CustomerData data) =>
        await data.GetByIdAsync(id) is Customer customer
            ? TypedResults.Ok(customer)
            : Results.NotFound()
    )
    .WithName("GetCustomerById")
    .WithOpenApi();

app.MapPost("/customers", async (Customer customer, CustomerData data) =>
    {
        var newCustomer = customer with { Id = Guid.NewGuid(), Projects = new() };
        await data.AddAsync(newCustomer);
        return Results.Created($"/customers/{newCustomer.Id}", newCustomer);
    })
    // .AddEndpointFilter(ValidationHelpers.ValidateAddCustomer)
    .AddEndpointFilter<ValidateCustomer>()
    .WithName("AddCustomer")
    .WithOpenApi();

app.MapPut("/customers/{id:guid}", async ([AsParameters] PutRequest request) =>
    {
        var existingCustomer = await request.Data.GetByIdAsync(request.Id);
        if (existingCustomer is null)
        {
            return Results.NotFound();
        }

        var updatedCustomer = existingCustomer with
        {
            CompanyName = request.Customer.CompanyName,
            Projects = request.Customer.Projects ?? new List<Project>()
        };
        await request.Data.UpdateAsync(updatedCustomer);
        return Results.Ok(updatedCustomer);
    })
    // .AddEndpointFilter(ValidationHelpers.ValidateAddCustomer)
    // .AddEndpointFilter<ValidateCustomer>()
    .WithParameterValidation()
    .WithName("UpdateCustomer")
    .WithOpenApi();

app.MapDelete("/customers/{id:guid}", async (Guid id, CustomerData data) =>
    {
        if (await data.GetByIdAsync(id) is null)
        {
            return Results.NotFound();
        }

        await data.DeleteAsync(id);
        return Results.NoContent();
    })
    .WithName("DeleteCustomer")
    .WithOpenApi();

app.Run();

public record Customer(Guid Id, [MinLength(10)] string CompanyName, List<Project> Projects);

public record Project(Guid Id, string ProjectName, Guid CustomerId);

public readonly record struct PutRequest
{
    [FromRoute(Name = "id")] [Required] public Guid Id { get; init; }
    [Required] public Customer Customer { get; init; }
    public CustomerData Data { get; init; }
}

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

public class ValidationHelpers
{
    internal static async ValueTask<object?> ValidateAddCustomer(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var customer = context.GetArgument<Customer>(0);
        if (customer is not null && string.IsNullOrEmpty(customer.CompanyName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>()
            {
                { "CompanyName", new[] { "Please enter a valid company name" } }
            });
        }

        return await next(context);
    }

    internal static async ValueTask<object?> ValidateUpdateCustomer(EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var customer = context.GetArgument<Customer>(1);
        if (customer is not null && string.IsNullOrEmpty(customer.CompanyName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>()
            {
                { "CompanyName", new[] { "Please enter a valid company name" } }
            });
        }

        return await next(context);
    }
}

public class ValidateCustomer : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var customer = context.Arguments.FirstOrDefault(a => a is Customer) as Customer;

        if (customer is not null && string.IsNullOrEmpty(customer.CompanyName))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>()
            {
                { "CompanyName", new[] { "Please enter a valid company name" } }
            });
        }

        return await next(context);
    }
}