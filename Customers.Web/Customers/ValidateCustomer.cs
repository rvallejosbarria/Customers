namespace Customers.Web.Customers;

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