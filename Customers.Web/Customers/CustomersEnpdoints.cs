using System.ComponentModel.DataAnnotations;

namespace Customers.Web.Customers;

public static class CustomersEnpdoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        var _customerGroup = app.MapGroup("/customers").WithOpenApi();
        var _customerGroupWithValidation = _customerGroup.MapGroup("/").WithParameterValidation();

        _customerGroup.MapGet("/", async (CustomerData data) =>
            {
                var customers = await data.ListAsync();
                return customers;
            })
            .WithName("ListCustomers");

        _customerGroup.MapGet("/{id:guid}", async (Guid id, CustomerData data) =>
                await data.GetByIdAsync(id) is Customer customer
                    ? TypedResults.Ok(customer)
                    : Results.NotFound()
            )
            .WithName("GetCustomerById");

        _customerGroupWithValidation.MapPost("/", async (CreateCustomerRequest request, CustomerData data) =>
            {
                var newCustomer = new Customer(Guid.NewGuid(), request.CompanyName, new());
                await data.AddAsync(newCustomer);
                return Results.Created($"/customers/{newCustomer.Id}", newCustomer);
            })
            .WithName("AddCustomer");

        _customerGroupWithValidation.MapPut("/{id:guid}",
                async (Guid id, UpdateCustomerRequest request, CustomerData data) =>
                {
                    var existingCustomer = await data.GetByIdAsync(id);
                    if (existingCustomer is null)
                    {
                        return Results.NotFound();
                    }

                    var updatedCustomer = existingCustomer with
                    {
                        CompanyName = request.CompanyName,
                        Projects = request.Projects ?? new List<Project>()
                    };
                    await data.UpdateAsync(updatedCustomer);
                    return Results.Ok(updatedCustomer);
                })
            .WithName("UpdateCustomer");

        _customerGroup.MapDelete("/{id:guid}", async (Guid id, CustomerData data) =>
            {
                if (await data.GetByIdAsync(id) is null)
                {
                    return Results.NotFound();
                }

                await data.DeleteAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteCustomer");
    }
}

public record Customer(Guid Id, string CompanyName, List<Project> Projects);

public record Project(Guid Id, string ProjectName, Guid CustomerId);

public readonly record struct CreateCustomerRequest
{
    [Required] [MinLength(10)] public string CompanyName { get; init; }
}

public readonly record struct UpdateCustomerRequest
{
    [Required] [MinLength(10)] public string CompanyName { get; init; }
    public List<Project> Projects { get; init; }
}